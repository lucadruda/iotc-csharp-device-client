﻿// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace iotc_csharp_device_client.Authentication
{
    public class SasAuthentication
    {

        const int DEFAULT_EXPIRATION = 21600; // 6 hours

        private IoTCClient Client { get; set; }

        public SasAuthentication(IoTCClient client) => Client = client;

        public async Task<DeviceClient> RegisterWithSaSKey(string symKey)
        {
            if (string.IsNullOrEmpty(Client.ScopeId) || string.IsNullOrEmpty(symKey) || string.IsNullOrEmpty(Client.Id))
            {
                throw new IoTCentralException("Wrong credentials values");
            }
            return await this.RegisterWithDeviceKey(this.ComputeKey(Convert.FromBase64String(symKey), Client.Id));
        }

        public async Task<DeviceClient> RegisterWithDeviceKey(string deviceKey)
        {
            if (string.IsNullOrEmpty(Client.ScopeId) || string.IsNullOrEmpty(deviceKey) || string.IsNullOrEmpty(Client.Id))
            {
                throw new IoTCentralException("Wrong credentials values");
            }
            //long time = (System.currentTimeMillis() / 1000 | 0) + DEFAULT_EXPIRATION;
            using (var security = new SecurityProviderSymmetricKey(Client.Id, deviceKey, null))
            {
                using (var transport = Utils.GetProvisioningTransportProtocol(Client.Protocol))
                {
                    ProvisioningDeviceClient provClient = ProvisioningDeviceClient.Create(Client.Endpoint, Client.ScopeId, security, transport);
                    DeviceRegistrationResult result = await provClient.RegisterAsync();
                    if (result.Status != ProvisioningRegistrationStatusType.Assigned)
                    {
                        throw new IoTCentralException($"Provisioning failed: {result.Status.ToString()} - {result.ErrorMessage}");
                    }
                    return DeviceClient.Create(result.AssignedHub, new DeviceAuthenticationWithRegistrySymmetricKey(Client.Id, deviceKey));
                }
            }
        }

        private string ComputeKey(byte[] masterKey, string registrationId)
        {
            using (var hmac = new HMACSHA256(masterKey))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(registrationId)));
            }
        }
    }
}