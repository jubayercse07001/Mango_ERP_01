using ERP.Models.HR.Common.HrSkill;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
namespace ERP.DAL.HR.Common
{
    public class HrSkillDAL
    {
        public static HrSkillInfo GetSkillByID(string RowId)
        {
            HrSkillInfo HrSkill = null;
            using (var repo = new GMContext())
            {
                HrSkill = repo.Database.SqlQuery<HrSkillInfo>(@"
                   SELECT a.[RowId]
                         ,a.[SkillTypeId]
                         ,b.[SkillTypeName]
                         ,a.[SkillId]
                         ,a.[SkillName]
                         ,a.[Description]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrSkill] a
                      INNER JOIN [HrSkillType] b ON a.[SkillTypeId] = b.[RowId]
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrSkill;
        }

        public static HrSkillInfo GetHrSkillByName(string SkillName, string RowId)
        {
            HrSkillInfo HrSkill = null;

            using (var repo = new GMContext())
            {
                HrSkill = repo.Database.SqlQuery<HrSkillInfo>(@"
            SELECT a.[RowId]
                         ,a.[SkillTypeId]
                         ,b.[SkillTypeName]
                         ,a.[SkillId]
                         ,a.[SkillName]
                         ,a.[Description]
                         ,a.[DomainStatus]
            FROM [dbo].[HrSkill] a
            INNER JOIN [HrSkillType] b ON a.[SkillTypeId] = b.[RowId]
            WHERE a.[SkillName] = @SkillName
              AND (@RowId IS NULL OR a.[RowId] <> @RowId)
              AND a.DomainStatus = 1",
                    new SqlParameter("@SkillName", SkillName),
                    new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return HrSkill;
        }
        public static HrSkillInfo GetHrSkillByCode(string SkillCode, string RowId)
        {
            HrSkillInfo HrSkill = null;
            using (var repo = new GMContext())
            {
                HrSkill = repo.Database.SqlQuery<HrSkillInfo>(@"
                     SELECT a.[RowId]
                         ,a.[SkillTypeId]
                         ,b.[SkillTypeName]
                         ,a.[SkillId]
                         ,a.[SkillName]
                         ,a.[Description]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrSkill] a
                      INNER JOIN [HrSkillType] b ON a.[SkillTypeId] = b.[RowId]
                      WHERE a.[SkillCode] = @SkillCode
              AND (@RowId IS NULL OR a.[RowId] <> @RowId)
              AND a.DomainStatus = 1", new SqlParameter("@SkillCode", SkillCode), new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrSkill;
        }
        public static List<HrSkillInfo> GetAllHrSkills()
        {
            List<HrSkillInfo> HrSkills = null;
            using (var repo = new GMContext())
            {
                HrSkills = repo.Database.SqlQuery<HrSkillInfo>(@"
                     SELECT a.[RowId]
                         ,a.[SkillTypeId]
                         ,b.[SkillTypeName]
                         ,a.[SkillId]
                         ,a.[SkillName]
                         ,a.[Description]
                         ,a.[DomainStatus]
                     FROM [dbo].[HrSkill] a
                      INNER JOIN [HrSkillType] b ON a.[SkillTypeId] = b.[RowId]
                      AND a.[DomainStatus] = 1
                     ORDER BY [SkillId]
               ").ToList();
            }
            return HrSkills;
        }
        public static void AddHrSkill(HrSkillInfo HrSkill, string loggedInUser)
        {
            HrSkillInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrSkillInfo>(@"
                        SELECT ISNULL(MAX([SkillId]), 0) + 1 AS SkillId
                        FROM [dbo].[HrSkill]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrSkill]
                                  ([RowId]
                                  ,[SkillTypeId]
                                  ,[SkillId]
                                  ,[SkillName]
                                  ,[Description]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@SkillTypeId
                                  ,@SkillId
                                  ,@SkillName
                                  ,@Description
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrSkill.RowId)
                , new SqlParameter("@SkillTypeId", HrSkill.SkillTypeId)
                , new SqlParameter("@SkillId", loc.SkillId)
                , new SqlParameter("@SkillName", HrSkill.SkillName)
                , new SqlParameter("@Description", HrSkill.Description)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrSkill(HrSkillInfo HrSkill, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrSkill]
                              SET [SkillTypeId] = @SkillTypeId
                                 ,[SkillName] = @SkillName
                                 ,[Description] = @Description
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrSkill.RowId)
               , new SqlParameter("@SkillTypeId", HrSkill.SkillTypeId)
                , new SqlParameter("@SkillId", HrSkill.SkillId)
                , new SqlParameter("@SkillName", HrSkill.SkillName)
                , new SqlParameter("@Description", HrSkill.Description)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrSkill(HrSkillInfo HrSkill, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrSkill]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrSkill.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrSkillResponse GetFilteredHrSkills(HttpRequestBase Request)
        {
            HrSkillResponse resp = new HrSkillResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrSkills = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrSkill] a
                      INNER JOIN [HrSkillType] b ON a.[SkillTypeId] = b.[RowId]
                        AND a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
                //if (!string.IsNullOrEmpty(SkillTypeId))
                //{
                //    sb.AppendLine(" AND DivisionType = @DivisionType ");
                //    sqlParameters.Add(new SqlParameter("@DivisionType", divisionType));
                //    sqlParameters1.Add(new SqlParameter("@DivisionType", divisionType));
                //}

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    //sb.AppendLine(" AND  CHARINDEX(@SearchValue, SkillID) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, SkillName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, SkillTypeName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrSkills = repo.Database.SqlQuery<HrSkillInfo>(string.Format(@"
                          SELECT a.[RowId]
                         ,a.[SkillTypeId]
                         ,b.[SkillTypeName]
                         ,a.[SkillId]
                         ,a.[SkillName]
                         ,a.[Description]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}