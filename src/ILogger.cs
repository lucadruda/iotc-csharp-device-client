using iotc_csharp_device_client.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iotc_csharp_device_client
{
    public interface ILogger
    {
        void Log(string message);

        void SetLevel(IoTCLogging level);
    }
}
