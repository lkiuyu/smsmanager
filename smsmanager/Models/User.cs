using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace smsmanager.Models
{
    public class User
    {
        [Display(Name = "用户名称")]
        [Required(ErrorMessage = "请填写用户名称")]
        public string uname { get; set; }
        [Display(Name = "密码")]
        [Required(ErrorMessage = "请填写密码")]
        public string upassword { get; set; }
    }
}
