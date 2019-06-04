// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace iotc_csharp_device_client.Models
{

    public class Command
    {
        public Command(string name, string payload, string requestId)
        {
            this.Name = name;
            this.Payload = payload;
            this.RequestId = requestId;
        }

        public string Name { get; set; }
        public string Payload { get; set; }
        public string RequestId { get; set; }

        /**
         * @param message The message to send back as a response for the command
         * @return Well-formed object to be sent as a property. The message will appear in the command tile in the application
         */
        public string GetResponseObject(string message)
        {
            return $"{{\"{Name}\":{{\"value\":\"{message}\"}}}}";
        }

    }
}