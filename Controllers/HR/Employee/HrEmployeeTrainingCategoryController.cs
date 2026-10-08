using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;
using ERP.Models.User;
using ERP.Models.HR.Employee.HrEmployeeTrainingCategory;

namespace ERP.Controllers.HR.Employee
{
    public class HrEmployeeTrainingCategoryController : Controller
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

            ViewBag.HrTrainingCategories = HrEmployeeTrainingCategoryDAL.GetAllHrTrainingCategories();
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
                return Redirect(Url.Action("Index", "HrEmployeeTrainingCategory"));

            ViewBag.User = user;

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeTrainingCategoryInfo TrainingCategory)
        {
            // Fixed: Uses the ActionResponse variant to prevent data contract masking
            HrEmployeeTrainingCategoryActionResponse response = new HrEmployeeTrainingCategoryActionResponse();

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

            var validationMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(TrainingCategory.TrainingCategoryName))
                validationMessages.Add("Training Category Name is required");

            else if (HrEmployeeTrainingCategoryDAL.GetHrTrainingCategoryByName(TrainingCategory.TrainingCategoryName, null) != null)
                validationMessages.Add("Training Category Name already exists");

            if (validationMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";
                response.ResponseMessage =
                    "<ul><li>" +
                    validationMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                TrainingCategory.RowId = Guid.NewGuid().ToString();
                HrEmployeeTrainingCategoryDAL.AddHrTrainingCategory(TrainingCategory, lgUser);
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
            response.ResponseMessage = "Training Category added successfully";

            return Json(response);
        }

        public ActionResult GetHrTrainingCategoryItemList()
        {
            var list = new List<HrEmployeeTrainingCategoryInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            // FIX: Explicitly point to the DAL namespace version to resolve ambiguity (CS0104)
            ERP.DAL.HR.Common.HrEmployeeTrainingCategoryResponse resp =
                HrEmployeeTrainingCategoryDAL.GetFilteredHrTrainingCategories(Request);

            return Json(new
            {
                data = resp.HrTrainingCategories,
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
                return Redirect(Url.Action("Index", "HrEmployeeTrainingCategory"));

            HrEmployeeTrainingCategoryInfo category =
                HrEmployeeTrainingCategoryDAL.GetTrainingCategoryByID(RouteData.Values["id"].ToString());

            if (category == null)
                return Redirect(Url.Action("Index", "HrEmployeeTrainingCategory"));

            ViewBag.User = user;
            ViewBag.HrTrainingCategory = category;

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeTrainingCategoryInfo TrainingCategory)
        {
            HrEmployeeTrainingCategoryActionResponse response = new HrEmployeeTrainingCategoryActionResponse();

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

            var validationMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(TrainingCategory.TrainingCategoryName))
                validationMessages.Add("Training Category Name is required");

            else if (HrEmployeeTrainingCategoryDAL.GetHrTrainingCategoryByName(
                TrainingCategory.TrainingCategoryName,
                TrainingCategory.RowId) != null)
                validationMessages.Add("Training Category Name already exists");

            if (validationMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    validationMessages.Aggregate((i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrEmployeeTrainingCategoryDAL.UpdateHrTrainingCategory(TrainingCategory, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Training Category updated successfully";

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
                return Redirect(Url.Action("Index", "HrEmployeeTrainingCategory"));

            HrEmployeeTrainingCategoryInfo category =
                HrEmployeeTrainingCategoryDAL.GetTrainingCategoryByID(RouteData.Values["id"].ToString());

            if (category == null)
                return Redirect(Url.Action("Index", "HrEmployeeTrainingCategory"));

            ViewBag.User = user;
            ViewBag.HrTrainingCategory = category;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeTrainingCategoryInfo TrainingCategory)
        {
            HrEmployeeTrainingCategoryActionResponse response = new HrEmployeeTrainingCategoryActionResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeTrainingCategoryDAL.DeleteHrTrainingCategory(TrainingCategory, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Training Category deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage = ex.Message;
            }

            return Json(response);
        }
    }

    // Renamed to isolate from the DAL namespace models
    public class HrEmployeeTrainingCategoryActionResponse
    {
        public int ResponseStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<string> ResponseMessages { get; set; } = new List<string>();
    }
}