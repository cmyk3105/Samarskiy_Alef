using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Samarskiy_Alef.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevelopController : Controller
    {
        [HttpGet("get_token/{id}")]
        [ActionName("get_token")]
        [AllowAnonymous]
        public ActionResult<string> GetToken(int id)
        {
            using (Services.UserService serv = new Services.UserService())
            {
                var user = serv.GetUserToken(id);
                if (user != null)
                {
                    HttpContext.Response.Cookies.Append("token", user.Token.ToString());
                    return "Идентификатор пользователя: " + user.Token.ToString();
                }
                else
                {
                    HttpContext.Response.Cookies.Delete("token");
                    return "Идентификатор пользователя: Неавторизованный пользователь";
                }
            }
        }

        [HttpGet("filter_test")]
        [ActionName("filter_test")]
        public ActionResult<string> FilterTest()
        {
            if (Filters.MyAuthenticationMiddleware.CurrentToken != null)
            {
                var user = Services.UserService.userinfo.Where(c => c.Value.Token == new Guid(Filters.MyAuthenticationMiddleware.CurrentToken)).SingleOrDefault();
                if (user.Value != null)
                {
                    if (user.Value.Role == Services.Role.Admin)
                    {
                        return "return Ok()";
                    }
                    else
                    {
                        return "Доступ запрещен";
                    }
                }
                else
                {
                    return "Доступ запрещен";
                }
            }
            else
            {
                return null;
            }

        }

        [HttpGet("guest_test")]
        [ActionName("guest_test")]
        public ActionResult<string> GuestTest()
        {
            if (Filters.MyAuthenticationMiddleware.CurrentToken != null)
            {
                var user = Services.UserService.userinfo.Where(c => c.Value.Token == new Guid(Filters.MyAuthenticationMiddleware.CurrentToken)).SingleOrDefault();
                if (user.Value != null)
                {
                    if (user.Value.Role == Services.Role.Guest)
                    {
                        return "Роль пользователя: " + user.Value.Role.ToString();
                    }
                    else
                    {
                        return "Доступ запрещен";
                    }
                }
                else
                {
                    return "Доступ запрещен";
                }
            }
            else
            {
                return null;
            }
        }

        // GET: api/<controller>
        [HttpGet]
        public ActionResult<string> Get()
        {
            if (Filters.MyAuthenticationMiddleware.CurrentToken != null)
            {
                return "Идентификатор пользователя: " + Filters.MyAuthenticationMiddleware.CurrentToken;
            }
            else
            {
                return "Идентификатор пользователя: Неавторизованный пользователь";
            }
        }
    }
}
