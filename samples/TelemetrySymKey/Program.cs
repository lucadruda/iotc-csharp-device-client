using iotc_csharp_device_client;
using System;
using System.Threading.Tasks;

namespace TelemetrySymKey
{
    class Program
    {
        static void Main(string[] args)
        {
            IoTCClient client = new IoTCClient("testcsharp", "0ne00052362", iotc_csharp_device_client.enums.IoTCConnect.SYMM_KEY, "68p6zEjwVNB6L/Dz8Wkz4VhaTrYqkndPrB0uJbWr2Hc/AmB+Qxz/eJJ9MIhLZFJ6hC0RmHMgfaYBkNTq84OCNQ==");
            client.SetLogging(iotc_csharp_device_client.enums.IoTCLogging.FULL);
            client.Connect().Wait();
            Random random = new Random();
            int temperature, pressure;
            for (int i = 0; i < 30; i++)
            {
                temperature = random.Next(0, 100);
                pressure = random.Next(300, 1000);
                client.SendTelemetry($"{{\"temp\":{temperature},\"pressure\":{pressure}}}", null).Wait();
            }


        }
    }
}
