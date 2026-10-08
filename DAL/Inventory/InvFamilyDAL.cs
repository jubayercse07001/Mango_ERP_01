using ERP.Models.Inventory.InvFamily;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvFamilyDAL
    {
        public static InvFamilyInfo GetFamilyByID(string RowId)
        {
            InvFamilyInfo family = null;

            using (var repo = new GMContext())
            {
                family = repo.Database.SqlQuery<InvFamilyInfo>(@"
                    SELECT
                        a.RowId,
                        a.FamilyId,
                        a.FamilyName,
                        a.ClassId,
                        b.ClassName
                    FROM InvFamily a
                    LEFT JOIN InvClass b
                        ON b.RowId = a.ClassId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return family;
        }

        public static InvFamilyInfo GetInvFamilyByName(
            string FamilyName,
            string RowId)
        {
            InvFamilyInfo family = null;

            using (var repo = new GMContext())
            {
                family = repo.Database.SqlQuery<InvFamilyInfo>(@"
                    SELECT *
                    FROM InvFamily
                    WHERE FamilyName = @FamilyName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@FamilyName", FamilyName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return family;
        }

        public static List<InvFamilyInfo> GetAllInvFamilies()
        {
            List<InvFamilyInfo> families = null;

            using (var repo = new GMContext())
            {
                families = repo.Database.SqlQuery<InvFamilyInfo>(@"
                    SELECT
                        RowId,
                        FamilyId,
                        FamilyName,
                        ClassId
                    FROM InvFamily
                    WHERE DomainStatus = 1
                    ORDER BY FamilyId
                ").ToList();
            }

            return families;
        }

        public static void AddInvFamily(
            InvFamilyInfo family,
            string loggedInUser)
        {
            InvFamilyInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvFamilyInfo>(@"
                    SELECT
                        ISNULL(MAX(FamilyId),0)+1 AS FamilyId
                    FROM InvFamily
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvFamily
                    (
                        RowId,
                        FamilyId,
                        FamilyName,
                        ClassId,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @FamilyId,
                        @FamilyName,
                        @ClassId,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", family.RowId),
                new SqlParameter("@FamilyId", obj.FamilyId),
                new SqlParameter("@FamilyName", family.FamilyName),
                new SqlParameter("@ClassId", (object)family.ClassId ?? DBNull.Value),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvFamily(
            InvFamilyInfo family,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvFamily
                    SET
                        FamilyName = @FamilyName,
                        ClassId = @ClassId,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", family.RowId),
                new SqlParameter("@FamilyName", family.FamilyName),
                new SqlParameter("@ClassId", (object)family.ClassId ?? DBNull.Value),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvFamily(
            InvFamilyInfo family,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvFamily
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", family.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvFamilyResponse GetFilteredInvFamilies(HttpRequestBase Request)
        {
            InvFamilyResponse resp = new InvFamilyResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvFamilies = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM InvFamily a
            LEFT JOIN InvClass b
                ON b.RowId = a.ClassId
            WHERE a.DomainStatus = 1
        ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
            SELECT COUNT(*) {0}
        ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                AND
                (
                    a.FamilyName LIKE '%' + @SearchValue + '%'
                    OR b.ClassName LIKE '%' + @SearchValue + '%'
                )");

                    sqlParameters.Add(
                        new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
            SELECT COUNT(*) {0}
        ", sb.ToString()), sqlParameters.ToArray()).Single();

                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter("@Take", gmdt.Length));

                string sortColumn = gmdt.SortColum.ToString();
                if (sortColumn == "RowId" || sortColumn == "FamilyId" || sortColumn == "FamilyName")
                {
                    sortColumn = "a." + sortColumn;
                }
                else if (sortColumn == "ClassName")
                {
                    sortColumn = "b.ClassName";
                }

                resp.InvFamilies = repo.Database.SqlQuery<InvFamilyInfo>(string.Format(@"
            SELECT
                 a.RowId
                ,a.FamilyId
                ,a.FamilyName
                ,a.ClassId
                ,b.ClassName

            {0}

            ORDER BY {1} {2}

            OFFSET (@Skip) ROWS
            FETCH NEXT (@Take) ROWS ONLY
        ",
                sb.ToString(),
                sortColumn,
                gmdt.SortDir),
                sqlParameters1.ToArray()).ToList();
            }

            return resp;
        }

        public class InvFamilyResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<InvFamilyInfo> InvFamilies { get; set; }
        }
    }
}