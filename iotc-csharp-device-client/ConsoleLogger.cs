using System;
using System.Collections.Generic;
using System.Text;
using iotc_csharp_device_client.enums;

namespace iotc_csharp_device_client
{
    public class ConsoleLogger : ILogger
    {
        private IoTCLogging logLevel = IoTCLogging.DISABLED;
        public void Log(string message)
        {
            if (this.logLevel != IoTCLogging.DISABLED)
                Console.WriteLine(message);
        }

        public void SetLevel(IoTCLogging level)
        {
            this.logLevel = level;
        }
    }
}
