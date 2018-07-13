using System;

namespace Commmunity.AspNetCore.ExceptionHandling.Builder
{
    public interface IExceptionMapping<TException> : IExceptionPolicyBuilder
    where TException : Exception
    {        
    }

    public interface IResponseHandlers<TException> : IExceptionMapping<TException>
        where TException : Exception
    {
    }
}