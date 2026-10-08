using ERP.Models.Inventory.InvClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvClassDAL
    {
        public static InvClassInfo GetInvClassByID(string RowId)
        {
            InvClassInfo invClass = null;

            using (var repo = new GMContext())
            {
                invClass = repo.Database.SqlQuery<InvClassInfo>(@"
                    SELECT
                        a.RowId,
                        a.ClassId,
                        a.ClassName,
                        a.GroupId,
                        b.GroupName
                    FROM InvClass a
                    INNER JOIN InvGroup b
                        ON b.RowId = a.GroupId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return invClass;
        }

        public static InvClassInfo GetInvClassByName(
            string ClassName,
            string RowId)
        {
            InvClassInfo invClass = null;

            using (var repo = new GMContext())
            {
                invClass = repo.Database.SqlQuery<InvClassInfo>(@"
                    SELECT *
                    FROM InvClass
                    WHERE ClassName = @ClassName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@ClassName", ClassName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return invClass;
        }

        public static List<InvClassInfo> GetAllInvClasses()
        {
            List<InvClassInfo> classes = null;

            using (var repo = new GMContext())
            {
                classes = repo.Database.SqlQuery<InvClassInfo>(@"
                    SELECT
                        a.RowId,
                        a.ClassId,
                        a.ClassName,
                        a.GroupId,
                        b.GroupName
                    FROM InvClass a
                    INNER JOIN InvGroup b
                        ON b.RowId = a.GroupId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.ClassId
                ").ToList();
            }

            return classes;
        }

        public static void AddInvClass(
            InvClassInfo invClass,
            string loggedInUser)
        {
            InvClassInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvClassInfo>(@"
                    SELECT
                        ISNULL(MAX(ClassId),0)+1 AS ClassId
                    FROM InvClass
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvClass
                    (
                        RowId,
                        ClassId,
                        ClassName,
                        GroupId,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @ClassId,
                        @ClassName,
                        @GroupId,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", invClass.RowId),
                new SqlParameter("@ClassId", obj.ClassId),
                new SqlParameter("@ClassName", invClass.ClassName),
                new SqlParameter("@GroupId", invClass.GroupId),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvClass(
            InvClassInfo invClass,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvClass
                    SET
                        ClassName = @ClassName,
                        GroupId = @GroupId,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", invClass.RowId),
                new SqlParameter("@ClassName", invClass.ClassName),
                new SqlParameter("@GroupId", invClass.GroupId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvClass(
            InvClassInfo invClass,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvClass
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", invClass.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvClassResponse GetFilteredInvClasses(HttpRequestBase Request)
        {
            InvClassResponse resp = new InvClassResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvClasss = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvClass a
                    INNER JOIN InvGroup b
                        ON b.RowId = a.GroupId
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
                            a.ClassName LIKE '%' + @SearchValue + '%'
                            OR b.GroupName LIKE '%' + @SearchValue + '%'
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

                // Guard table mapping context when processing dynamic DataTables ordering field indicators
                string sortColumn = gmdt.SortColum.ToString();
                if (sortColumn == "RowId" || sortColumn == "ClassId" || sortColumn == "ClassName")
                {
                    sortColumn = "a." + sortColumn;
                }
                else if (sortColumn == "GroupName")
                {
                    sortColumn = "b." + sortColumn;
                }

                resp.InvClasss = repo.Database.SqlQuery<InvClassInfo>(string.Format(@"
                    SELECT
                         a.RowId
                        ,a.ClassId
                        ,a.ClassName
                        ,a.GroupId
                        ,b.GroupName

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