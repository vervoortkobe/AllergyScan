using System.Net;

namespace AllergyScan.Services
{
    /// <summary>An error returned by the AllergyScan WebAPI or by an Azure service.</summary>
    public class ApiException : Exception
    {
        public ApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
