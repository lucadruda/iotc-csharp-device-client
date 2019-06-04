using iotc_csharp_device_client;
using iotc_csharp_device_client.Models;
using System;
using System.Threading.Tasks;

namespace SettingSymKey
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                IoTCClient client = new IoTCClient("testcsharp", "0ne00052362", iotc_csharp_device_client.enums.IoTCConnect.SYMM_KEY, "68p6zEjwVNB6L/Dz8Wkz4VhaTrYqkndPrB0uJbWr2Hc/AmB+Qxz/eJJ9MIhLZFJ6hC0RmHMgfaYBkNTq84OCNQ==");
                client.SetLogging(iotc_csharp_device_client.enums.IoTCLogging.FULL);
                client.on(iotc_csharp_device_client.enums.IoTCEvents.SettingsUpdated, async (obj) =>
                {
                    if (obj.GetType() == typeof(Setting))
                    {
                        Setting setting = (Setting)obj;
                        Console.WriteLine($"Setting: {setting.Name}, Value:{setting.Value}");
                        await client.SendProperty(setting.GetResponseobject("Synced"), null);
                    }
                    return null;
                });
                await client.Connect();
                while (true) ;
            }).Wait();

        }

    }
}
