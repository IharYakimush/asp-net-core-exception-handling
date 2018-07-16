using System;
using System.IO;
using System.Threading.Tasks;
using Commmunity.AspNetCore.ExceptionHandling.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Commmunity.AspNetCore.ExceptionHandling.NewtonsoftJson
{
    public static class PolicyBuilderExtensions
    {
        /// <summary>
        /// Write serialized object to response using <see cref="JsonSerializer"/> and pass control to next handler.
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
        /// <param name="settings">
        /// The JSON serializer settings <see cref="JsonSerializerSettings"/>.
        /// </param>
        /// <param name="index" optional="true">
        /// Handler index in the chain. Optional. By default handler added to the end of chain.
        /// </param>
        /// <returns>
        /// Policy builder
        /// </returns>
        [Obsolete("In case of using netcore2.1+ use Commmunity.AspNetCore.ExceptionHandling.Mvc instead")]
        public static IResponseHandlers<TException> WithBodyJson<TException, TObject>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, TException, TObject> valueFactory, JsonSerializerSettings settings = null, int index = -1)
            where TException : Exception
        {
            return builder.WithBody((request, streamWriter, exception) =>
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

                TObject val = valueFactory(request, exception);

                //return Task.CompletedTask;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(ms, streamWriter.Encoding, 1024, true))
                    {
                        jsonSerializer.Serialize(sw, val);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] array = ms.ToArray();
                    BinaryWriter binaryWriter = new BinaryWriter(streamWriter.BaseStream, streamWriter.Encoding, true);
                    binaryWriter.Write(array);

                    return Task.CompletedTask;
                }
            });
        }
    }
}
