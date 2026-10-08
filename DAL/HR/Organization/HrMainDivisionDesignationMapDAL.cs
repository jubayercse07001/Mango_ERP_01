using ERP.Models.HR.Organization.HrMainDivisionDesignationMap;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Organization
{
    public class HrMainDivisionDesignationMapDAL
    {
        public static HrMainDivisionDesignationMapInfo GetMapByID(string RowId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrMainDivisionDesignationMapInfo>(@"

                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.MainDivisionId
                        ,a.DesignationId
                        ,b.MainDivisionName
                        ,c.DesignationName

                    FROM HrMainDivisionDesignationMap a

                    LEFT JOIN HrMainDivision b
                        ON a.MainDivisionId = b.RowId

                    LEFT JOIN HrDesignation c
                        ON a.DesignationId = c.RowId

                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1

                ", new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }
        }

        public static List<HrMainDivisionDesignationMapInfo> GetAllMaps()
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrMainDivisionDesignationMapInfo>(@"

                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.MainDivisionId
                        ,a.DesignationId
                        ,b.MainDivisionName
                        ,c.DesignationName

                    FROM HrMainDivisionDesignationMap a

                    LEFT JOIN HrMainDivision b
                        ON a.MainDivisionId = b.RowId

                    LEFT JOIN HrDesignation c
                        ON a.DesignationId = c.RowId

                    WHERE a.DomainStatus = 1

                    ORDER BY a.MapId

                ").ToList();
            }
        }

        public static HrMainDivisionDesignationMapInfo GetDuplicateMap(
            string MainDivisionId,
            string DesignationId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrMainDivisionDesignationMapInfo>(@"

                    SELECT
                         RowId
                        ,MapId
                        ,MainDivisionId
                        ,DesignationId

                    FROM HrMainDivisionDesignationMap

                    WHERE MainDivisionId = @MainDivisionId
                    AND DesignationId = @DesignationId
                    AND DomainStatus = 1

                ",
                new SqlParameter("@MainDivisionId", MainDivisionId),
                new SqlParameter("@DesignationId", DesignationId)
                )
                .FirstOrDefault();
            }
        }

        public static void AddMap(
            HrMainDivisionDesignationMapInfo map,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                var obj = repo.Database
                    .SqlQuery<HrMainDivisionDesignationMapInfo>(@"

                        SELECT
                            ISNULL(MAX(MapId),0)+1 AS MapId

                        FROM HrMainDivisionDesignationMap

                    ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"

                INSERT INTO HrMainDivisionDesignationMap
                (
                     RowId
                    ,MapId
                    ,MainDivisionId
                    ,DesignationId
                    ,CreateUser
                    ,CreateDate
                    ,DomainStatus
                )
                VALUES
                (
                @RowId
                ,@MapId
                ,@MainDivisionId
                ,@DesignationId
                ,@CreateUser
                ,GETDATE()
                ,1
                )

                ",
                 new SqlParameter("@RowId", map.RowId),
                 new SqlParameter("@MapId", obj.MapId),
                 new SqlParameter("@MainDivisionId", map.MainDivisionId),
                 new SqlParameter("@DesignationId", map.DesignationId),
                 new SqlParameter("@CreateUser", loggedInUser));

                            }
                        }

        

        public static void UpdateMap(
            HrMainDivisionDesignationMapInfo map,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"

                    UPDATE HrMainDivisionDesignationMap

                    SET
                         MainDivisionId = @MainDivisionId
                        ,DesignationId = @DesignationId
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()

                    WHERE RowId = @RowId

                ",
                new SqlParameter("@RowId", map.RowId),
                new SqlParameter("@MainDivisionId", map.MainDivisionId),
                new SqlParameter("@DesignationId", map.DesignationId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteMap(
            HrMainDivisionDesignationMapInfo map,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"

                    UPDATE HrMainDivisionDesignationMap

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

        public static List<HrMainDivisionDesignationMapInfo>
            GetDesignationByMainDivision(string MainDivisionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrMainDivisionDesignationMapInfo>(@"

                    SELECT
                         a.RowId
                        ,a.MapId
                        ,a.MainDivisionId
                        ,a.DesignationId
                        ,c.DesignationName

                    FROM HrMainDivisionDesignationMap a

                    INNER JOIN HrDesignation c
                        ON a.DesignationId = c.RowId

                    WHERE a.MainDivisionId = @MainDivisionId
                    AND a.DomainStatus = 1

                    ORDER BY c.DesignationName

                ",
                new SqlParameter("@MainDivisionId", MainDivisionId))
                .ToList();
            }
            
        }
        public static HrMainDivisionDesignationMapResponse GetFilteredHrMainDivisionDesignationMaps(HttpRequestBase Request)
        {
            HrMainDivisionDesignationMapResponse resp =
                new HrMainDivisionDesignationMapResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM [dbo].[HrMainDivisionDesignationMap] a
                    INNER JOIN [dbo].[HrMainDivision] b
                        ON a.MainDivisionId = b.RowId
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
                            CHARINDEX(@SearchValue,b.MainDivisionName) > 0
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

                resp.HrMainDivisionDesignationMaps =
                    repo.Database.SqlQuery<HrMainDivisionDesignationMapInfo>(string.Format(@"
                    SELECT
                        a.RowId,
                        a.MapId,
                        a.MainDivisionId,
                        a.DesignationId,
                        b.MainDivisionName,
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