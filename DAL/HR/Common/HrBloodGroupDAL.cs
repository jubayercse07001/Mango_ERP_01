using ERP.Models.HR.Common.HrBloodGroup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrBloodGroupDAL
    {

        public static HrBloodGroupInfo GetBloodGroupByID(string RowId)
        {
            HrBloodGroupInfo HrBloodGroup = null;
            using (var repo = new GMContext())
            {
                HrBloodGroup = repo.Database.SqlQuery<HrBloodGroupInfo>(@"
                   SELECT [RowId]
                         ,[BloodGroupId]
                         ,[BloodGroupName]
                         ,[DomainStatus]
                     FROM [dbo].[HrBloodGroup] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrBloodGroup;
        }

        public static HrBloodGroupInfo GetHrBloodGroupByName(string BloodGroupName)
        {
            HrBloodGroupInfo HrBloodGroup = null;
            using (var repo = new GMContext())
            {
                HrBloodGroup = repo.Database.SqlQuery<HrBloodGroupInfo>(@"
                     SELECT [RowId]
                         ,[BloodGroupId]
                         ,[BloodGroupName]
                         ,[DomainStatus]
                     FROM [dbo].[HrBloodGroup] 
                     WHERE [DomainStatus] = 1
                       AND [BloodGroupName] = @BloodGroupName", new SqlParameter("@BloodGroupName", BloodGroupName)).FirstOrDefault();
            }
            return HrBloodGroup;
        }
        public static List<HrBloodGroupInfo> GetAllHrBloodGroups()
        {
            List<HrBloodGroupInfo> HrBloodGroups = null;
            using (var repo = new GMContext())
            {
                HrBloodGroups = repo.Database.SqlQuery<HrBloodGroupInfo>(@"
                   SELECT a.[RowId] 
                         ,a.[BloodGroupId]
                         ,a.[BloodGroupName]
                         ,a.[DomainStatus]
                      FROM [Mango_ERP].[dbo].[HrBloodGroup] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY a.[BloodGroupId]
               ").ToList();
            }
            return HrBloodGroups;
        }
        public static void AddHrBloodGroup(HrBloodGroupInfo HrBloodGroup, string loggedInUser)
        {
            HrBloodGroupInfo loc;
            using (var repo = new GMContext())
            {
                 loc = repo.Database.SqlQuery<HrBloodGroupInfo>(@"
                        SELECT ISNULL(MAX([BloodGroupId]), 0) + 1 AS BloodGroupId
                        FROM [dbo].[HrBloodGroup]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrBloodGroup]
                                  ([RowId]
                                  ,[BloodGroupId]
                                  ,[BloodGroupName]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@BloodGroupId
                                  ,@BloodGroupName
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", Guid.NewGuid().ToString())
                , new SqlParameter("@BloodGroupId", loc.BloodGroupId)
                , new SqlParameter("@BloodGroupName", HrBloodGroup.BloodGroupName)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrBloodGroup(HrBloodGroupInfo HrBloodGroup, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrBloodGroup]
                              SET [BloodGroupName] = @BloodGroupName
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrBloodGroup.RowId)
                , new SqlParameter("@BloodGroupName", HrBloodGroup.BloodGroupName)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrBloodGroup(HrBloodGroupInfo HrBloodGroup, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrBloodGroup]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrBloodGroup.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrBloodGroupResponse GetFilteredHrBloodGroups(HttpRequestBase Request)
        {
            HrBloodGroupResponse resp = new HrBloodGroupResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrBloodGroups = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrBloodGroup] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
                //if (!string.IsNullOrEmpty(divisionType))
                //{
                //    sb.AppendLine(" AND DivisionType = @DivisionType ");
                //    sqlParameters.Add(new SqlParameter("@DivisionType", divisionType));
                //    sqlParameters1.Add(new SqlParameter("@DivisionType", divisionType));
                //}

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    //sb.AppendLine(" AND  CHARINDEX(@SearchValue, BloodGroupID) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, BloodGroupName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                    
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrBloodGroups = repo.Database.SqlQuery<HrBloodGroupInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[BloodGroupId]
                           ,a.[BloodGroupName]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}