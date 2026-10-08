using ERP.Models.Inventory.InvJob;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory.InvJob
{
    public class InvJobDAL
    {
        public static InvJobInfo GetJobByID(string RowId)
        {
            InvJobInfo job = null;

            using (var repo = new GMContext())
            {
                job = repo.Database.SqlQuery<InvJobInfo>(@"
                    SELECT
                    a.RowId,
                    a.JobId,
                    a.JobNo,
                    a.JobName,
                    a.JobDescription,
                    a.DepartmentId,
                    a.StartDate,
                    a.EndDate,
                    a.Status,
                        CASE
                        WHEN a.Status = 1 THEN 'Open'
                        WHEN a.Status = 2 THEN 'In Progress'
                        WHEN a.Status = 3 THEN 'Completed'
                        WHEN a.Status = 4 THEN 'Cancelled'
                        ELSE 'Unknown'
                    END AS StatusName,

                    b.DepartmentName

                FROM InvJob a
                LEFT JOIN HrDepartment b
                    ON b.RowId = a.DepartmentId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return job;
        }

        public static InvJobInfo GetInvJobByName(
            string JobName,
            string RowId)
        {
            InvJobInfo job = null;

            using (var repo = new GMContext())
            {
                job = repo.Database.SqlQuery<InvJobInfo>(@"
                    SELECT *
                    FROM InvJob
                    WHERE JobName = @JobName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@JobName", JobName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return job;
        }

        public static List<InvJobInfo> GetAllInvJobs()
        {
            List<InvJobInfo> jobs = null;

            using (var repo = new GMContext())
            {
                jobs = repo.Database.SqlQuery<InvJobInfo>(@"
                    SELECT
                        a.RowId,
                        a.JobId,
                        a.JobNo,
                        a.JobName,
                        a.JobDescription,
                        a.DepartmentId,
                        a.StartDate,
                        a.EndDate,
                        a.Status,
                        CASE
                        WHEN a.Status = 1 THEN 'Open'
                        WHEN a.Status = 2 THEN 'In Progress'
                        WHEN a.Status = 3 THEN 'Completed'
                        WHEN a.Status = 4 THEN 'Cancelled'
                        ELSE 'Unknown'
                    END AS StatusName,
                        b.DepartmentName
                    FROM InvJob a
                    LEFT JOIN HrDepartment b
                        ON b.RowId = a.DepartmentId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.JobId
                ").ToList();
            }

            return jobs;
        }

        public static int GetNextJobNo()
        {
            using (var repo = new GMContext())
            {
                int maxRank = repo.Database.SqlQuery<int?>(@"
             SELECT ISNULL(MAX(JobId), 0) 
            FROM InvJob
            WHERE DomainStatus = 1
        ").FirstOrDefault() ?? 0;

                return maxRank + 1;
            }
        }
        public static void AddInvJob(
            InvJobInfo job,
            string loggedInUser)
        {
            InvJobInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvJobInfo>(@"
                    SELECT
                        ISNULL(MAX(JobId),0)+1 AS JobId
                    FROM InvJob
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvJob
                    (
                        RowId,
                        JobId,
                        JobNo,
                        JobName,
                        JobDescription,
                        DepartmentId,
                        StartDate,
                        EndDate,
                        Status,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @JobId,
                        @JobNo,
                        @JobName,
                        @JobDescription,
                        @DepartmentId,
                        @StartDate,
                        @EndDate,
                        @Status,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", job.RowId),
                new SqlParameter("@JobId", obj.JobId),
                new SqlParameter("@JobNo", job.JobNo ?? (object)DBNull.Value),
                new SqlParameter("@JobName", job.JobName),
                new SqlParameter("@JobDescription", job.JobDescription ?? (object)DBNull.Value),
                new SqlParameter("@DepartmentId", (object)job.DepartmentId ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)job.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)job.EndDate ?? DBNull.Value),
                new SqlParameter("@Status", (object)job.Status ?? DBNull.Value),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvJob(
            InvJobInfo job,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvJob
                    SET
                        JobNo = @JobNo,
                        JobName = @JobName,
                        JobDescription = @JobDescription,
                        DepartmentId = @DepartmentId,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        Status= @Status,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", job.RowId),
                new SqlParameter("@JobNo", job.JobNo ?? (object)DBNull.Value),
                new SqlParameter("@JobName", job.JobName),
                new SqlParameter("@JobDescription", job.JobDescription ?? (object)DBNull.Value),
                new SqlParameter("@DepartmentId", (object)job.DepartmentId ?? DBNull.Value),
                new SqlParameter("@StartDate", (object)job.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object)job.EndDate ?? DBNull.Value),
                new SqlParameter("@Status", (object)job.Status ?? DBNull.Value),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvJob(
            InvJobInfo job,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvJob
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", job.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvJobResponse GetFilteredInvJobs(HttpRequestBase Request)
        {
            InvJobResponse resp = new InvJobResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvJobs = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM InvJob a
            LEFT JOIN HrDepartment b
                ON b.RowId = a.DepartmentId
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
                    a.JobNo LIKE '%' + @SearchValue + '%'
                    OR a.JobName LIKE '%' + @SearchValue + '%'
                    OR a.JobDescription LIKE '%' + @SearchValue + '%'
                    OR b.DepartmentName LIKE '%' + @SearchValue + '%'
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
                if (sortColumn == "RowId" || sortColumn == "JobId" || sortColumn == "JobNo" || sortColumn == "JobName" || sortColumn == "StartDate" || sortColumn == "EndDate")
                {
                    sortColumn = "a." + sortColumn;
                }
                else if (sortColumn == "DepartmentName")
                {
                    sortColumn = "b." + sortColumn;
                }

                resp.InvJobs = repo.Database.SqlQuery<InvJobInfo>(string.Format(@"
            SELECT
                 a.RowId
                ,a.JobId
                ,a.JobNo
                ,a.JobName
                ,a.JobDescription
                ,a.DepartmentId
                ,a.StartDate
                ,a.EndDate
                ,CASE
                        WHEN a.Status = 1 THEN 'Open'
                        WHEN a.Status = 2 THEN 'In Progress'
                        WHEN a.Status = 3 THEN 'Completed'
                        WHEN a.Status = 4 THEN 'Cancelled'
                        ELSE 'Unknown'
                    END AS StatusName
                ,b.DepartmentName

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

        public class InvJobResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<InvJobInfo> InvJobs { get; set; }
        }
    }
}