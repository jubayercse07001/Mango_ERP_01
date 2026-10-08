using ERP.Models.HR.Employee.HrEmployeeTrainingCategory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeTrainingCategoryDAL
    {
        public static HrEmployeeTrainingCategoryInfo GetTrainingCategoryByID(string RowId)
        {
            HrEmployeeTrainingCategoryInfo category = null;

            using (var repo = new GMContext())
            {
                category = repo.Database.SqlQuery<HrEmployeeTrainingCategoryInfo>(@"
                    SELECT
                        RowId,
                        TrainingCategoryId,
                        TrainingCategoryName,
                        DomainStatus
                    FROM HrEmployeeTrainingCategory
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return category;
        }

        public static HrEmployeeTrainingCategoryInfo GetHrTrainingCategoryByName(string TrainingCategoryName, string RowId)
        {
            HrEmployeeTrainingCategoryInfo category = null;

            using (var repo = new GMContext())
            {
                category = repo.Database.SqlQuery<HrEmployeeTrainingCategoryInfo>(@"
                    SELECT *
                    FROM [dbo].[HrEmployeeTrainingCategory]
                    WHERE TrainingCategoryName = @TrainingCategoryName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@TrainingCategoryName", TrainingCategoryName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return category;
        }

        public static List<HrEmployeeTrainingCategoryInfo> GetAllHrTrainingCategories()
        {
            List<HrEmployeeTrainingCategoryInfo> categories = null;

            using (var repo = new GMContext())
            {
                categories = repo.Database.SqlQuery<HrEmployeeTrainingCategoryInfo>(@"
                    SELECT
                        [RowId],
                        [TrainingCategoryId],
                        [TrainingCategoryName],
                        [DomainStatus]
                    FROM [dbo].[HrEmployeeTrainingCategory]
                    WHERE DomainStatus = 1
                    ORDER BY TrainingCategoryId
                ").ToList();
            }

            return categories;
        }

        public static void AddHrTrainingCategory(HrEmployeeTrainingCategoryInfo category, string loggedInUser)
        {
            HrEmployeeTrainingCategoryInfo obj;

            using (var repo = new GMContext())
            {
                // Auto-increment generation logic matching your Division implementation pattern
                obj = repo.Database.SqlQuery<HrEmployeeTrainingCategoryInfo>(@"
                    SELECT ISNULL(MAX(TrainingCategoryId),0)+1 AS TrainingCategoryId
                    FROM HrEmployeeTrainingCategory
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeTrainingCategory]
                    (
                        RowId,
                        TrainingCategoryId,
                        TrainingCategoryName,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @TrainingCategoryId,
                        @TrainingCategoryName,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", category.RowId),
                new SqlParameter("@TrainingCategoryId", obj.TrainingCategoryId),
                new SqlParameter("@TrainingCategoryName", category.TrainingCategoryName),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrTrainingCategory(HrEmployeeTrainingCategoryInfo category, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeTrainingCategory
                    SET
                        TrainingCategoryName = @TrainingCategoryName,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", category.RowId),
                new SqlParameter("@TrainingCategoryName", category.TrainingCategoryName),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrTrainingCategory(HrEmployeeTrainingCategoryInfo category, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeTrainingCategory
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

        public static HrEmployeeTrainingCategoryResponse GetFilteredHrTrainingCategories(HttpRequestBase Request)
        {
            HrEmployeeTrainingCategoryResponse resp = new HrEmployeeTrainingCategoryResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrEmployeeTrainingCategory a
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            TrainingCategoryName LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployeeTrainingCategory WHERE DomainStatus = 1"
                ).Single();

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    parameters.ToArray()
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                // Re-cloning array variables to circumvent EF parameters constraint execution reuse barriers
                var commandParams = parameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray();

                resp.HrTrainingCategories = repo.Database.SqlQuery<HrEmployeeTrainingCategoryInfo>(@"
                    SELECT
                        a.RowId,
                        a.TrainingCategoryId,
                        a.TrainingCategoryName
                    " + sb.ToString() + @"
                    ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                    ",
                    commandParams).ToList();
            }

            return resp;
        }
    }

    // Accompanying Response Wrapper used directly within Paginated Server-side Grids
    public class HrEmployeeTrainingCategoryResponse
    {
        public int TotalRecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
        public List<HrEmployeeTrainingCategoryInfo> HrTrainingCategories { get; set; }
    }
}