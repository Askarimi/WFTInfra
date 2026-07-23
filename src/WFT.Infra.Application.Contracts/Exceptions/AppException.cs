using System;

namespace WFT.Infra.Application.Contracts.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
