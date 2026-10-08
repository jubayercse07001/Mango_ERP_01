using ERP.Models.HR.Organization.HrCompanyInformation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
namespace ERP.DAL.HR.Organization
{
    public class HrCompanyInformationDAL
    {
        public static HrCompanyInformationInfo GetCompanyInformationByID(string RowId)
        {
            HrCompanyInformationInfo HrCompanyInformation = null;
            using (var repo = new GMContext())
            {
                HrCompanyInformation = repo.Database.SqlQuery<HrCompanyInformationInfo>(@"
                   SELECT [RowId]
                          ,[CompanyId]
                          ,[CompanyName]
                          ,[ShortCompanyName]
                          ,[AddressLine1]
                          ,[AddressLine2]
                          ,[Phone1]
                          ,[Phone2]
                          ,[Phone3]
                          ,[Mobile1]
                          ,[Mobile2]
                          ,[Mobile3]
                          ,[Fax]
                          ,[Email1]
                          ,[Email2]
                          ,[Email3]
                          ,[Web]
                          ,[Logo]
                          ,[LogoUrl]
                     FROM [dbo].[HrCompanyInformation] 
                     WHERE [RowId] = @RowId and DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCompanyInformation;
        }

        public static HrCompanyInformationInfo GetHrCompanyInformationByName(string CompanyName, string RowId)
        {
            HrCompanyInformationInfo HrCompanyInformation = null;
            using (var repo = new GMContext())
            {
                HrCompanyInformation = repo.Database.SqlQuery<HrCompanyInformationInfo>(@"
                     SELECT [RowId]
                          ,[CompanyId]
                          ,[CompanyName]
                          ,[ShortCompanyName]
                          ,[AddressLine1]
                          ,[AddressLine2]
                          ,[Phone1]
                          ,[Phone2]
                          ,[Phone3]
                          ,[Mobile1]
                          ,[Mobile2]
                          ,[Mobile3]
                          ,[Fax]
                          ,[Email1]
                          ,[Email2]
                          ,[Email3]
                          ,[Web]
                          ,[Logo]
                          ,[LogoUrl]
                     FROM [dbo].[HrCompanyInformation] 
                     WHERE [CompanyName] = @CompanyName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@CompanyName", CompanyName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrCompanyInformation;
        }


        public static List<HrCompanyInformationInfo> GetAllHrCompanyInformations()
        {
            List<HrCompanyInformationInfo> HrCompanyInformations = null;
            using (var repo = new GMContext())
            {
                HrCompanyInformations = repo.Database.SqlQuery<HrCompanyInformationInfo>(@"
                   SELECT [RowId]
                          ,[CompanyId]
                          ,[CompanyName]
                          ,[ShortCompanyName]
                          ,[AddressLine1]
                          ,[AddressLine2]
                          ,[Phone1]
                          ,[Phone2]
                          ,[Phone3]
                          ,[Mobile1]
                          ,[Mobile2]
                          ,[Mobile3]
                          ,[Fax]
                          ,[Email1]
                          ,[Email2]
                          ,[Email3]
                          ,[Web]
                          ,[Logo]
                          ,[LogoUrl]
                     FROM [dbo].[HrCompanyInformation] 
                       WHERE [DomainStatus] = 1
                     ORDER BY [CompanyId]
               ").ToList();
            }
            return HrCompanyInformations;
        }
        public static void AddHrCompanyInformation(HrCompanyInformationInfo HrCompanyInformation, string loggedInUser)
        {
            HrCompanyInformationInfo loc;

            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrCompanyInformationInfo>(@"
                SELECT ISNULL(MAX([CompanyId]), 0) + 1 AS CompanyId
                FROM [dbo].[HrCompanyInformation]
                WHERE [DomainStatus] = 1
            ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
            INSERT INTO [dbo].[HrCompanyInformation]
            (
                [RowId],
                [CompanyId],
                [CompanyName],
                [ShortCompanyName],
                [AddressLine1],
                [AddressLine2],
                [Phone1],
                [Phone2],
                [Phone3],
                [Mobile1],
                [Mobile2],
                [Mobile3],
                [Fax],
                [Email1],
                [Email2],
                [Email3],
                [Web],
                [Logo],
                [LogoUrl],
                [CreateUser],
                [CreateDate],
                [DomainStatus]
            )
            VALUES
            (
                @RowId,
                @CompanyId,
                @CompanyName,
                @ShortCompanyName,
                @AddressLine1,
                @AddressLine2,
                @Phone1,
                @Phone2,
                @Phone3,
                @Mobile1,
                @Mobile2,
                @Mobile3,
                @Fax,
                @Email1,
                @Email2,
                @Email3,
                @Web,
                @Logo,
                @LogoUrl,
                @CreateUser,
                GETDATE(),
                1
            )
        ",
                new SqlParameter("@RowId", HrCompanyInformation.RowId),
                new SqlParameter("@CompanyId", loc.CompanyId),
                new SqlParameter("@CompanyName", HrCompanyInformation.CompanyName),
                new SqlParameter("@ShortCompanyName", HrCompanyInformation.ShortCompanyName ?? (object)DBNull.Value),
                new SqlParameter("@AddressLine1", HrCompanyInformation.AddressLine1 ?? (object)DBNull.Value),
                new SqlParameter("@AddressLine2", HrCompanyInformation.AddressLine2 ?? (object)DBNull.Value),
                new SqlParameter("@Phone1", HrCompanyInformation.Phone1 ?? (object)DBNull.Value),
                new SqlParameter("@Phone2", HrCompanyInformation.Phone2 ?? (object)DBNull.Value),
                new SqlParameter("@Phone3", HrCompanyInformation.Phone3 ?? (object)DBNull.Value),
                new SqlParameter("@Mobile1", HrCompanyInformation.Mobile1 ?? (object)DBNull.Value),
                new SqlParameter("@Mobile2", HrCompanyInformation.Mobile2 ?? (object)DBNull.Value),
                new SqlParameter("@Mobile3", HrCompanyInformation.Mobile3 ?? (object)DBNull.Value),
                new SqlParameter("@Fax", HrCompanyInformation.Fax ?? (object)DBNull.Value),
                new SqlParameter("@Email1", HrCompanyInformation.Email1 ?? (object)DBNull.Value),
                new SqlParameter("@Email2", HrCompanyInformation.Email2 ?? (object)DBNull.Value),
                new SqlParameter("@Email3", HrCompanyInformation.Email3 ?? (object)DBNull.Value),
                new SqlParameter("@Web", HrCompanyInformation.Web ?? (object)DBNull.Value),
                new SqlParameter("@Logo", HrCompanyInformation.Logo ?? (object)DBNull.Value),
                new SqlParameter("@LogoUrl", HrCompanyInformation.LogoUrl ?? (object)DBNull.Value),     
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrCompanyInformation(HrCompanyInformationInfo HrCompanyInformation, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
        UPDATE [dbo].[HrCompanyInformation]
           SET [CompanyName] = @CompanyName,
               [ShortCompanyName] = @ShortCompanyName,
               [AddressLine1] = @AddressLine1,
               [Phone1] = @Phone1,
               [Mobile1] = @Mobile1,
               [Fax] = @Fax,
               [Email1] = @Email1,
               [Web] = @Web,
               [LogoUrl] = @LogoUrl,
               [UpdateUser] = @UpdateUser,
               [UpdateDate] = GETDATE()
         WHERE [RowId] = @RowId",

                    new SqlParameter("@RowId",
                        HrCompanyInformation.RowId),

                    new SqlParameter("@CompanyName",
                        (object)HrCompanyInformation.CompanyName ?? DBNull.Value),

                    new SqlParameter("@ShortCompanyName",
                        (object)HrCompanyInformation.ShortCompanyName ?? DBNull.Value),

                    new SqlParameter("@AddressLine1",
                        (object)HrCompanyInformation.AddressLine1 ?? DBNull.Value),

                    new SqlParameter("@Phone1",
                        (object)HrCompanyInformation.Phone1 ?? DBNull.Value),

                    new SqlParameter("@Mobile1",
                        (object)HrCompanyInformation.Mobile1 ?? DBNull.Value),

                    new SqlParameter("@Fax",
                        (object)HrCompanyInformation.Fax ?? DBNull.Value),

                    new SqlParameter("@Email1",
                        (object)HrCompanyInformation.Email1 ?? DBNull.Value),

                    new SqlParameter("@Web",
                        (object)HrCompanyInformation.Web ?? DBNull.Value),

                    new SqlParameter("@LogoUrl",
                        (object)HrCompanyInformation.LogoUrl ?? DBNull.Value),

                    new SqlParameter("@UpdateUser",
                        (object)loggedInUser ?? DBNull.Value)
                );
            }
        }
        public static void DeleteHrCompanyInformation(HrCompanyInformationInfo HrCompanyInformation, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrCompanyInformation]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrCompanyInformation.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrCompanyInformationResponse GetFilteredHrCompanyInformations(HttpRequestBase Request)
        {
            HrCompanyInformationResponse resp = new HrCompanyInformationResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrCompanyInformations = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrCompanyInformation] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, CompanyName) > 0 ");
                    //sb.AppendLine(" AND  CHARINDEX(@SearchValue, ShortCompanyName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrCompanyInformations = repo.Database.SqlQuery<HrCompanyInformationInfo>(string.Format(@"
                          SELECT 
                            a.[RowId]
                          ,a.[CompanyId]
                          ,a.[CompanyName]
                          ,a.[ShortCompanyName]
                          ,a.[AddressLine1]
                       --   ,a.[AddressLine2]
                          ,a.[Phone1]
                          ,a.[Mobile1]
                          ,a.[Email1]
                       --   ,a.[Web]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}