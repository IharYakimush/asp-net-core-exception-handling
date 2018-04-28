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
        public static IExceptionMapping<TException> For<TException>(
            this IExceptionPolicyBuilder builder, int index = -1) where TException : Exception
        {
            builder.Options.EnsureException(typeof(TException), index);
            return new ExceptionMapping<TException>(builder);
        }

        public static void EnsureHandler<TException,THandler>(
            this IExceptionPolicyBuilder builder, int index = -1) 
            where THandler : class , IExceptionHandler
            where TException : Exception

        {
            builder.Options.Value.EnsureHandler(typeof(TException), typeof(THandler), index);
            builder.Services.TryAddSingleton<THandler>();
        }

        public static IExceptionMapping<TException> RemoveHandler<TException, THandler>(
            this IExceptionMapping<TException> builder)
            where THandler : IExceptionHandler
            where TException : Exception
        {
            builder.Options.Value.RemoveHandler(typeof(TException), typeof(THandler));
            return builder;
        }

        public static IExceptionMapping<TException> Clear<TException>(
            this IExceptionMapping<TException> builder)
            where TException : Exception
        {
            builder.Options.Value.ClearHandlers(typeof(TException));
            return builder;
        }

        // rethrow
        public static IExceptionPolicyBuilder AddRethrow<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, ReThrowExceptionHandler>(index);
            return builder;
        }

        // Log
        public static IExceptionMapping<TException> AddLog<TException>(
            this IExceptionMapping<TException> builder, Action<LogHandlerOptions<TException>> settings = null, int index = -1)
            where TException : Exception
        {
            LogHandlerOptions<TException> options = new LogHandlerOptions<TException>();
            settings?.Invoke(options);
            builder.Services.TryAddSingleton(options);
            builder.EnsureHandler<TException, LogExceptionHandler<TException>>(index);

            return builder;
        }

        // Set status code
        public static IResponseHandlers<TException> AddNewResponse<TException>(
            this IExceptionMapping<TException> builder, Func<TException,int> statusCodeFactory = null, int index = -1)
            where TException : Exception
        {
            if (statusCodeFactory != null)
            {
                builder.Services.Configure<NewResponseOptions<TException>>(codeOptions =>
                    codeOptions.StatusCodeFactory = statusCodeFactory);
            }

            builder.EnsureHandler<TException, NewResponseHandler<TException>>(index);

            return builder as IResponseHandlers<TException>;
        }

        public static IResponseHandlers<TException> WithHeaders<TException>(
            this IResponseHandlers<TException> builder, Action<IHeaderDictionary,TException> settings = null, int index = -1)
            where TException : Exception
        {
            if (settings != null)
            {
                builder.Services.Configure<SetHeadersOptions<TException>>(codeOptions =>
                    codeOptions.SetHeadersAction = settings);
            }

            builder.EnsureHandler<TException, SetHeadersHandler<TException>>(index);

            return builder;
        }
    }
}