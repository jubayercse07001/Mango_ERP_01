using ERP.Models.HR.Organization.HrDesignation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrDesignationDAL
    {
        public static HrDesignationInfo GetDesignationByID(string RowId)
        {
            HrDesignationInfo HrDesignation = null;

            using (var repo = new GMContext())
            {
                HrDesignation = repo.Database.SqlQuery<HrDesignationInfo>(@"

                    SELECT
                         a.[RowId]
                        ,a.[DesignationId]
                        ,a.[DesignationName]
                        ,a.[ShortDesignationName]
                        ,a.[JobDescription]
                        ,a.[JobSpecification]
                        ,a.[Rank]
                        ,f.DesignationGroup AS DesignationGroupName

                    FROM HrDesignation a
                    LEFT JOIN HrDesignationGroup f
                        ON a.DesignationGroupId = f.RowId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1

                ", new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return HrDesignation;
        }

        public static HrDesignationInfo GetHrDesignationByName(string DesignationName,string RowId)
        {
            HrDesignationInfo HrDesignation = null;

            using (var repo = new GMContext())
            {
                HrDesignation = repo.Database.SqlQuery<HrDesignationInfo>(@"

                    SELECT
                         RowId
                        ,DesignationId
                        ,DesignationName

                    FROM HrDesignation

                    WHERE DesignationName = @DesignationName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1

                ",
                new SqlParameter("@DesignationName", DesignationName),
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return HrDesignation;
        }

        public static List<HrDesignationInfo> GetAllHrDesignations()
        {
            List<HrDesignationInfo> list = null;

            using (var repo = new GMContext())
            {
                list = repo.Database.SqlQuery<HrDesignationInfo>(@"

                    SELECT
                         a.RowId
                        ,a.DesignationId
                        ,a.DesignationName
                        ,a.ShortDesignationName
                        ,a.Rank
                        ,f.DesignationGroup AS DesignationGroupName
                        ,(SELECT ISNULL(MAX([Rank]), 0) + 1 FROM [HrDesignation]) AS NextRank
                    FROM HrDesignation a
                    LEFT JOIN HrDesignationGroup f ON a.DesignationGroupId = f.RowId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.DesignationId

                ").ToList();
            }

            return list;
        }

        public static void AddHrDesignation(
            HrDesignationInfo HrDesignation,
            string loggedInUser)
        {
            HrDesignationInfo loc;

            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrDesignationInfo>(@"

                    SELECT
                        ISNULL(MAX(DesignationId),0)+1
                        AS DesignationId
                    FROM HrDesignation
                    WHERE DomainStatus = 1

                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"

                    INSERT INTO HrDesignation
                    (
                         RowId
                        ,DesignationId
                        ,DesignationName
                        ,ShortDesignationName
                        ,JobDescription
                        ,JobSpecification
                        ,Rank
                        ,DesignationGroupId
                        ,CreateUser
                        ,CreateDate
                        ,DomainStatus
                    )

                    VALUES
                    (
                         @RowId
                        ,@DesignationId
                        ,@DesignationName
                        ,@ShortDesignationName
                        ,@JobDescription
                        ,@JobSpecification
                        ,@Rank
                        ,@DesignationGroupId
                        ,@CreateUser
                        ,GETDATE()
                        ,1
                    )

                ",
                new SqlParameter("@RowId", HrDesignation.RowId),
                new SqlParameter("@DesignationId", loc.DesignationId),
                new SqlParameter("@DesignationName", HrDesignation.DesignationName),
                new SqlParameter("@ShortDesignationName", (object)HrDesignation.ShortDesignationName ?? DBNull.Value),
                new SqlParameter("@JobDescription", (object)HrDesignation.JobDescription ?? DBNull.Value),
                new SqlParameter("@JobSpecification", (object)HrDesignation.JobSpecification ?? DBNull.Value),
                new SqlParameter("@Rank", HrDesignation.Rank),
                new SqlParameter("@DesignationGroupId", HrDesignation.DesignationGroupId),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrDesignation(HrDesignationInfo HrDesignation, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
            UPDATE HrDesignation
            SET
                 DesignationName = @DesignationName
                ,ShortDesignationName = @ShortDesignationName
                ,JobDescription = @JobDescription
                ,JobSpecification = @JobSpecification
                ,Rank = @Rank
                ,DesignationGroupId = @DesignationGroupId
                ,UpdateUser = @UpdateUser
                ,UpdateDate = GETDATE()
            WHERE RowId = @RowId
        ",
                new SqlParameter("@RowId", HrDesignation.RowId),
                new SqlParameter("@DesignationName", HrDesignation.DesignationName ?? (object)DBNull.Value),
                new SqlParameter("@ShortDesignationName", (object)HrDesignation.ShortDesignationName ?? DBNull.Value),
                new SqlParameter("@JobDescription", (object)HrDesignation.JobDescription ?? DBNull.Value),
                new SqlParameter("@JobSpecification", (object)HrDesignation.JobSpecification ?? DBNull.Value),
                new SqlParameter("@Rank", HrDesignation.Rank),
                new SqlParameter("@DesignationGroupId", (object)HrDesignation.DesignationGroupId ?? DBNull.Value),
                new SqlParameter("@UpdateUser", loggedInUser ?? (object)DBNull.Value));
            }
        }

        public static void DeleteHrDesignation(
            HrDesignationInfo HrDesignation,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"

                    UPDATE HrDesignation

                    SET
                         DomainStatus = 0
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()

                    WHERE RowId = @RowId

                ",
                new SqlParameter("@RowId", HrDesignation.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrDesignationResponse GetFilteredHrDesignations(
            HttpRequestBase Request)
        {
            HrDesignationResponse resp =
                new HrDesignationResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"

                    FROM HrDesignation a LEFT JOIN HrDesignationGroup f
                        ON a.DesignationGroupId = f.RowId
                    WHERE a.DomainStatus = 1

                ");

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString())
                    .Single();

                List<SqlParameter> sqlParameters =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"

                        AND
                        (
                            CHARINDEX(@SearchValue,a.DesignationName) > 0
                            OR CHARINDEX(@SearchValue,f.DesignationGroup) > 0
                        )

                    ");

                    sqlParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    sqlParameters.ToArray())
                    .Single();

                sqlParameters.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                sqlParameters.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrDesignations =
                    repo.Database.SqlQuery<HrDesignationInfo>(
                    string.Format(@"

                    SELECT
                         a.RowId
                        ,a.DesignationId
                        ,a.DesignationName
                        ,a.ShortDesignationName
                        ,a.JobDescription
                        ,a.JobSpecification
                        ,a.Rank
                        ,f.DesignationGroup AS DesignationGroupName

                    {0}

                    ORDER BY {1} {2}

                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY

                    ",
                    sb.ToString(),
                    gmdt.SortColum,
                    gmdt.SortDir),
                    sqlParameters.ToArray()).ToList();
            }

            return resp;
        }
    }
}