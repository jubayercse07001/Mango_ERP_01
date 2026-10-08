using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models.User;
using ERP.Models.Inventory.InvUnitConversion;

namespace ERP.Controllers.Inventory
{
    public class InvUnitConversionController : Controller
    {
        // =========================================================
        // INDEX
        // =========================================================
        [HttpGet]
        public ActionResult Index()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(Url.Action("Logout", "Login"));

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(Url.Action("Logout", "Login"));

            if (user.PermissionGroup[0] != '1')
                return Redirect(
                    Url.Action("Index", "Home")
                );

            ViewBag.InvUnitConversions =
                InvUnitConversionDAL.GetAllInvUnitConversions();

            ViewBag.User = user;

            return View();
        }


        // =========================================================
        // ADD - GET
        // =========================================================
        [HttpGet]
        public ActionResult Add()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action("Logout", "Login")
                );

            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action("Logout", "Login")
                );

            if (user.PermissionGroup[1] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );

            ViewBag.User = user;

            // Load Unit List
            ViewBag.UnitList =
                InvUnitDAL.GetAllInvUnits();

            return View();
        }


        // =========================================================
        // ADD - POST
        // =========================================================
        [HttpPost]
        public ActionResult Add(
            InvUnitConversionInfo invUnitConversion)
        {
            InvUnitConversionResponse response =
                new InvUnitConversionResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();


            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Not Logged In";

                return Json(response);
            }


            // -----------------------------------------------------
            // USER CHECK
            // -----------------------------------------------------
            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode = "Invalid User";

                return Json(response);
            }


            // -----------------------------------------------------
            // PERMISSION CHECK
            // -----------------------------------------------------
            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";

                return Json(response);
            }


            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                invUnitConversion.FirstUnitId))
            {
                response.ResponseMessages.Add(
                    "First Unit is required"
                );
            }


            if (string.IsNullOrWhiteSpace(
                invUnitConversion.SecondUnitId))
            {
                response.ResponseMessages.Add(
                    "Second Unit is required"
                );
            }


            if (invUnitConversion.ConversionRate <= 0)
            {
                response.ResponseMessages.Add(
                    "Conversion Rate must be greater than zero"
                );
            }


            // -----------------------------------------------------
            // UNIT COMPARISON / DUPLICATE CHECK
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    invUnitConversion.FirstUnitId)
                &&
                !string.IsNullOrWhiteSpace(
                    invUnitConversion.SecondUnitId))
            {
                // Same unit is not allowed
                if (invUnitConversion.FirstUnitId ==
                    invUnitConversion.SecondUnitId)
                {
                    response.ResponseMessages.Add(
                        "First Unit and Second Unit cannot be the same"
                    );
                }
                else
                {
                    // Check duplicate conversion
                    InvUnitConversionInfo existingConversion =
                        InvUnitConversionDAL
                            .GetInvUnitConversionByUnit(
                                invUnitConversion.FirstUnitId,
                                invUnitConversion.SecondUnitId,
                                null
                            );

                    if (existingConversion != null)
                    {
                        response.ResponseMessages.Add(
                            "This unit conversion already exists"
                        );
                    }
                }
            }


            // -----------------------------------------------------
            // VALIDATION RESPONSE
            // -----------------------------------------------------

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) =>
                            i + "</li><li>" + j
                    ) +
                    "</li></ul>";

                return Json(response);
            }


            // -----------------------------------------------------
            // SAVE
            // -----------------------------------------------------

            try
            {
                invUnitConversion.RowId =
                    Guid.NewGuid().ToString();

                InvUnitConversionDAL
                    .AddInvUnitConversion(
                        invUnitConversion,
                        lgUser
                    );
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Database Error";

                response.ResponseMessage =
                    ex.Message;

                return Json(response);
            }


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            response.ResponseStatus = 1;
            response.ResponseCode = "Success";

            response.ResponseMessage =
                "Unit conversion added successfully";

            return Json(response);
        }


        // =========================================================
        // DATATABLES LIST
        // =========================================================
        [HttpGet]
        public ActionResult GetInvUnitConversionItemList()
        {
            List<InvUnitConversionInfo> list =
                new List<InvUnitConversionInfo>();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser =
                    Session["LoggedInUser"].ToString();


            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
            {
                return Json(
                    list,
                    JsonRequestBehavior.AllowGet
                );
            }


            // -----------------------------------------------------
            // USER CHECK
            // -----------------------------------------------------
            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                return Json(
                    list,
                    JsonRequestBehavior.AllowGet
                );
            }


            // -----------------------------------------------------
            // GET FILTERED DATA
            // -----------------------------------------------------

            InvUnitConversionDAL.InvUnitConversionResponse resp =
                InvUnitConversionDAL
                    .GetFilteredInvUnitConversions(
                        Request
                    );


            return Json(
                new
                {
                    data =
                        resp.InvUnitConversions,

                    draw =
                        Request["draw"],

                    recordsTotal =
                        resp.TotalRecordCount,

                    recordsFiltered =
                        resp.FilteredRecordCount
                },
                JsonRequestBehavior.AllowGet
            );
        }


        // =========================================================
        // EDIT - GET
        // =========================================================
        [HttpGet]
        public ActionResult Edit()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser =
                    Session["LoggedInUser"].ToString();


            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action(
                        "Logout",
                        "Login"
                    )
                );


            // -----------------------------------------------------
            // USER CHECK
            // -----------------------------------------------------
            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action(
                        "Logout",
                        "Login"
                    )
                );


            // -----------------------------------------------------
            // PERMISSION CHECK
            // -----------------------------------------------------
            if (user.PermissionGroup[2] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );


            // -----------------------------------------------------
            // GET ID
            // -----------------------------------------------------
            string id =
                RouteData.Values["id"] != null
                    ? RouteData.Values["id"].ToString()
                    : "";


            if (string.IsNullOrWhiteSpace(id))
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );


            // -----------------------------------------------------
            // GET CONVERSION
            // -----------------------------------------------------
            InvUnitConversionInfo
                invUnitConversion =
                    InvUnitConversionDAL
                        .GetInvUnitConversionByID(id);


            if (invUnitConversion == null)
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );


            // -----------------------------------------------------
            // VIEW DATA
            // -----------------------------------------------------
            ViewBag.User = user;

            ViewBag.InvUnitConversion =
                invUnitConversion;

            // Load Unit List for Edit
            ViewBag.UnitList =
                InvUnitDAL.GetAllInvUnits();


            return View();
        }


        // =========================================================
        // EDIT - POST
        // =========================================================
        [HttpPost]
        public ActionResult Edit(
            InvUnitConversionInfo invUnitConversion)
        {
            InvUnitConversionResponse response =
                new InvUnitConversionResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser =
                    Session["LoggedInUser"].ToString();


            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Not Logged In";

                return Json(response);
            }


            // -----------------------------------------------------
            // USER CHECK
            // -----------------------------------------------------
            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Invalid User";

                return Json(response);
            }


            // -----------------------------------------------------
            // PERMISSION CHECK
            // -----------------------------------------------------
            if (user.PermissionGroup[2] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode =
                    "Not Authorized";

                return Json(response);
            }


            // -----------------------------------------------------
            // VALIDATION
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                invUnitConversion.RowId))
            {
                response.ResponseMessages.Add(
                    "Invalid conversion information"
                );
            }


            if (string.IsNullOrWhiteSpace(
                invUnitConversion.FirstUnitId))
            {
                response.ResponseMessages.Add(
                    "First Unit is required"
                );
            }


            if (string.IsNullOrWhiteSpace(
                invUnitConversion.SecondUnitId))
            {
                response.ResponseMessages.Add(
                    "Second Unit is required"
                );
            }


            if (invUnitConversion.ConversionRate <= 0)
            {
                response.ResponseMessages.Add(
                    "Conversion Rate must be greater than zero"
                );
            }


            // -----------------------------------------------------
            // DUPLICATE UNIT CONVERSION CHECK
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    invUnitConversion.FirstUnitId)
                &&
                !string.IsNullOrWhiteSpace(
                    invUnitConversion.SecondUnitId))
            {
                // Same unit is not allowed
                if (invUnitConversion.FirstUnitId ==
                    invUnitConversion.SecondUnitId)
                {
                    response.ResponseMessages.Add(
                        "First Unit and Second Unit cannot be the same"
                    );
                }
                else
                {
                    // Check duplicate conversion
                    // Exclude current RowId
                    InvUnitConversionInfo
                        existingConversion =
                            InvUnitConversionDAL
                                .GetInvUnitConversionByUnit(
                                    invUnitConversion.FirstUnitId,
                                    invUnitConversion.SecondUnitId,
                                    invUnitConversion.RowId
                                );


                    if (existingConversion != null)
                    {
                        response.ResponseMessages.Add(
                            "This unit conversion already exists"
                        );
                    }
                }
            }


            // -----------------------------------------------------
            // VALIDATION RESPONSE
            // -----------------------------------------------------

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) =>
                            i + "</li><li>" + j
                    ) +
                    "</li></ul>";

                return Json(response);
            }


            // -----------------------------------------------------
            // UPDATE
            // -----------------------------------------------------

            try
            {
                InvUnitConversionDAL
                    .UpdateInvUnitConversion(
                        invUnitConversion,
                        lgUser
                    );
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Database Error";

                response.ResponseMessage =
                    ex.Message;

                return Json(response);
            }


            // -----------------------------------------------------
            // SUCCESS
            // -----------------------------------------------------

            response.ResponseStatus = 1;
            response.ResponseCode =
                "Success";

            response.ResponseMessage =
                "Unit conversion updated successfully";

            return Json(response);
        }


        // =========================================================
        // DELETE - GET
        // =========================================================
        [HttpGet]
        public ActionResult Delete()
        {
            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser =
                    Session["LoggedInUser"].ToString();


            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
                return Redirect(
                    Url.Action(
                        "Logout",
                        "Login"
                    )
                );


            // -----------------------------------------------------
            // USER CHECK
            // -----------------------------------------------------
            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
                return Redirect(
                    Url.Action(
                        "Logout",
                        "Login"
                    )
                );


            // -----------------------------------------------------
            // PERMISSION CHECK
            // -----------------------------------------------------
            if (user.PermissionGroup[3] != '1')
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );


            // -----------------------------------------------------
            // GET ID
            // -----------------------------------------------------
            string id =
                RouteData.Values["id"] != null
                    ? RouteData.Values["id"].ToString()
                    : "";


            if (string.IsNullOrWhiteSpace(id))
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );


            // -----------------------------------------------------
            // GET CONVERSION
            // -----------------------------------------------------
            InvUnitConversionInfo
                invUnitConversion =
                    InvUnitConversionDAL
                        .GetInvUnitConversionByID(id);


            if (invUnitConversion == null)
                return Redirect(
                    Url.Action(
                        "Index",
                        "InvUnitConversion"
                    )
                );


            // -----------------------------------------------------
            // VIEW DATA
            // -----------------------------------------------------
            ViewBag.User = user;

            ViewBag.InvUnitConversion =
                invUnitConversion;


            return View();
        }


        // =========================================================
        // DELETE - POST
        // =========================================================
        [HttpPost]
        public ActionResult Delete(
            InvUnitConversionInfo invUnitConversion)
        {
            InvUnitConversionResponse response =
                new InvUnitConversionResponse();

            string lgUser =
                Session["LoggedInUser"]?.ToString();


            // -----------------------------------------------------
            // LOGIN CHECK
            // -----------------------------------------------------
            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Not Logged In";

                return Json(response);
            }


            // -----------------------------------------------------
            // USER CHECK
            // -----------------------------------------------------
            UserInfo user =
                UserDAL.GetUserByUserCode(lgUser);

            if (user == null)
            {
                response.ResponseStatus = -1;
                response.ResponseCode =
                    "Invalid User";

                return Json(response);
            }


            // -----------------------------------------------------
            // PERMISSION CHECK
            // -----------------------------------------------------
            if (user.PermissionGroup[3] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode =
                    "Not Authorized";

                return Json(response);
            }


            // -----------------------------------------------------
            // DELETE
            // -----------------------------------------------------
            try
            {
                InvUnitConversionDAL
                    .DeleteInvUnitConversion(
                        invUnitConversion,
                        lgUser
                    );


                response.ResponseStatus = 1;
                response.ResponseCode =
                    "Success";

                response.ResponseMessage =
                    "Unit conversion deleted successfully";
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode =
                    "Database Error";

                response.ResponseMessage =
                    ex.Message;
            }


            return Json(response);
        }
    }
}