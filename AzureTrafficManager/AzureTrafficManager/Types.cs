using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureTrafficManager
{
    public class HttpResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Response { get; set; }
    }

    public enum OperationType
    {
        RemoveTM, GetTM, CreateTMProfile, CreateTMDefinition, CheckDns
    }

    public enum ParameterType
    {
        SubscriptonId, CertificateThumbprint, ProfileName, ProfileDomain, ServiceName
    }

}
