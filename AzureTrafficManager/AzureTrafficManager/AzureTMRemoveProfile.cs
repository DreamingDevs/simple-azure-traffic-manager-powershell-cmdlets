﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AzureTrafficManager
{
    [Cmdlet(VerbsCommon.Remove, "TrafficManagerProfile")]
    public class AzureTMRemoveProfile : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string SubscriptionId;

        [Parameter(Position = 1, Mandatory = true)]
        public string CertificateThumbprint;

        [Parameter(Position = 2, Mandatory = true)]
        public string ProfileName;

        protected override void ProcessRecord()
        {
            try
            {
                // Get Management certificate
                X509Certificate2 certificate = Helper.GetCertificate(CertificateThumbprint);

                // URI variable.
                Uri requestUri = Helper.GetUri(OperationType.RemoveTM, new string[] { SubscriptionId, ProfileName });

                // Make Http Request
                HttpResult result = Helper.CustomWebRequest("DELETE", requestUri, certificate);

                // Print response
                WriteObject(result.PrintResponse());
            }
            catch (CryptographicException crypex)
            {
                WriteObject(crypex.Message);
            }
            catch (WebException webex)
            {
                HttpWebResponse response = (HttpWebResponse)webex.Response;
                WriteObject(response.StatusDescription);
            }
            catch (Exception ex)
            {
                WriteObject(ex.Message);
            }
        }
    }
}
