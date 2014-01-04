using System;
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
    [Cmdlet(VerbsCommon.Find, "TrafficManagerProfile")]
    public class AzureTMCheckDnsAvailability : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string SubscriptionId;

        [Parameter(Position = 1, Mandatory = true)]
        public string CertificateThumbprint;

        [Parameter(Position = 2, Mandatory = true)]
        public string ProfileDomain;

        protected override void ProcessRecord()
        {
            try
            {
                // Validations
                SubscriptionId.Validate(ParameterType.SubscriptonId);
                CertificateThumbprint.Validate(ParameterType.CertificateThumbprint);
                ProfileDomain.Validate(ParameterType.ProfileDomain);

                // Get Management certificate
                X509Certificate2 certificate = Helper.GetCertificate(CertificateThumbprint);

                // URI variable.
                Uri requestUri = Helper.GetUri(OperationType.CheckDns, new string[] { SubscriptionId, ProfileDomain });

                // Make Http Request
                HttpResult result = Helper.CustomWebRequest(requestUri, certificate, "GET", string.Empty);

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
