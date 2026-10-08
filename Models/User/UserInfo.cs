using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.User
{
    public class UserInfo
    {
        public string UserCode { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public string PermissionGroup { get; set; }
        public string OfficeCode { get; set; }
        public string DisplayName { get; set; }

        
    }
}