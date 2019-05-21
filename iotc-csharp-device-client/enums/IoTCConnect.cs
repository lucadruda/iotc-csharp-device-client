using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iotc_csharp_device_client.enums
{
    public enum IoTCConnect
    {
        [Description("SYMM_KEY")]
        SYMM_KEY,
        [Description("DEVICE_KEY")]
        DEVICE_KEY,
        [Description("X509_CERT")]
        X509_CERT
    }
}
