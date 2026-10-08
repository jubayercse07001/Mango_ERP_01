
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.DAL;
using ERP.Models.User;

namespace ERP.Controllers
{
    public class LoginController : Controller
    {

        public ActionResult Index()
        {
            string lgUser = string.Empty;
            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();
            if (!string.IsNullOrEmpty(lgUser))
            {
                return Redirect(Url.Action("Index", "Home"));
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserInfo loginUser)
        {
            LoginResponse logresp = new LoginResponse();
            if (String.IsNullOrEmpty(loginUser.Username) || String.IsNullOrEmpty(loginUser.Password))
            {
                logresp.ResponseStatus = 0;
                logresp.ResponseCode = "Null Information";
                logresp.ResponseMessages.Add("Username and Password cannot be empty.");
                logresp.ResponseMessage = "<ul><li>" + logresp.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(logresp);
            }

            UserInfo user = UserDAL.GetUserByUsername(loginUser.Username);

            if (user == null)
            {
                logresp.ResponseStatus = 0;
                logresp.ResponseCode = "Invalid User";
                logresp.ResponseMessages.Add("Username or Password is not correct.");
                logresp.ResponseMessage = "<ul><li>" + logresp.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(logresp);
            }
            string pHash = GMDAL.Hash(loginUser.Password);

            if (pHash != user.Password)
            {
                logresp.ResponseStatus = 0;
                logresp.ResponseCode = "Invalid credential";
                logresp.ResponseMessages.Add("Username or Password is not correct.");
                logresp.ResponseMessage = "<ul><li>" + logresp.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j) + "</li></ul>";
                return Json(logresp);
            }


            Session["LoggedInUser"] = user.UserCode;

            logresp.ResponseStatus = 1;
            logresp.ResponseCode = "Success";
            return Json(logresp);
        }

        public ActionResult Logout()
        {
            Session["LoggedInUser"] = null;
            return Redirect(Url.Action("Index", "Login"));
        }


    }
}