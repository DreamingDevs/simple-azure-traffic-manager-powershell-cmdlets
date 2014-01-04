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
    [Cmdlet(VerbsCommon.Get, "TrafficManagerProfile")]
    public class AzureTMGetProfile : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string SubscriptionId;

        [Parameter(Position = 1, Mandatory = true)]
        public string ProfileName;

        [Parameter(Position = 3, Mandatory = true)]
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

            try
            {
                // Create the request.
                requestUri = new Uri("https://management.core.windows.net/"
                                     + SubscriptionId
                                     + "/services/WATM/profiles/" + ProfileName);

                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);

                // Add the certificate to the request.
                httpWebRequest.ClientCertificates.Add(certificate);
                httpWebRequest.Method = "GET";
                httpWebRequest.Headers.Add("x-ms-version", "2011-10-01");

                // Make the call using the web request.
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                // Parse the web response.
                responseStream = httpWebResponse.GetResponseStream();
                reader = new StreamReader(responseStream);

                string result = reader.ReadToEnd();
                WriteObject(httpWebResponse.StatusCode);

                // Close the resources no longer needed.
                httpWebResponse.Close();
                responseStream.Close();
                reader.Close();
            }
            catch (WebException wex)
            {
                WriteObject(((HttpWebResponse)wex.Response).StatusCode);
            }
            catch(Exception e)
            {
                WriteObject("Got Error");
            }
        }
    }
}
