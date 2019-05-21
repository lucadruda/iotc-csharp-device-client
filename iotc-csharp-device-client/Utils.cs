using iotc_csharp_device_client.enums;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace iotc_csharp_device_client
{
    public static class Utils
    {
        public static string GetEnumDesc<T>(T enumerationValue)
        {
            Type type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
            }

            //Tries to find a DescriptionAttribute for a potential friendly name
            //for the enum
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    //Pull out the description value
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            //If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        public static ProvisioningTransportHandler GetProvisioningTransportProtocol(TransportType protocol)
        {
            switch (protocol)
            {
                case TransportType.Amqp:
                    return new ProvisioningTransportHandlerAmqp();
                case TransportType.Http1:
                    return new ProvisioningTransportHandlerHttp();
                default:
                    return new ProvisioningTransportHandlerMqtt();

            }
        }
    }
}
