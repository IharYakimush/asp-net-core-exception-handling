using System;
using Commmunity.AspNetCore.ExceptionHandling.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Commmunity.AspNetCore.ExceptionHandling.Mvc
{
    public static class PolicyBuilderExtensions
    {
        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public static IResponseHandlers<TException> WithBodyActionResult<TException, TResult>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, TException, TResult> resultFactory, int index = -1)
            where TException : Exception
        where TResult : IActionResult
        {
            return builder.WithBody((request, streamWriter, exception) =>
            {
                var context = request.HttpContext;
                var executor = context.RequestServices.GetService<IActionResultExecutor<TResult>>();

                if (executor == null)
                {
                    throw new InvalidOperationException($"No result executor for '{typeof(TResult).FullName}' has been registered.");
                }

                var routeData = context.GetRouteData() ?? EmptyRouteData;

                var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

                return executor.ExecuteAsync(actionContext, resultFactory(request, exception));
            });
        }

        public static IResponseHandlers<TException> WithBodyActionResult<TException, TResult>(
            this IResponseHandlers<TException> builder, TResult result, int index = -1)
            where TException : Exception
            where TResult : IActionResult
        {
            return builder.WithBodyActionResult((request, exception) => result);
        }

        public static IResponseHandlers<TException> WithBodyObjectResult<TException, TObject>(
            this IResponseHandlers<TException> builder, TObject value, int index = -1)
            where TException : Exception
        {
            return builder.WithBodyActionResult(new ObjectResult(value), index);
        }

        public static IResponseHandlers<TException> WithBodyObjectResult<TException, TObject>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, TException, TObject> valueFactory, int index = -1)
            where TException : Exception
        {
            return builder.WithBodyActionResult(
                (request, exception) => new ObjectResult(valueFactory(request, exception)), index);
        }
    }
}
