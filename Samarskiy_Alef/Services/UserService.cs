using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Samarskiy_Alef.Models;

namespace Samarskiy_Alef.Services
{

    public class UserService: IDisposable
    {

        /// <summary>
        /// словарь информации о пользователях
        /// </summary>
        public static Dictionary<int, Models.UserModel> userinfo = new Dictionary<int, Models.UserModel>()
            {
                { 1, new Models.UserModel() {Token = new Guid("B42CDA11-4A3E-42B6-B162-0B4916E0A1D5"), Role = Role.Guest } },
                { 2, new Models.UserModel() {Token = new Guid("5EB8A0EF-681C-42DC-96BB-B95D23CEFDA0"), Role = Role.Admin } }
            };

        public Models.UserModel GetUserToken(int id)
        {
            var user = userinfo.Where(c => c.Key == id).SingleOrDefault();
            return user.Value;
        }

        void IDisposable.Dispose()
        {
        }
    }
}
