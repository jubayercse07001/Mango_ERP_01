using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.HR.Common;

using ERP.Models.User;
using ERP.Models.HR.Organization.HrBranch;

namespace ERP.Controllers.HR.Organization
{
    public class HrBranchController : Controller
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

            ViewBag.HrBranches = HrBranchDAL.GetAllHrBranches();
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
                return Redirect(Url.Action("Index", "HrBranch"));

            ViewBag.User = user;

            ViewBag.DivisionList =
                HrDivisionDAL.GetAllHrDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Add(HrBranchInfo HrBranch)
        {
            HrBranchResponse response =
                new HrBranchResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";

                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

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

            if (string.IsNullOrWhiteSpace(HrBranch.BranchName))
                response.ResponseMessages.Add(
                    "Branch Name is required");

            else if (HrBranchDAL.GetHrBranchByName(HrBranch.BranchName,null, HrBranch.DivisionId) != null)

                response.ResponseMessages.Add(
                    "Branch Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrBranch.RowId =
                    Guid.NewGuid().ToString();

                HrBranchDAL.AddHrBranch(
                    HrBranch,
                    lgUser);
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
            response.ResponseMessage =
                "Branch added successfully";

            return Json(response);
        }

        public ActionResult GetHrBranchItemList()
        {
            List<HrBranchInfo> list =
                new List<HrBranchInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Json(
                    list,
                    JsonRequestBehavior.AllowGet);

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Json(
                    list,
                    JsonRequestBehavior.AllowGet);

            HrBranchResponse resp =
                HrBranchDAL.GetFilteredHrBranches(
                    Request);

            return Json(new
            {
                data = resp.HrBranches,
                draw = Request["draw"],
                recordsTotal =
                    resp.TotalRecordCount,
                recordsFiltered =
                    resp.FilteredRecordCount
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
                return Redirect(
                    Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));

            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action("Index", "HrBranch"));

            HrBranchInfo branch =
                HrBranchDAL.GetBranchByID(
                    RouteData.Values["id"].ToString());

            if (branch == null)
                return Redirect(
                    Url.Action("Index", "HrBranch"));

            ViewBag.User = user;
            ViewBag.HrBranch = branch;

            ViewBag.DivisionList =
                HrDivisionDAL.GetAllHrDivisions();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(HrBranchInfo HrBranch)
        {
            HrBranchResponse response =
                new HrBranchResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                return Json(response);
            }

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

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

            if (string.IsNullOrWhiteSpace(
                HrBranch.BranchName))

                response.ResponseMessages.Add(
                    "Branch Name is required");

            else if (HrBranchDAL.GetHrBranchByName(HrBranch.BranchName,HrBranch.RowId, HrBranch.DivisionId) != null)

                response.ResponseMessages.Add(
                    "Branch Name already exists");

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) => i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }

            try
            {
                HrBranchDAL.UpdateHrBranch(
                    HrBranch,
                    lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage =
                "Branch updated successfully";

            return Json(response);
        }

        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login"));

            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action("Index", "HrBranch"));

            HrBranchInfo branch =
                HrBranchDAL.GetBranchByID(
                    RouteData.Values["id"].ToString());

            if (branch == null)
                return Redirect(
                    Url.Action("Index", "HrBranch"));

            ViewBag.User = user;
            ViewBag.HrBranch = branch;

            return View();
        }

        [HttpPost]
        public ActionResult Delete(HrBranchInfo HrBranch)
        {
            HrBranchResponse response =
                new HrBranchResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();

            try
            {
                HrBranchDAL.DeleteHrBranch(
                    HrBranch,
                    lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage =
                    "Branch deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseMessage =
                    ex.Message;
            }

            return Json(response);
        }
    }


}
