using ERP.Models.HR.Organization.HrMainDivision;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrMainDivisionDAL
    {
        public static HrMainDivisionInfo GetMainDivisionByID(string RowId)
        {
            HrMainDivisionInfo HrMainDivision = null;

            using (var repo = new GMContext())
            {
                HrMainDivision = repo.Database.SqlQuery<HrMainDivisionInfo>(@"
            SELECT
                 [RowId]
                ,[MainDivisionId]
                ,[MainDivisionName]
                ,[ShortMainDivisionName]
                ,[Address]
                ,[Phone]
                ,CAST([Rank] AS INT) AS [Rank]
            FROM [dbo].[HrMainDivision]

            WHERE [RowId] = @RowId
              AND [DomainStatus] = 1

        ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return HrMainDivision;
        }

        public static HrMainDivisionInfo GetHrMainDivisionByName(string MainDivisionName, string RowId)
        {
            HrMainDivisionInfo HrMainDivision = null;

            using (var repo = new GMContext())
            {
                HrMainDivision = repo.Database.SqlQuery<HrMainDivisionInfo>(@"

                    SELECT [RowId]
                          ,[MainDivisionId]
                          ,[MainDivisionName]
                          ,[ShortMainDivisionName]
                          ,[Address]
                          ,[Phone]
                          ,[Rank]

                    FROM [dbo].[HrMainDivision]

                    WHERE [MainDivisionName] = @MainDivisionName
                    AND (@RowId IS NULL OR [RowId] <> @RowId)
                    AND [DomainStatus] = 1

                ",
                new SqlParameter("@MainDivisionName", MainDivisionName),
                new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return HrMainDivision;
        }

        public static List<HrMainDivisionInfo> GetAllHrMainDivisions()
        {
            List<HrMainDivisionInfo> HrMainDivisions = null;

            using (var repo = new GMContext())
            {
                HrMainDivisions = repo.Database.SqlQuery<HrMainDivisionInfo>(@"

            SELECT 
                  [RowId]
                ,[MainDivisionId]
                ,[MainDivisionName]
                ,[ShortMainDivisionName]
                ,[Address]
                ,[Phone]
                ,CAST([Rank] AS INT) AS [Rank]

            FROM [dbo].[HrMainDivision]

            WHERE [DomainStatus] = 1

            ORDER BY [MainDivisionId]

        ").ToList();
            }

            return HrMainDivisions;
        }

        public static void AddHrMainDivision(HrMainDivisionInfo HrMainDivision, string loggedInUser)
        {
            HrMainDivisionInfo loc;

            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrMainDivisionInfo>(@"

                    SELECT ISNULL(MAX([MainDivisionId]),0) + 1 AS MainDivisionId
                    FROM [dbo].[HrMainDivision]
                    WHERE [DomainStatus] = 1

                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"

                    INSERT INTO [dbo].[HrMainDivision]
                    (
                        [RowId],
                        [MainDivisionId],
                        [MainDivisionName],
                        [ShortMainDivisionName],
                        [Address],
                        [Phone],
                        [Rank],
                        [CreateUser],
                        [CreateDate],
                        [DomainStatus]
                    )

                    VALUES
                    (
                        @RowId,
                        @MainDivisionId,
                        @MainDivisionName,
                        @ShortMainDivisionName,
                        @Address,
                        @Phone,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )

                ",

                new SqlParameter("@RowId", HrMainDivision.RowId),

                new SqlParameter("@MainDivisionId", loc.MainDivisionId),

                new SqlParameter("@MainDivisionName",
                    HrMainDivision.MainDivisionName),

                new SqlParameter("@ShortMainDivisionName",
                    HrMainDivision.ShortMainDivisionName ?? (object)DBNull.Value),

                new SqlParameter("@Address",
                    HrMainDivision.Address ?? (object)DBNull.Value),

                new SqlParameter("@Phone",
                    HrMainDivision.Phone ?? (object)DBNull.Value),

                new SqlParameter("@Rank",
                    HrMainDivision.Rank),

                new SqlParameter("@CreateUser",
                    loggedInUser));
            }
        }

        public static void UpdateHrMainDivision(HrMainDivisionInfo HrMainDivision, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"

                    UPDATE [dbo].[HrMainDivision]

                    SET [MainDivisionName] = @MainDivisionName,
                        [ShortMainDivisionName] = @ShortMainDivisionName,
                        [Address] = @Address,
                        [Phone] = @Phone,
                        [Rank] = @Rank,
                        [UpdateUser] = @UpdateUser,
                        [UpdateDate] = GETDATE()

                    WHERE [RowId] = @RowId

                ",

                new SqlParameter("@RowId",
                    HrMainDivision.RowId),

                new SqlParameter("@MainDivisionName",
                    HrMainDivision.MainDivisionName),

                new SqlParameter("@ShortMainDivisionName",
                    HrMainDivision.ShortMainDivisionName ?? (object)DBNull.Value),

                new SqlParameter("@Address",
                    HrMainDivision.Address ?? (object)DBNull.Value),

                new SqlParameter("@Phone",
                    HrMainDivision.Phone ?? (object)DBNull.Value),

                new SqlParameter("@Rank",
                    HrMainDivision.Rank),

                new SqlParameter("@UpdateUser",
                    loggedInUser));
            }
        }

        public static void DeleteHrMainDivision(HrMainDivisionInfo HrMainDivision, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"

                    UPDATE [dbo].[HrMainDivision]

                    SET [DomainStatus] = 0,
                        [UpdateUser] = @UpdateUser,
                        [UpdateDate] = GETDATE()

                    WHERE [RowId] = @RowId

                ",

                new SqlParameter("@RowId",
                    HrMainDivision.RowId),

                new SqlParameter("@UpdateUser",
                    loggedInUser));
            }
        }

        public static HrMainDivisionResponse GetFilteredHrMainDivisions(HttpRequestBase Request)
        {
            HrMainDivisionResponse resp = new HrMainDivisionResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.HrMainDivisions = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"

            FROM [dbo].[HrMainDivision] a

            WHERE a.[DomainStatus] = 1

        ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"

            SELECT COUNT(*) {0}

        ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND CHARINDEX(@SearchValue, a.MainDivisionName) > 0 ");

                    sqlParameters.Add(
                        new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"

            SELECT COUNT(*) {0}

        ", sb.ToString()), sqlParameters.ToArray()).Single();

                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrMainDivisions = repo.Database.SqlQuery<HrMainDivisionInfo>(string.Format(@"

            SELECT
                 a.[RowId]
                ,a.[MainDivisionId]
                ,a.[MainDivisionName]
                ,a.[ShortMainDivisionName]
                ,a.[Address]
                ,a.[Phone]
                ,CAST(a.[Rank] AS INT) AS [Rank]

            {0}

            ORDER BY {1} {2}

            OFFSET (@Skip) ROWS
            FETCH NEXT (@Take) ROWS ONLY

        ",
                sb.ToString(),
                gmdt.SortColum.ToString(),
                gmdt.SortDir),

                sqlParameters1.ToArray()).ToList();
            }

            return resp;
        }
    }
}