using ERP.Models.HR.Organization.HrDivisionDesignationMap;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrDivisionDesignationMapDAL
    {
        public static HrDivisionDesignationMapInfo GetMapByID(string RowId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDivisionDesignationMapInfo>(@"
                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.DivisionId
                        ,a.DesignationId
                        ,b.DivisionName
                        ,c.DesignationName
                    FROM HrDivisionDesignationMap a
                    LEFT JOIN HrDivision b
                        ON a.DivisionId = b.RowId
                    LEFT JOIN HrDesignation c
                        ON a.DesignationId = c.RowId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }
        }

        public static List<HrDivisionDesignationMapInfo> GetAllMaps()
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDivisionDesignationMapInfo>(@"
                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.DivisionId
                        ,a.DesignationId
                        ,b.DivisionName
                        ,c.DesignationName
                    FROM HrDivisionDesignationMap a
                    LEFT JOIN HrDivision b
                        ON a.DivisionId = b.RowId
                    LEFT JOIN HrDesignation c
                        ON a.DesignationId = c.RowId
                    WHERE a.DomainStatus = 1
                    ORDER BY a.MapId
                ").ToList();
            }
        }

        public static HrDivisionDesignationMapInfo GetDuplicateMap(string DivisionId, string DesignationId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDivisionDesignationMapInfo>(@"
                    SELECT
                         RowId
                        ,MapId
                        ,DivisionId
                        ,DesignationId
                    FROM HrDivisionDesignationMap
                    WHERE DivisionId = @DivisionId
                    AND DesignationId = @DesignationId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@DivisionId", DivisionId),
                new SqlParameter("@DesignationId", DesignationId)
                )
                .FirstOrDefault();
            }
        }

        public static void AddMap(HrDivisionDesignationMapInfo map, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                var obj = repo.Database.SqlQuery<HrDivisionDesignationMapInfo>(@"
                    SELECT
                        ISNULL(MAX(MapId), 0) + 1 AS MapId
                    FROM HrDivisionDesignationMap
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO HrDivisionDesignationMap
                    (
                         RowId
                        ,MapId
                        ,DivisionId
                        ,DesignationId
                        ,CreateUser
                        ,CreateDate
                        ,DomainStatus
                    )
                    VALUES
                    (
                         @RowId
                        ,@MapId
                        ,@DivisionId
                        ,@DesignationId
                        ,@CreateUser
                        ,GETDATE()
                        ,1
                    )
                ",
                 new SqlParameter("@RowId", map.RowId),
                 new SqlParameter("@MapId", obj.MapId),
                 new SqlParameter("@DivisionId", map.DivisionId),
                 new SqlParameter("@DesignationId", map.DesignationId),
                 new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateMap(HrDivisionDesignationMapInfo map, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDivisionDesignationMap
                    SET
                         DivisionId = @DivisionId
                        ,DesignationId = @DesignationId
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", map.RowId),
                new SqlParameter("@DivisionId", map.DivisionId),
                new SqlParameter("@DesignationId", map.DesignationId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteMap(HrDivisionDesignationMapInfo map, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDivisionDesignationMap
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

        public static List<HrDivisionDesignationMapInfo> GetDesignationByDivision(string DivisionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrDivisionDesignationMapInfo>(@"
                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.DivisionId
                        ,a.DesignationId
                        ,c.DesignationName
                    FROM HrDivisionDesignationMap a
                    INNER JOIN HrDesignation c
                        ON a.DesignationId = c.RowId
                    WHERE a.DivisionId = @DivisionId
                    AND a.DomainStatus = 1
                    ORDER BY c.DesignationName
                ",
                new SqlParameter("@DivisionId", DivisionId))
                .ToList();
            }
        }

        public static HrDivisionDesignationMapResponse GetFilteredHrDivisionDesignationMaps(HttpRequestBase Request)
        {
            HrDivisionDesignationMapResponse resp = new HrDivisionDesignationMapResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM [dbo].[HrDivisionDesignationMap] a
                    INNER JOIN [dbo].[HrDivision] b
                        ON a.DivisionId = b.RowId
                    INNER JOIN [dbo].[HrDesignation] c
                        ON a.DesignationId = c.RowId
                    WHERE a.DomainStatus = 1
                ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    string.Format("SELECT COUNT(*) {0}", sb)).Single();

                List<SqlParameter> searchParam = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND
                        (
                            CHARINDEX(@SearchValue, b.DivisionName) > 0
                            OR CHARINDEX(@SearchValue, c.DesignationName) > 0
                        )
                    ");

                    searchParam.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    string.Format("SELECT COUNT(*) {0}", sb), searchParam.ToArray()).Single();

                List<SqlParameter> pagingParam = new List<SqlParameter>();
                pagingParam.AddRange(searchParam);
                pagingParam.Add(new SqlParameter("@Skip", gmdt.Start));
                pagingParam.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrDivisionDesignationMaps = repo.Database.SqlQuery<HrDivisionDesignationMapInfo>(
                    string.Format(@"
                        SELECT
                            a.RowId,
                            a.MapId,
                            a.DivisionId,
                            a.DesignationId,
                            b.DivisionName,
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