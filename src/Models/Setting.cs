// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace iotc_csharp_device_client.Models
{

    public class Setting
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Version { get; set; }

        public Setting(string name, string value, int version)
        {
            this.Name = name;
            this.Value = value;
            this.Version = version;
        }


        /**
         * @param message The message to send back as a response for the command
         * @return Well-formed object to be sent as a property. The message will appear
         *         in the command tile in the application
         */
        public string GetResponseobject(string message)
        {
            return $"{{\"{Name}\":{{\"value\":\"{Value}\",\"message\":\"{message}\",\"status\":\"completed\",\"desiredVersion\":{Version}}}}}";
        }


    }
}