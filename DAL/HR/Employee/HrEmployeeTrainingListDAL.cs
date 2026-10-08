using ERP.Models.HR.Employee.HrEmployeeTrainingList;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeTrainingListDAL
    {
        public static HrEmployeeTrainingListInfo GetTrainingListByID(string RowId)
        {
            HrEmployeeTrainingListInfo trainingList = null;

            using (var repo = new GMContext())
            {
                trainingList = repo.Database.SqlQuery<HrEmployeeTrainingListInfo>(@"
                    SELECT
                        a.RowId,
                        a.TrainingListId,
                        a.TrainingCategoryId,
                        a.TrainingTitle,
                        a.Description,
                        b.TrainingCategoryName,
                        a.DomainStatus
                    FROM HrEmployeeTrainingList a
                    INNER JOIN HrEmployeeTrainingCategory b ON b.RowId = a.TrainingCategoryId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return trainingList;
        }

        public static HrEmployeeTrainingListInfo GetHrEmployeeTrainingListByTitle(string TrainingTitle, string RowId)
        {
            HrEmployeeTrainingListInfo trainingList = null;

            using (var repo = new GMContext())
            {
                trainingList = repo.Database.SqlQuery<HrEmployeeTrainingListInfo>(@"
                    SELECT *
                    FROM [dbo].[HrEmployeeTrainingList]
                    WHERE TrainingTitle = @TrainingTitle
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@TrainingTitle", TrainingTitle),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return trainingList;
        }

        public static List<HrEmployeeTrainingListInfo> GetAllHrEmployeeTrainingLists()
        {
            List<HrEmployeeTrainingListInfo> trainingLists = null;

            using (var repo = new GMContext())
            {
                trainingLists = repo.Database.SqlQuery<HrEmployeeTrainingListInfo>(@"
                    SELECT
                        a.[RowId],
                        a.[TrainingListId],
                        a.[TrainingCategoryId],
                        a.[TrainingTitle],
                        a.[Description],
                        b.[TrainingCategoryName],
                        a.[DomainStatus]
                    FROM [dbo].[HrEmployeeTrainingList] a
                    INNER JOIN [dbo].[HrEmployeeTrainingCategory] b ON b.RowId = a.TrainingCategoryId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.TrainingListId
                ").ToList();
            }

            return trainingLists;
        }

        public static void AddHrEmployeeTrainingList(HrEmployeeTrainingListInfo trainingList, string loggedInUser)
        {
            HrEmployeeTrainingListInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrEmployeeTrainingListInfo>(@"
                    SELECT ISNULL(MAX(TrainingListId), 0) + 1 AS TrainingListId
                    FROM HrEmployeeTrainingList
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeTrainingList]
                    (
                        RowId,
                        TrainingListId,
                        TrainingCategoryId,
                        TrainingTitle,
                        Description,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @TrainingListId,
                        @TrainingCategoryId,
                        @TrainingTitle,
                        @Description,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", trainingList.RowId),
                new SqlParameter("@TrainingListId", obj.TrainingListId),
                new SqlParameter("@TrainingCategoryId", trainingList.TrainingCategoryId ?? ""),
                new SqlParameter("@TrainingTitle", trainingList.TrainingTitle),
                new SqlParameter("@Description", trainingList.Description ?? ""),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeTrainingList(HrEmployeeTrainingListInfo trainingList, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeTrainingList
                    SET
                        TrainingCategoryId = @TrainingCategoryId,
                        TrainingTitle = @TrainingTitle,
                        Description = @Description,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", trainingList.RowId),
                new SqlParameter("@TrainingCategoryId", trainingList.TrainingCategoryId ?? ""),
                new SqlParameter("@TrainingTitle", trainingList.TrainingTitle),
                new SqlParameter("@Description", trainingList.Description ?? ""),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeTrainingList(HrEmployeeTrainingListInfo trainingList, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeTrainingList
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", trainingList.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeTrainingListResponse GetFilteredHrEmployeeTrainingLists(HttpRequestBase Request)
{
    HrEmployeeTrainingListResponse resp = new HrEmployeeTrainingListResponse();

    GMDataTableDAL gmdt = new GMDataTableDAL(Request);

    using (var repo = new GMContext())
    {
        StringBuilder sb = new StringBuilder(@"
            FROM HrEmployeeTrainingList a
            INNER JOIN HrEmployeeTrainingCategory b ON b.RowId = a.TrainingCategoryId
            WHERE a.DomainStatus = 1
        ");

        List<SqlParameter> filterParameters = new List<SqlParameter>();

        if (!string.IsNullOrEmpty(gmdt.SearchValue))
        {
            sb.Append(@"
                AND (
                    a.TrainingTitle LIKE '%' + @SearchValue + '%'
                    OR b.TrainingCategoryName LIKE '%' + @SearchValue + '%'
                    OR a.Description LIKE '%' + @SearchValue + '%'
                )");

            filterParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
        }

        resp.TotalRecordCount = repo.Database.SqlQuery<int>(
            "SELECT COUNT(*) FROM HrEmployeeTrainingList WHERE DomainStatus = 1"
        ).Single();

        // 1. Pass a fresh copy array instance here so parameters aren't bound exclusively 
        resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
            "SELECT COUNT(*) " + sb.ToString(),
            filterParameters.Select(p => ((ICloneable)p).Clone()).Cast<SqlParameter>().ToArray()
        ).Single();

        // 2. Build a completely fresh array list for the final data execution pipeline
        List<SqlParameter> finalParameters = new List<SqlParameter>();
        if (!string.IsNullOrEmpty(gmdt.SearchValue))
        {
            finalParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
        }
        finalParameters.Add(new SqlParameter("@Skip", gmdt.Start));
        finalParameters.Add(new SqlParameter("@Take", gmdt.Length));

        // Ensure sortColumn is explicitly handled as a structural database string name
        string sortColumn = gmdt.SortColum.ToString() ?? "TrainingListId";
        
        // Guard against DataTables passing an index number string like "0", "1", "2"
        if (int.TryParse(sortColumn, out int columnIndex))
        {
            // Map column indexes back to fallback property values safely
            switch (columnIndex)
            {
                case 0: sortColumn = "a.TrainingListId"; break;
                case 1: sortColumn = "a.TrainingTitle"; break;
                case 2: sortColumn = "b.TrainingCategoryName"; break;
                case 3: sortColumn = "a.Description"; break;
                default: sortColumn = "a.TrainingListId"; break;
            }
        }
        else
        {
            if (sortColumn == "TrainingCategoryName") { sortColumn = "b.TrainingCategoryName"; }
            else if (!sortColumn.Contains(".")) { sortColumn = "a." + sortColumn; }
        }

        string sortDir = gmdt.SortDir?.ToString() ?? "ASC";
        if (sortDir != "ASC" && sortDir != "DESC") { sortDir = "ASC"; }

        string queryText = string.Format(@"
            SELECT
                a.RowId,
                a.TrainingListId,
                a.TrainingCategoryId,
                a.TrainingTitle,
                a.Description,
                b.TrainingCategoryName
            {0}
            ORDER BY {1} {2}
            OFFSET @Skip ROWS
            FETCH NEXT @Take ROWS ONLY
            ", sb.ToString(), sortColumn, sortDir);

        resp.HrEmployeeTrainingLists = repo.Database.SqlQuery<HrEmployeeTrainingListInfo>(
            queryText,
            finalParameters.ToArray()
        ).ToList();
    }

    return resp;
}
    }
}