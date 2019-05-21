// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using iotc_csharp_device_client.Authentication;
using iotc_csharp_device_client.enums;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace iotc_csharp_device_client
{

    public class IoTCClient : IIoTCClient
    {

        const int DEFAULT_EXPIRATION = 21600;
        const string DPS_DEFAULT_ENDPOINT = "global.azure-devices-provisioning.net";
        const string DPS_DEFAULT_API = "2018-09-01-preview";
        private DeviceClient deviceClient;

        private Dictionary<IoTCEvents, Delegate> callbacks;

        public ILogger Logger { get; set; }
        public string Id { get; set; }
        public string ScopeId { get; set; }
        public string ModelId { get; set; }
        public IoTCConnect AuthenticationType { get; set; }
        public string SasKey { get; set; }
        public IoTCentralCert Certificate { get; set; }
        public TransportType Protocol { get; set; } = TransportType.Mqtt;
        public string Endpoint { get; set; } = DPS_DEFAULT_ENDPOINT;
        public string ApiVersion { get; set; } = DPS_DEFAULT_API;

        /**
         * 
         * @param id                 The device Id
         * @param scopeId            Scope Id of the application
         * @param authenticationType Type of authentication: It can be Group symmetric
         *                           key, Device SAS key or x.509
         * @param options            Value for authentication: keys for symmetric and
         *                           SAS key authentication or x.509 certificate
         */
        public IoTCClient(string id, string scopeId, IoTCConnect authenticationType, object options)
        {
            Logger = new ConsoleLogger();
            Id = id;
            ScopeId = scopeId;
            AuthenticationType = authenticationType;
            if (AuthenticationType == IoTCConnect.SYMM_KEY || AuthenticationType == IoTCConnect.DEVICE_KEY)
            {
                SasKey = (string)options;
            }
            else if (this.AuthenticationType == IoTCConnect.X509_CERT)
            {
                Certificate = (IoTCentralCert)options;
            }
            callbacks = new Dictionary<IoTCEvents, Delegate>();
        }

        /**
         * @return the modelId
         */
        public string GetModelId()
        {
            return ModelId;
        }

        public IoTCClient(string id, string modelId, string scopeId, IoTCConnect authenticationType, object options) : this(id, scopeId, authenticationType, options)
        {

            ModelId = modelId;
        }

        /**
         * 
         * @param id                 The device Id
         * @param scopeId            Scope Id of the application
         * @param authenticationType Type of authentication: It can be Group symmetric
         *                           key, Device SAS key or x.509
         * @param options            Value for authentication: keys for symmetric and
         *                           SAS key authentication or x.509 certificate
         * @param logger             A custom logger implementing the ILogger interface
         */
        public IoTCClient(string id, string scopeId, IoTCConnect authenticationType, object options, ILogger logger) : this(id, scopeId, authenticationType, options)
        {

            Logger = logger;
        }

        public IoTCClient(string id, string modelId, string scopeId, IoTCConnect authenticationType, object options,
                ILogger logger) : this(id, scopeId, authenticationType, options, logger)
        {

            ModelId = modelId;
        }

        /**
         * @param logLevel the logger to set
         */
        public void SetLogging(IoTCLogging logLevel)
        {
            Logger.SetLevel(logLevel);
        }

        public void SetProtocol(IoTCProtocol transport)
        {
            Protocol = (TransportType)Enum.Parse(typeof(TransportType), Utils.GetEnumDesc(transport));
            Logger.Log("Transport set to " + transport);
        }

        public void SetGlobalEndpoint(string endpoint)
        {
            Endpoint = endpoint;
            Logger.Log("Endpoint changed to: " + endpoint);
        }


        public void SetProxy(HttpProxyOptions options)
        {
            //this.logger.Log("Setting proxy to " + options.host_address);
            //System.setProperty("http.proxyHost", options.host_address);
            //System.setProperty("http.proxyPort", string.valueOf(options.port));
            //System.setProperty("http.proxyUser", options.username);
            //System.setProperty("http.proxyPassword", options.password);
            //Authenticator.setDefault(new Authenticator() {
            //
            //public PasswordAuthentication getPasswordAuthentication()
            //{
            //    return new PasswordAuthentication(options.username, options.password.toCharArray());
            //}
        }


        public async Task Disconnect(Action<object> callback)
        {
            await deviceClient.CloseAsync();
            callback?.Invoke("Disconnected");
        }

        /**
         * Register a device in IoTCentral using authentication provided at construction
         * time
         * 
         * @return DeviceClient instance
         * @throws IoTCentralException
         */
        private async Task<DeviceClient> Register()
        {
            if (AuthenticationType == IoTCConnect.SYMM_KEY)
            {
                return await new SasAuthentication(this).RegisterWithSaSKey(SasKey);
            }
            else if (AuthenticationType == IoTCConnect.DEVICE_KEY)
            {
                return await new SasAuthentication(this).RegisterWithDeviceKey(SasKey);
            }
            return await new CertAuthentication(this).Register(Certificate.GetCertificate());
            // this.logger).Register();
        }


        public async Task Connect()
        {
            deviceClient = await Register();

            await deviceClient.OpenAsync();
            if (callbacks.ContainsKey(IoTCEvents.ConnectionStatus))
                deviceClient.SetConnectionStatusChangesHandler((ConnectionStatusChangesHandler)callbacks[IoTCEvents.ConnectionStatus]);
            if (callbacks.ContainsKey(IoTCEvents.Command))
                await deviceClient.SetMethodDefaultHandlerAsync((MethodCallback)callbacks[IoTCEvents.Command], null);
            if (callbacks.ContainsKey(IoTCEvents.SettingsUpdated))
                await deviceClient.SetDesiredPropertyUpdateCallbackAsync((DesiredPropertyUpdateCallback)callbacks[IoTCEvents.SettingsUpdated], null);

            this.Logger.Log("Device connected");

        }


        public Task SendTelemetry(object payload, Action<object> callback)
        {
            return SendEvent(payload, callback);
        }


        public Task SendState(object payload, Action<object> callback)
        {
            return SendEvent(payload, callback);
        }


        public async Task SendEvent(object payload, Action<object> callback)
        {
            JObject jsonObj;
            if (payload.GetType() == typeof(string))
            {
                jsonObj = JObject.Parse((string)payload);
            }
            else if (payload.GetType() == typeof(JObject))
            {
                jsonObj = (JObject)payload;
            }
            else
            {
                jsonObj = JObject.Parse(JsonConvert.SerializeObject(payload));
            }
            Message msg = new Message(Encoding.UTF8.GetBytes(jsonObj.ToString(Formatting.None)));
            await deviceClient.SendEventAsync(msg);
            Logger.Log($"Message: {jsonObj.ToString(Formatting.None)}");
            callback?.Invoke("Event sent");

        }


        public async Task SendProperty(object payload, Action<object> callback)
        {
            TwinCollection propertiesSet = new TwinCollection();
            JObject jsonObj;
            if (payload.GetType() == typeof(string))
            {
                jsonObj = JObject.Parse((string)payload);
            }
            else if (payload.GetType() == typeof(JObject))
            {
                jsonObj = (JObject)payload;
            }
            else
            {
                jsonObj = JObject.Parse(JsonConvert.SerializeObject(payload));
            }

            foreach (var obj in jsonObj)
            {
                propertiesSet[obj.Key] = obj.Value;
            }

            await deviceClient.UpdateReportedPropertiesAsync(propertiesSet);
            callback?.Invoke("Properties sent");
        }



        public void on(IoTCEvents iotcEvent, Delegate callback)
        {
            switch (iotcEvent)
            {
                case IoTCEvents.ConnectionStatus:
                    callbacks[IoTCEvents.ConnectionStatus] = callback;
                    break;
                case IoTCEvents.SettingsUpdated:
                    callbacks[IoTCEvents.SettingsUpdated] = callback;
                    break;
                case IoTCEvents.Command:
                    callbacks[IoTCEvents.Command] = callback;
                    break;
                default:
                    break;
            }
        }


        public void SetDPSApiVersion(string apiversion)
        {
            ApiVersion = apiversion;
            Logger.Log("API version changed to: " + apiversion);
        }

    }
}