using System;
using System.IO;
using System.Net;
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
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

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
        public static IExceptionPolicyBuilder Rethrow<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, ReThrowExceptionHandler>(index);
            return builder;
        }

        // next chain
        public static IExceptionPolicyBuilder NextChain<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, NextChainHandler>(index);
            return builder;
        }

        // mark handled
        public static IExceptionPolicyBuilder Handled<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {
            builder.EnsureHandler<TException, MarkHandledHandler>(index);
            return builder;
        }

        // Retry
        public static IExceptionMapping<TException> Retry<TException>(
           this IExceptionMapping<TException> builder, Action<RetryHandlerOptions<TException>> settings = null, int index = -1)
           where TException : Exception
        {
            builder.Services.Configure<RetryHandlerOptions<TException>>(opt => settings?.Invoke(opt));

            builder.EnsureHandler<TException, RetryHandler<TException>>(index);

            return builder;
        }

        // Log
        public static IExceptionMapping<TException> Log<TException>(
            this IExceptionMapping<TException> builder, Action<LogHandlerOptions<TException>> settings = null, int index = -1)
            where TException : Exception
        {
            builder.Services.Configure<LogHandlerOptions<TException>>(opt => settings?.Invoke(opt));

            builder.EnsureHandler<TException, LogExceptionHandler<TException>>(index);

            return builder;
        }

        public static IExceptionMapping<TException> DisableLog<TException>(
            this IExceptionMapping<TException> builder, int index = -1)
            where TException : Exception
        {            
            builder.EnsureHandler<TException, DisableLoggingHandler>(index);

            return builder;
        }

        // Set status code
        public static IResponseHandlers<TException> Response<TException>(
            this IExceptionMapping<TException> builder, 
            Func<TException,int> statusCodeFactory = null, 
            RequestStartedBehaviour requestStartedBehaviour = RequestStartedBehaviour.ReThrow, 
            int index = -1)
            where TException : Exception
        {            
            builder.Services.Configure<RawResponseHandlerOptions<TException>>(responceOptions =>
            {
                responceOptions.RequestStartedBehaviour = requestStartedBehaviour;
                
                responceOptions.SetResponse.Add((context, exception) =>
                {
                    return RawResponseExceptionHandler<TException>.SetStatusCode(context, exception, statusCodeFactory);
                });
            });

            builder.EnsureHandler<TException, RawResponseExceptionHandler<TException>>(index);

            return builder as IResponseHandlers<TException>;
        }

        public static IResponseHandlers<TException> WithHeaders<TException>(
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

        public static IResponseHandlers<TException> WithBody<TException>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, Stream, TException, Task> settings, int index = -1)
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
                    if (context.Response.Body.CanSeek)
                        context.Response.Body.SetLength(0L);

                    return settings(context.Request,context.Response.Body, exception);
                });
            });

            return builder;
        }

        public static IResponseHandlers<TException> WithBodyJson<TException, TObject>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, TException, TObject> value, JsonSerializerSettings settings = null, int index = -1)
            where TException : Exception
        {
            return builder.WithBody((request, stream, exception) =>
            {
                if (settings == null)
                {                    
                    settings = request.HttpContext.RequestServices.GetService<JsonSerializerSettings>();
                }

                if (settings == null)
                {
                    settings = new JsonSerializerSettings();
                }

                JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

                var headers = request.HttpContext.Response.Headers;
                if (!headers.ContainsKey(HeaderNames.ContentType))
                {
                    headers[HeaderNames.ContentType] = "application/json";
                }

                using (TextWriter textWriter = new StreamWriter(stream))
                {
                    jsonSerializer.Serialize(textWriter, value(request, exception));
                }

                return Task.CompletedTask;
            });
        }
    }
}