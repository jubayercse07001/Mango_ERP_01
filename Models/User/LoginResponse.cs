using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.User
{
    public class LoginResponse : WebResponse
    {
        public LoginResponse()
        {
            ResponseTS = DateTime.Now;
            ResponseMessages = new List<string>();
            users = new List<UserInfo>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public UserInfo user { get; set; }
        public List<UserInfo> users { get; set; }
    }
}