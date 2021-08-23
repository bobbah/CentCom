using System;

namespace CentCom.Server.Exceptions
{
    class BanSourceUnavailableException : Exception
    {
        public string ResponseContent { get; set; }

        public BanSourceUnavailableException(string message, string responseContent) : base(message)
        {
            ResponseContent = responseContent;
        }

        public BanSourceUnavailableException(string message, string responseContent, Exception inner) : base(message, inner)
        {
            ResponseContent = responseContent;
        }
    }
}
