using System;
using Commmunity.AspNetCore.ExceptionHandling.Builder;
using Commmunity.AspNetCore.ExceptionHandling.Exc;
using Commmunity.AspNetCore.ExceptionHandling.Logs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commmunity.AspNetCore.ExceptionHandling
{
    public static class PolicyBuilderExtensions
    {
        public static ExceptionMapping<TException> ForException<TException>(
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

        public static IExceptionPolicyBuilder EnsureCommonHandler<THandler>(
            this IExceptionPolicyBuilder builder, int index = -1)
            where THandler : class, IExceptionHandler
        {
            builder.Options.Value.EnsureCommonHandler(typeof(THandler), index);
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

        public static IExceptionPolicyBuilder RemoveCommonHandler<THandler>(
            this IExceptionPolicyBuilder builder, int index = -1)
            where THandler : class, IExceptionHandler
        {
            builder.Options.RemoveCommonHandler(typeof(THandler));
            return builder;
        }

        public static ExceptionMapping<TException> Clear<TException>(
            this ExceptionMapping<TException> builder)
            where TException : Exception
        {
            builder.Options.Value.ClearHandlers(typeof(TException));
            return builder;
        }

        public static IExceptionPolicyBuilder ClearCommonHandlers(
            this IExceptionPolicyBuilder builder)
        {
            builder.Options.ClearCommonHandlers();
            return builder;
        }

        // rethrow
        public static ExceptionMapping<TException> AddRethrowHandler<TException>(
            this ExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            return builder.EnsureHandler<TException,ReThrowExceptionHandler>(index);
        }

        public static IExceptionPolicyBuilder AddRethrowHandler<TException>(
            this IExceptionPolicyBuilder builder, int index = -1)
            where TException : Exception
        {
            return builder.EnsureCommonHandler<ReThrowExceptionHandler>(index);
        }

        // Log
        public static ExceptionMapping<TException> AddLogHandler<TException>(
            this ExceptionMapping<TException> builder, Action<LogHandlerOptions<TException>> settings = null, int index = -1)
            where TException : Exception
        {
            LogHandlerOptions<TException> options = new LogHandlerOptions<TException>();
            settings?.Invoke(options);
            builder.Services.TryAddSingleton(options);
            return builder.EnsureHandler<TException, LogExceptionHandler<TException>>(index);
        }

        public static IExceptionPolicyBuilder AddLogHandler(
            this IExceptionPolicyBuilder builder, Action<LogHandlerOptions<CommonConfigurationException>> settings = null, int index = -1)
        {
            LogHandlerOptions<CommonConfigurationException> options = new LogHandlerOptions<CommonConfigurationException>();
            settings?.Invoke(options);
            builder.Services.TryAddSingleton(options);

            return builder.EnsureCommonHandler<LogExceptionHandler<CommonConfigurationException>>(index);
        }
    }
}