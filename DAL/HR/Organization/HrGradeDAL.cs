using ERP.Models.HR.Organization.HrGrade;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrGradeDAL
    {
        public static HrGradeInfo GetGradeByID(string RowId)
        {
            HrGradeInfo grade = null;

            using (var repo = new GMContext())
            {
                grade = repo.Database.SqlQuery<HrGradeInfo>(@"
                    SELECT
                        a.RowId,
                        a.GradeId,
                        a.GradeName,
                        a.PayScale,
                        a.Rank,
                        a.EmployeeTypeId,
                        b.EmployeeTypeName
                    FROM HrGrade a
                    INNER JOIN HrEmployeeType b
                        ON b.RowId = a.EmployeeTypeId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return grade;
        }

        public static HrGradeInfo GetHrGradeByName(
            string GradeName,
            string RowId)
        {
            HrGradeInfo grade = null;

            using (var repo = new GMContext())
            {
                grade = repo.Database.SqlQuery<HrGradeInfo>(@"
                    SELECT *
                    FROM HrGrade
                    WHERE GradeName = @GradeName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@GradeName", GradeName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return grade;
        }

        public static List<HrGradeInfo> GetAllHrGrades()
        {
            List<HrGradeInfo> grades = null;

            using (var repo = new GMContext())
            {
                grades = repo.Database.SqlQuery<HrGradeInfo>(@"
                    SELECT
                        RowId,
                        GradeId,
                        GradeName,
                        PayScale,
                        EmployeeTypeId,
                        Rank
                    FROM HrGrade
                    WHERE DomainStatus = 1
                    ORDER BY GradeId
                ").ToList();
            }

            return grades;
        }

        public static void AddHrGrade(
            HrGradeInfo grade,
            string loggedInUser)
        {
            HrGradeInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrGradeInfo>(@"
                    SELECT
                        ISNULL(MAX(GradeId),0)+1 AS GradeId
                    FROM HrGrade
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO HrGrade
                    (
                        RowId,
                        GradeId,
                        GradeName,
                        PayScale,
                        EmployeeTypeId,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @GradeId,
                        @GradeName,
                        @PayScale,
                        @EmployeeTypeId,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", grade.RowId),
                new SqlParameter("@GradeId", obj.GradeId),
                new SqlParameter("@GradeName", grade.GradeName),
                new SqlParameter("@PayScale", grade.PayScale ?? ""),
                new SqlParameter("@EmployeeTypeId", grade.EmployeeTypeId),
                new SqlParameter("@Rank", grade.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrGrade(
            HrGradeInfo grade,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrGrade
                    SET
                        GradeName = @GradeName,
                        PayScale = @PayScale,
                        EmployeeTypeId = @EmployeeTypeId,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", grade.RowId),
                new SqlParameter("@GradeName", grade.GradeName),
                new SqlParameter("@PayScale", grade.PayScale ?? ""),
                new SqlParameter("@EmployeeTypeId", grade.EmployeeTypeId),
                new SqlParameter("@Rank", grade.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrGrade(
            HrGradeInfo grade,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrGrade
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", grade.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrGradeResponse GetFilteredHrGrades(HttpRequestBase Request)
        {
            HrGradeResponse resp = new HrGradeResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.HrGrades = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM HrGrade a
            INNER JOIN HrEmployeeType b
                ON b.RowId = a.EmployeeTypeId
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
                    a.GradeName LIKE '%' + @SearchValue + '%'
                    OR a.PayScale LIKE '%' + @SearchValue + '%'
                    OR b.EmployeeTypeName LIKE '%' + @SearchValue + '%'
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
                if (sortColumn == "RowId" || sortColumn == "Rank")
                {
                    sortColumn = "a." + sortColumn;
                }

                resp.HrGrades = repo.Database.SqlQuery<HrGradeInfo>(string.Format(@"
            SELECT
                 a.RowId
                ,a.GradeId
                ,a.GradeName
                ,a.PayScale
                ,CAST(a.Rank AS INT) AS [Rank]
                ,a.EmployeeTypeId
                ,b.EmployeeTypeName

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

        public class HrGradeResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<HrGradeInfo> HrGrades { get; set; }
        }
    }
}