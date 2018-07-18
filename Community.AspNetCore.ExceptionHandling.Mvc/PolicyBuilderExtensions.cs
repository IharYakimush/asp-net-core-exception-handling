using System;
using Community.AspNetCore.ExceptionHandling.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Community.AspNetCore.ExceptionHandling.Mvc
{
    public static class PolicyBuilderExtensions
    {
        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        /// <summary>
        /// Set <see cref="IActionResult"/> to response and pass control to next handler.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The action result type. Should implement <see cref="IActionResult"/>.
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="resultFactory">
        /// The <see cref="IActionResult"/> factory.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IResponseHandlers<TException> WithActionResult<TException, TResult>(
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

        /// <summary>
        /// Set <see cref="IActionResult"/> to response and pass control to next handler.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The action result type. Should implement <see cref="IActionResult"/>.
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="result">
        /// The <see cref="IActionResult"/> action result.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IResponseHandlers<TException> WithActionResult<TException, TResult>(
            this IResponseHandlers<TException> builder, TResult result, int index = -1)
            where TException : Exception
            where TResult : IActionResult
        {
            return WithActionResult(builder, (request, exception) => result);
        }

        /// <summary>
        /// Set <see cref="ObjectResult"/> to response and pass control to next handler.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <typeparam name="TObject">
        /// The result object type.
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="value">
        /// The result object.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IResponseHandlers<TException> WithObjectResult<TException, TObject>(
            this IResponseHandlers<TException> builder, TObject value, int index = -1)
            where TException : Exception
        {
            return WithActionResult(builder, new ObjectResult(value), index);
        }

        /// <summary>
        /// Set <see cref="ObjectResult"/> to response and pass control to next handler.
        /// </summary>
        /// <typeparam name="TException">
        /// The exception type
        /// </typeparam>
        /// <typeparam name="TObject">
        /// The result object type.
        /// </typeparam>
        /// <param name="builder">
        /// The policy builder 
        /// </param>
        /// <param name="valueFactory">
        /// The result object factory.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        public static IResponseHandlers<TException> WithObjectResult<TException, TObject>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, TException, TObject> valueFactory, int index = -1)
            where TException : Exception
        {
            return WithActionResult(builder, (request, exception) => new ObjectResult(valueFactory(request, exception)), index);
        }
    }
}
