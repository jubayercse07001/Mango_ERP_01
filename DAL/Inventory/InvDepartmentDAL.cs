using ERP.Models.Inventory.InvDepartment;
using ERP.Models.Inventory.InvClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;


namespace ERP.DAL.Inventory
{
    public class InvDepartmentDAL
    {
        // ============================================================
        // Get Department By RowId
        // ============================================================
        public static InvDepartmentInfo GetDepartmentByID(string RowId)
        {
            InvDepartmentInfo department = null;

            using (var repo = new GMContext())
            {
                department = repo.Database.SqlQuery<InvDepartmentInfo>(@"
                    SELECT
                        RowId,
                        DepartmentId,
                        DepartmentName,
                        ShortDeptName
                    FROM InvDepartment
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return department;
        }


        // ============================================================
        // Get Department By Name
        // Used for duplicate DepartmentName validation
        // ============================================================
        public static InvDepartmentInfo GetInvDepartmentByName(
            string DepartmentName,
            string RowId)
        {
            InvDepartmentInfo department = null;

            using (var repo = new GMContext())
            {
                department = repo.Database.SqlQuery<InvDepartmentInfo>(@"
                    SELECT
                        RowId,
                        DepartmentId,
                        DepartmentName,
                        ShortDeptName
                    FROM InvDepartment
                    WHERE DepartmentName = @DepartmentName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@DepartmentName", DepartmentName),
                new SqlParameter(
                    "@RowId",
                    (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return department;
        }


        // ============================================================
        // Get All Departments
        // ============================================================
        public static List<InvDepartmentInfo> GetAllInvDepartments()
        {
            List<InvDepartmentInfo> departments = null;

            using (var repo = new GMContext())
            {
                departments = repo.Database.SqlQuery<InvDepartmentInfo>(@"
                    SELECT
                        RowId,
                        DepartmentId,
                        DepartmentName,
                        ShortDeptName
                    FROM InvDepartment
                    WHERE DomainStatus = 1
                    ORDER BY DepartmentId
                ").ToList();
            }

            return departments;
        }


        // ============================================================
        // Add Department
        // ============================================================
        public static void AddInvDepartment(
            InvDepartmentInfo department,
            string loggedInUser)
        {
            InvDepartmentInfo obj;

            using (var repo = new GMContext())
            {
                // Generate DepartmentId
                obj = repo.Database.SqlQuery<InvDepartmentInfo>(@"
                    SELECT
                        ISNULL(MAX(DepartmentId), 0) + 1 AS DepartmentId
                    FROM InvDepartment
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartment
                    (
                        RowId,
                        DepartmentId,
                        DepartmentName,
                        ShortDeptName,
                        DomainStatus,
                        CreateUser,
                        CreateDate
                    )
                    VALUES
                    (
                        @RowId,
                        @DepartmentId,
                        @DepartmentName,
                        @ShortDeptName,
                        1,
                        @CreateUser,
                        GETDATE()
                    )
                ",
                new SqlParameter("@RowId", department.RowId),
                new SqlParameter(
                    "@DepartmentId",
                    obj.DepartmentId),
                new SqlParameter(
                    "@DepartmentName",
                    department.DepartmentName ?? ""),
                new SqlParameter(
                    "@ShortDeptName",
                    department.ShortDeptName ?? ""),
                new SqlParameter(
                    "@CreateUser",
                    loggedInUser));
            }
        }


        // ============================================================
        // Update Department
        // ============================================================
        public static void UpdateInvDepartment(
            InvDepartmentInfo department,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartment
                    SET
                        DepartmentName = @DepartmentName,
                        ShortDeptName = @ShortDeptName,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter(
                    "@RowId",
                    department.RowId),
                new SqlParameter(
                    "@DepartmentName",
                    department.DepartmentName ?? ""),
                new SqlParameter(
                    "@ShortDeptName",
                    department.ShortDeptName ?? ""),
                new SqlParameter(
                    "@UpdateUser",
                    loggedInUser));
            }
        }


        // ============================================================
        // Delete Department
        // Soft Delete
        // ============================================================
        public static void DeleteInvDepartment(
            InvDepartmentInfo department,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartment
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter(
                    "@RowId",
                    department.RowId),
                new SqlParameter(
                    "@UpdateUser",
                    loggedInUser));
            }
        }


        // ============================================================
        // DataTables Server-Side Filtering
        // ============================================================
        public static InvDepartmentResponse GetFilteredInvDepartments(
            HttpRequestBase Request)
        {
            InvDepartmentResponse resp =
                new InvDepartmentResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            resp.InvDepartments = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvDepartment
                    WHERE DomainStatus = 1
                ");


                // ----------------------------------------------------
                // Total Record Count
                // ----------------------------------------------------
                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(@"
                        SELECT COUNT(*)
                        FROM InvDepartment
                        WHERE DomainStatus = 1
                    ").Single();


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
                            DepartmentName LIKE '%' + @SearchValue + '%'
                            OR ShortDeptName LIKE '%' + @SearchValue + '%'
                        )
                    ");

                    sqlParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }


                // ----------------------------------------------------
                // Filtered Record Count
                // ----------------------------------------------------
                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        string.Format(@"
                            SELECT COUNT(*)
                            {0}
                        ", sb.ToString()),
                        sqlParameters.ToArray()
                    ).Single();


                // ----------------------------------------------------
                // Parameters for Paging
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
                // Safe DataTables Sort Column
                // ----------------------------------------------------
                string sortColumn =
                    gmdt.SortColum.ToString();


                // Allow only valid columns
                switch (sortColumn)
                {
                    case "RowId":
                        sortColumn = "RowId";
                        break;

                    case "DepartmentId":
                        sortColumn = "DepartmentId";
                        break;

                    case "DepartmentName":
                        sortColumn = "DepartmentName";
                        break;

                    case "ShortDeptName":
                        sortColumn = "ShortDeptName";
                        break;

                    default:
                        sortColumn = "DepartmentId";
                        break;
                }


                // ----------------------------------------------------
                // Get Paged Records
                // ----------------------------------------------------
                resp.InvDepartments =
                    repo.Database.SqlQuery<InvDepartmentInfo>(
                        string.Format(@"
                            SELECT
                                RowId,
                                DepartmentId,
                                DepartmentName,
                                ShortDeptName

                            {0}

                            ORDER BY {1} {2}

                            OFFSET (@Skip) ROWS
                            FETCH NEXT (@Take) ROWS ONLY
                        ",
                        sb.ToString(),
                        sortColumn,
                        gmdt.SortDir),
                        sqlParameters1.ToArray()
                    ).ToList();
            }

            return resp;
        }


        // ============================================================
        // Response Class For DataTables
        // ============================================================
        public class InvDepartmentResponse
        {
            public int TotalRecordCount { get; set; }

            public int FilteredRecordCount { get; set; }

            public List<InvDepartmentInfo> InvDepartments
            {
                get;
                set;
            }
        }
    }
}