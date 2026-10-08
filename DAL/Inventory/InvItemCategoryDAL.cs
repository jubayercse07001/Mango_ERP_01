
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using ERP.DAL;
using ERP.DAL.Inventory;
using ERP.Models;
using ERP.Models.Inventory.InvItemCategory;

namespace ERP.DAL.Inventory
{
    public class InvItemCategoryDAL
    {
        public static InvItemCategoryInfo GetInvItemCategoryByID(string RowId)
        {
            InvItemCategoryInfo category = null;

            using (var repo = new GMContext())
            {
                category = repo.Database.SqlQuery<InvItemCategoryInfo>(@"
                    SELECT
                        RowId,
                        CategoryId,
                        CategoryName,
                        Description
                    FROM InvItemCategory
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return category;
        }

        public static InvItemCategoryInfo GetInvItemCategoryByName(
            string CategoryName,
            string RowId)
        {
            InvItemCategoryInfo category = null;

            using (var repo = new GMContext())
            {
                category = repo.Database.SqlQuery<InvItemCategoryInfo>(@"
                    SELECT *
                    FROM InvItemCategory
                    WHERE CategoryName = @CategoryName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@CategoryName", CategoryName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return category;
        }

        public static List<InvItemCategoryInfo> GetAllInvItemCategories()
        {
            List<InvItemCategoryInfo> categories = null;

            using (var repo = new GMContext())
            {
                categories = repo.Database.SqlQuery<InvItemCategoryInfo>(@"
                    SELECT
                        RowId,
                        CategoryId,
                        CategoryName,
                        Description
                    FROM InvItemCategory
                    WHERE DomainStatus = 1
                    ORDER BY CategoryId
                ").ToList();
            }

            return categories;
        }

        public static void AddInvItemCategory(
            InvItemCategoryInfo category,
            string loggedInUser)
        {
            InvItemCategoryInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvItemCategoryInfo>(@"
                    SELECT
                        ISNULL(MAX(CategoryId), 0) + 1 AS CategoryId
                    FROM InvItemCategory
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvItemCategory
                    (
                        RowId,
                        CategoryId,
                        CategoryName,
                        Description,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @CategoryId,
                        @CategoryName,
                        @Description,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", category.RowId),
                new SqlParameter("@CategoryId", obj.CategoryId),
                new SqlParameter("@CategoryName", category.CategoryName),
                new SqlParameter("@Description", category.Description ?? ""),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvItemCategory(
            InvItemCategoryInfo category,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvItemCategory
                    SET
                        CategoryName = @CategoryName,
                        Description = @Description,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", category.RowId),
                new SqlParameter("@CategoryName", category.CategoryName),
                new SqlParameter("@Description", category.Description ?? ""),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvItemCategory(
            InvItemCategoryInfo category,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvItemCategory
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", category.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvItemCategoryResponse GetFilteredInvItemCategories(HttpRequestBase Request)
        {
            InvItemCategoryResponse resp = new InvItemCategoryResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvItemCategorys = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvItemCategory
                    WHERE DomainStatus = 1
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
                            CategoryName LIKE '%' + @SearchValue + '%'
                            OR Description LIKE '%' + @SearchValue + '%'
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

                resp.InvItemCategorys = repo.Database.SqlQuery<InvItemCategoryInfo>(string.Format(@"
                    SELECT
                         RowId
                        ,CategoryId
                        ,CategoryName
                        ,Description

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