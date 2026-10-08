using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory.InvJob;
using ERP.DAL.HR.Organization;

using ERP.Models.User;
using ERP.Models.Inventory.InvJob;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.Inventory
{
    public class InvJobController : Controller
    {
        public ActionResult Index()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.InvJobs = InvJobDAL.GetAllInvJobs();
            ViewBag.User = user;

            return View();
        }

        [HttpGet]
        public JsonResult GetnextJob()
        {
            int nextJob = InvJobDAL.GetNextJobNo();
            return Json(nextJob, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[1] != '1')
                return Redirect(Url.Action("Index", "InvJob"));

            ViewBag.User = user;

            // dropdowns
            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvJobInfo InvJob)
        {
            InvJobResponse response = new InvJobResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";
                return Json(response);
            }

            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(InvJob.JobName))
                response.ResponseMessages.Add("Job Name is required");

            else if (InvJobDAL.GetInvJobByName(InvJob.JobName, null) != null)
                response.ResponseMessages.Add("Job Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                InvJob.RowId = Guid.NewGuid().ToString();

                InvJobDAL.AddInvJob(InvJob, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseCode = "Success";
            response.ResponseMessage = "Job added successfully";

            return Json(response);
        }

        public ActionResult GetInvJobItemList()
        {
            List<InvJobInfo> list = new List<InvJobInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            InvJobDAL.InvJobResponse resp =
                InvJobDAL.GetFilteredInvJobs(Request);

            return Json(new
            {
                data = resp.InvJobs,
                draw = Request["draw"],
                recordsTotal = resp.TotalRecordCount,
                recordsFiltered = resp.FilteredRecordCount
            },
            JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "InvJob"));

            InvJobInfo job =
                InvJobDAL.GetJobByID(RouteData.Values["id"].ToString());

            if (job == null)
                return Redirect(Url.Action("Index", "InvJob"));

            ViewBag.User = user;
            ViewBag.InvJob = job;

            ViewBag.DepartmentList = HrDepartmentDAL.GetAllHrDepartments();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(InvJobInfo InvJob)
        {
            InvJobResponse response = new InvJobResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                return Json(response);
            }

            if (string.IsNullOrWhiteSpace(InvJob.JobName))
                response.ResponseMessages.Add("Job Name is required");

            else if (InvJobDAL.GetInvJobByName(
                InvJob.JobName,
                InvJob.RowId) != null)
                response.ResponseMessages.Add("Job Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                InvJobDAL.UpdateInvJob(InvJob, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Job updated successfully";

            return Json(response);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "InvJob"));

            InvJobInfo job =
                InvJobDAL.GetJobByID(RouteData.Values["id"].ToString());

            if (job == null)
                return Redirect(Url.Action("Index", "InvJob"));

            ViewBag.User = user;
            ViewBag.InvJob = job;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(InvJobInfo InvJob)
        {
            InvJobResponse response = new InvJobResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                InvJobDAL.DeleteInvJob(InvJob, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Job deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }

    public class InvJobResponse
    {
        public int ResponseStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<string> ResponseMessages { get; set; } = new List<string>();
    }
}