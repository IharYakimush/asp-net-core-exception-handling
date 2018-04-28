using System;
using Commmunity.AspNetCore.ExceptionHandling.Builder;
using Commmunity.AspNetCore.ExceptionHandling.Exc;
using Commmunity.AspNetCore.ExceptionHandling.Logs;
using Commmunity.AspNetCore.ExceptionHandling.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public static class PolicyBuilderExtensions
    {
        public static ExceptionMapping<TException> For<TException>(
            this IExceptionPolicyBuilder builder, int index = -1) where TException : Exception
        {
            builder.Options.EnsureException(typeof(TException), index);
            return new ExceptionMapping<TException>(builder);
        }

        public static ExceptionMapping<TException> EnsureHandler<TException,THandler>(
            this ExceptionMapping<TException> builder, int index = -1) 
            where THandler : class , IExceptionHandler
            where TException : Exception

        {
            builder.Options.Value.EnsureHandler(typeof(TException), typeof(THandler), index);
            builder.Services.TryAddSingleton<THandler>();
            return builder;
        }

        public static ExceptionMapping<TException> RemoveHandler<TException, THandler>(
            this ExceptionMapping<TException> builder)
            where THandler : IExceptionHandler
            where TException : Exception
        {
            builder.Options.Value.RemoveHandler(typeof(TException), typeof(THandler));
            return builder;
        }

        public static ExceptionMapping<TException> Clear<TException>(
            this ExceptionMapping<TException> builder)
            where TException : Exception
        {
            builder.Options.Value.ClearHandlers(typeof(TException));
            return builder;
        }

        // rethrow
        public static IExceptionPolicyBuilder AddRethrow<TException>(
            this ExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {            
            return builder.EnsureHandler<TException,ReThrowExceptionHandler>(index);
        }

        // Log
        public static ExceptionMapping<TException> AddLog<TException>(
            this ExceptionMapping<TException> builder, Action<LogHandlerOptions<TException>> settings = null, int index = -1)
            where TException : Exception
        {
            LogHandlerOptions<TException> options = new LogHandlerOptions<TException>();
            settings?.Invoke(options);
            builder.Services.TryAddSingleton(options);
            return builder.EnsureHandler<TException, LogExceptionHandler<TException>>(index);
        }

        // Set status code
        public static ExceptionMapping<TException> AddStatusCode<TException>(
            this ExceptionMapping<TException> builder, Func<TException,int> settings = null, int index = -1)
            where TException : Exception
        {
            if (settings != null)
            {
                builder.Services.Configure<SetStatusCodeOptions<TException>>(codeOptions =>
                    codeOptions.StatusCodeFactory = settings);
            }
            
            return builder.EnsureHandler<TException, SetStatusCodeHandler<TException>>(index);
        }

        public static ExceptionMapping<TException> AddHeaders<TException>(
            this ExceptionMapping<TException> builder, Action<IHeaderDictionary,TException> settings = null, int index = -1)
            where TException : Exception
        {
            if (settings != null)
            {
                builder.Services.Configure<SetHeadersOptions<TException>>(codeOptions =>
                    codeOptions.SetHeadersAction = settings);
            }

            return builder.EnsureHandler<TException, SetHeadersHandler<TException>>(index);
        }
    }
}