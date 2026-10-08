using ERP.Models.HR.Common.HrState;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
namespace ERP.DAL.HR.Common
{
    public class HrStateDAL
    {
        public static HrStateInfo GetStateByID(string RowId)
        {
            HrStateInfo HrState = null;
            using (var repo = new GMContext())
            {
                HrState = repo.Database.SqlQuery<HrStateInfo>(@"
                   SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[StateId]
                         ,a.[StateName]
                         ,a.[StateCode]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrState] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrState;
        }

        public static HrStateInfo GetHrStateByName(string StateName, string RowId)
        {
            HrStateInfo HrState = null;

            using (var repo = new GMContext())
            {
                HrState = repo.Database.SqlQuery<HrStateInfo>(@"
            SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[StateId]
                         ,a.[StateName]
                         ,a.[StateCode]
                         ,a.[DomainStatus]
            FROM [dbo].[HrState] a
            INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
            WHERE a.[StateName] = @StateName
              AND (@RowId IS NULL OR a.[RowId] <> @RowId)
              AND a.DomainStatus = 1",
                    new SqlParameter("@StateName", StateName),
                    new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return HrState;
        }
        public static HrStateInfo GetHrStateByCode(string StateCode, string RowId)
        {
            HrStateInfo HrState = null;
            using (var repo = new GMContext())
            {
                HrState = repo.Database.SqlQuery<HrStateInfo>(@"
                     SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[StateId]
                         ,a.[StateName]
                         ,a.[StateCode]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrState] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                      WHERE a.[StateCode] = @StateCode
              AND (@RowId IS NULL OR a.[RowId] <> @RowId)
              AND a.DomainStatus = 1", new SqlParameter("@StateCode", StateCode), new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrState;
        }
        public static HrStateInfo GetStateByCountry(string CountryId)
        {
            HrStateInfo HrState = null;
            using (var repo = new GMContext())
            {
                HrState = repo.Database.SqlQuery<HrStateInfo>(@"
                    SELECT
                        a.RowId,
                        a.StateName
                       FROM dbo.HrState a
                    INNER JOIN dbo.HrCountry b 
                        ON a.CountryId = b.RowId
                    WHERE a.CountryId = @CountryId
                      AND a.DomainStatus = 1
                    ORDER BY a.StateName;", new SqlParameter("@CountryId", CountryId)).FirstOrDefault();
            }
            return HrState;
        }
        
        public static List<HrStateInfo> GetAllHrStates()
        {
            List<HrStateInfo> HrStates = null;
            using (var repo = new GMContext())
            {
                HrStates = repo.Database.SqlQuery<HrStateInfo>(@"
                     SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[StateId]
                         ,a.[StateName]
                         ,a.[StateCode]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrState] a
                      INNER JOIN [HrCountry] b ON a.[CountryId] = b.[RowId]
                      AND a.[DomainStatus] = 1
                     ORDER BY [StateId]
               ").ToList();
            }
            return HrStates;
        }
        public static void AddHrState(HrStateInfo HrState, string loggedInUser)
        {
            HrStateInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrStateInfo>(@"
                        SELECT ISNULL(MAX([StateId]), 0) + 1 AS StateId
                        FROM [dbo].[HrState]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrState]
                                  ([RowId]
                                  ,[CountryId]
                                  ,[StateId]
                                  ,[StateName]
                                  ,[StateCode]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@CountryId
                                  ,@StateId
                                  ,@StateName
                                  ,@StateCode
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrState.RowId)
                , new SqlParameter("@CountryId", HrState.CountryId)
                , new SqlParameter("@StateId", loc.StateId)
                , new SqlParameter("@StateName", HrState.StateName)
                , new SqlParameter("@StateCode", HrState.StateCode)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrState(HrStateInfo HrState, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrState]
                              SET [CountryId] = @CountryId
                                 ,[StateName] = @StateName
                                 ,[StateCode] = @StateCode
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrState.RowId)
               , new SqlParameter("@CountryId", HrState.CountryId)
                , new SqlParameter("@StateId", HrState.StateId)
                , new SqlParameter("@StateName", HrState.StateName)
                , new SqlParameter("@StateCode", HrState.StateCode)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrState(HrStateInfo HrState, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrState]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrState.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrStateResponse GetFilteredHrStates(HttpRequestBase Request)
        {
            HrStateResponse resp = new HrStateResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrStates = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrState] a
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
                    //sb.AppendLine(" AND  CHARINDEX(@SearchValue, StateID) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, StateName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, CountryName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrStates = repo.Database.SqlQuery<HrStateInfo>(string.Format(@"
                          SELECT a.[RowId]
                         ,a.[CountryId]
                         ,b.[CountryName]
                         ,a.[StateId]
                         ,a.[StateName]
                         ,a.[StateCode]
                        {0}
                        ORDER BY {1}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}