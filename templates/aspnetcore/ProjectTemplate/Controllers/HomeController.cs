﻿using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Vostok.Tracing;

namespace ProjectTemplate.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet("{*url}")]
        public object Echo()
        {
            return Json(new
            {
                url = Request.GetUri(),
                traceId = $"http://localhost:80/{TraceContext.Current.TraceId}"
            });
        }
    }
}
