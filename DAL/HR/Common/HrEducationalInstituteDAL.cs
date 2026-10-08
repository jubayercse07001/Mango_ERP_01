using ERP.Models.HR.Common.HrEducationalInstitute;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
   
    public class HrEducationalInstituteDAL
    {
        public static HrEducationalInstituteInfo GetEducationalInstituteByID(string RowId)
        {
            HrEducationalInstituteInfo HrEducationalInstitute = null;
            using (var repo = new GMContext())
            {
                HrEducationalInstitute = repo.Database.SqlQuery<HrEducationalInstituteInfo>(@"
                   SELECT [RowId]
                         ,[InstituteId]
                         ,[InstituteName]
                         ,[Address]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrEducationalInstitute] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrEducationalInstitute;
        }

        public static HrEducationalInstituteInfo GetHrEducationalInstituteByName(string EducationalInstituteName, string RowId)
        {
            HrEducationalInstituteInfo HrEducationalInstitute = null;
            using (var repo = new GMContext())
            {
                HrEducationalInstitute = repo.Database.SqlQuery<HrEducationalInstituteInfo>(@"
                     SELECT [RowId]
                         ,[InstituteId]
                         ,[InstituteName]
                         ,[Address]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrEducationalInstitute] 
                     WHERE [InstituteName] = @InstituteName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@InstituteName", EducationalInstituteName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrEducationalInstitute;
        }
        //public static List<HrEducationalInstituteInfo> GetAllHrEducationalInstitutes()
        //{
        //    List<HrEducationalInstituteInfo> HrEducationalInstitutes = null;
        //    using (var repo = new GMContext())
        //    {
        //        HrEducationalInstitutes = repo.Database.SqlQuery<HrEducationalInstituteInfo>(@"
        //            SELECT [RowId]
        //                 ,[DegreeId]
        //                 ,[DegreeName]
        //                 ,[Rank]
        //                 ,[DomainStatus]
        //             FROM [dbo].[HrEducationalDegree] a
        //               WHERE a.[DomainStatus] = 1
        //             ORDER BY [DegreeId]
        //       ").ToList();
        //    }
        //    return HrEducationalInstitutes;
        //}

        public static List<HrEducationalInstituteInfo> GetAllHrEducationalInstitutes()
        {
            List<HrEducationalInstituteInfo> HrEducationalInstitutes = null;
            using (var repo = new GMContext())
            {
                HrEducationalInstitutes = repo.Database.SqlQuery<HrEducationalInstituteInfo>(@"
                    SELECT [RowId]
                         ,[InstituteId]
                         ,[InstituteName]
                         ,[Address]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrEducationalInstitute] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [InstituteId]
               ").ToList();
            }
            return HrEducationalInstitutes;
        }
        public static void AddHrEducationalInstitute(HrEducationalInstituteInfo HrEducationalInstitute, string loggedInUser)
        {
            HrEducationalInstituteInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrEducationalInstituteInfo>(@"
                        SELECT ISNULL(MAX([InstituteId]), 0) + 1 AS InstituteId
                        FROM [dbo].[HrEducationalInstitute]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrEducationalInstitute]
                                  ([RowId]
                                  ,[InstituteId]
                                  ,[InstituteName]
                                  ,[Address]
                                  ,[Rank]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@InstituteId
                                  ,@InstituteName
                                  ,@Address
                                  ,@Rank
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrEducationalInstitute.RowId)
                , new SqlParameter("@InstituteId", loc.InstituteId)
                , new SqlParameter("@InstituteName", HrEducationalInstitute.InstituteName)
                , new SqlParameter("@Address", HrEducationalInstitute.Address)
                , new SqlParameter("@Rank", HrEducationalInstitute.Rank)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrEducationalInstitute(HrEducationalInstituteInfo HrEducationalInstitute, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrEducationalInstitute]
                              SET [InstituteName] = @InstituteName
                                 ,[Address] = @Address 
                                 ,[Rank] = @Rank
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrEducationalInstitute.RowId)
                , new SqlParameter("@InstituteName", HrEducationalInstitute.InstituteName)
                , new SqlParameter("@Address", HrEducationalInstitute.Address)
                , new SqlParameter("@Rank", HrEducationalInstitute.Rank)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrEducationalInstitute(HrEducationalInstituteInfo HrEducationalInstitute, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrEducationalInstitute]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrEducationalInstitute.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrEducationalInstituteResponse GetFilteredHrEducationalInstitutes(HttpRequestBase Request)
        {
            HrEducationalInstituteResponse resp = new HrEducationalInstituteResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrEducationalInstitutes = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrEducationalInstitute] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, InstituteName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, Address) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrEducationalInstitutes = repo.Database.SqlQuery<HrEducationalInstituteInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[InstituteId]
                           ,a.[InstituteName]
                           ,a.[Address]
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