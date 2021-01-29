using cw5.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cw5.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        private WriteToFile _write = new WriteToFileImp();

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            if (httpContext.Request != null)
            {
                string path = httpContext.Request.Path;
                string method = httpContext.Request.Method;
                string queryString = httpContext.Request.QueryString.ToString();
                string body = "";
                using(var reader=new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
               {
                    body = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                }
                _write.Write(@".\requestsLog.txt", DateTime.Now.ToString());
                _write.Write(@".\requestsLog.txt", "path: "+path);
                _write.Write(@".\requestsLog.txt", "method: "+method);
                _write.Write(@".\requestsLog.txt", "queryString: "+queryString);
                _write.Write(@".\requestsLog.txt", "body: "+body);
            }
            if (_next !=null) await _next(httpContext);
        }
    }
}
