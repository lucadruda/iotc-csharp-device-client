// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using iotc_csharp_device_client.Authentication;
using iotc_csharp_device_client.enums;
using iotc_csharp_device_client.Models;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iotc_csharp_device_client
{

    public class IoTCClient : IIoTCClient
    {

        const int DEFAULT_EXPIRATION = 21600;
        const string DPS_DEFAULT_ENDPOINT = "global.azure-devices-provisioning.net";
        const string DPS_DEFAULT_API = "2018-09-01-preview";
        private DeviceClient deviceClient;
        private IMqttClient mqttClient;

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
            mqttClient = new MqttFactory().CreateMqttClient();
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

        /// <summary>
        /// Register a device in IoTCentral using authentication provided at 
        /// </summary>
        ///  /// <exception cref="IoTCentralException">Thrown when Registration failed</exception>
        /// <returns>DeviceClient instance</returns>
        private async Task<MqttCredentials> Register()
        {
            if (AuthenticationType == IoTCConnect.SYMM_KEY)
            {
                return await new SasAuthentication(this).RegisterWithSaSKey(SasKey);
            }
            //else if (AuthenticationType == IoTCConnect.DEVICE_KEY)
            else
            {
                return await new SasAuthentication(this).RegisterWithDeviceKey(SasKey);
            }
            //return await new CertAuthentication(this).Register(Certificate.GetCertificate());
            // this.logger).Register();
        }

        /// <summary>
        /// Connect device to IoTCentral application
        /// </summary>
        /// <exception cref="IoTCentralException">Thrown when connection failed</exception>
        /// <returns>task</returns>
        public async Task Connect()
        {
            Protocol = TransportType.Http1;
            var creds = await Register();
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Id)
                .WithTcpServer(creds.HostName)
                .WithCredentials(creds.UserName, creds.Password)
                .WithTls()
                .WithCleanSession()
                .Build();
            mqttClient.UseConnectedHandler(async e =>
            {
                //connected
                if (callbacks.ContainsKey(IoTCEvents.ConnectionStatus))
                {
                    callbacks[IoTCEvents.ConnectionStatus].DynamicInvoke(e);
                }

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"devices/{Id}/messages/devicebound/#").Build());
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"devices/{Id}/messages/events/#").Build());
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"$iothub/twin/res/#").Build());
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"$iothub/twin/PATCH/properties/desired/#").Build());
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"$iothub/methods/POST/#").Build());

                // get twin
                await mqttClient.PublishAsync("$iothub/twin/GET/?$rid=0");
                mqttClient.UseApplicationMessageReceivedHandler(async (m) =>
                  {
                      if (m.ApplicationMessage.Topic.StartsWith(SettingsTopic))
                      {
                          var fields = new Regex(@"\$iothub/twin/PATCH/properties/desired/\?\$version=([\d]+)$").Match(m.ApplicationMessage.Topic).Groups;
                          if (fields.Count > 1)
                          {
                              if (callbacks.ContainsKey(IoTCEvents.SettingsUpdated))
                              {
                                  JObject obj = JObject.Parse(Encoding.UTF8.GetString(m.ApplicationMessage.Payload));
                                  foreach (var property in obj.Properties())
                                  {
                                      if (property.Name == "$version")
                                          continue;
                                      callbacks[IoTCEvents.SettingsUpdated].DynamicInvoke(new Setting(property.Name, property.Value.Value<string>("value"), int.Parse(fields[1].Value)));
                                  }

                              }
                          }
                      }
                      else if (m.ApplicationMessage.Topic.StartsWith(CommandsTopic))
                      {
                          var fields = new Regex(@"\$iothub/methods/POST/([\S]+)/\?\$rid=([\d]+)$").Match(m.ApplicationMessage.Topic).Groups;
                          if (fields.Count > 2)
                          {
                              await mqttClient.PublishAsync($"$iothub/methods/res/0/?$rid={fields[2].Value}");

                              if (callbacks.ContainsKey(IoTCEvents.Command))
                              {
                                  callbacks[IoTCEvents.Command].DynamicInvoke(new Command(fields[1].Value, Encoding.UTF8.GetString(m.ApplicationMessage.Payload), fields[2].Value));
                              }
                          }
                      }
                  });
            });
            await mqttClient.ConnectAsync(options);


            this.Logger.Log($"Device connected to hub {creds.HostName}");
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
            string body;
            if (payload.GetType() == typeof(string))
            {
                body = (string)payload;
            }
            else if (payload.GetType() == typeof(JObject))
            {
                body = ((JObject)payload).ToString(Formatting.None);
            }
            else
            {
                body = JsonConvert.SerializeObject(payload);
            }
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{Id}/messages/events/")
                .WithPayload(body)
                .WithAtMostOnceQoS()
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(message);

            Logger.Log($"Message: {body}");
            callback?.Invoke("Event sent");

        }


        public async Task SendProperty(object payload, Action<object> callback)
        {
            string body;
            if (payload.GetType() == typeof(string))
            {
                body = (string)payload;
            }
            else if (payload.GetType() == typeof(JObject))
            {
                body = ((JObject)payload).ToString(Formatting.None);
            }
            else
            {
                body = JsonConvert.SerializeObject(payload);
            }
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"$iothub/twin/PATCH/properties/reported/?$rid={new Random().Next(0, 10)}")
                .WithPayload(body)
                .WithAtMostOnceQoS()
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(message);

            Logger.Log($"Message: {body}");
            callback?.Invoke("Event sent");
        }



        public void on(IoTCEvents iotcEvent, Func<object, string> callback)
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

        public void on(IoTCEvents iotcEvent, Func<object, Task<string>> callback)
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

        private string SettingsTopic { get { return "$iothub/twin/PATCH/properties/desired/"; } }
        private string CommandsTopic { get { return $"$iothub/methods/POST/"; } }

    }
}