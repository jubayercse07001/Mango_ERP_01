using ERP.Models.HR.Common.HrCountry;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrCountryDAL
    {
        public static HrCountryInfo GetCountryByID(string RowId)
        {
            HrCountryInfo HrCountry = null;
            using (var repo = new GMContext())
            {
                HrCountry = repo.Database.SqlQuery<HrCountryInfo>(@"
                   SELECT [RowId]
                         ,[CountryId]
                         ,[CountryName]
                         ,[CountryCode]
                         ,[DomainStatus]
                     FROM [dbo].[HrCountry] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCountry;
        }
       
        public static HrCountryInfo GetHrCountryByName(string CountryName, string RowId)
        {
            HrCountryInfo HrCountry = null;
            using (var repo = new GMContext())
            {
                HrCountry = repo.Database.SqlQuery<HrCountryInfo>(@"
                     SELECT [RowId]
                         ,[CountryId]
                         ,[CountryName]
                         ,[CountryCode]
                         ,[DomainStatus]
                     FROM [dbo].[HrCountry] 
                     WHERE [CountryName] = @CountryName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@CountryName", CountryName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCountry;
        }

        public static HrCountryInfo GetHrCountryByCode(string CountryCode, string RowId)
        {
            HrCountryInfo HrCountry = null;
            using (var repo = new GMContext())
            {
                HrCountry = repo.Database.SqlQuery<HrCountryInfo>(@"
                     SELECT [RowId]
                         ,[CountryId]
                         ,[CountryName]
                         ,[CountryCode]
                         ,[DomainStatus]
                     FROM [dbo].[HrCountry] 
                      WHERE [CountryCode] = @CountryCode
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@CountryCode", CountryCode)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCountry;
        }
        public static List<HrCountryInfo> GetAllHrCountrys()
        {
            List<HrCountryInfo> HrCountrys = null;
            using (var repo = new GMContext())
            {
                HrCountrys = repo.Database.SqlQuery<HrCountryInfo>(@"
                    SELECT a.[RowId] 
                         ,a.[CountryId]
                         ,a.[CountryName]
                         ,a.[CountryCode]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrCountry] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [CountryId]
               ").ToList();
            }
            return HrCountrys;
        }
        public static void AddHrCountry(HrCountryInfo HrCountry, string loggedInUser)
        {
            HrCountryInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrCountryInfo>(@"
                        SELECT ISNULL(MAX([CountryId]), 0) + 1 AS CountryId
                        FROM [dbo].[HrCountry]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrCountry]
                                  ([RowId]
                                  ,[CountryId]
                                  ,[CountryName]
                                  ,[CountryCode]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@CountryId
                                  ,@CountryName
                                  ,@CountryCode
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrCountry.RowId)
                , new SqlParameter("@CountryId", loc.CountryId)
                , new SqlParameter("@CountryName", HrCountry.CountryName)
                , new SqlParameter("@CountryCode", HrCountry.CountryCode)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrCountry(HrCountryInfo HrCountry, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrCountry]
                              SET [CountryName] = @CountryName
                                 ,[CountryCode] = @CountryCode
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrCountry.RowId)
                , new SqlParameter("@CountryName", HrCountry.CountryName)
                , new SqlParameter("@CountryCode", HrCountry.CountryCode)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrCountry(HrCountryInfo HrCountry, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrCountry]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrCountry.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrCountryResponse GetFilteredHrCountrys(HttpRequestBase Request)
        {
            HrCountryResponse resp = new HrCountryResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrCountrys = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrCountry] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, CountryName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, CountryCode) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrCountrys = repo.Database.SqlQuery<HrCountryInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[CountryId]
                           ,a.[CountryName]
                           ,a.[CountryCode]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}