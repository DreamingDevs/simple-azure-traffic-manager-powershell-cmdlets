using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
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
        public string PrimaryService;

        [Parameter(Position = 2, Mandatory = true)]
        public string SecondaryService;

        [Parameter(Position = 3, Mandatory = true)]
        public string ProfileName;

        [Parameter(Position = 4, Mandatory = true)]
        public string CertificateThumbprint;


        protected override void ProcessRecord()
        {
            // X.509 certificate variables.
            X509Store certStore = null;
            X509Certificate2Collection certCollection = null;
            X509Certificate2 certificate = null;

            // Request and response variables.
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            // Stream variables.
            Stream responseStream = null;
            StreamReader reader = null;

            // URI variable.
            Uri requestUri = null;

            // The thumbprint for the certificate. This certificate would have been
            // previously added as a management certificate within the Windows Azure management portal.
            string thumbPrint = CertificateThumbprint;

            // Open the certificate store for the current user.
            certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);

            // Find the certificate with the specified thumbprint.
            certCollection = certStore.Certificates.Find(
                                 X509FindType.FindByThumbprint,
                                 thumbPrint,
                                 false);

            // Close the certificate store.
            certStore.Close();

            // Check to see if a matching certificate was found.
            if (0 == certCollection.Count)
            {
                throw new Exception("No certificate found containing thumbprint " + thumbPrint);
            }

            // A matching certificate was found.
            certificate = certCollection[0];


            // Create the request.
            requestUri = new Uri("https://management.core.windows.net/"
                                 + SubscriptionId
                                 + "/services/WATM/profiles/" + ProfileName + "/definitions");

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);

            // Add the certificate to the request.
            httpWebRequest.ClientCertificates.Add(certificate);
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("x-ms-version", "2011-10-01");


            string str = @"<Definition xmlns=""http://schemas.microsoft.com/windowsazure""><DnsOptions><TimeToLiveInSeconds>300</TimeToLiveInSeconds></DnsOptions><Monitors><Monitor><IntervalInSeconds>30</IntervalInSeconds><TimeoutInSeconds>10</TimeoutInSeconds><ToleratedNumberOfFailures>3</ToleratedNumberOfFailures><Protocol>HTTP</Protocol><Port>80</Port><HttpOptions><Verb>GET</Verb><RelativePath>/</RelativePath><ExpectedStatusCode>200</ExpectedStatusCode></HttpOptions></Monitor></Monitors><Policy><LoadBalancingMethod>RoundRobin</LoadBalancingMethod><Endpoints><Endpoint><DomainName>" + PrimaryService + "</DomainName><Status>Enabled</Status></Endpoint><Endpoint><DomainName>" + SecondaryService + "</DomainName><Status>Enabled</Status></Endpoint></Endpoints></Policy></Definition>";
            byte[] bodyStart = System.Text.Encoding.UTF8.GetBytes(str.ToString());
            Stream dataStream = httpWebRequest.GetRequestStream();
            dataStream.Write(bodyStart, 0, str.ToString().Length);

            // Make the call using the web request.
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            // Parse the web response.
            responseStream = httpWebResponse.GetResponseStream();
            reader = new StreamReader(responseStream);

            // Close the resources no longer needed.
            httpWebResponse.Close();
            responseStream.Close();
            reader.Close();
        }
    }
}
