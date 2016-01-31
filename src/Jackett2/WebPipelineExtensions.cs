using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackett2
{
    public static class WebPipelineExtensions
    {
        public static IApplicationBuilder Rewrite(this IApplicationBuilder builder, string from, string to)
        {
            builder.Use(async (context,next) =>
             {
                 if (context.Request.Path.HasValue &&
                 context.Request.Path.Value.StartsWith(from, StringComparison.OrdinalIgnoreCase))
                 {
                     context.Request.Path = new PathString(to);
                 }

                 await next();
            });
            return builder;
        }
    }
}
