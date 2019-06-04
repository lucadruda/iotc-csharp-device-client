using iotc_csharp_device_client;
using iotc_csharp_device_client.Models;
using System;
using System.Threading.Tasks;

namespace CommandSymKey
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                IoTCClient client = new IoTCClient("testcsharp", "0ne00052362", iotc_csharp_device_client.enums.IoTCConnect.SYMM_KEY, "68p6zEjwVNB6L/Dz8Wkz4VhaTrYqkndPrB0uJbWr2Hc/AmB+Qxz/eJJ9MIhLZFJ6hC0RmHMgfaYBkNTq84OCNQ==");
                client.SetLogging(iotc_csharp_device_client.enums.IoTCLogging.FULL);
                client.on(iotc_csharp_device_client.enums.IoTCEvents.Command, async (obj) =>
                {
                    if (obj.GetType() == typeof(Command))
                    {
                        Command command = (Command)obj;
                        Console.WriteLine($"Command: {command.Name}, Value:{command.Payload}");
                        await client.SendProperty(command.GetResponseObject("Executed"), null);
                    }
                    return null;
                });
                await client.Connect();
                while (true) ;
            }).Wait();

        }

    }
}
