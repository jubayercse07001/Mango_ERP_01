using ERP.Models.HR.Common.HrBenefit;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrBenefitDAL
    {
        /*
        public static HrBenefitInfo GetHrBenefitByHrBenefitID(string HrBenefitID)
        {
            HrBenefitInfo HrBenefit = null;
            using (var repo = new GMContext())
            {
                HrBenefit = repo.Database.SqlQuery<HrBenefitInfo>(@"
                    SELECT a.[HrBenefitID]
                          ,a.[AccountNumber]
                          ,b.[BankID]
                          ,c.[BankName]
                          ,a.[BranchID]
                          ,b.[BranchName]
                          ,a.[AccountTypeID]
                          ,dp.[AccountTypeName]
                          ,a.[AccountName]
                          ,a.[AccountShortCode]
                          ,a.[OpeningDate]
                          ,a.[OpeningBalance]
                          ,a.[InterestRate]
                          ,a.[Balance]
                          ,a.[HrBenefitStatus]
                          ,a.[DomainStatus]
                      FROM [dbo].[HrBenefit] a
                        INNER JOIN [dbo].[Branch] b ON a.[BranchID] = b.[BranchID]
                        INNER JOIN [dbo].[Bank] c ON b.[BankID] = c.[BankID]
                        INNER JOIN [dbo].[AccountType] dp on a.[AccountTypeID] = dp.[AccountTypeName]
	                  WHERE a.[HrBenefitID] = @HrBenefitID and a.DomainStatus = 1
                ", new SqlParameter("@HrBenefitID", HrBenefitID)).FirstOrDefault();
            }
            return HrBenefit;
        }

        public static HrBenefitInfo GetHrBenefitByAccountName(string AccountName)
        {
            HrBenefitInfo HrBenefit = null;
            using (var repo = new GMContext())
            {
                HrBenefit = repo.Database.SqlQuery<HrBenefitInfo>(@"
                      SELECT [HrBenefitID]
                          ,[AccountNumber]
                          ,[BranchID]
                          ,[AccountTypeID]
                          ,[AccountName]
                          ,[AccountShortCode]
                          ,[OpeningDate]
                          ,[OpeningBalance]
                          ,[InterestRate]
                          ,[Balance]
                          ,[HrBenefitStatus]
                          ,[DomainStatus]
                      FROM [dbo].[HrBenefit] 
	                  WHERE [DomainStatus] = 1
                        AND [AccountName] = @AccountName", new SqlParameter("@AccountName", AccountName)).FirstOrDefault();
            }
            return HrBenefit;
        }
        public static List<HrBenefitInfo> GetAllHrBenefits()
        {
            List<HrBenefitInfo> HrBenefits = null;
            using (var repo = new GMContext())
            {
                HrBenefits = repo.Database.SqlQuery<HrBenefitInfo>(@"
                     SELECT a.[HrBenefitID]
                          ,a.[AccountNumber]
                          ,b.[BankID]
                          ,c.[BankName]
                          ,a.[BranchID]
                          ,b.[BranchName]
                          ,a.[AccountTypeID]
                          ,dp.[AccountTypeName]
                          ,a.[AccountName]
                          ,a.[AccountShortCode]
                          ,a.[OpeningDate]
                          ,a.[OpeningBalance]
                          ,a.[InterestRate]
                          ,a.[Balance]
                          ,a.[HrBenefitStatus]
                          ,a.[DomainStatus]
                      FROM [dbo].[HrBenefit] a
                        INNER JOIN [dbo].[Branch] b ON a.[BranchID] = b.[BranchID]
                        INNER JOIN [dbo].[Bank] c ON b.[BankID] = c.[BankID]
                        INNER JOIN [dbo].[AccountType] dp on a.[AccountTypeID] = dp.AccountTypeID
                        WHERE a.[DomainStatus] = 1
                      ORDER BY [AccountName]
                ").ToList();
            }
            return HrBenefits;
        }
        public static void AddHrBenefit(HrBenefitInfo HrBenefit, string loggedInUser)
        {
            HrBenefitInfo loc;
            using (var repo = new GMContext())
            {
                //loc = repo.Database.SqlQuery<HrBenefitInfo>(@"
                //     SELECT CAST((ISNULL(MAX([HrBenefitID]-0),0) + 1) as varchar)  HrBenefitID  
                //        FROM [dbo].[HrBenefit]
                //        where [DomainStatus] = 1
                //").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrBenefit]
                                   ([HrBenefitID]
                                   ,[AccountNumber]
                                   ,[BranchID]
                                   ,[AccountTypeID]
                                   ,[AccountName]
                                   ,[AccountShortCode]
                                   ,[OpeningDate]
                                   ,[OpeningBalance]
                                   ,[InterestRate]
                                   ,[Balance]
                                   ,[HrBenefitStatus]
                                   ,[CreateUser]
                                   ,[CreateDate]
                                   ,[DomainStatus])
                             VALUES
                                   (@HrBenefitID
                                   ,@AccountNumber
                                   ,@BranchID
                                   ,@AccountTypeID
                                   ,@AccountName
                                   ,@AccountShortCode
                                   ,@OpeningDate
                                   ,@OpeningBalance
                                   ,@InterestRate
                                   ,@Balance
                                   ,@HrBenefitStatus
                                   ,@CreateUser
                                   ,getdate() 
                                   ,1)
                ", new SqlParameter("@HrBenefitID", Guid.NewGuid().ToString())//loc.HrBenefitID)
                , new SqlParameter("@AccountNumber", HrBenefit.AccountNumber)
                , new SqlParameter("@BranchID", HrBenefit.BranchID)
                , new SqlParameter("@AccountTypeID", HrBenefit.AccountTypeID)
                , new SqlParameter("@AccountName", HrBenefit.AccountName)
                , new SqlParameter("@AccountShortCode", HrBenefit.AccountShortCode)
                , new SqlParameter("@OpeningDate", HrBenefit.OpeningDate)
                , new SqlParameter("@OpeningBalance", HrBenefit.OpeningBalance)
                , new SqlParameter("@InterestRate", HrBenefit.InterestRate)
                , new SqlParameter("@Balance", HrBenefit.Balance)
                , new SqlParameter("@HrBenefitStatus", "Active")
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrBenefit(HrBenefitInfo HrBenefit, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                     UPDATE [dbo].[HrBenefit]
                               SET [AccountNumber] = @AccountNumber
                                  ,[BranchID] = @BranchID
                                  ,[AccountTypeID] = @AccountTypeID
                                  ,[AccountName] = @AccountName
                                  ,[AccountShortCode] = @AccountShortCode
                                  ,[OpeningDate] = @OpeningDate
                                  ,[OpeningBalance] = @OpeningBalance
                                  ,[InterestRate] = @InterestRate
                                  ,[Balance] = @Balance
                                  ,[HrBenefitStatus] = @HrBenefitStatus
                                  ,[UpdateUser] = @UpdateUser
                                  ,[UpdateDate] = getdate()
                             WHERE [HrBenefitID] = @HrBenefitID
                ", new SqlParameter("@HrBenefitID", Guid.NewGuid().ToString())//loc.HrBenefitID)
                , new SqlParameter("@AccountName", HrBenefit.AccountName)
                , new SqlParameter("@BranchID", HrBenefit.BranchID)
                , new SqlParameter("@AccountTypeID", HrBenefit.AccountTypeID)
                , new SqlParameter("@AccountName", HrBenefit.AccountName)
                , new SqlParameter("@AccountShortCode", HrBenefit.AccountShortCode)
                , new SqlParameter("@OpeningDate", HrBenefit.OpeningDate)
                , new SqlParameter("@OpeningBalance", HrBenefit.OpeningBalance)
                , new SqlParameter("@InterestRate", HrBenefit.InterestRate)
                , new SqlParameter("@Balance", HrBenefit.Balance)
                , new SqlParameter("@HrBenefitStatus", "Active")
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrBenefit(HrBenefitInfo HrBenefit, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrBenefit]
                       SET [DomainStatus] = 0
                          ,[UpdateUser] = @UpdateUser
                          ,[UpdateDate] = getdate() 
                     WHERE [HrBenefitID] = @HrBenefitID
                ", new SqlParameter("@HrBenefitID", HrBenefit.HrBenefitID)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }
        */
        public static HrBenefitResponse GetFilteredHrBenefits(HttpRequestBase Request)
        {
            HrBenefitResponse resp = new HrBenefitResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrBenefits = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrBenefit] a
                        INNER JOIN [dbo].[Branch] b ON a.[BranchID] = b.[BranchID]
                        INNER JOIN [dbo].[Bank] c ON b.[BankID] = c.[BankID]
                        INNER JOIN [dbo].[AccountType] dp on a.[AccountTypeID] = dp.[AccountTypeName]
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
                //if (!string.IsNullOrEmpty(divisionType))
                //{
                //    sb.AppendLine(" AND DivisionType = @DivisionType ");
                //    sqlParameters.Add(new SqlParameter("@DivisionType", divisionType));
                //    sqlParameters1.Add(new SqlParameter("@DivisionType", divisionType));
                //}

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, AccountName) > 0 ");

                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                    sqlParameters1.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrBenefits = repo.Database.SqlQuery<HrBenefitInfo>(string.Format(@"
                          SELECT 
                           a.[HrBenefitID]
                           ,c.[BankName]
                          ,b.[BranchName]
                          ,dp.[AccountTypeName]
                          ,a.[AccountNumber]
                          ,a.[AccountName]
                          ,a.[AccountShortCode]
                          ,a.[Balance]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}