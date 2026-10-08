using ERP.Models.HR.Common.HrDistrict;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrDistrictDAL
    {
        public static HrDistrictInfo GetDistrictByID(string RowId)
        {
            HrDistrictInfo district = null;

            using (var repo = new GMContext())
            {
                district = repo.Database.SqlQuery<HrDistrictInfo>(@"
                    SELECT
                        RowId,
                        DistrictId,
                        DistrictName
                    FROM HrDistrict
                    WHERE RowId=@RowId
                    AND DomainStatus=1
                ",
                new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return district;
        }

        public static HrDistrictInfo GetHrDistrictByName(string DistrictName, string RowId)
        {
            HrDistrictInfo district = null;

            using (var repo = new GMContext())
            {
                district = repo.Database.SqlQuery<HrDistrictInfo>(@"
                    SELECT *
                    FROM HrDistrict
                    WHERE DistrictName=@DistrictName
                    AND (@RowId IS NULL OR RowId<>@RowId)
                    AND DomainStatus=1
                ",
                new SqlParameter("@DistrictName", DistrictName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return district;
        }

        public static List<HrDistrictInfo> GetAllHrDistrict()
        {
            List<HrDistrictInfo> districts = null;

            using (var repo = new GMContext())
            {
                districts = repo.Database.SqlQuery<HrDistrictInfo>(@"
                    SELECT
                        RowId,
                        DistrictId,
                        DistrictName
                    FROM HrDistrict
                    WHERE DomainStatus=1
                    ORDER BY DistrictId
                ").ToList();
            }

            return districts;
        }

        public static void AddHrDistrict(HrDistrictInfo district, string loggedInUser)
        {
            HrDistrictInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrDistrictInfo>(@"
                    SELECT ISNULL(MAX(DistrictId),0)+1 AS DistrictId
                    FROM HrDistrict
                    WHERE DomainStatus=1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO HrDistrict
                    (
                        RowId,
                        DistrictId,
                        DistrictName,
                        DomainStatus,
                        CreateUser,
                        CreateDate
                    )
                    VALUES
                    (
                        @RowId,
                        @DistrictId,
                        @DistrictName,
                        1,
                        @CreateUser,
                        GETDATE()
                    )
                ",
                new SqlParameter("@RowId", district.RowId),
                new SqlParameter("@DistrictId", obj.DistrictId),
                new SqlParameter("@DistrictName", district.DistrictName),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrDistrict(HrDistrictInfo district, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDistrict
                    SET
                        DistrictName=@DistrictName,
                        UpdateUser=@UpdateUser,
                        UpdateDate=GETDATE()
                    WHERE RowId=@RowId
                ",
                new SqlParameter("@RowId", district.RowId),
                new SqlParameter("@DistrictName", district.DistrictName),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrDistrict(HrDistrictInfo district, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrDistrict
                    SET
                        DomainStatus=0,
                        UpdateUser=@UpdateUser,
                        UpdateDate=GETDATE()
                    WHERE RowId=@RowId
                ",
                new SqlParameter("@RowId", district.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrDistrictResponse GetFilteredHrDistrict(HttpRequestBase Request)
        {
            HrDistrictResponse resp = new HrDistrictResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrDistrict
                    WHERE DomainStatus=1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND
                        (
                            DistrictName LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(
                        new SqlParameter("@SearchValue", gmdt.SearchValue)
                    );
                }

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HrDistrict WHERE DomainStatus=1"
                    ).Single();

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) " + sb.ToString(),
                        parameters.ToArray()
                    ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrDistricts =
                    repo.Database.SqlQuery<HrDistrictInfo>(@"
                        SELECT
                            RowId,
                            DistrictId,
                            DistrictName
                    " + sb.ToString() + @"
                    ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                    ",
                    parameters.ToArray()).ToList();
            }

            return resp;
        }
    }
}