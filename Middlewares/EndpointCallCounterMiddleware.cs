using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagementAPI.Middlewares
{
    public class EndpointCallCounterMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly Dictionary<string, int> _endpointCounts = new();

        public EndpointCallCounterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.ToString();
            lock (_endpointCounts)
            {
                if (_endpointCounts.ContainsKey(endpoint))
                    _endpointCounts[endpoint]++;
                else
                    _endpointCounts[endpoint] = 1;
            }

            await _next(context);
        }

        public static IReadOnlyDictionary<string, int> EndpointCounts => _endpointCounts;
    }

}