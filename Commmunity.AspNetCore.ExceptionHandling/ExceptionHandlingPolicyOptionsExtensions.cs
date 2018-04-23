using System;
using Microsoft.Extensions.Options;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public static class ExceptionHandlingPolicyOptionsExtensions
    {
        public static ExceptionMapping EnsureException<TException>(
            this ExceptionHandlingPolicyOptions options, int index = -1) where TException : Exception
        {            
            options.EnsureException(typeof(TException), index);
            return new ExceptionMapping(options, typeof(TException));
        }

        public static ExceptionMapping EnsureHandler<THandler>(
            this ExceptionMapping options, int index = -1)
            where THandler : IExceptionHandler
        {
            options.Value.EnsureHandler(options.Type, typeof(THandler), index);
            return options;
        }

        public static ExceptionMapping RemoveHandler<THandler>(
            this ExceptionMapping options)
            where THandler : IExceptionHandler
        {
            options.Value.RemoveHandler(options.Type, typeof(THandler));
            return options;
        }

        public static ExceptionMapping Clear(
            this ExceptionMapping options)
        {
            options.Value.ClearHandlers(options.Type);
            return options;
        }
    }

    public class ExceptionMapping : IOptions<ExceptionHandlingPolicyOptions>        
    {
        public Type Type { get; }

        public ExceptionHandlingPolicyOptions Value { get; }

        internal ExceptionMapping(ExceptionHandlingPolicyOptions options, Type type)
        {
            this.Type = type;
            this.Value = options;
        }
    }
}