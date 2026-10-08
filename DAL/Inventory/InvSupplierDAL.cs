using ERP.Models.Inventory.InvSupplier;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvSupplierDAL
    {
        public static InvSupplierInfo GetInvSupplierByID(string RowId)
        {
            InvSupplierInfo supplier = null;

            using (var repo = new GMContext())
            {
                supplier = repo.Database.SqlQuery<InvSupplierInfo>(@"
                    SELECT
                         a.RowId
                        ,a.SupplierId
                        ,a.SupplierName
                        ,a.ContactPerson
                        ,a.ContactDesignation
                        ,a.MailingAddress
                        ,a.BillingAddress
                        ,a.OfficePhone
                        ,a.Mobile
                        ,a.Email
                        ,a.Web
                        ,a.ItemId
                        ,a.DepartmentId
                        ,HD.DepartmentName
                        ,a.IsForeignSupplier
                        ,a.ComissionPercent
                        ,a.CountryId
                        ,HC.CountryName
                    FROM InvSupplier a
                    inner join HrCountry HC on HC.RowId = a.CountryId and HC.DomainStatus = 1
                    inner join HrDepartment HD on HD.RowId = a.DepartmentId and HD.DomainStatus = 1
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return supplier;
        }


        public static InvSupplierInfo GetInvSupplierByName(
            string SupplierName,
            string RowId)
        {
            InvSupplierInfo supplier = null;

            using (var repo = new GMContext())
            {
                supplier = repo.Database.SqlQuery<InvSupplierInfo>(@"
                    SELECT
                         RowId
                        ,SupplierId
                        ,SupplierName
                        ,ContactPerson
                        ,ContactDesignation
                        ,MailingAddress
                        ,BillingAddress
                        ,OfficePhone
                        ,Mobile
                        ,Email
                        ,Web
                        ,ItemId
                        ,DepartmentId
                        ,IsForeignSupplier
                        ,ComissionPercent
                        ,CountryId
                    FROM InvSupplier
                    WHERE SupplierName = @SupplierName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@SupplierName", SupplierName),
                new SqlParameter(
                    "@RowId",
                    (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return supplier;
        }


        public static List<InvSupplierInfo> GetAllInvSuppliers()
        {
            List<InvSupplierInfo> suppliers = null;

            using (var repo = new GMContext())
            {
                suppliers =
                    repo.Database.SqlQuery<InvSupplierInfo>(@"
                        SELECT
                             RowId
                            ,SupplierId
                            ,SupplierName
                            ,ContactPerson
                            ,ContactDesignation
                            ,MailingAddress
                            ,BillingAddress
                            ,OfficePhone
                            ,Mobile
                            ,Email
                            ,Web
                            ,ItemId
                            ,DepartmentId
                            ,IsForeignSupplier
                            ,ComissionPercent
                            ,CountryId
                        FROM InvSupplier
                        WHERE DomainStatus = 1
                        ORDER BY SupplierId
                    ")
                    .ToList();
            }

            return suppliers;
        }


        public static void AddInvSupplier(
            InvSupplierInfo supplier,
            string loggedInUser)
        {
            InvSupplierInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvSupplierInfo>(@"
                    SELECT
                        ISNULL(MAX(SupplierId), 0) + 1 AS SupplierId
                    FROM InvSupplier
                    WHERE DomainStatus = 1
                ")
                .FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvSupplier
                    (
                         RowId
                        ,SupplierId
                        ,SupplierName
                        ,ContactPerson
                        ,ContactDesignation
                        ,MailingAddress
                        ,BillingAddress
                        ,OfficePhone
                        ,Mobile
                        ,Email
                        ,Web
                        ,ItemId
                        ,DepartmentId
                        ,IsForeignSupplier
                        ,ComissionPercent
                        ,CountryId
                        ,DomainStatus
                        ,CreateUser
                        ,CreateDate
                    )
                    VALUES
                    (
                         @RowId
                        ,@SupplierId
                        ,@SupplierName
                        ,@ContactPerson
                        ,@ContactDesignation
                        ,@MailingAddress
                        ,@BillingAddress
                        ,@OfficePhone
                        ,@Mobile
                        ,@Email
                        ,@Web
                        ,@ItemId
                        ,@DepartmentId
                        ,@IsForeignSupplier
                        ,@ComissionPercent
                        ,@CountryId
                        ,1
                        ,@CreateUser
                        ,GETDATE()
                    )
                ",
                new SqlParameter("@RowId", supplier.RowId),
                new SqlParameter("@SupplierId", obj.SupplierId),
                new SqlParameter("@SupplierName",
                    supplier.SupplierName ?? ""),
                new SqlParameter("@ContactPerson",
                    supplier.ContactPerson ?? ""),
                new SqlParameter("@ContactDesignation",
                    (object)supplier.ContactDesignation ?? DBNull.Value),
                new SqlParameter("@MailingAddress",
                    supplier.MailingAddress ?? ""),
                new SqlParameter("@BillingAddress",
                    supplier.BillingAddress ?? ""),
                new SqlParameter("@OfficePhone",
                    supplier.OfficePhone ?? ""),
                new SqlParameter("@Mobile",
                    supplier.Mobile ?? ""),
                new SqlParameter("@Email",
                    supplier.Email ?? ""),
                new SqlParameter("@Web",
                    supplier.Web ?? ""),
                new SqlParameter("@ItemId",
                    supplier.ItemId ?? ""),
                new SqlParameter("@DepartmentId",
                    supplier.DepartmentId ?? ""),
                new SqlParameter("@IsForeignSupplier",
                    supplier.IsForeignSupplier),
                new SqlParameter("@ComissionPercent",
                    (object)supplier.ComissionPercent ?? DBNull.Value),
                new SqlParameter("@CountryId",
                    supplier.CountryId ?? ""),
                new SqlParameter("@CreateUser",
                    loggedInUser));
            }
        }


        public static void UpdateInvSupplier(
            InvSupplierInfo supplier,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvSupplier
                    SET
                         SupplierName = @SupplierName
                        ,ContactPerson = @ContactPerson
                        ,ContactDesignation = @ContactDesignation
                        ,MailingAddress = @MailingAddress
                        ,BillingAddress = @BillingAddress
                        ,OfficePhone = @OfficePhone
                        ,Mobile = @Mobile
                        ,Email = @Email
                        ,Web = @Web
                        ,ItemId = @ItemId
                        ,DepartmentId = @DepartmentId
                        ,IsForeignSupplier =
                            @IsForeignSupplier
                        ,ComissionPercent = @ComissionPercent
                        ,CountryId = @CountryId
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", supplier.RowId),
                new SqlParameter("@SupplierName",
                    supplier.SupplierName ?? ""),
                new SqlParameter("@ContactPerson",
                    supplier.ContactPerson ?? ""),
                new SqlParameter("@ContactDesignation",
                    (object)supplier.ContactDesignation ?? DBNull.Value),
                new SqlParameter("@MailingAddress",
                    supplier.MailingAddress ?? ""),
                new SqlParameter("@BillingAddress",
                    supplier.BillingAddress ?? ""),
                new SqlParameter("@OfficePhone",
                    supplier.OfficePhone ?? ""),
                new SqlParameter("@Mobile",
                    supplier.Mobile ?? ""),
                new SqlParameter("@Email",
                    supplier.Email ?? ""),
                new SqlParameter("@Web",
                    supplier.Web ?? ""),
                new SqlParameter("@ItemId",
                    supplier.ItemId ?? ""),
                new SqlParameter("@DepartmentId",
                    supplier.DepartmentId ?? ""),
                new SqlParameter("@IsForeignSupplier",
                    supplier.IsForeignSupplier),
                new SqlParameter("@ComissionPercent",
                    (object)supplier.ComissionPercent ?? DBNull.Value),
                new SqlParameter("@CountryId",
                    supplier.CountryId ?? ""),
                new SqlParameter("@UpdateUser",
                    loggedInUser));
            }
        }


        public static void DeleteInvSupplier(
            InvSupplierInfo supplier,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvSupplier
                    SET
                         DomainStatus = 0
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", supplier.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }


        // =========================================================
        // DataTables Filter
        // =========================================================
        public static InvSupplierResponse GetFilteredInvSuppliers(
            HttpRequestBase Request)
        {
            InvSupplierResponse resp =
                new InvSupplierResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb =
                    new StringBuilder(@"
                        FROM InvSupplier a
                        inner join HrCountry HC on HC.RowId = a.CountryId and HC.DomainStatus = 1 
                        WHERE a.DomainStatus = 1
                    ");

                // -------------------------------------------------
                // Total Records
                // -------------------------------------------------
                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                        string.Format(@"
                            SELECT COUNT(*)
                            {0}
                        ",
                        sb.ToString()))
                    .Single();


                List<SqlParameter> sqlParameters =
                    new List<SqlParameter>();

                List<SqlParameter> sqlParameters1 =
                    new List<SqlParameter>();


                // -------------------------------------------------
                // Global Search
                // -------------------------------------------------
                if (!string.IsNullOrEmpty(
                    gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                               a.SupplierName LIKE '%' + @SearchValue + '%'
                            OR a.ContactPerson LIKE '%' + @SearchValue + '%'
                            OR a.ContactDesignation LIKE '%' + @SearchValue + '%'
                            OR a.Mobile LIKE '%' + @SearchValue + '%'
                            OR a.Email LIKE '%' + @SearchValue + '%'
                            OR a.OfficePhone LIKE '%' + @SearchValue + '%'
                            OR a.Web LIKE '%' + @SearchValue + '%'
                        )
                    ");

                    sqlParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }


                // -------------------------------------------------
                // Filtered Records
                // -------------------------------------------------
                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        string.Format(@"
                            SELECT COUNT(*)
                            {0}
                        ",
                        sb.ToString()),
                        sqlParameters.ToArray())
                    .Single();


                sqlParameters1.AddRange(
                    sqlParameters);


                // -------------------------------------------------
                // Paging
                // -------------------------------------------------
                sqlParameters1.Add(
                    new SqlParameter(
                        "@Skip",
                        gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter(
                        "@Take",
                        gmdt.Length));


                // -------------------------------------------------
                // Safe Sorting
                // -------------------------------------------------
                string sortColumn =
                    Convert.ToString(gmdt.SortColum);

                switch (sortColumn)
                {
                    case "SupplierId":
                        sortColumn = "a.SupplierId";
                        break;

                    case "SupplierName":
                        sortColumn = "a.SupplierName";
                        break;

                    case "ContactPerson":
                        sortColumn = "a.ContactPerson";
                        break;

                    case "Mobile":
                        sortColumn = "a.Mobile";
                        break;

                    case "Email":
                        sortColumn = "a.Email";
                        break;

                    case "ComissionPercent":
                        sortColumn = "a.ComissionPercent";
                        break;

                    case "IsForeignSupplier":
                        sortColumn = "a.IsForeignSupplier";
                        break;

                    case "RowId":
                        sortColumn = "a.RowId";
                        break;

                    default:
                        sortColumn = "a.SupplierId";
                        break;
                }


                // -------------------------------------------------
                // Safe Sort Direction
                // -------------------------------------------------
                string sortDir =
                    Convert.ToString(gmdt.SortDir)
                    .ToUpper();

                if (sortDir != "ASC" &&
                    sortDir != "DESC")
                {
                    sortDir = "ASC";
                }


                // -------------------------------------------------
                // Get Data
                // -------------------------------------------------
                resp.InvSuppliers =
                    repo.Database.SqlQuery<InvSupplierInfo>(
                        string.Format(@"
                            SELECT
                                 a.RowId
                                ,a.SupplierId
                                ,a.SupplierName
                                ,a.ContactPerson
                                -- ,a.ContactDesignation
                                -- ,a.MailingAddress
                                -- ,a.BillingAddress
                                -- ,a.OfficePhone
                                ,a.Mobile
                                ,a.Email
                                -- ,a.Web
                                -- ,a.ItemId
                                -- ,a.DepartmentId
                                ,a.IsForeignSupplier
                                ,a.ComissionPercent
                                ,HC.CountryName
                               --  ,CAST(NULL AS NVARCHAR(200)) AS CountryName
                            {0}

                            ORDER BY {1} {2}

                            OFFSET @Skip ROWS

                            FETCH NEXT @Take ROWS ONLY
                        ",
                        sb.ToString(),
                        sortColumn,
                        sortDir),
                        sqlParameters1.ToArray())
                    .ToList();
            }

            return resp;
        }




        public class InvSupplierResponse
        {
            public int TotalRecordCount { get; set; }

            public int FilteredRecordCount { get; set; }

            public List<InvSupplierInfo>
                InvSuppliers
            { get; set; }
        }
    }
}