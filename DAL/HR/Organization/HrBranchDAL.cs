using ERP.Models.HR.Organization.HrBranch;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrBranchDAL
    {
        public static HrBranchInfo GetBranchByID(string RowId)
        {
            HrBranchInfo branch = null;


        using (var repo = new GMContext())
            {
                branch = repo.Database.SqlQuery<HrBranchInfo>(@"
                SELECT
                    a.RowId,
                    a.BranchId,
                    a.DivisionId,
                    b.DivisionName,
                    a.BranchName,
                    a.ShortBranchName,
                    a.BranchAddress,
                    a.ContactNumber,
                    a.Rank
                FROM HrBranch a
                INNER JOIN HrDivision b
                    ON b.RowId = a.DivisionId
                WHERE a.RowId = @RowId
                AND a.DomainStatus = 1
            ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return branch;
        }

        public static HrBranchInfo GetHrBranchByName(string BranchName, string RowId,string DivisionId)
        {
            HrBranchInfo branch = null;

            using (var repo = new GMContext())
            {
                branch = repo.Database.SqlQuery<HrBranchInfo>(@"
                SELECT *
                FROM HrBranch
                WHERE BranchName = @BranchName
                AND (@RowId IS NULL OR RowId <> @RowId OR DivisionId <> @DivisionId)
                AND DomainStatus = 1
            ",
                new SqlParameter("@BranchName", BranchName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value),
                new SqlParameter("@DivisionId", (object)DivisionId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return branch;
        }
        public static List<HrBranchInfo> GetBranchByDivisionId(string DivisionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrBranchInfo>(@"
            SELECT
                RowId,
                BranchName
            FROM HrBranch
            WHERE DivisionId = @DivisionId
            AND DomainStatus = 1
            ORDER BY BranchName
        ",
                new SqlParameter("@DivisionId", DivisionId))
                .ToList();
            }
        }
        public static List<HrBranchInfo> GetAllHrBranches()
        {
            List<HrBranchInfo> branches = null;

            using (var repo = new GMContext())
            {
                branches = repo.Database.SqlQuery<HrBranchInfo>(@"
                SELECT
                    RowId,
                    BranchId,
                    DivisionId,
                    BranchName,
                    ShortBranchName,
                    BranchAddress,
                    ContactNumber,
                    Rank
                FROM HrBranch
                WHERE DomainStatus = 1
                ORDER BY BranchId
            ").ToList();
            }

            return branches;
        }

        public static void AddHrBranch(HrBranchInfo branch, string loggedInUser)
        {
            HrBranchInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrBranchInfo>(@"
                SELECT
                    ISNULL(MAX(BranchId),0)+1 AS BranchId
                FROM HrBranch
                WHERE DomainStatus = 1
            ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                INSERT INTO HrBranch
                (
                    RowId,
                    BranchId,
                    DivisionId,
                    BranchName,
                    ShortBranchName,
                    BranchAddress,
                    ContactNumber,
                    Rank,
                    CreateUser,
                    CreateDate,
                    DomainStatus
                )
                VALUES
                (
                    @RowId,
                    @BranchId,
                    @DivisionId,
                    @BranchName,
                    @ShortBranchName,
                    @BranchAddress,
                    @ContactNumber,
                    @Rank,
                    @CreateUser,
                    GETDATE(),
                    1
                )
            ",
                new SqlParameter("@RowId", branch.RowId),
                new SqlParameter("@BranchId", obj.BranchId),
                new SqlParameter("@DivisionId", branch.DivisionId),
                new SqlParameter("@BranchName", branch.BranchName),
                new SqlParameter("@ShortBranchName", branch.ShortBranchName ?? ""),
                new SqlParameter("@BranchAddress", branch.BranchAddress ?? ""),
                new SqlParameter("@ContactNumber", branch.ContactNumber ?? ""),
                new SqlParameter("@Rank", branch.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrBranch(HrBranchInfo branch, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                UPDATE HrBranch
                SET
                    DivisionId = @DivisionId,
                    BranchName = @BranchName,
                    ShortBranchName = @ShortBranchName,
                    BranchAddress = @BranchAddress,
                    ContactNumber = @ContactNumber,
                    Rank = @Rank,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RowId = @RowId
            ",
                new SqlParameter("@RowId", branch.RowId),
                new SqlParameter("@DivisionId", branch.DivisionId),
                new SqlParameter("@BranchName", branch.BranchName),
                new SqlParameter("@ShortBranchName", branch.ShortBranchName ?? ""),
                new SqlParameter("@BranchAddress", branch.BranchAddress ?? ""),
                new SqlParameter("@ContactNumber", branch.ContactNumber ?? ""),
                new SqlParameter("@Rank", branch.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrBranch(HrBranchInfo branch, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                UPDATE HrBranch
                SET
                    DomainStatus = 0,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RowId = @RowId
            ",
                new SqlParameter("@RowId", branch.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrBranchResponse GetFilteredHrBranches(HttpRequestBase Request)
        {
            HrBranchResponse resp = new HrBranchResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                FROM HrBranch a
                INNER JOIN HrDivision b
                    ON b.RowId = a.DivisionId
                WHERE a.DomainStatus = 1
            ");

                List<SqlParameter> parameters =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                    AND
                    (
                        a.BranchName LIKE '%' + @SearchValue + '%'
                        OR a.ShortBranchName LIKE '%' + @SearchValue + '%'
                        OR b.DivisionName LIKE '%' + @SearchValue + '%'
                    )");

                    parameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue
                        ));
                }

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HrBranch WHERE DomainStatus = 1")
                    .Single();

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) " + sb.ToString(),
                        parameters.ToArray())
                    .Single();

                parameters.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                parameters.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrBranches = repo.Database.SqlQuery<HrBranchInfo>(@"
                SELECT
                    a.RowId,
                    a.BranchId,
                    b.DivisionName,
                    a.BranchName,
                    a.ShortBranchName,
                    a.ContactNumber,
                    a.Rank
                "
                    + sb.ToString() +
                    @"
                ORDER BY "
                    + gmdt.SortColum + " "
                    + gmdt.SortDir +
                    @"
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
