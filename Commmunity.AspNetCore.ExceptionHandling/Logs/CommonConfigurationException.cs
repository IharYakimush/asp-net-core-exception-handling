using System;

namespace Commmunity.AspNetCore.ExceptionHandling.Logs
{
    public class CommonConfigurationException : Exception
    {
        public CommonConfigurationException()
        {
            throw new NotSupportedException("This exception not intended to use");
        }
    }
}