using ERP.Models.HR.Common.HrCurrency;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
namespace ERP.DAL.HR.Common
{
    public class HrCurrencyDAL
    {
        public static HrCurrencyInfo GetCurrencyByID(string RowId)
        {
            HrCurrencyInfo HrCurrency = null;
            using (var repo = new GMContext())
            {
                HrCurrency = repo.Database.SqlQuery<HrCurrencyInfo>(@"
                   SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[CurrencyId]
                         ,a.[CurrencyName]
                         ,a.[CurrencyCode]
                         ,a.[CurrencySymbol]
                         ,a.[Rank]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrCurrency] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCurrency;
        }

        public static HrCurrencyInfo GetHrCurrencyByName(string CurrencyName, string RowId)
        {
            HrCurrencyInfo HrCurrency = null;

            using (var repo = new GMContext())
            {
                HrCurrency = repo.Database.SqlQuery<HrCurrencyInfo>(@"
            SELECT a.[RowId],
                   a.[CountryId],
                   b.[CountryName],
                   a.[CurrencyId],
                   a.[CurrencyName],
                   a.[CurrencySymbol],
                   a.[Rank],
                   a.[DomainStatus]
            FROM [dbo].[HrCurrency] a
            INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
            WHERE a.[CurrencyName] = @CurrencyName
              AND (@RowId IS NULL OR a.[RowId] <> @RowId)
              AND a.DomainStatus = 1",
                    new SqlParameter("@CurrencyName", CurrencyName),
                    new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return HrCurrency;
        }
        public static HrCurrencyInfo GetHrCurrencyByCode(string CurrencyCode, string RowId)
        {
            HrCurrencyInfo HrCurrency = null;
            using (var repo = new GMContext())
            {
                HrCurrency = repo.Database.SqlQuery<HrCurrencyInfo>(@"
                      SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[CurrencyId]
                         ,a.[CurrencyName]
                         ,a.[CurrencySymbol]
                         ,a.[Rank]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrCurrency] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                      WHERE a.[CurrencyCode] = @CurrencyCode
              AND (@RowId IS NULL OR a.[RowId] <> @RowId)
              AND a.DomainStatus = 1", new SqlParameter("@CurrencyCode", CurrencyCode), new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCurrency;
        }
        public static List<HrCurrencyInfo> GetAllHrCurrencys()
        {
            List<HrCurrencyInfo> HrCurrencys = null;
            using (var repo = new GMContext())
            {
                HrCurrencys = repo.Database.SqlQuery<HrCurrencyInfo>(@"
                     SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[CurrencyId]
                         ,a.[CurrencyName]
                         ,a.[CurrencySymbol]
                         ,a.[Rank]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrCurrency] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                      AND a.[DomainStatus] = 1
                     ORDER BY [CurrencyId]
               ").ToList();
            }
            return HrCurrencys;
        }
        public static void AddHrCurrency(HrCurrencyInfo HrCurrency, string loggedInUser)
        {
            HrCurrencyInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrCurrencyInfo>(@"
                        SELECT ISNULL(MAX([CurrencyId]), 0) + 1 AS CurrencyId
                        FROM [dbo].[HrCurrency]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrCurrency]
                                  ([RowId]
                                  ,[CountryId]
                                  ,[CurrencyId]
                                  ,[CurrencyName]
                                  ,[CurrencyCode]
                                  ,[CurrencySymbol]
                                  ,[Rank]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@CountryId
                                  ,@CurrencyId
                                  ,@CurrencyName
                                  ,@CurrencyCode
                                  ,@CurrencySymbol
                                  ,@Rank
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrCurrency.RowId)
                , new SqlParameter("@CountryId", HrCurrency.CountryId)
                , new SqlParameter("@CurrencyId", loc.CurrencyId)
                , new SqlParameter("@CurrencyName", HrCurrency.CurrencyName)
                , new SqlParameter("@CurrencyCode", HrCurrency.CurrencyCode)
                , new SqlParameter("@CurrencySymbol", HrCurrency.CurrencySymbol)
                , new SqlParameter("@Rank", HrCurrency.Rank)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrCurrency(HrCurrencyInfo HrCurrency, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrCurrency]
                              SET [CountryId] = @CountryId
                                 ,[CurrencyName] = @CurrencyName
                                 ,[CurrencyCode] = @CurrencyCode
                                 ,[CurrencySymbol] = @CurrencySymbol
                                 ,[Rank] = @Rank
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrCurrency.RowId)
               , new SqlParameter("@CountryId", HrCurrency.CountryId)
                , new SqlParameter("@CurrencyId", HrCurrency.CurrencyId)
                , new SqlParameter("@CurrencyName", HrCurrency.CurrencyName)
                , new SqlParameter("@CurrencyCode", HrCurrency.CurrencyCode)
                , new SqlParameter("@CurrencySymbol", HrCurrency.CurrencySymbol)
                , new SqlParameter("@Rank", HrCurrency.Rank)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrCurrency(HrCurrencyInfo HrCurrency, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrCurrency]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrCurrency.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrCurrencyResponse GetFilteredHrCurrencys(HttpRequestBase Request)
        {
            HrCurrencyResponse resp = new HrCurrencyResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrCurrencys = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrCurrency] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                        AND a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
                //if (!string.IsNullOrEmpty(CountryId))
                //{
                //    sb.AppendLine(" AND DivisionType = @DivisionType ");
                //    sqlParameters.Add(new SqlParameter("@DivisionType", divisionType));
                //    sqlParameters1.Add(new SqlParameter("@DivisionType", divisionType));
                //}

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    //sb.AppendLine(" AND  CHARINDEX(@SearchValue, CurrencyID) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, CurrencyName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, CountryName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrCurrencys = repo.Database.SqlQuery<HrCurrencyInfo>(string.Format(@"
                          SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[CurrencyId]
                         ,a.[CurrencyName]
                         ,a.[CurrencyCode]
                         ,a.[CurrencySymbol]
                         ,a.[Rank]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}