﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace AzureTrafficManager
{
    public static class Helper
    {
        public static X509Certificate2 GetCertificate(string certificateThumbprint)
        {
            // X.509 certificate variables.
            X509Store certStore = null;
            X509Certificate2Collection certCollection = null;

            // The thumbprint for the certificate. This certificate would have been
            // previously added as a management certificate within the Windows Azure management portal.
            string thumbPrint = certificateThumbprint;

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
                throw new CryptographicException("No certificate found containing thumbprint " + thumbPrint);
            }

            // A matching certificate was found, return it.
            return certCollection[0];
        }

        public static Uri GetUri(OperationType operationType, params string[] parameters)
        {
            string uriString;
            string managementUri = "https://management.core.windows.net";

            switch (operationType)
            {
                case OperationType.RemoveTM:
                    uriString = String.Format("{0}/{1}/services/WATM/profiles/{2}", managementUri, parameters[0], parameters[1]);
                    break;
                case OperationType.GetTM:
                    uriString = String.Format("{0}/{1}/services/WATM/profiles/{2}", managementUri, parameters[0], parameters[1]);
                    break;
                case OperationType.CreateTMProfile:
                    uriString = String.Format("{0}/{1}/services/WATM/profiles", managementUri, parameters[0]);
                    break;
                case OperationType.CreateTMDefinition:
                    uriString = String.Format("{0}/{1}/services/WATM/profiles/{2}/definitions", managementUri, parameters[0], parameters[1]);
                    break;
                case OperationType.CheckDns:
                    uriString = String.Format("{0}/{1}/services/WATM/operations/isavailable/{2}", managementUri, parameters[0], parameters[1]);
                    break;
                default:
                    uriString = String.Format("{0}/{1}/services/WATM/profiles/", managementUri, parameters[0]);
                    break;
            }

            return new Uri(uriString);
        }

        public static HttpResult CustomWebRequest(Uri requestUri, X509Certificate2 certificate, string method, string payload)
        {
            // Request and response variables.
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            // Stream variables.
            Stream responseStream = null;
            StreamReader reader = null;

            httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(requestUri);

            // Add the certificate to the request.
            httpWebRequest.ClientCertificates.Add(certificate);
            httpWebRequest.Method = method;
            httpWebRequest.Headers.Add("x-ms-version", "2011-10-01");

            if (!string.IsNullOrWhiteSpace(payload))
            {
                byte[] bodyStart = System.Text.Encoding.UTF8.GetBytes(payload.ToString());
                Stream dataStream = httpWebRequest.GetRequestStream();
                dataStream.Write(bodyStart, 0, payload.ToString().Length);
            }

            // Make the call using the web request.
            httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            // Parse the web response.
            responseStream = httpWebResponse.GetResponseStream();
            reader = new StreamReader(responseStream);

            // Close the resources no longer needed.
            httpWebResponse.Close();
            responseStream.Close();
            reader.Close();

            return new HttpResult() { StatusCode = httpWebResponse.StatusCode, Response = reader.ReadToEnd() };
        }

        public static string PrintResponse(this HttpResult result)
        {
            return string.Format("Status: {0} `n Response : {1}", result.StatusCode, result.Response);
        }

        public static bool Validate(this string parameter, ParameterType paramterType)
        {
            // Create a facade of validations here and use type to invoke corresponding validations
            // For now we have basic set of validations
            if (string.IsNullOrWhiteSpace(parameter))
                throw new ArgumentNullException(parameter + " cannot be empty");

            return true;
        }
    }
}
