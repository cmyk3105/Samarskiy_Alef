using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samarskiy_Alef.Filters
{
    public class MyAuthenticationMiddleware
    {
        /// <summary>
        /// текущий аутентифицированный пользователь
        /// </summary>
        public static string CurrentToken = null;

        private RequestDelegate _next;
        public MyAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            CurrentToken = context.Request.Cookies.Where(c => c.Key == "token").SingleOrDefault().Value;
            //если пользователь не аутентифицирован
            if (CurrentToken == null)
            {
                context.Response.StatusCode = 401;
            }
            await _next.Invoke(context);
        }
    }
}
