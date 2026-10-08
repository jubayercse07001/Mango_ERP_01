using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.DAL;
using ERP.DAL.HR.Common; 
using ERP.Models.User;
using ERP.Models.HR.Employee.HrEmployeeTraining;

namespace ERP.Controllers.HR.Employee
{
    public class HrEmployeeTrainingController : Controller
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

            // Note: Ensure your HrEmployeeTrainingDAL implements GetAllHrEmployeeTrainings()
            ViewBag.HrEmployeeTrainings = HrEmployeeTrainingDAL.GetAllHrEmployeeTrainings();
            ViewBag.User = user;

            return View();
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
                return Redirect(Url.Action("Index", "HrEmployeeTraining"));

            ViewBag.User = user;

            // Populating lookup dropdown values if training forms utilize relational entities
            // ViewBag.TrainingList = HrTrainingListDAL.GetAllActiveTrainings(); 

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeTrainingInfo HrEmployeeTraining)
        {
            HrEmployeeTrainingResponse response = new HrEmployeeTrainingResponse();

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

            // Server-Side Input Validation Rules
            if (string.IsNullOrWhiteSpace(HrEmployeeTraining.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeTraining.TrainingListId))
                response.ResponseMessages.Add("Training Title Selection is required");

            if (HrEmployeeTraining.StartDate == DateTime.MinValue)
                response.ResponseMessages.Add("Valid Start Date is required");

            if (HrEmployeeTraining.EndDate == DateTime.MinValue)
                response.ResponseMessages.Add("Valid End Date is required");

            if (HrEmployeeTraining.StartDate > HrEmployeeTraining.EndDate)
                response.ResponseMessages.Add("Start Date cannot be later than End Date");

            // Evaluate Business Unique Rules / Overlap Context Validation 
            if (!string.IsNullOrWhiteSpace(HrEmployeeTraining.EmployeeId) && !string.IsNullOrWhiteSpace(HrEmployeeTraining.TrainingListId))
            {
                if (HrEmployeeTrainingDAL.GetHrEmployeeTrainingDuplicate(HrEmployeeTraining.EmployeeId, HrEmployeeTraining.TrainingListId, null) != null)
                {
                    response.ResponseMessages.Add("This specific training record already exists for the employee");
                }
            }

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
                HrEmployeeTraining.RowId = Guid.NewGuid().ToString();

                HrEmployeeTrainingDAL.AddHrEmployeeTraining(HrEmployeeTraining, lgUser);
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
            response.ResponseMessage = "Employee training record added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeTrainingItemList()
        {
            List<HrEmployeeTrainingInfo> list = new List<HrEmployeeTrainingInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeTrainingResponse resp =
                HrEmployeeTrainingDAL.GetFilteredHrEmployeeTrainings(Request);

            return Json(new
            {
                data = resp.HrEmployeeTrainings,
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
                return Redirect(Url.Action("Index", "HrEmployeeTraining"));

            HrEmployeeTrainingInfo training =
                HrEmployeeTrainingDAL.GetTrainingByID(RouteData.Values["id"].ToString());

            if (training == null)
                return Redirect(Url.Action("Index", "HrEmployeeTraining"));

            ViewBag.User = user;
            ViewBag.HrEmployeeTraining = training;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeTrainingInfo HrEmployeeTraining)
        {
            HrEmployeeTrainingResponse response = new HrEmployeeTrainingResponse();

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

            // Input Validation Rules
            if (string.IsNullOrWhiteSpace(HrEmployeeTraining.EmployeeId))
                response.ResponseMessages.Add("Employee ID is required");

            if (string.IsNullOrWhiteSpace(HrEmployeeTraining.TrainingListId))
                response.ResponseMessages.Add("Training Title Selection is required");

            if (HrEmployeeTraining.StartDate == DateTime.MinValue)
                response.ResponseMessages.Add("Valid Start Date is required");

            if (HrEmployeeTraining.EndDate == DateTime.MinValue)
                response.ResponseMessages.Add("Valid End Date is required");

            if (HrEmployeeTraining.StartDate > HrEmployeeTraining.EndDate)
                response.ResponseMessages.Add("Start Date cannot be later than End Date");

            // Avoid collision matches against other table rows while modifying existing record context
            if (!string.IsNullOrWhiteSpace(HrEmployeeTraining.EmployeeId) && !string.IsNullOrWhiteSpace(HrEmployeeTraining.TrainingListId))
            {
                if (HrEmployeeTrainingDAL.GetHrEmployeeTrainingDuplicate(HrEmployeeTraining.EmployeeId, HrEmployeeTraining.TrainingListId, HrEmployeeTraining.RowId) != null)
                {
                    response.ResponseMessages.Add("This specific training record already exists for the employee");
                }
            }

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
                HrEmployeeTrainingDAL.UpdateHrEmployeeTraining(HrEmployeeTraining, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Training details updated successfully";

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
                return Redirect(Url.Action("Index", "HrEmployeeTraining"));

            HrEmployeeTrainingInfo training =
                HrEmployeeTrainingDAL.GetTrainingByID(RouteData.Values["id"].ToString());

            if (training == null)
                return Redirect(Url.Action("Index", "HrEmployeeTraining"));

            ViewBag.User = user;
            ViewBag.HrEmployeeTraining = training;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeTrainingInfo HrEmployeeTraining)
        {
            HrEmployeeTrainingResponse response = new HrEmployeeTrainingResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeTrainingDAL.DeleteHrEmployeeTraining(HrEmployeeTraining, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Training details dropped successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }
}