using ERP.Models.HR.Common.HrNationality;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{

    public class HrNationalityDAL
    {
        public static HrNationalityInfo GetNationalityByID(string RowId)
        {
            HrNationalityInfo HrNationality = null;
            using (var repo = new GMContext())
            {
                HrNationality = repo.Database.SqlQuery<HrNationalityInfo>(@"
                   SELECT [RowId]
                         ,[NationalityId]
                         ,[NationalityName]
                         ,[DomainStatus]
                     FROM [dbo].[HrNationality] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrNationality;
        }

        public static HrNationalityInfo GetHrNationalityByName(string NationalityName, string RowId)
        {
            HrNationalityInfo HrNationality = null;
            using (var repo = new GMContext())
            {
                HrNationality = repo.Database.SqlQuery<HrNationalityInfo>(@"
                    SELECT [RowId]
                         ,[NationalityId]
                         ,[NationalityName]
                         ,[DomainStatus]
                     FROM [dbo].[HrNationality] 
                     WHERE [NationalityName] = @NationalityName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@NationalityName", NationalityName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrNationality;
        }


        public static List<HrNationalityInfo> GetAllHrNationalitys()
        {
            List<HrNationalityInfo> HrNationalitys = null;
            using (var repo = new GMContext())
            {
                HrNationalitys = repo.Database.SqlQuery<HrNationalityInfo>(@"
                   SELECT [RowId]
                         ,[NationalityId]
                         ,[NationalityName]
                         ,[DomainStatus]
                     FROM [dbo].[HrNationality] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [NationalityId]
               ").ToList();
            }
            return HrNationalitys;
        }
        public static void AddHrNationality(HrNationalityInfo HrNationality, string loggedInUser)
        {
            HrNationalityInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrNationalityInfo>(@"
                        SELECT ISNULL(MAX([NationalityId]), 0) + 1 AS NationalityId
                        FROM [dbo].[HrNationality]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrNationality]
                                  ([RowId]
                                  ,[NationalityId]
                                  ,[NationalityName]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@NationalityId
                                  ,@NationalityName
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrNationality.RowId)
                , new SqlParameter("@NationalityId", loc.NationalityId)
                , new SqlParameter("@NationalityName", HrNationality.NationalityName)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrNationality(HrNationalityInfo HrNationality, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrNationality]
                              SET [NationalityName] = @NationalityName
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrNationality.RowId)
                , new SqlParameter("@NationalityName", HrNationality.NationalityName)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrNationality(HrNationalityInfo HrNationality, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrNationality]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrNationality.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrNationalityResponse GetFilteredHrNationalitys(HttpRequestBase Request)
        {
            HrNationalityResponse resp = new HrNationalityResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrNationalitys = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrNationality] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, NationalityName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrNationalitys = repo.Database.SqlQuery<HrNationalityInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[NationalityId]
                           ,a.[NationalityName]
                        {0}
                        ORDER BY {1} 
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}