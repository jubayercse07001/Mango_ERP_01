using ERP.Models.HR.Organization.HrDivision;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrDivisionDAL
    {
        public static HrDivisionInfo GetDivisionByID(string RowId)
        {
            HrDivisionInfo division = null;

            using (var repo = new GMContext())
            {
                division = repo.Database.SqlQuery<HrDivisionInfo>(@"
                    SELECT
                        a.RowId,
                        a.DivisionId,
						a.ParentId,
						b.CompanyName,
                        a.DivisionName,
                        a.ShortDivisionName,
                        a.Address,
                        a.Phone,
                        a.Fax,
                        a.Mobile,
                        a.MainDivisionId,
						c.MainDivisionName,
                        a.Rank,
                        a.DomainStatus
						 FROM HrDivision a
                    inner join HrCompanyInformation b on b.RowId = a.ParentId
					inner join HrMainDivision c on c.RowId = a.MainDivisionId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return division;
        }

        public static HrDivisionInfo GetHrDivisionByName(string DivisionName, string RowId)
        {
            HrDivisionInfo division = null;

            using (var repo = new GMContext())
            {
                division = repo.Database.SqlQuery<HrDivisionInfo>(@"
                    SELECT *
                    FROM [dbo].[HrDivision]
                    WHERE DivisionName = @DivisionName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@DivisionName", DivisionName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return division;
        }

        public static List<HrDivisionInfo> GetAllHrDivisions()
        {
            List<HrDivisionInfo> divisions = null;

            using (var repo = new GMContext())
            {
                divisions = repo.Database.SqlQuery<HrDivisionInfo>(@"
                    SELECT
                        [RowId],
                        [DivisionId],
                        [DivisionName],
                        [ShortDivisionName],
                        [Address],
                        [Phone],
                        [Fax],
                        [Mobile],
                        [ParentId],
                        [MainDivisionId],
                        [Rank],
                        [DomainStatus]
                    FROM [dbo].[HrDivision]
                    WHERE DomainStatus = 1
                    ORDER BY DivisionId
                ").ToList();
            }

            return divisions;
        }

        public static void AddHrDivision(HrDivisionInfo division, string loggedInUser)
        {
            HrDivisionInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrDivisionInfo>(@"
                    SELECT ISNULL(MAX(DivisionId),0)+1 AS DivisionId
                    FROM HrDivision
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrDivision]
                    (
                        RowId,
                        DivisionId,
                        DivisionName,
                        ShortDivisionName,
                        Address,
                        Phone,
                        Fax,
                        Mobile,
                        ParentId,
                        MainDivisionId,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @DivisionId,
                        @DivisionName,
                        @ShortDivisionName,
                        @Address,
                        @Phone,
                        @Fax,
                        @Mobile,
                        @ParentId,
                        @MainDivisionId,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", division.RowId),
                new SqlParameter("@DivisionId", obj.DivisionId),
                new SqlParameter("@DivisionName", division.DivisionName),
                new SqlParameter("@ShortDivisionName", division.ShortDivisionName ?? ""),
                new SqlParameter("@Address", division.Address ?? ""),
                new SqlParameter("@Phone", division.Phone ?? ""),
                new SqlParameter("@Fax", division.Fax ?? ""),
                new SqlParameter("@Mobile", division.Mobile ?? ""),
                new SqlParameter("@ParentId", division.ParentId ?? ""),
                new SqlParameter("@MainDivisionId", division.MainDivisionId ?? ""),
                new SqlParameter("@Rank", division.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrDivision(HrDivisionInfo division, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDivision
                    SET
                        DivisionName = @DivisionName,
                        ShortDivisionName = @ShortDivisionName,
                        Address = @Address,
                        Phone = @Phone,
                        Fax = @Fax,
                        Mobile = @Mobile,
                        ParentId = @ParentId,
                        MainDivisionId = @MainDivisionId,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", division.RowId),
                new SqlParameter("@DivisionName", division.DivisionName),
                new SqlParameter("@ShortDivisionName", division.ShortDivisionName ?? ""),
                new SqlParameter("@Address", division.Address ?? ""),
                new SqlParameter("@Phone", division.Phone ?? ""),
                new SqlParameter("@Fax", division.Fax ?? ""),
                new SqlParameter("@Mobile", division.Mobile ?? ""),
                new SqlParameter("@ParentId", division.ParentId ?? ""),
                new SqlParameter("@MainDivisionId", division.MainDivisionId ?? ""),
                new SqlParameter("@Rank", division.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrDivision(HrDivisionInfo division, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDivision
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", division.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrDivisionResponse GetFilteredHrDivisions(HttpRequestBase Request)
        {
            HrDivisionResponse resp = new HrDivisionResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrDivision a
                    inner join HrCompanyInformation b on b.RowId = a.ParentId
					inner join HrMainDivision c on c.RowId = a.MainDivisionId
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            DivisionName LIKE '%' + @SearchValue + '%'
                            OR ShortDivisionName LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(
                        new SqlParameter("@SearchValue", gmdt.SearchValue)
                    );
                }

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrDivision WHERE DomainStatus = 1"
                    ).Single();

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    parameters.ToArray()
                    ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrDivisions =
                    repo.Database.SqlQuery<HrDivisionInfo>(@"
                    SELECT
                        a.RowId,
                        a.DivisionId,
                        a.DivisionName,
                        a.ShortDivisionName,
						b.CompanyName,
						c.MainDivisionName,
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