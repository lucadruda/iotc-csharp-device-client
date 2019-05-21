// Copyright (c) Luca Druda. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace iotc_csharp_device_client
{
    public class IoTCentralCert
    {
        public string CertificateFile { get; set; }
        public string CertificatePassword { get; set; }
        public IoTCentralCert(string certFilePath, string certPassword)
        {
            CertificateFile = certFilePath;
            CertificatePassword = certPassword;
        }


        public X509Certificate2 GetCertificate()
        {

            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(CertificateFile, CertificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2 certificate = null;

            foreach (X509Certificate2 element in certificateCollection)
            {

                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                throw new FileNotFoundException($"{CertificateFile} did not contain any certificate with a private key.");
            }
            return certificate;

        }
    }
}