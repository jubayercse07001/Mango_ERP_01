using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common; // Contains HrEmployeeTrainingListDAL and HrEmployeeTrainingCategoryDAL

using ERP.Models.User;
using ERP.Models.HR.Employee.HrEmployeeTrainingList;

namespace ERP.Controllers.HR.Employee
{
    public class HrEmployeeTrainingListController : Controller
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

            // Index view mapping permission
            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.HrEmployeeTrainingLists = HrEmployeeTrainingListDAL.GetAllHrEmployeeTrainingLists();
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

            // Add view mapping permission
            if (user.PermissionGroup[1] != '1')
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            ViewBag.User = user;

            // Load Training Category dropdown list data using Category's existing DAL
            ViewBag.TrainingCategoryList = HrEmployeeTrainingCategoryDAL.GetAllHrTrainingCategories();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrEmployeeTrainingListInfo HrEmployeeTrainingList)
        {
            HrEmployeeTrainingListResponse response = new HrEmployeeTrainingListResponse();

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

            // Input Validation 
            if (string.IsNullOrWhiteSpace(HrEmployeeTrainingList.TrainingTitle))
                response.ResponseMessages.Add("Training Title is required");

            else if (HrEmployeeTrainingListDAL.GetHrEmployeeTrainingListByTitle(HrEmployeeTrainingList.TrainingTitle, null) != null)
                response.ResponseMessages.Add("Training Title already exists");

            if (string.IsNullOrWhiteSpace(HrEmployeeTrainingList.TrainingCategoryId))
                response.ResponseMessages.Add("Training Category is required");

            if (response.ResponseMessages != null && response.ResponseMessages.Count > 0)
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
                HrEmployeeTrainingList.RowId = Guid.NewGuid().ToString();

                HrEmployeeTrainingListDAL.AddHrEmployeeTrainingList(HrEmployeeTrainingList, lgUser);
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
            response.ResponseMessage = "Training List Item added successfully";

            return Json(response);
        }

        public ActionResult GetHrEmployeeTrainingListItemList()
        {
            List<HrEmployeeTrainingListInfo> list = new List<HrEmployeeTrainingListInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(list, JsonRequestBehavior.AllowGet);

            UserInfo user = UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(list, JsonRequestBehavior.AllowGet);

            HrEmployeeTrainingListResponse resp =
                HrEmployeeTrainingListDAL.GetFilteredHrEmployeeTrainingLists(Request);

            return Json(new
            {
                data = resp.HrEmployeeTrainingLists,
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

            // Edit view mapping permission
            if (user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            HrEmployeeTrainingListInfo trainingList =
                HrEmployeeTrainingListDAL.GetTrainingListByID(RouteData.Values["id"].ToString());

            if (trainingList == null)
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            ViewBag.User = user;
            ViewBag.HrEmployeeTrainingList = trainingList;

            // Dropdown dataset for entity context mapping updates
            ViewBag.TrainingCategoryList = HrEmployeeTrainingCategoryDAL.GetAllHrTrainingCategories();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrEmployeeTrainingListInfo HrEmployeeTrainingList)
        {
            HrEmployeeTrainingListResponse response = new HrEmployeeTrainingListResponse();

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

            // Validation checks during save phase updates
            if (string.IsNullOrWhiteSpace(HrEmployeeTrainingList.TrainingTitle))
                response.ResponseMessages.Add("Training Title is required");

            else if (HrEmployeeTrainingListDAL.GetHrEmployeeTrainingListByTitle(
                HrEmployeeTrainingList.TrainingTitle,
                HrEmployeeTrainingList.RowId) != null)
                response.ResponseMessages.Add("Training Title already exists");

            if (string.IsNullOrWhiteSpace(HrEmployeeTrainingList.TrainingCategoryId))
                response.ResponseMessages.Add("Training Category is required");

            if (response.ResponseMessages != null && response.ResponseMessages.Count > 0)
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
                HrEmployeeTrainingListDAL.UpdateHrEmployeeTrainingList(HrEmployeeTrainingList, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Training item details updated successfully";

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

            // Delete view mapping permission
            if (user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            if (RouteData.Values["id"] == null)
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            HrEmployeeTrainingListInfo trainingList =
                HrEmployeeTrainingListDAL.GetTrainingListByID(RouteData.Values["id"].ToString());

            if (trainingList == null)
                return Redirect(Url.Action("Index", "HrEmployeeTrainingList"));

            ViewBag.User = user;
            ViewBag.HrEmployeeTrainingList = trainingList;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrEmployeeTrainingListInfo HrEmployeeTrainingList)
        {
            HrEmployeeTrainingListResponse response = new HrEmployeeTrainingListResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            try
            {
                HrEmployeeTrainingListDAL.DeleteHrEmployeeTrainingList(HrEmployeeTrainingList, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Training List entry has been deleted successfully";
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