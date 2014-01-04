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
    [Cmdlet(VerbsCommon.New, "TrafficManagerDefinition")]
    public class AzureTMCreateDefinition : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string SubscriptionId;

        [Parameter(Position = 1, Mandatory = true)]
        public string CertificateThumbprint;

        [Parameter(Position = 2, Mandatory = true)]
        public string ProfileName;

        [Parameter(Position = 3, Mandatory = true)]
        public string PrimaryService;

        [Parameter(Position = 4, Mandatory = true)]
        public string SecondaryService;


        protected override void ProcessRecord()
        {
            try
            {
                // Validations
                SubscriptionId.Validate(ParameterType.SubscriptonId);
                CertificateThumbprint.Validate(ParameterType.CertificateThumbprint);
                ProfileName.Validate(ParameterType.ProfileName);
                PrimaryService.Validate(ParameterType.ServiceName);
                SecondaryService.Validate(ParameterType.ServiceName);

                // Get Management certificate
                X509Certificate2 certificate = Helper.GetCertificate(CertificateThumbprint);

                // URI variable.
                Uri requestUri = Helper.GetUri(OperationType.CreateTMDefinition, new string[] { SubscriptionId, ProfileName });

                // Make Http Request
                // TODO: Use XmlSerializer to form this xml
                string payload = string.Format("<Definition xmlns=\"http://schemas.microsoft.com/windowsazure\">" +
                                               "<DnsOptions><TimeToLiveInSeconds>300</TimeToLiveInSeconds></DnsOptions>" + 
                                               "<Monitors><Monitor><IntervalInSeconds>30</IntervalInSeconds>" +
                                               "<TimeoutInSeconds>10</TimeoutInSeconds>" + 
                                               "<ToleratedNumberOfFailures>3</ToleratedNumberOfFailures>" +
                                               "<Protocol>HTTP</Protocol><Port>80</Port>" +
                                               "<HttpOptions><Verb>GET</Verb><RelativePath>/</RelativePath>" +
                                               "<ExpectedStatusCode>200</ExpectedStatusCode></HttpOptions></Monitor></Monitors>" +
                                               "<Policy><LoadBalancingMethod>RoundRobin</LoadBalancingMethod><Endpoints><Endpoint>" +
                                               "<DomainName>{0}</DomainName><Status>Enabled</Status></Endpoint><Endpoint>" +
                                               "<DomainName>{1}</DomainName><Status>Enabled</Status></Endpoint></Endpoints></Policy>" +
                                               "</Definition>", PrimaryService, SecondaryService);
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
