using ERP.Models.HR.Organization.HrDepartmentDesignationMap;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrDepartmentDesignationMapDAL
    {
        public static HrDepartmentDesignationMapInfo GetMapByID(string RowId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDepartmentDesignationMapInfo>(@"
                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.DepartmentId
                        ,a.DesignationId
                        ,b.DepartmentName
                        ,c.DesignationName
                    FROM HrDepartmentDesignationMap a
                    LEFT JOIN HrDepartment b
                        ON a.DepartmentId = b.RowId
                    LEFT JOIN HrDesignation c
                        ON a.DesignationId = c.RowId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }
        }

        public static List<HrDepartmentDesignationMapInfo> GetAllMaps()
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDepartmentDesignationMapInfo>(@"
                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.DepartmentId
                        ,a.DesignationId
                        ,b.DepartmentName
                        ,c.DesignationName
                    FROM HrDepartmentDesignationMap a
                    LEFT JOIN HrDepartment b
                        ON a.DepartmentId = b.RowId
                    LEFT JOIN HrDesignation c
                        ON a.DesignationId = c.RowId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.MapId
                ").ToList();
            }
        }

        public static HrDepartmentDesignationMapInfo GetDuplicateMap(
            string DepartmentId,
            string DesignationId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDepartmentDesignationMapInfo>(@"
                    SELECT
                         RowId
                        ,MapId
                        ,DepartmentId
                        ,DesignationId
                    FROM HrDepartmentDesignationMap
                    WHERE DepartmentId = @DepartmentId
                    AND DesignationId = @DesignationId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@DepartmentId", DepartmentId),
                new SqlParameter("@DesignationId", DesignationId)
                )
                .FirstOrDefault();
            }
        }

        public static void AddMap(
            HrDepartmentDesignationMapInfo map,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                var obj = repo.Database
                    .SqlQuery<HrDepartmentDesignationMapInfo>(@"
                        SELECT
                            ISNULL(MAX(MapId),0)+1 AS MapId
                        FROM HrDepartmentDesignationMap
                    ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                INSERT INTO HrDepartmentDesignationMap
                (
                     RowId
                    ,MapId
                    ,DepartmentId
                    ,DesignationId
                    ,CreateUser
                    ,CreateDate
                    ,DomainStatus
                )
                VALUES
                (
                     @RowId
                    ,@MapId
                    ,@DepartmentId
                    ,@DesignationId
                    ,@CreateUser
                    ,GETDATE()
                    ,1
                )
                ",
                 new SqlParameter("@RowId", map.RowId),
                 new SqlParameter("@MapId", obj.MapId),
                 new SqlParameter("@DepartmentId", map.DepartmentId),
                 new SqlParameter("@DesignationId", map.DesignationId),
                 new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateMap(
            HrDepartmentDesignationMapInfo map,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDepartmentDesignationMap
                    SET
                         DepartmentId = @DepartmentId
                        ,DesignationId = @DesignationId
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", map.RowId),
                new SqlParameter("@DepartmentId", map.DepartmentId),
                new SqlParameter("@DesignationId", map.DesignationId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteMap(
            HrDepartmentDesignationMapInfo map,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDepartmentDesignationMap
                    SET
                         DomainStatus = 0
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", map.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static List<HrDepartmentDesignationMapInfo>
            GetDesignationByDepartment(string DepartmentId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDepartmentDesignationMapInfo>(@"
                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.DepartmentId
                        ,a.DesignationId
                        ,c.DesignationName
                    FROM HrDepartmentDesignationMap a
                    INNER JOIN HrDesignation c
                        ON a.DesignationId = c.RowId
                    WHERE a.DepartmentId = @DepartmentId
                    AND a.DomainStatus = 1
                    ORDER BY c.DesignationName
                ",
                new SqlParameter("@DepartmentId", DepartmentId))
                .ToList();
            }
        }

        public static HrDepartmentDesignationMapResponse GetFilteredHrDepartmentDesignationMaps(HttpRequestBase Request)
        {
            HrDepartmentDesignationMapResponse resp =
                new HrDepartmentDesignationMapResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM [dbo].[HrDepartmentDesignationMap] a
                    INNER JOIN [dbo].[HrDepartment] b
                        ON a.DepartmentId = b.RowId
                    INNER JOIN [dbo].[HrDesignation] c
                        ON a.DesignationId = c.RowId
                    WHERE a.DomainStatus = 1
                ");

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                    string.Format("SELECT COUNT(*) {0}", sb))
                    .Single();

                List<SqlParameter> searchParam =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND
                        (
                            CHARINDEX(@SearchValue,b.DepartmentName) > 0
                            OR CHARINDEX(@SearchValue,c.DesignationName) > 0
                        )
                    ");

                    searchParam.Add(
                        new SqlParameter("@SearchValue",
                        gmdt.SearchValue));
                }

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                    string.Format("SELECT COUNT(*) {0}", sb),
                    searchParam.ToArray())
                    .Single();

                List<SqlParameter> pagingParam =
                    new List<SqlParameter>();

                pagingParam.AddRange(searchParam);

                pagingParam.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                pagingParam.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrDepartmentDesignationMaps =
                    repo.Database.SqlQuery<HrDepartmentDesignationMapInfo>(string.Format(@"
                    SELECT
                        a.RowId,
                        a.MapId,
                        a.DepartmentId,
                        a.DesignationId,
                        b.DepartmentName,
                        c.DesignationName
                    {0}
                    ORDER BY {1} {2}
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                ",
                sb.ToString(),
                gmdt.SortColum,
                gmdt.SortDir),
                pagingParam.ToArray()).ToList();
            }

            return resp;
        }
    }
}