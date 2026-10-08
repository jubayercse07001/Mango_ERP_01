using ERP.Models.Inventory.InvRefferenceInformation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvRefferenceInformationDAL
    {
        // ============================================================
        // GET BY ROW ID
        // ============================================================
        public static InvRefferenceInformationInfo
            GetInvRefferenceInformationByID(string RowId)
        {
            InvRefferenceInformationInfo obj = null;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvRefferenceInformationInfo>(@"
                    SELECT
                        RowId,
                        RefferenceId,
                        RefferenceName
                    FROM InvRefferenceInformation
                    WHERE RowId = @RowId
                      AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return obj;
        }


        // ============================================================
        // GET BY NAME
        // ============================================================
        public static InvRefferenceInformationInfo
            GetInvRefferenceInformationByName(
                string RefferenceName,
                string RowId)
        {
            InvRefferenceInformationInfo obj = null;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvRefferenceInformationInfo>(@"
                    SELECT
                        RowId,
                        RefferenceId,
                        RefferenceName
                    FROM InvRefferenceInformation
                    WHERE RefferenceName = @RefferenceName
                      AND (@RowId IS NULL OR RowId <> @RowId)
                      AND DomainStatus = 1
                ",
                new SqlParameter(
                    "@RefferenceName",
                    RefferenceName),

                new SqlParameter(
                    "@RowId",
                    (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return obj;
        }


        // ============================================================
        // GET ALL
        // ============================================================
        public static List<InvRefferenceInformationInfo>
            GetAllInvRefferenceInformations()
        {
            List<InvRefferenceInformationInfo> list = null;

            using (var repo = new GMContext())
            {
                list = repo.Database
                    .SqlQuery<InvRefferenceInformationInfo>(@"
                        SELECT
                            RowId,
                            RefferenceId,
                            RefferenceName
                        FROM InvRefferenceInformation
                        WHERE DomainStatus = 1
                        ORDER BY RefferenceId
                    ")
                    .ToList();
            }

            return list;
        }


        // ============================================================
        // ADD
        // ============================================================
        public static void AddInvRefferenceInformation(
            InvRefferenceInformationInfo obj,
            string loggedInUser)
        {
            InvRefferenceInformationInfo newObj;

            using (var repo = new GMContext())
            {
                // Generate next RefferenceId
                newObj = repo.Database
                    .SqlQuery<InvRefferenceInformationInfo>(@"
                        SELECT
                            ISNULL(MAX(RefferenceId), 0) + 1
                            AS RefferenceId
                        FROM InvRefferenceInformation
                        WHERE DomainStatus = 1
                    ")
                    .FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvRefferenceInformation
                    (
                        RowId,
                        RefferenceId,
                        RefferenceName,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @RefferenceId,
                        @RefferenceName,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter(
                    "@RowId",
                    obj.RowId),

                new SqlParameter(
                    "@RefferenceId",
                    newObj.RefferenceId),

                new SqlParameter(
                    "@RefferenceName",
                    obj.RefferenceName ?? ""),

                new SqlParameter(
                    "@CreateUser",
                    loggedInUser));
            }
        }


        // ============================================================
        // UPDATE
        // ============================================================
        public static void UpdateInvRefferenceInformation(
            InvRefferenceInformationInfo obj,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvRefferenceInformation
                    SET
                        RefferenceName = @RefferenceName,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter(
                    "@RowId",
                    obj.RowId),

                new SqlParameter(
                    "@RefferenceName",
                    obj.RefferenceName ?? ""),

                new SqlParameter(
                    "@UpdateUser",
                    loggedInUser));
            }
        }


        // ============================================================
        // DELETE - SOFT DELETE
        // ============================================================
        public static void DeleteInvRefferenceInformation(
            InvRefferenceInformationInfo obj,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvRefferenceInformation
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter(
                    "@RowId",
                    obj.RowId),

                new SqlParameter(
                    "@UpdateUser",
                    loggedInUser));
            }
        }


        // ============================================================
        // FILTERED LIST - DATATABLE
        // ============================================================
        public static InvRefferenceInformationResponse
            GetFilteredInvRefferenceInformations(
                HttpRequestBase Request)
        {
            InvRefferenceInformationResponse resp =
                new InvRefferenceInformationResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            resp.InvRefferenceInformations = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvRefferenceInformation a
                    WHERE a.DomainStatus = 1
                ");

                // ----------------------------------------------------
                // Total records
                // ----------------------------------------------------
                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(@"
                        SELECT COUNT(*)
                        " + sb.ToString())
                    .Single();


                List<SqlParameter> sqlParameters =
                    new List<SqlParameter>();

                List<SqlParameter> sqlParameters1 =
                    new List<SqlParameter>();


                // ----------------------------------------------------
                // Search
                // ----------------------------------------------------
                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                            a.RefferenceName
                                LIKE '%' + @SearchValue + '%'
                        )
                    ");

                    sqlParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }


                // ----------------------------------------------------
                // Filtered records
                // ----------------------------------------------------
                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(@"
                        SELECT COUNT(*)
                        " + sb.ToString(),
                        sqlParameters.ToArray())
                    .Single();


                // ----------------------------------------------------
                // Parameters for paging
                // ----------------------------------------------------
                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(
                    new SqlParameter(
                        "@Skip",
                        gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter(
                        "@Take",
                        gmdt.Length));


                // ----------------------------------------------------
                // Protect dynamic ORDER BY column
                // ----------------------------------------------------
                string sortColumn =
                    gmdt.SortColum.ToString();

                switch (sortColumn)
                {
                    case "RowId":
                        sortColumn = "a.RowId";
                        break;

                    case "RefferenceId":
                        sortColumn = "a.RefferenceId";
                        break;

                    case "RefferenceName":
                        sortColumn = "a.RefferenceName";
                        break;

                    default:
                        sortColumn = "a.RefferenceId";
                        break;
                }


                // ----------------------------------------------------
                // Protect sort direction
                // ----------------------------------------------------
                string sortDirection =
                    gmdt.SortDir.ToString()
                        .ToUpper();

                if (sortDirection != "ASC" &&
                    sortDirection != "DESC")
                {
                    sortDirection = "ASC";
                }


                // ----------------------------------------------------
                // Final DataTables query
                // ----------------------------------------------------
                resp.InvRefferenceInformations =
                    repo.Database.SqlQuery
                    <InvRefferenceInformationInfo>(@"
                        SELECT
                            a.RowId,
                            a.RefferenceId,
                            a.RefferenceName

                        " + sb.ToString() + @"

                        ORDER BY " +
                        sortColumn + " " +
                        sortDirection + @"

                        OFFSET (@Skip) ROWS
                        FETCH NEXT (@Take) ROWS ONLY
                    ",
                    sqlParameters1.ToArray())
                    .ToList();
            }

            return resp;
        }


        // ============================================================
        // RESPONSE CLASS
        // ============================================================
        public class InvRefferenceInformationResponse
        {
            public int TotalRecordCount { get; set; }

            public int FilteredRecordCount { get; set; }

            public List<InvRefferenceInformationInfo>
                InvRefferenceInformations
            { get; set; }

            public int ResponseStatus { get; set; }

            public string ResponseCode { get; set; }

            public string ResponseMessage { get; set; }

            public List<string> ResponseMessages { get; set; }

            public InvRefferenceInformationResponse()
            {
                ResponseMessages =
                    new List<string>();

                InvRefferenceInformations =
                    new List<InvRefferenceInformationInfo>();
            }
        }
    }
}