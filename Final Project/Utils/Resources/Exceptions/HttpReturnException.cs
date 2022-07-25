﻿using System.Net;

namespace Final_Project.Utils.Resources.Exceptions
{
    public class HttpReturnException : Exception
    {
        public HttpStatusCode Status { get; private set; }

        public HttpReturnException(HttpStatusCode status, string msg) : base(msg)
        {
            Status = status;
        }
    }
}