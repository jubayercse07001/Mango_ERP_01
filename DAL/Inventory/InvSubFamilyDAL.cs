using ERP.Models.Inventory.InvFamily;
using ERP.Models.Inventory.InvSubFamily;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvSubFamilyDAL
    {
        public static InvSubFamilyInfo GetSubFamilyByID(string RowId)
        {
            InvSubFamilyInfo subFamily = null;

            using (var repo = new GMContext())
            {
                subFamily = repo.Database.SqlQuery<InvSubFamilyInfo>(@"
                    SELECT
                        a.RowId,
                        a.SubFamilyId,
                        a.SubFamilyName,
                        a.FamilyId,
                        b.FamilyName
                    FROM InvSubFamily a
                    INNER JOIN InvFamily b
                        ON b.RowId = a.FamilyId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return subFamily;
        }

        public static InvSubFamilyInfo GetInvSubFamilyByName(
            string SubFamilyName,
            string RowId)
        {
            InvSubFamilyInfo subFamily = null;

            using (var repo = new GMContext())
            {
                subFamily = repo.Database.SqlQuery<InvSubFamilyInfo>(@"
                    SELECT *
                    FROM InvSubFamily
                    WHERE SubFamilyName = @SubFamilyName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@SubFamilyName", SubFamilyName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return subFamily;
        }

        public static List<InvSubFamilyInfo> GetAllInvSubFamilies()
        {
            List<InvSubFamilyInfo> subFamilies = null;

            using (var repo = new GMContext())
            {
                subFamilies = repo.Database.SqlQuery<InvSubFamilyInfo>(@"
                    SELECT
                        a.RowId,
                        a.SubFamilyId,
                        a.SubFamilyName,
                        a.FamilyId,
                        b.FamilyName
                    FROM InvSubFamily a
                    INNER JOIN InvFamily b
                        ON b.RowId = a.FamilyId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.SubFamilyId
                ").ToList();
            }

            return subFamilies;
        }

        public static void AddInvSubFamily(
            InvSubFamilyInfo subFamily,
            string loggedInUser)
        {
            InvSubFamilyInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvSubFamilyInfo>(@"
                    SELECT
                        ISNULL(MAX(SubFamilyId),0)+1 AS SubFamilyId
                    FROM InvSubFamily
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvSubFamily
                    (
                        RowId,
                        SubFamilyId,
                        SubFamilyName,
                        FamilyId,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @SubFamilyId,
                        @SubFamilyName,
                        @FamilyId,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", subFamily.RowId),
                new SqlParameter("@SubFamilyId", obj.SubFamilyId),
                new SqlParameter("@SubFamilyName", subFamily.SubFamilyName),
                new SqlParameter("@FamilyId", subFamily.FamilyId),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvSubFamily(
            InvSubFamilyInfo subFamily,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvSubFamily
                    SET
                        SubFamilyName = @SubFamilyName,
                        FamilyId = @FamilyId,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", subFamily.RowId),
                new SqlParameter("@SubFamilyName", subFamily.SubFamilyName),
                new SqlParameter("@FamilyId", subFamily.FamilyId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvSubFamily(
            InvSubFamilyInfo subFamily,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvSubFamily
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", subFamily.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvSubFamilyResponse GetFilteredInvSubFamilies(HttpRequestBase Request)
        {
            InvSubFamilyResponse resp = new InvSubFamilyResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvSubFamilies = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvSubFamily a
                    INNER JOIN InvFamily b
                        ON b.RowId = a.FamilyId
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
                            a.SubFamilyName LIKE '%' + @SearchValue + '%'
                            OR b.FamilyName LIKE '%' + @SearchValue + '%'
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
                if (sortColumn == "RowId" || sortColumn == "SubFamilyId" || sortColumn == "SubFamilyName")
                {
                    sortColumn = "a." + sortColumn;
                }

                resp.InvSubFamilies = repo.Database.SqlQuery<InvSubFamilyInfo>(string.Format(@"
                    SELECT
                        a.RowId,
                        a.SubFamilyId,
                        a.SubFamilyName,
                        a.FamilyId,
                        b.FamilyName
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

        public class InvSubFamilyResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<InvSubFamilyInfo> InvSubFamilies { get; set; }
        }
    }
}