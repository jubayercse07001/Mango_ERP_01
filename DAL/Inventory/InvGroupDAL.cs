using ERP.Models.Inventory.InvGroup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvGroupDAL
    {
        public static InvGroupInfo GetInvGroupByID(string RowId)
        {
            InvGroupInfo group = null;

            using (var repo = new GMContext())
            {
                group = repo.Database.SqlQuery<InvGroupInfo>(@"
                    SELECT
                        RowId,
                        GroupId,
                        GroupName,
                        GroupDescription,
                        Rank
                    FROM InvGroup
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId ?? (object)DBNull.Value))
                .FirstOrDefault();
            }

            return group;
        }

        public static InvGroupInfo GetInvGroupByName(
            string GroupName,
            string RowId)
        {
            InvGroupInfo group = null;

            using (var repo = new GMContext())
            {
                group = repo.Database.SqlQuery<InvGroupInfo>(@"
                    SELECT
                        RowId,
                        GroupId,
                        GroupName,
                        GroupDescription,
                        Rank
                    FROM InvGroup
                    WHERE GroupName = @GroupName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@GroupName", GroupName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return group;
        }

        public static List<InvGroupInfo> GetAllInvGroups()
        {
            List<InvGroupInfo> groups = null;

            using (var repo = new GMContext())
            {
                groups = repo.Database.SqlQuery<InvGroupInfo>(@"
                    SELECT
                        RowId,
                        GroupId,
                        GroupName,
                        GroupDescription,
                        Rank
                    FROM InvGroup
                    WHERE DomainStatus = 1
                    ORDER BY GroupId
                ").ToList();
            }

            return groups;
        }

        public static int GetNextRank()
        {
            using (var repo = new GMContext())
            {
                int maxRank = repo.Database.SqlQuery<int?>(@"
            SELECT MAX(Rank) 
            FROM [Mango_ERP].[dbo].[InvGroup] 
            WHERE DomainStatus = 1
        ").FirstOrDefault() ?? 0;

                return maxRank + 1;
            }
        }
        public static void AddInvGroup(
            InvGroupInfo group,
            string loggedInUser)
        {
            InvGroupInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvGroupInfo>(@"
                    SELECT
                        ISNULL(MAX(GroupId),0)+1 AS GroupId
                    FROM InvGroup
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvGroup
                    (
                        RowId,
                        GroupId,
                        GroupName,
                        GroupDescription,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @GroupId,
                        @GroupName,
                        @GroupDescription,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", group.RowId),
                new SqlParameter("@GroupId", obj.GroupId),
                new SqlParameter("@GroupName", group.GroupName),
                new SqlParameter("@GroupDescription", group.GroupDescription ?? ""),
                new SqlParameter("@Rank", group.Rank),
                new SqlParameter("@CreateUser", loggedInUser ?? (object)DBNull.Value));
            }
        }

        public static void UpdateInvGroup(
            InvGroupInfo group,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvGroup
                    SET
                        GroupName = @GroupName,
                        GroupDescription = @GroupDescription,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", group.RowId),
                new SqlParameter("@GroupName", group.GroupName),
                new SqlParameter("@GroupDescription", group.GroupDescription ?? ""),
                new SqlParameter("@Rank", group.Rank),
                new SqlParameter("@UpdateUser", loggedInUser ?? (object)DBNull.Value));
            }
        }

        public static void DeleteInvGroup(
            InvGroupInfo group,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvGroup
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", group.RowId),
                new SqlParameter("@UpdateUser", loggedInUser ?? (object)DBNull.Value));
            }
        }

        public static InvGroupResponse GetFilteredInvGroups(HttpRequestBase Request)
        {
            InvGroupResponse resp = new InvGroupResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvGroups = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvGroup a
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
                            a.GroupName LIKE '%' + @SearchValue + '%'
                            OR a.GroupDescription LIKE '%' + @SearchValue + '%'
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
                if (sortColumn == "RowId" || sortColumn == "GroupId" || sortColumn == "GroupName" || sortColumn == "Rank")
                {
                    sortColumn = "a." + sortColumn;
                }

                resp.InvGroups = repo.Database.SqlQuery<InvGroupInfo>(string.Format(@"
                    SELECT
                         a.RowId
                        ,a.GroupId
                        ,a.GroupName
                        ,a.GroupDescription
                        ,CAST(a.Rank AS INT) AS [Rank]

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
    }
}