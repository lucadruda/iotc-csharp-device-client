using System;
using System.Collections.Generic;
using System.Text;

namespace iotc_csharp_device_client.Authentication
{
    public class MqttCredentials
    {
        public MqttCredentials(string userName, string password, string hostName)
        {
            UserName = userName;
            Password = password;
            HostName = hostName;
        }

        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
