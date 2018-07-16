using System;
using System.IO;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Builder;
using Commmunity.AspNetCore.ExceptionHandling.Exc;
using Commmunity.AspNetCore.ExceptionHandling.Handlers;
using Commmunity.AspNetCore.ExceptionHandling.Logs;
using Commmunity.AspNetCore.ExceptionHandling.Response;
using Commmunity.AspNetCore.ExceptionHandling.Retry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

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
        /// <summary>
        /// Re throw exception and stop handlers chain processing.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IExceptionPolicyBuilder Rethrow<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, ReThrowExceptionHandler>(index);
            return builder;
        }

        // next chain
        /// <summary>
        /// Terminates current handlers chain and try to execute next handlers chain which match exception type.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IExceptionPolicyBuilder NextChain<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, NextChainHandler>(index);
            return builder;
        }

        // mark handled
        /// <summary>
        /// Terminate handlers chain execution and mark exception as "handled" which means that it will not be re thrown.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IExceptionPolicyBuilder Handled<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, MarkHandledHandler>(index);
            return builder;
        }

        // Retry
        /// <summary>
        /// Terminate handlers chain and execute middleware pipeline (registered after exception handling policy middleware) again.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="settings">
        /// The retry settings. See <see cref="RetryHandlerOptions{TException}"/> for details.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IExceptionMapping<TException> Retry<TException>(
           this IExceptionMapping<TException> builder, Action<RetryHandlerOptions<TException>> settings = null, int index = -1)
           where TException : Exception
        {
            builder.Services.Configure<RetryHandlerOptions<TException>>(opt => settings?.Invoke(opt));

            builder.EnsureHandler<TException, RetryHandler<TException>>(index);

            return builder;
        }

        // Log
        /// <summary>
        /// Log exception using <see cref="ILoggerFacrory"/> registered in services collection and pass control to next handler in chain
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="settings">
        /// The logs settings. See <see cref="LogHandlerOptions{TException}"/> for details.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IExceptionMapping<TException> Log<TException>(
            this IExceptionMapping<TException> builder, Action<LogHandlerOptions<TException>> settings = null, int index = -1)
            where TException : Exception
        {
            builder.Services.Configure<LogHandlerOptions<TException>>(opt => settings?.Invoke(opt));

            builder.EnsureHandler<TException, LogExceptionHandler<TException>>(index);

            return builder;
        }
        /// <summary>
        /// Disable logging of this particular exception in further handlers for current request.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IExceptionMapping<TException> DisableFurtherLog<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {            
            builder.EnsureHandler<TException, DisableLoggingHandler>(index);

            return builder;
        }

        // Set status code
        /// <summary>
        /// Configure response and pass control to next handler. It is recommended to finish chain using <see cref="Handled{TException}"/> when response builder will be configured.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="responseAlreadyStartedBehaviour">
        /// The begaviour <see cref="ResponseAlreadyStartedBehaviour"/> in case response already started.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <param name="statusCodeFactory">
        /// The factory for response status code. By default 500 will be used, unless code was already set by another handler for current request.
        /// </param>
        /// <returns>
        /// Response builder.
        /// </returns>
        public static IResponseHandlers<TException> Response<TException>(
            this IExceptionMapping<TException> builder, 
            Func<TException,int> statusCodeFactory = null, 
            ResponseAlreadyStartedBehaviour responseAlreadyStartedBehaviour = ResponseAlreadyStartedBehaviour.ReThrow, 
            int index = -1)
            where TException : Exception
        {            
            builder.Services.Configure<RawResponseHandlerOptions<TException>>(responceOptions =>
            {
                responceOptions.ResponseAlreadyStartedBehaviour = responseAlreadyStartedBehaviour;
                
                responceOptions.SetResponse.Add((context, exception) =>
                {
                    return RawResponseExceptionHandler<TException>.SetStatusCode(context, exception, statusCodeFactory);
                });
            });

            builder.EnsureHandler<TException, RawResponseExceptionHandler<TException>>(index);

            return builder as IResponseHandlers<TException>;
        }

        /// <summary>
        /// Modify response headers
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="settings">
        /// Action for response headers modification
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Response builder
        /// </returns>
        public static IResponseHandlers<TException> Headers<TException>(
            this IResponseHandlers<TException> builder, Action<IHeaderDictionary,TException> settings, int index = -1)
            where TException : Exception
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            builder.Services.Configure<RawResponseHandlerOptions<TException>>(responceOptions =>
            {                
                responceOptions.SetResponse.Add((context, exception) =>
                {
                    settings.Invoke(context.Response.Headers, exception);
                    return Task.CompletedTask;
                });
            });

            return builder;
        }

        /// <summary>
        /// Set response headers which revents response from being cached.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IResponseHandlers<TException> ClearCacheHeaders<TException>(
            this IResponseHandlers<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.Services.Configure<RawResponseHandlerOptions<TException>>(responceOptions =>
            {
                responceOptions.SetResponse.Add((context, exception) =>
                {
                    HttpResponse response = context.Response;

                    response.Headers[HeaderNames.CacheControl] = "no-cache";
                    response.Headers[HeaderNames.Pragma] = "no-cache";
                    response.Headers[HeaderNames.Expires] = "-1";
                    response.Headers.Remove(HeaderNames.ETag);

                    return Task.CompletedTask;
                });
            });

            return builder;
        }

        /// <summary>
        /// Set response body, close response builder and pass control to next handler.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="settings">
        /// Delegate to write response using <see cref="StreamWriter"/>.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IResponseHandlers<TException> WithBody<TException>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, StreamWriter, TException, Task> settings, int index = -1)
            where TException : Exception
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }            

            builder.Services.Configure<RawResponseHandlerOptions<TException>>(responceOptions =>
            {
                responceOptions.SetResponse.Add((context, exception) =>
                {
                    return RawResponseExceptionHandler<TException>.SetBody(context, exception, settings);
                });
            });

            return builder;
        }        
    }
}