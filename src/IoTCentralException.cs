using iotc_csharp_device_client.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iotc_csharp_device_client
{
    public class IoTCentralException : Exception
    {
        private IoTCConnectionState ConnectionState { get; set; }

        public IoTCentralException(String message) : base(message)
        {
        }

        public IoTCentralException(IoTCConnectionState connectionState) : base()
        {
            ConnectionState = connectionState;
        }

    }
}
