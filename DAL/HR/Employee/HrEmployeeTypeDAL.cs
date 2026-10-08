using ERP.Models.HR.Employee;
using ERP.Models.HR.Organization.HrEmployeeType;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrEmployeeTypeDAL
    {
        public static HrEmployeeTypeInfo GetEmployeeTypeByID(string RowId)
        {
            HrEmployeeTypeInfo HrEmployeeType = null;


        using (var repo = new GMContext())
            {
                HrEmployeeType = repo.Database.SqlQuery<HrEmployeeTypeInfo>(@"
                SELECT [RowId]
                      ,[EmployeeTypeId]
                      ,[EmployeeTypeName]
                      ,[Rank]
                FROM [dbo].[HrEmployeeType]
                WHERE [RowId] = @RowId
                AND [DomainStatus] = 1
            ", new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return HrEmployeeType;
        }

        public static HrEmployeeTypeInfo GetHrEmployeeTypeByName(string EmployeeTypeName, string RowId)
        {
            HrEmployeeTypeInfo HrEmployeeType = null;

            using (var repo = new GMContext())
            {
                HrEmployeeType = repo.Database.SqlQuery<HrEmployeeTypeInfo>(@"
            SELECT [RowId]
                  ,[EmployeeTypeId]
                  ,[EmployeeTypeName]
                  ,[Rank]
            FROM [dbo].[HrEmployeeType]
            WHERE [EmployeeTypeName] = @EmployeeTypeName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND [DomainStatus] = 1
        ",
                new SqlParameter("@EmployeeTypeName", EmployeeTypeName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return HrEmployeeType;
        }

        public static List<HrEmployeeTypeInfo> GetAllHrEmployeeTypes()
        {
            List<HrEmployeeTypeInfo> HrEmployeeTypes = null;

            using (var repo = new GMContext())
            {
                HrEmployeeTypes = repo.Database.SqlQuery<HrEmployeeTypeInfo>(@"
                SELECT [RowId]
                      ,[EmployeeTypeId]
                      ,[EmployeeTypeName]
                      ,[Rank]
                FROM [dbo].[HrEmployeeType]
                WHERE [DomainStatus] = 1
                ORDER BY [EmployeeTypeId]
            ").ToList();
            }

            return HrEmployeeTypes;
        }

        public static void AddHrEmployeeType(
            HrEmployeeTypeInfo HrEmployeeType,
            string loggedInUser)
        {
            HrEmployeeTypeInfo loc;

            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrEmployeeTypeInfo>(@"
                SELECT ISNULL(MAX([EmployeeTypeId]),0)+1
                       AS EmployeeTypeId
                FROM [dbo].[HrEmployeeType]
                WHERE [DomainStatus] = 1
            ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                INSERT INTO [dbo].[HrEmployeeType]
                (
                    [RowId],
                    [EmployeeTypeId],
                    [EmployeeTypeName],
                    [Rank],
                    [CreateUser],
                    [CreateDate],
                    [DomainStatus]
                )
                VALUES
                (
                    @RowId,
                    @EmployeeTypeId,
                    @EmployeeTypeName,
                    @Rank,
                    @CreateUser,
                    GETDATE(),
                    1
                )
            ",
                new SqlParameter("@RowId", HrEmployeeType.RowId),
                new SqlParameter("@EmployeeTypeId", loc.EmployeeTypeId),
                new SqlParameter("@EmployeeTypeName", HrEmployeeType.EmployeeTypeName),
                new SqlParameter("@Rank", HrEmployeeType.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeType(
            HrEmployeeTypeInfo HrEmployeeType,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                UPDATE [dbo].[HrEmployeeType]
                SET [EmployeeTypeName] = @EmployeeTypeName,
                    [Rank] = @Rank,
                    [UpdateUser] = @UpdateUser,
                    [UpdateDate] = GETDATE()
                WHERE [RowId] = @RowId
            ",
                new SqlParameter("@RowId", HrEmployeeType.RowId),
                new SqlParameter("@EmployeeTypeName", HrEmployeeType.EmployeeTypeName),
                new SqlParameter("@Rank", HrEmployeeType.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeType(
            HrEmployeeTypeInfo HrEmployeeType,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                UPDATE [dbo].[HrEmployeeType]
                SET [DomainStatus] = 0,
                    [UpdateUser] = @UpdateUser,
                    [UpdateDate] = GETDATE()
                WHERE [RowId] = @RowId
            ",
                new SqlParameter("@RowId", HrEmployeeType.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeTypeResponse GetFilteredHrEmployeeTypes(
            HttpRequestBase Request)
        {
            HrEmployeeTypeResponse resp = new HrEmployeeTypeResponse();

            GMDataTableDAL gmdt =  new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                FROM [dbo].[HrEmployeeType] a
                WHERE a.[DomainStatus] = 1
            ");

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                    string.Format(
                    @"SELECT COUNT(*) {0}",
                    sb.ToString()))
                    .Single();

                List<SqlParameter> sqlParameters =
                    new List<SqlParameter>();

                List<SqlParameter> sqlParameters1 =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                    AND (
                        CHARINDEX(@SearchValue,
                        a.EmployeeTypeName) > 0
                    )
                ");

                    sqlParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                    string.Format(
                    @"SELECT COUNT(*) {0}",
                    sb.ToString()),
                    sqlParameters.ToArray())
                    .Single();

                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(
                    new SqlParameter(
                        "@Skip",
                        gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter(
                        "@Take",
                        gmdt.Length));

                resp.HrEmployeeTypes =
                    repo.Database.SqlQuery<HrEmployeeTypeInfo>(
                    string.Format(@"
                    SELECT
                        a.[RowId],
                        a.[EmployeeTypeId],
                        a.[EmployeeTypeName],
                        a.[Rank]
                    {0}
                    ORDER BY {1} {2}
                    OFFSET (@Skip) ROWS
                    FETCH NEXT (@Take) ROWS ONLY
                ",
                    sb.ToString(),
                    gmdt.SortColum,
                    gmdt.SortDir),
                    sqlParameters1.ToArray())
                    .ToList();
            }

            return resp;
        }
    }
}
