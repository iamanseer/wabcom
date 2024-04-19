using Microsoft.AspNetCore.Http;
using PB.Model;
using PB.Shared;
using PB.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace PB.Server.Helper
{
    public class Middleware
    {
        //Reference:https://jasonwatmore.com/post/2020/10/02/aspnet-core-31-global-error-handler-tutorial

        private readonly RequestDelegate _next;
        private readonly ILogger<Middleware> _logger;

        public Middleware(RequestDelegate next, ILogger<Middleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
         {
            if (context.Request.GetTypedHeaders().AcceptLanguage.FirstOrDefault() != null)
            {
                var lang = context.Request.GetTypedHeaders().AcceptLanguage.FirstOrDefault().ToString();
                var cultureInfo = new CultureInfo(lang);
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
            }
            try
            {
                await _next(context);
            }
            catch (PBException error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                string result = "";
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new Error(error.Response.ResponseMessage, error.Response.ResponseTitle, error.Response.ResponseErrorDescription), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                await response.WriteAsync(result);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                string result = JsonSerializer.Serialize(new DbError(error.Message), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                _logger.LogError(error.Message);
                await response.WriteAsync(result);
            }
        }
    }
}
