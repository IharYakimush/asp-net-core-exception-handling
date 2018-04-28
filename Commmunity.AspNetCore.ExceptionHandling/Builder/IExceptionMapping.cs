using System;

namespace Commmunity.AspNetCore.ExceptionHandling.Builder
{
    public interface IExceptionMapping<TException> : IExceptionPolicyBuilder
    where TException : Exception
    {        
    }

    public interface IResponseHandlers<TException> : IExceptionPolicyBuilder
        where TException : Exception
    {
    }
}