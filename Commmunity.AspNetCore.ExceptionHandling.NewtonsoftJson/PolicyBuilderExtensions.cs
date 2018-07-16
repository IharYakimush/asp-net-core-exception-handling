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
        [Obsolete("In case of using netcore2.1+ use Commmunity.AspNetCore.ExceptionHandling.Mvc instead")]
        public static IResponseHandlers<TException> WithBodyJson<TException, TObject>(
            this IResponseHandlers<TException> builder, Func<HttpRequest, TException, TObject> value, JsonSerializerSettings settings = null, int index = -1)
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

                TObject val = value(request, exception);

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
