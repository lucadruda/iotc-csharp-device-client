using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iotc_csharp_device_client.enums
{
    public enum IoTCConnectionState
    {
        [Description("Connection_Ok")]
        CONNECTION_OK,
        [Description("Expired_SAS_Token")]
        EXPIRED_SAS_TOKEN,
        [Description("Device_Disabled")]
        DEVICE_DISABLED,
        [Description("Bad_Credential")]
        BAD_CREDENTIAL,
        [Description("Retry_Expired")]
        RETRY_EXPIRED,
        [Description("No_Network")]
        NO_NETWORK,
        [Description("Communication_Error")]
        COMMUNICATION_ERROR,
        [Description("Client_Close")]
        CLIENT_CLOSE
    }
}
