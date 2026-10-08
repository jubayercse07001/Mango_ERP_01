using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models;

namespace ERP.DAL.Inventory
{
    public class InvItemDAL
    {
        public static InvItemInfo GetInvItemByID(string RowId)
        {
            InvItemInfo item = null;

            using (var repo = new GMContext())
            {
                item = repo.Database.SqlQuery<InvItemInfo>(@"
                    SELECT
                        I.RowId,
                        I.ItemId,
                        I.CategoryId,
						IC.CategoryName,
                        I.ItemCode,
                        I.OldItemCode,
                        I.ItemName,
                        I.ItemDescription,
                        I.GroupId,
						IG.GroupName,
                        I.ClassId,
                        ICl.ClassName,
						I.FamilyId,
						IFa.FamilyName,
                        I.SubFamilyId,
						ISF.SubFamilyName,
                        I.SerialNumber,
                        I.ModelNumber,
                        I.MinimumReOrderLevel,
                        I.MaximumReOrderLevel,
                        I.CountryId,
						HC.CountryName,
                        I.MinimumUnitId,
						MU.MinUnitName,
                        I.RegisteredUnitId,
					    RU.MinUnitName RegisteredUniName,
                        I.SaleUnitId,
						SU.MinUnitName SaleUnitName,
                        I.PurchaseUnitId,
					    PU.MinUnitName PurchaseUnitName,
                        I.ManufacturerId,
						IM.ManufacturerName,
                        I.BatchNumber,
                        I.ItemImage,
                        I.IsStockItem
                    FROM InvItem I
					inner join InvItemCategory IC on IC.RowId = I.CategoryId and IC.DomainStatus = 1
					inner join InvGroup IG on IG.RowId = I.GroupId and IG.DomainStatus = 1
					inner join InvClass ICl on ICl.RowId = I.ClassId and ICl.DomainStatus = 1
					inner join InvFamily IFa on IFa.RowId = I.FamilyId and IFa.DomainStatus = 1
					inner join InvSubFamily ISF on ISF.RowId = I.SubFamilyId and ISF.DomainStatus = 1
					inner join HrCountry HC on HC.RowId = I.CountryId and HC.DomainStatus = 1
					inner join InvManufacturer IM on IM.RowId = I.ManufacturerId and IM.DomainStatus = 1
					inner join InvUnit MU on MU.RowId = I.MinimumUnitId and MU.DomainStatus = 1
					inner join InvUnit RU on RU.RowId = I.RegisteredUnitId and RU.DomainStatus = 1
					inner join InvUnit SU on SU.RowId = I.SaleUnitId and SU.DomainStatus = 1
					inner join InvUnit PU on PU.RowId = I.PurchaseUnitId and PU.DomainStatus = 1
                    WHERE I.RowId = @RowId
                    AND I.DomainStatus = 1
                ",
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return item;
        }

        public static InvItemInfo GetInvItemByCode(string ItemCode, string RowId)
        {
            InvItemInfo item = null;

            using (var repo = new GMContext())
            {
                item = repo.Database.SqlQuery<InvItemInfo>(@"
                    SELECT *
                    FROM InvItem
                    WHERE ItemCode = @ItemCode
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@ItemCode", ItemCode),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return item;
        }
        public static InvItemInfo GetInvItemByName(string ItemName, string RowId)
        {
            InvItemInfo item = null;

            using (var repo = new GMContext())
            {
                item = repo.Database.SqlQuery<InvItemInfo>(@"
                    SELECT *
                    FROM InvItem
                    WHERE ItemName = @ItemName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@ItemName", ItemName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return item;
        }
        public static List<InvItemInfo> GetAllInvItems()
        {
            List<InvItemInfo> items = null;

            using (var repo = new GMContext())
            {
                items = repo.Database.SqlQuery<InvItemInfo>(@"
                    SELECT
                        RowId,
                        ItemId,
                        CategoryId,
                        ItemCode,
                        OldItemCode,
                        ItemName,
                        ItemDescription,
                        GroupId,
                        ClassId,
                        FamilyId,
                        SubFamilyId,
                        SerialNumber,
                        ModelNumber,
                        MinimumReOrderLevel,
                        MaximumReOrderLevel,
                        CountryId,
                        MinimumUnitId,
                        RegisteredUnitId,
                        SaleUnitId,
                        PurchaseUnitId,
                        ManufacturerId,
                        BatchNumber,
                        ItemImage,
                        IsStockItem
                    FROM InvItem
                    WHERE DomainStatus = 1
                    ORDER BY ItemId
                ").ToList();
            }

            return items;
        }

        public static void AddInvItem(InvItemInfo item, string loggedInUser)
        {
            InvItemInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvItemInfo>(@"
                    SELECT
                        ISNULL(MAX(ItemId), 0) + 1 AS ItemId
                    FROM InvItem
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvItem
                    (
                        RowId,
                        ItemId,
                        CategoryId,
                        ItemCode,
                        OldItemCode,
                        ItemName,
                        ItemDescription,
                        GroupId,
                        ClassId,
                        FamilyId,
                        SubFamilyId,
                        SerialNumber,
                        ModelNumber,
                        MinimumReOrderLevel,
                        MaximumReOrderLevel,
                        CountryId,
                        MinimumUnitId,
                        RegisteredUnitId,
                        SaleUnitId,
                        PurchaseUnitId,
                        ManufacturerId,
                        BatchNumber,
                        ItemImage,
                        IsStockItem,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @ItemId,
                        @CategoryId,
                        @ItemCode,
                        @OldItemCode,
                        @ItemName,
                        @ItemDescription,
                        @GroupId,
                        @ClassId,
                        @FamilyId,
                        @SubFamilyId,
                        @SerialNumber,
                        @ModelNumber,
                        @MinimumReOrderLevel,
                        @MaximumReOrderLevel,
                        @CountryId,
                        @MinimumUnitId,
                        @RegisteredUnitId,
                        @SaleUnitId,
                        @PurchaseUnitId,
                        @ManufacturerId,
                        @BatchNumber,
                        @ItemImage,
                        @IsStockItem,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", item.RowId),
                new SqlParameter("@ItemId", obj.ItemId),
                new SqlParameter("@CategoryId", item.CategoryId),
                new SqlParameter("@ItemCode", item.ItemCode ?? ""),
                new SqlParameter("@OldItemCode", item.OldItemCode ?? ""),
                new SqlParameter("@ItemName", item.ItemName ?? ""),
                new SqlParameter("@ItemDescription", item.ItemDescription ?? ""),
                new SqlParameter("@GroupId", item.GroupId ?? ""),
                new SqlParameter("@ClassId", item.ClassId ?? ""),
                new SqlParameter("@FamilyId", item.FamilyId ?? ""),
                new SqlParameter("@SubFamilyId", item.SubFamilyId ?? ""),
                new SqlParameter("@SerialNumber", item.SerialNumber ?? ""),
                new SqlParameter("@ModelNumber", item.ModelNumber ?? ""),
                new SqlParameter("@MinimumReOrderLevel", item.MinimumReOrderLevel),
                new SqlParameter("@MaximumReOrderLevel", item.MaximumReOrderLevel),
                new SqlParameter("@CountryId", item.CountryId ?? ""),
                new SqlParameter("@MinimumUnitId", item.MinimumUnitId),
                new SqlParameter("@RegisteredUnitId", item.RegisteredUnitId ?? ""),
                new SqlParameter("@SaleUnitId", item.SaleUnitId ?? ""),
                new SqlParameter("@PurchaseUnitId", item.PurchaseUnitId ?? ""),
                new SqlParameter("@ManufacturerId", item.ManufacturerId ?? ""),
                new SqlParameter("@BatchNumber", item.BatchNumber ?? ""),
                new SqlParameter("@ItemImage", (object)item.ItemImage ?? DBNull.Value),
                new SqlParameter("@IsStockItem", item.IsStockItem),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvItem(InvItemInfo item, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvItem
                    SET
                        CategoryId = @CategoryId,
                        ItemCode = @ItemCode,
                        OldItemCode = @OldItemCode,
                        ItemName = @ItemName,
                        ItemDescription = @ItemDescription,
                        GroupId = @GroupId,
                        ClassId = @ClassId,
                        FamilyId = @FamilyId,
                        SubFamilyId = @SubFamilyId,
                        SerialNumber = @SerialNumber,
                        ModelNumber = @ModelNumber,
                        MinimumReOrderLevel = @MinimumReOrderLevel,
                        MaximumReOrderLevel = @MaximumReOrderLevel,
                        CountryId = @CountryId,
                        MinimumUnitId = @MinimumUnitId,
                        RegisteredUnitId = @RegisteredUnitId,
                        SaleUnitId = @SaleUnitId,
                        PurchaseUnitId = @PurchaseUnitId,
                        ManufacturerId = @ManufacturerId,
                        BatchNumber = @BatchNumber,
                        ItemImage = @ItemImage,
                        IsStockItem = @IsStockItem,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", item.RowId),
                new SqlParameter("@CategoryId", item.CategoryId),
                new SqlParameter("@ItemCode", item.ItemCode ?? ""),
                new SqlParameter("@OldItemCode", item.OldItemCode ?? ""),
                new SqlParameter("@ItemName", item.ItemName ?? ""),
                new SqlParameter("@ItemDescription", item.ItemDescription ?? ""),
                new SqlParameter("@GroupId", item.GroupId ?? ""),
                new SqlParameter("@ClassId", item.ClassId ?? ""),
                new SqlParameter("@FamilyId", item.FamilyId ?? ""),
                new SqlParameter("@SubFamilyId", item.SubFamilyId ?? ""),
                new SqlParameter("@SerialNumber", item.SerialNumber ?? ""),
                new SqlParameter("@ModelNumber", item.ModelNumber ?? ""),
                new SqlParameter("@MinimumReOrderLevel", item.MinimumReOrderLevel),
                new SqlParameter("@MaximumReOrderLevel", item.MaximumReOrderLevel),
                new SqlParameter("@CountryId", item.CountryId ?? ""),
                new SqlParameter("@MinimumUnitId", item.MinimumUnitId),
                new SqlParameter("@RegisteredUnitId", item.RegisteredUnitId ?? ""),
                new SqlParameter("@SaleUnitId", item.SaleUnitId ?? ""),
                new SqlParameter("@PurchaseUnitId", item.PurchaseUnitId ?? ""),
                new SqlParameter("@ManufacturerId", item.ManufacturerId ?? ""),
                new SqlParameter("@BatchNumber", item.BatchNumber ?? ""),
                new SqlParameter("@ItemImage", (object)item.ItemImage ?? DBNull.Value),
                new SqlParameter("@IsStockItem", item.IsStockItem),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvItem(InvItemInfo item, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvItem
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", item.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvItemResponse GetFilteredInvItems(HttpRequestBase Request)
        {
            InvItemResponse resp = new InvItemResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvItems = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvItem a
                    WHERE a.DomainStatus = 1
                ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                            a.ItemCode LIKE '%' + @SearchValue + '%'
                            OR a.OldItemCode LIKE '%' + @SearchValue + '%'
                            OR a.ItemName LIKE '%' + @SearchValue + '%'
                            OR a.ItemDescription LIKE '%' + @SearchValue + '%'
                            OR a.ModelNumber LIKE '%' + @SearchValue + '%'
                            OR a.SerialNumber LIKE '%' + @SearchValue + '%'
                            OR a.BatchNumber LIKE '%' + @SearchValue + '%'
                        )");

                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString()), sqlParameters.ToArray()).Single();

                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                string sortColumn = gmdt.SortColum.ToString();

                // Explicit alias safety guard for DataTables sorting
                if (sortColumn == "RowId" || sortColumn == "ItemId" || sortColumn == "ItemCode" || sortColumn == "ItemName")
                {
                    sortColumn = "a." + sortColumn;
                }

                resp.InvItems = repo.Database.SqlQuery<InvItemInfo>(string.Format(@"
                    SELECT
                        a.RowId,
                        a.ItemId,
                        a.CategoryId,
                        a.ItemCode,
                        a.OldItemCode,
                        a.ItemName,
                        a.ItemDescription,
                        a.GroupId,
                        a.ClassId,
                        a.FamilyId,
                        a.SubFamilyId,
                        a.SerialNumber,
                        a.ModelNumber,
                        a.MinimumReOrderLevel,
                        a.MaximumReOrderLevel,
                        a.CountryId,
                        a.MinimumUnitId,
                        a.RegisteredUnitId,
                        a.SaleUnitId,
                        a.PurchaseUnitId,
                        a.ManufacturerId,
                        a.BatchNumber,
                        a.ItemImage,
                        a.IsStockItem
                    {0}
                    ORDER BY {1} {2}
                    OFFSET (@Skip) ROWS
                    FETCH NEXT (@Take) ROWS ONLY
                ",
                sb.ToString(),
                sortColumn,
                gmdt.SortDir),
                sqlParameters1.ToArray()).ToList();
            }

            return resp;
        }
    }

    public class InvItemResponse
    {
        public int TotalRecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
        public int ResponseStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<string> ResponseMessages { get; set; } = new List<string>();
        public List<InvItemInfo> InvItems { get; set; }
    }
}