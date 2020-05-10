using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Samarskiy_Alef.Models
{
    #region модель информации о пользователе
    /// <summary>
    /// модель информации о пользователе
    /// </summary>
    public class UserModel: IdentityUser
    {
        [Required]
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public Guid Token { get; set; }

        [Required]
        /// <summary>
        /// роль пользователя
        /// </summary>
        public Services.Role Role { get; set; }
    }
    #endregion
}
