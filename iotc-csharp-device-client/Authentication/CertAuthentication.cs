using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace iotc_csharp_device_client.Authentication
{
    public class CertAuthentication
    {
        public CertAuthentication(IoTCClient client) => Client = client;

        public IoTCClient Client { get; private set; }

        public async Task<DeviceClient> Register(X509Certificate2 certificate)
        {
            if (certificate == null || string.IsNullOrEmpty(Client.Id))
            {
                throw new IoTCentralException("Wrong credentials values");
            }
            //long time = (System.currentTimeMillis() / 1000 | 0) + DEFAULT_EXPIRATION;
            using (var security = new SecurityProviderX509Certificate(certificate))
            {
                using (var transport = Utils.GetProvisioningTransportProtocol(Client.Protocol))
                {
                    ProvisioningDeviceClient provClient = ProvisioningDeviceClient.Create(Client.Endpoint, Client.ScopeId, security, transport);
                    DeviceRegistrationResult result = await provClient.RegisterAsync();
                    if (result.Status != ProvisioningRegistrationStatusType.Assigned)
                    {
                        throw new IoTCentralException($"Provisioning failed: {result.Status.ToString()} - {result.ErrorMessage}");
                    }
                    return DeviceClient.Create(result.AssignedHub, new DeviceAuthenticationWithX509Certificate(Client.Id, certificate));
                }
            }
        }
    }
}
