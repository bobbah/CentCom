using System;
using System.Collections.Generic;
using System.Text;

namespace CentCom.Server.Exceptions
{
    class BanSourceUnavailableException : Exception
    {
        public BanSourceUnavailableException()
        {

        }

        public BanSourceUnavailableException(string message) : base(message)
        {

        }

        public BanSourceUnavailableException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
