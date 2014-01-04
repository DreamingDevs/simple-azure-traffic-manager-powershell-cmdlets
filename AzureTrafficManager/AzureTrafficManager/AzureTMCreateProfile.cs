using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace AzureTrafficManager
{
    [Cmdlet(VerbsCommon.New,"TrafficManagerProfile")]
    public class AzureTMCreateProfile : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory=true)]
        public string SubscriptionId;

        [Parameter(Position = 1, Mandatory = true)]
        public string CertificateThumbprint;

        [Parameter(Position = 2, Mandatory = true)]
        public string ProfileName;

        [Parameter(Position = 3, Mandatory = true)]
        public string ProfileDomain;

        protected override void ProcessRecord()
        {
            try
            {
                // Validations
                SubscriptionId.Validate(ParameterType.SubscriptonId);
                CertificateThumbprint.Validate(ParameterType.CertificateThumbprint);
                ProfileName.Validate(ParameterType.ProfileName);
                ProfileDomain.Validate(ParameterType.ProfileDomain);

                // Get Management certificate
                X509Certificate2 certificate = Helper.GetCertificate(CertificateThumbprint);

                // URI variable.
                Uri requestUri = Helper.GetUri(OperationType.CreateTMProfile, new string[] { SubscriptionId }); 
                
                // Make Http Request
                string payload = string.Format("<Profile xmlns=\"http://schemas.microsoft.com/windowsazure\"><DomainName>{0}</DomainName>" +
                                               "<Name>{1}</Name></Profile>", ProfileDomain, ProfileName);
                HttpResult result = Helper.CustomWebRequest(requestUri, certificate, "POST", payload);

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
