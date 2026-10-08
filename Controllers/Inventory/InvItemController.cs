using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ERP.DAL.Inventory;
using ERP.Models.User;
using ERP.Models.Inventory;
using ERP.DAL;
using ERP.Models;
using ERP.DAL.HR.Common;

namespace ERP.Controllers.Inventory
{
    public class InvItemController : Controller
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

            // Check Permission (Index view)
            if (user.PermissionGroup[0] != '1')
                return Redirect(Url.Action("Index", "Home"));

            ViewBag.InvItems = InvItemDAL.GetAllInvItems();
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
                return Redirect(Url.Action("Index", "InvItem"));

            ViewBag.User = user;
            ViewBag.CategoryList = InvItemCategoryDAL.GetAllInvItemCategories();
            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();
            ViewBag.ClassList = InvClassDAL.GetAllInvClasses();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();
            ViewBag.FamilyList = InvFamilyDAL.GetAllInvFamilies();
            ViewBag.SubFamilyList = InvSubFamilyDAL.GetAllInvSubFamilies();
            ViewBag.ManufacturerList = InvManufacturerDAL.GetAllInvManufacturers();
            ViewBag.CountryList = HrCountryDAL.GetAllHrCountrys();

            return View();
        }

        [HttpPost]
        public ActionResult Add(InvItemInfo InvItem, HttpPostedFileBase ImageFile)
        {
            InvItemResponse response = new InvItemResponse();

            string lgUser = "";

            if (Session["LoggedInUser"] != null)
                lgUser = Session["LoggedInUser"].ToString();

            // =====================================================
            // LOGIN CHECK
            // =====================================================

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


            // =====================================================
            // PERMISSION CHECK
            // =====================================================

            if (user.PermissionGroup[1] != '1')
            {
                response.ResponseStatus = -2;
                response.ResponseCode = "Not Authorized";

                return Json(response);
            }


            


            // =====================================================
            // ITEM CODE VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(InvItem.ItemCode))
            {
                response.ResponseMessages.Add(
                    "Item Code is required");
            }
            else if (
                InvItemDAL.GetInvItemByCode(
                    InvItem.ItemCode,
                    null) != null)
            {
                response.ResponseMessages.Add(
                    "Item Code already exists");
            }


            // =====================================================
            // ITEM NAME VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(InvItem.ItemName))
            {
                response.ResponseMessages.Add(
                    "Item Name is required");
            }
            else if (
                InvItemDAL.GetInvItemByName(
                    InvItem.ItemName,
                    null) != null)
            {
                response.ResponseMessages.Add(
                    "Item Name already exists");
            }

            // =====================================================
            // CATEGORY VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(InvItem.CategoryId))
            {
                response.ResponseMessages.Add(
                    "Category is required");
            }
            // =====================================================
            // IMAGE VALIDATION
            // =====================================================

            if (ImageFile != null &&
                ImageFile.ContentLength > 0)
            {
                string extension =
                    Path.GetExtension(ImageFile.FileName)
                    .ToLower();

                string[] allowedExtensions =
                {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".webp"
        };

                if (!allowedExtensions.Contains(extension))
                {
                    response.ResponseMessages.Add(
                        "Only JPG, JPEG, PNG, GIF, BMP and WEBP images are allowed");
                }

                // 2 MB maximum
                if (ImageFile.ContentLength > 2 * 1024 * 1024)
                {
                    response.ResponseMessages.Add(
                        "Image size cannot exceed 2 MB");
                }
            }


            // =====================================================
            // VALIDATION RESPONSE
            // =====================================================

            if (response.ResponseMessages.Count > 0)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Invalid Data";

                response.ResponseMessage =
                    "<ul><li>" +
                    response.ResponseMessages.Aggregate(
                        (i, j) =>
                        i + "</li><li>" + j)
                    + "</li></ul>";

                return Json(response);
            }


            // =====================================================
            // SAVE
            // =====================================================

            try
            {
                InvItem.RowId =
                    Guid.NewGuid().ToString();


                // -------------------------------------------------
                // Convert uploaded image to byte[]
                // -------------------------------------------------

                if (ImageFile != null &&
                    ImageFile.ContentLength > 0)
                {
                    using (BinaryReader reader =
                           new BinaryReader(ImageFile.InputStream))
                    {
                        InvItem.ItemImage =
                            reader.ReadBytes(
                                ImageFile.ContentLength);
                    }
                }
                else
                {
                    InvItem.ItemImage = null;
                }


                // -------------------------------------------------
                // Add Item
                // -------------------------------------------------

                InvItemDAL.AddInvItem(
                    InvItem,
                    lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }


            // =====================================================
            // SUCCESS
            // =====================================================

            response.ResponseStatus = 1;
            response.ResponseCode = "Success";
            response.ResponseMessage =
                "Item added successfully";

            return Json(response);
        }
        // DataTables Server-Side AJAX Handler
        [HttpGet]
        public ActionResult GetInvItemList()
        {
            var response = InvItemDAL.GetFilteredInvItems(Request);

            return Json(new
            {
                draw = Convert.ToInt32(Request["draw"]),
                recordsTotal = response.TotalRecordCount,
                recordsFiltered = response.FilteredRecordCount,
                data = response.InvItems
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: InvItem/Edit/guid
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

            // Check Permission (Edit view)
            if (user.PermissionGroup[2] != '1')
                return Redirect(Url.Action("Index", "InvItem"));

            string rowId = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : "";
            InvItemInfo invItem = InvItemDAL.GetInvItemByID(rowId);

            if (invItem == null)
                return Redirect(Url.Action("Index", "InvItem"));

            ViewBag.User = user;
            ViewBag.InvItem = invItem;

            // Populate Dropdowns

            ViewBag.CategoryList = InvItemCategoryDAL.GetAllInvItemCategories();
            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();
            ViewBag.ClassList = InvClassDAL.GetAllInvClasses();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();
            ViewBag.FamilyList = InvFamilyDAL.GetAllInvFamilies();
            ViewBag.SubFamilyList = InvSubFamilyDAL.GetAllInvSubFamilies();
            ViewBag.ManufacturerList = InvManufacturerDAL.GetAllInvManufacturers();
            ViewBag.CountryList = HrCountryDAL.GetAllHrCountrys();

            return View();
        }

        // POST: InvItem/Edit
        [HttpPost]
        public ActionResult Edit(InvItemInfo invItem, HttpPostedFileBase itemImageFile)
        {
            InvItemResponse response = new InvItemResponse();

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

            // Validations
            if (string.IsNullOrWhiteSpace(invItem.ItemName))
                response.ResponseMessages.Add("Item Name is required");

            if (string.IsNullOrWhiteSpace(invItem.ItemCode))
                response.ResponseMessages.Add("Item Code is required");
            else if (InvItemDAL.GetInvItemByCode(invItem.ItemCode, invItem.RowId) != null)
                response.ResponseMessages.Add("Item Code already exists");

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
                // Handle new image upload, retain old byte array if no new file is uploaded
                if (itemImageFile != null && itemImageFile.ContentLength > 0)
                {
                    using (BinaryReader reader = new BinaryReader(itemImageFile.InputStream))
                    {
                        invItem.ItemImage = reader.ReadBytes(itemImageFile.ContentLength);
                    }
                }
                else
                {
                    InvItemInfo existingItem = InvItemDAL.GetInvItemByID(invItem.RowId);
                    if (existingItem != null)
                    {
                        invItem.ItemImage = existingItem.ItemImage;
                    }
                }

                InvItemDAL.UpdateInvItem(invItem, lgUser);
            }
            catch (Exception ex)
            {
                response.ResponseStatus = 0;
                response.ResponseCode = "Database Error";
                response.ResponseMessage = ex.Message;

                return Json(response);
            }

            response.ResponseStatus = 1;
            response.ResponseMessage = "Item updated successfully";

            return Json(response);
        }

        // GET: InvItem/Delete/guid
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

            // Check Permission (Delete view)
            if (user.PermissionGroup[3] != '1')
                return Redirect(Url.Action("Index", "InvItem"));

            string rowId = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : "";
            InvItemInfo invItem = InvItemDAL.GetInvItemByID(rowId);

            if (invItem == null)
                return Redirect(Url.Action("Index", "InvItem"));

            ViewBag.User = user;
            ViewBag.InvItem = invItem;
            ViewBag.CategoryList = InvItemCategoryDAL.GetAllInvItemCategories();
            ViewBag.GroupList = InvGroupDAL.GetAllInvGroups();
            ViewBag.ClassList = InvClassDAL.GetAllInvClasses();
            ViewBag.UnitList = InvUnitDAL.GetAllInvUnits();
            ViewBag.FamilyList = InvFamilyDAL.GetAllInvFamilies();
            ViewBag.SubFamilyList = InvSubFamilyDAL.GetAllInvSubFamilies();
            return View();
        }

        // POST: InvItem/Delete
        [HttpPost]
        public ActionResult Delete(InvItemInfo invItem)
        {
            InvItemResponse response = new InvItemResponse();

            string lgUser = Session["LoggedInUser"]?.ToString();

            if (string.IsNullOrEmpty(lgUser))
            {
                response.ResponseStatus = -1;
                response.ResponseMessage = "Not Logged In";
                return Json(response);
            }

            try
            {
                InvItemDAL.DeleteInvItem(invItem, lgUser);

                response.ResponseStatus = 1;
                response.ResponseMessage = "Item deleted successfully";
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