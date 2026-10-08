using ERP.Models.HR.Employee.HrEmployeeTraining;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeTrainingDAL
    {
        public static HrEmployeeTrainingInfo GetTrainingByID(string RowId)
        {
            HrEmployeeTrainingInfo training = null;

            using (var repo = new GMContext())
            {
                training = repo.Database.SqlQuery<HrEmployeeTrainingInfo>(@"
                    SELECT
                        a.RowId,
                        a.TrainingId,
                        a.EmployeeId,
                        e.EmployeeName, -- Assumed field name in your employee table
                        a.TrainingSource,
                        a.TrainingListId,
                        tl.TrainingTitle,
                        a.Trainer,
                        a.InstituteName,
                        a.StartDate,
                        a.EndDate,
                        a.Duration,
                        a.DomainStatus
                    FROM HrEmployeeTraining a
                    INNER JOIN HrEmployee e ON e.RowId = a.EmployeeId
                    LEFT JOIN HrEmployeeTrainingList tl ON tl.RowId = a.TrainingListId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return training;
        }
        public static HrEmployeeTrainingInfo GetHrEmployeeTrainingDuplicate(string employeeId, string trainingListId, string rowId)
        {
            HrEmployeeTrainingInfo hrEmployeeTraining = null;
            using (var repo = new GMContext())
            {
                hrEmployeeTraining = repo.Database.SqlQuery<HrEmployeeTrainingInfo>(@"
            SELECT [RowId]
                  ,[EmployeeId]
                  ,[TrainingListId]
                  
            FROM [dbo].[HrEmployeeTraining] 
            WHERE [EmployeeId] = @EmployeeId
              AND [TrainingListId] = @TrainingListId
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1",
                    new SqlParameter("@EmployeeId", employeeId),
                    new SqlParameter("@TrainingListId", trainingListId),
                    new SqlParameter("@RowId", string.IsNullOrEmpty(rowId) ? (object)DBNull.Value : rowId)
                ).FirstOrDefault();
            }
            return hrEmployeeTraining;
        }
        public static List<HrEmployeeTrainingInfo> GetAllHrEmployeeTrainings()
        {
            List<HrEmployeeTrainingInfo> trainings = null;

            using (var repo = new GMContext())
            {
                trainings = repo.Database.SqlQuery<HrEmployeeTrainingInfo>(@"
                    SELECT
                        [RowId],
                        [TrainingId],
                        [EmployeeId],
                        [TrainingSource],
                        [TrainingListId],
                        [Trainer],
                        [InstituteName],
                        [StartDate],
                        [EndDate],
                        [Duration],
                        [DomainStatus]
                    FROM [dbo].[HrEmployeeTraining]
                    WHERE DomainStatus = 1
                    ORDER BY TrainingId
                ").ToList();
            }

            return trainings;
        }

        public static void AddHrEmployeeTraining(HrEmployeeTrainingInfo training, string loggedInUser)
        {
            HrEmployeeTrainingInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrEmployeeTrainingInfo>(@"
                    SELECT ISNULL(MAX(TrainingId), 0) + 1 AS TrainingId
                    FROM HrEmployeeTraining
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeTraining]
                    (
                        RowId,
                        TrainingId,
                        EmployeeId,
                        TrainingSource,
                        TrainingListId,
                        Trainer,
                        InstituteName,
                        StartDate,
                        EndDate,
                        Duration,
                        DomainStatus,
                        CreateUser,
                        CreateDate
                    )
                    VALUES
                    (
                        @RowId,
                        @TrainingId,
                        @EmployeeId,
                        @TrainingSource,
                        @TrainingListId,
                        @Trainer,
                        @InstituteName,
                        @StartDate,
                        @EndDate,
                        @Duration,
                        1,
                        @CreateUser,
                        GETDATE()
                    )
                ",
                new SqlParameter("@RowId", training.RowId),
                new SqlParameter("@TrainingId", obj.TrainingId),
                new SqlParameter("@EmployeeId", training.EmployeeId),
                new SqlParameter("@TrainingSource", training.TrainingSource),
                new SqlParameter("@TrainingListId", (object)training.TrainingListId ?? DBNull.Value),
                new SqlParameter("@Trainer", (object)training.Trainer ?? DBNull.Value),
                new SqlParameter("@InstituteName", (object)training.InstituteName ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)training.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)training.EndDate ?? DBNull.Value),
                new SqlParameter("@Duration", (object)training.Duration ?? DBNull.Value),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeTraining(HrEmployeeTrainingInfo training, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeTraining
                    SET
                        EmployeeId = @EmployeeId,
                        TrainingSource = @TrainingSource,
                        TrainingListId = @TrainingListId,
                        Trainer = @Trainer,
                        InstituteName = @InstituteName,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        Duration = @Duration,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", training.RowId),
                new SqlParameter("@EmployeeId", training.EmployeeId),
                new SqlParameter("@TrainingSource", training.TrainingSource),
                new SqlParameter("@TrainingListId", (object)training.TrainingListId ?? DBNull.Value),
                new SqlParameter("@Trainer", (object)training.Trainer ?? DBNull.Value),
                new SqlParameter("@InstituteName", (object)training.InstituteName ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)training.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)training.EndDate ?? DBNull.Value),
                new SqlParameter("@Duration", (object)training.Duration ?? DBNull.Value),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeTraining(HrEmployeeTrainingInfo training, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeTraining
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", training.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeTrainingResponse GetFilteredHrEmployeeTrainings(HttpRequestBase Request)
        {
            HrEmployeeTrainingResponse resp = new HrEmployeeTrainingResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM HrEmployeeTraining a
            INNER JOIN HrEmployee b ON b.RowId = a.EmployeeId
            LEFT JOIN HrEmployeeTrainingList c ON c.RowId = a.TrainingListId
            WHERE a.DomainStatus = 1
        ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                AND (
                    b.EmployeeName LIKE '%' + @SearchValue + '%'
                    OR c.TrainingTitle LIKE '%' + @SearchValue + '%'
                    OR a.Trainer LIKE '%' + @SearchValue + '%'
                    OR a.InstituteName LIKE '%' + @SearchValue + '%'
                )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployeeTraining WHERE DomainStatus = 1"
                ).Single();

                // Clone parameters for query verification runs to avoid parameter binding duplication bugs
                List<SqlParameter> countParams = new List<SqlParameter>();
                foreach (var p in parameters)
                {
                    countParams.Add(new SqlParameter(p.ParameterName, p.Value));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    countParams.ToArray()
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                // FIX: Safely parse the column tracking identifier to a string representation
                string rawSortColumn = gmdt.SortColum.ToString();
                string sortColumn = "a.TrainingId"; // Clear default fallback initialization

                // Safely map both string property tokens and positional integer fallback states
                if (rawSortColumn == "EmployeeName" || rawSortColumn == "1")
                {
                    sortColumn = "b.EmployeeName";
                }
                else if (rawSortColumn == "TrainingTitle" || rawSortColumn == "2")
                {
                    sortColumn = "c.TrainingTitle";
                }
                else if (rawSortColumn == "TrainingId" || rawSortColumn == "0")
                {
                    sortColumn = "a.TrainingId";
                }

                resp.HrEmployeeTrainings = repo.Database.SqlQuery<HrEmployeeTrainingInfo>(@"
            SELECT
                a.RowId,
                a.TrainingId,
                a.EmployeeId,
                b.EmployeeName,
                a.TrainingSource,
                a.TrainingListId,
                c.TrainingTitle,
                a.Trainer,
                a.InstituteName,
                a.StartDate,
                a.EndDate,
                a.Duration
            " + sb.ToString() + @"
            ORDER BY " + sortColumn + " " + gmdt.SortDir + @"
            OFFSET @Skip ROWS
            FETCH NEXT @Take ROWS ONLY
            ",
                    parameters.ToArray()).ToList();
            }

            return resp;
        }
    }
}