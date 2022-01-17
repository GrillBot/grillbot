﻿using GrillBot.Data.Services.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure
{
    public class ErrorHandlingMiddleware
    {
        private RequestDelegate Next { get; }
        private LoggingService Logging { get; }

        public ErrorHandlingMiddleware(RequestDelegate next, LoggingService logging)
        {
            Next = next;
            Logging = logging;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await Next(context);
            }
            catch (Exception ex)
            {
                await Logging.ErrorAsync("API", $"An error occured while request processing ({context.Request.Path})", ex);
                throw;
            }
        }
    }
}
