using ERP.Models.HR.Organization.HrDepartment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrDepartmentDAL
    {
        public static HrDepartmentInfo GetDepartmentByID(string RowId)
        {
            HrDepartmentInfo department = null;

            using (var repo = new GMContext())
            {
                department = repo.Database.SqlQuery<HrDepartmentInfo>(@"
                    SELECT
                        a.RowId,
                        a.DepartmentId,
                        a.DepartmentName,
                        a.ShortDeptName,
                        a.DepartmentGroupId,
                        b.DepartmentGroupName,
                        a.BranchId,
                        c.BranchName,
                        a.DivisionId,
                        d.DivisionName,
                        a.Rank
                    FROM HrDepartment a
                    INNER JOIN HrDepartmentGroup b
                        ON b.RowId = a.DepartmentGroupId
                    INNER JOIN HrBranch c
                        ON c.RowId = a.BranchId
                    INNER JOIN HrDivision d
                        ON d.RowId = a.DivisionId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return department;
        }

        public static HrDepartmentInfo GetHrDepartmentByName(
            string DepartmentName,
            string RowId)
        {
            HrDepartmentInfo department = null;

            using (var repo = new GMContext())
            {
                department = repo.Database.SqlQuery<HrDepartmentInfo>(@"
                    SELECT *
                    FROM HrDepartment
                    WHERE DepartmentName = @DepartmentName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@DepartmentName", DepartmentName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return department;
        }

        public static List<HrDepartmentInfo> GetAllHrDepartments()
        {
            List<HrDepartmentInfo> departments = null;

            using (var repo = new GMContext())
            {
                departments = repo.Database.SqlQuery<HrDepartmentInfo>(@"
                    SELECT
                        RowId,
                        DepartmentId,
                        DepartmentName,
                        ShortDeptName,
                        DepartmentGroupId,
                        BranchId,
                        DivisionId,
                        Rank
                    FROM HrDepartment
                    WHERE DomainStatus = 1
                    ORDER BY DepartmentId
                ").ToList();
            }

            return departments;
        }

        public static void AddHrDepartment(
            HrDepartmentInfo department,
            string loggedInUser)
        {
            HrDepartmentInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrDepartmentInfo>(@"
                    SELECT
                        ISNULL(MAX(DepartmentId),0)+1 AS DepartmentId
                    FROM HrDepartment
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO HrDepartment
                    (
                        RowId,
                        DepartmentId,
                        DepartmentName,
                        ShortDeptName,
                        DepartmentGroupId,
                        BranchId,
                        DivisionId,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @DepartmentId,
                        @DepartmentName,
                        @ShortDeptName,
                        @DepartmentGroupId,
                        @BranchId,
                        @DivisionId,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", department.RowId),
                new SqlParameter("@DepartmentId", obj.DepartmentId),
                new SqlParameter("@DepartmentName", department.DepartmentName),
                new SqlParameter("@ShortDeptName", department.ShortDeptName ?? ""),
                new SqlParameter("@DepartmentGroupId", department.DepartmentGroupId),
                new SqlParameter("@BranchId", department.BranchId),
                new SqlParameter("@DivisionId", department.DivisionId),
                new SqlParameter("@Rank", department.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrDepartment(
            HrDepartmentInfo department,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDepartment
                    SET
                        DepartmentName = @DepartmentName,
                        ShortDeptName = @ShortDeptName,
                        DepartmentGroupId = @DepartmentGroupId,
                        BranchId = @BranchId,
                        DivisionId = @DivisionId,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", department.RowId),
                new SqlParameter("@DepartmentName", department.DepartmentName),
                new SqlParameter("@ShortDeptName", department.ShortDeptName ?? ""),
                new SqlParameter("@DepartmentGroupId", department.DepartmentGroupId),
                new SqlParameter("@BranchId", department.BranchId),
                new SqlParameter("@DivisionId", department.DivisionId),
                new SqlParameter("@Rank", department.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrDepartment(
            HrDepartmentInfo department,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDepartment
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", department.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrDepartmentResponse GetFilteredHrDepartments(
            HttpRequestBase Request)
        {
            HrDepartmentResponse resp = new HrDepartmentResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrDepartment a
                    INNER JOIN HrDepartmentGroup b
                        ON b.RowId = a.DepartmentGroupId
                    INNER JOIN HrBranch c
                        ON c.RowId = a.BranchId
                    INNER JOIN HrDivision d
                        ON d.RowId = a.DivisionId
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND
                        (
                            a.DepartmentName LIKE '%' + @SearchValue + '%'
                            OR a.ShortDeptName LIKE '%' + @SearchValue + '%'
                            OR b.DepartmentGroupName LIKE '%' + @SearchValue + '%'
                            OR c.BranchName LIKE '%' + @SearchValue + '%'
                            OR d.DivisionName LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(@"
                        SELECT COUNT(*)
                        FROM HrDepartment
                        WHERE DomainStatus = 1
                    ").Single();

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) " + sb.ToString(),
                        parameters.ToArray())
                    .Single();

                parameters.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                parameters.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrDepartments =
                    repo.Database.SqlQuery<HrDepartmentInfo>(@"
                    SELECT
                        a.RowId,
                        a.DepartmentId,
                        a.DepartmentName,
                        a.ShortDeptName,
                        b.DepartmentGroupName,
                        c.BranchName,
                        d.DivisionName,
                        a.Rank
                    " + sb.ToString() + @"
                    ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                    ",
                    parameters.ToArray()).ToList();
            }

            return resp;
        }
    }
}