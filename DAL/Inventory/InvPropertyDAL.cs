using ERP.Models.Inventory.InvProperty;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory.InvProperty
{
    public class InvPropertyDAL
    {
        public static InvPropertyInfo GetPropertyByID(string RowId)
        {
            InvPropertyInfo property = null;

            using (var repo = new GMContext())
            {
                property = repo.Database.SqlQuery<InvPropertyInfo>(@"
                    SELECT
                        a.RowId,
                        a.PropertyId,
                        a.PropertyName,
                        a.PropertyDescription,
                        a.GroupId,
                        g.GroupName
                    FROM InvProperty a
                    INNER JOIN InvGroup g on g.RowId = a.GroupId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return property;
        }

        public static InvPropertyInfo GetInvPropertyByName(string PropertyName, string RowId)
        {
            InvPropertyInfo property = null;

            using (var repo = new GMContext())
            {
                property = repo.Database.SqlQuery<InvPropertyInfo>(@"
                    SELECT 
                        RowId,
                        PropertyId,
                        PropertyName,
                        PropertyDescription,
                        GroupId
                    FROM InvProperty
                    WHERE PropertyName = @PropertyName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@PropertyName", PropertyName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return property;
        }

        public static List<InvPropertyInfo> GetAllInvProperties()
        {
            List<InvPropertyInfo> properties = null;

            using (var repo = new GMContext())
            {
                properties = repo.Database.SqlQuery<InvPropertyInfo>(@"
                    SELECT
                        RowId,
                        PropertyId,
                        PropertyName,
                        PropertyDescription,
                        GroupId
                    FROM InvProperty
                    WHERE DomainStatus = 1
                    ORDER BY PropertyId
                ").ToList();
            }

            return properties;
        }

        public static void AddInvProperty(InvPropertyInfo property, string loggedInUser)
        {
            InvPropertyInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvPropertyInfo>(@"
                    SELECT
                        ISNULL(MAX(PropertyId), 0) + 1 AS PropertyId
                    FROM InvProperty
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvProperty
                    (
                        RowId,
                        PropertyId,
                        PropertyName,
                        PropertyDescription,
                        GroupId,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @PropertyId,
                        @PropertyName,
                        @PropertyDescription,
                        @GroupId,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", property.RowId),
                new SqlParameter("@PropertyId", obj != null ? obj.PropertyId : 1),
                new SqlParameter("@PropertyName", property.PropertyName),
                new SqlParameter("@PropertyDescription", property.PropertyDescription ?? ""),
                new SqlParameter("@GroupId", (object)property.GroupId ?? DBNull.Value),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvProperty(InvPropertyInfo property, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvProperty
                    SET
                        PropertyName = @PropertyName,
                        PropertyDescription = @PropertyDescription,
                        GroupId = @GroupId,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", property.RowId),
                new SqlParameter("@PropertyName", property.PropertyName),
                new SqlParameter("@PropertyDescription", property.PropertyDescription ?? ""),
                new SqlParameter("@GroupId", (object)property.GroupId ?? DBNull.Value),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvProperty(InvPropertyInfo property, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvProperty
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", property.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvPropertyResponse GetFilteredInvProperties(HttpRequestBase Request)
        {
            InvPropertyResponse resp = new InvPropertyResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvProperties = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvProperty a
                    INNER JOIN InvGroup g on g.RowId = a.GroupId
                    WHERE a.DomainStatus = 1
                ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                            a.PropertyName LIKE '%' + @SearchValue + '%'
                            OR a.PropertyDescription LIKE '%' + @SearchValue + '%'
                        )");

                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString()), sqlParameters.ToArray()).Single();

                List<SqlParameter> sqlParameters1 = new List<SqlParameter>(sqlParameters)
                {
                    new SqlParameter("@Skip", gmdt.Start),
                    new SqlParameter("@Take", gmdt.Length)
                };

                string sortColumn = gmdt.SortColum.ToString();
                if (sortColumn == "RowId" || sortColumn == "PropertyId" || sortColumn == "PropertyName")
                {
                    sortColumn = "a." + sortColumn;
                }

                resp.InvProperties = repo.Database.SqlQuery<InvPropertyInfo>(string.Format(@"
                    SELECT
                        a.RowId,
                        a.PropertyId,
                        a.PropertyName,
                        a.PropertyDescription,
                        g.GroupName
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

        public class InvPropertyResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<InvPropertyInfo> InvProperties { get; set; }
        }
    }
}