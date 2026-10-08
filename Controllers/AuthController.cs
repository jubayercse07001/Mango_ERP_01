using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.Models.User;

namespace ERP.Controllers
{
    public class AuthController : Controller
    {
        // GET: Auth
        
            public ActionResult Login()
            {
                return View();
            }

            [HttpPost]
            public ActionResult Login(UserInfo loginUser)
            {
                return View();
            }

            public ActionResult About()
            {
                ViewBag.Message = "Your application description page.";

                return View();
            }

            public ActionResult Contact()
            {
                ViewBag.Message = "Your contact page.";

                return View();
            }
    }
}