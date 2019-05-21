using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iotc_csharp_device_client.enums
{
    public enum IoTCProtocol
    {
        [Description("Mqtt")]
        MQTT,
        [Description("Amqp")]
        AMQP,
        [Description("Http1")]
        HTTP
    }
}
