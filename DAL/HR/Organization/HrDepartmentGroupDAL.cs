using ERP.Models.HR.Organization.HrDepartmentGroup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrDepartmentGroupDAL
    {
        public static HrDepartmentGroupInfo GetDepartmentGroupByID(string RowId)
        {
            HrDepartmentGroupInfo departmentGroup = null;

            using (var repo = new GMContext())
            {
                departmentGroup = repo.Database.SqlQuery<HrDepartmentGroupInfo>(@"
                    SELECT
                        RowId,
                        DepartmentGroupId,
                        DepartmentGroupName,
                        Rank
                    FROM HrDepartmentGroup
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return departmentGroup;
        }

        public static HrDepartmentGroupInfo GetHrDepartmentGroupByName(
            string DepartmentGroupName,
            string RowId)
        {
            HrDepartmentGroupInfo departmentGroup = null;

            using (var repo = new GMContext())
            {
                departmentGroup = repo.Database.SqlQuery<HrDepartmentGroupInfo>(@"
                    SELECT *
                    FROM HrDepartmentGroup
                    WHERE DepartmentGroupName = @DepartmentGroupName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@DepartmentGroupName", DepartmentGroupName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return departmentGroup;
        }

        public static List<HrDepartmentGroupInfo> GetAllHrDepartmentGroups()
        {
            List<HrDepartmentGroupInfo> departmentGroups = null;

            using (var repo = new GMContext())
            {
                departmentGroups = repo.Database.SqlQuery<HrDepartmentGroupInfo>(@"
                    SELECT
                        RowId,
                        DepartmentGroupId,
                        DepartmentGroupName,
                        Rank
                    FROM HrDepartmentGroup
                    WHERE DomainStatus = 1
                    ORDER BY DepartmentGroupId
                ").ToList();
            }

            return departmentGroups;
        }

        public static void AddHrDepartmentGroup(
            HrDepartmentGroupInfo departmentGroup,
            string loggedInUser)
        {
            HrDepartmentGroupInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrDepartmentGroupInfo>(@"
                    SELECT
                        ISNULL(MAX(DepartmentGroupId),0)+1
                        AS DepartmentGroupId
                    FROM HrDepartmentGroup
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO HrDepartmentGroup
                    (
                        RowId,
                        DepartmentGroupId,
                        DepartmentGroupName,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @DepartmentGroupId,
                        @DepartmentGroupName,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", departmentGroup.RowId),
                new SqlParameter("@DepartmentGroupId", obj.DepartmentGroupId),
                new SqlParameter("@DepartmentGroupName", departmentGroup.DepartmentGroupName),
                new SqlParameter("@Rank", departmentGroup.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrDepartmentGroup(
            HrDepartmentGroupInfo departmentGroup,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDepartmentGroup
                    SET
                        DepartmentGroupName = @DepartmentGroupName,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", departmentGroup.RowId),
                new SqlParameter("@DepartmentGroupName", departmentGroup.DepartmentGroupName),
                new SqlParameter("@Rank", departmentGroup.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrDepartmentGroup(
            HrDepartmentGroupInfo departmentGroup,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDepartmentGroup
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", departmentGroup.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrDepartmentGroupResponse GetFilteredHrDepartmentGroups(
            HttpRequestBase Request)
        {
            HrDepartmentGroupResponse resp =
                new HrDepartmentGroupResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrDepartmentGroup
                    WHERE DomainStatus = 1
                ");

                List<SqlParameter> parameters =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            DepartmentGroupName LIKE '%' + @SearchValue + '%'
                        )
                    ");

                    parameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue)
                    );
                }

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(@"
                        SELECT COUNT(*)
                        FROM HrDepartmentGroup
                        WHERE DomainStatus = 1
                    ").Single();

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) " + sb.ToString(),
                        parameters.ToArray()
                    ).Single();

                parameters.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                parameters.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrDepartmentGroups =
                    repo.Database.SqlQuery<HrDepartmentGroupInfo>(@"
                        SELECT
                            RowId,
                            DepartmentGroupId,
                            DepartmentGroupName,
                            Rank
                    " + sb.ToString() + @"
                        ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"
                        OFFSET @Skip ROWS
                        FETCH NEXT @Take ROWS ONLY
                    ",
                    parameters.ToArray())
                    .ToList();
            }

            return resp;
        }
    }
}

