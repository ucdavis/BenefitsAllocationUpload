using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BenefitsAllocation.Core.Domain;
using BenefitsAllocationUpload.Models;
using UCDArch.Core.PersistanceSupport;

namespace BenefitsAllocationUpload.Controllers
{
    //[Authorize]
    [Authorize(Roles = RoleNames.User)]
    public class ExpiredAccountsController : Controller
    {
        private readonly IRepository<ExpiredAccount> _expiredAccountRepository;

        public ExpiredAccountsController(IRepository<ExpiredAccount> expiredAccountRepository)
        {
            _expiredAccountRepository = expiredAccountRepository;
        }

        public ActionResult Index()
        {
            var results = new List<ExpiredAccount>();
            using (var db = new FISDataMartEntities())
            {
                var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
                var unit = user.Units.FirstOrDefault();
                var orgId = string.Empty;

                var today = DateTime.Now;
                var year = today.Year;
                var month = today.Month;

                if (month >= 8)
                    year += 1;
                var fiscalYear = year.ToString();

                var schoolCodeParameter = new SqlParameter("schoolCode", unit.SchoolCode);

                orgId = db.Database.SqlQuery<string>(
                    "SELECT dbo.udf_GetOrgIdForSchoolCode(@schoolCode)", schoolCodeParameter).FirstOrDefault();

                var command = db.Database.Connection.CreateCommand();
                command.CommandText = "dbo.usp_GetExpiredAccountsForOrg";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = new SqlParameter
                    {
                        ParameterName = "@FiscalYear",
                        SqlDbType = SqlDbType.VarChar,
                        Direction = ParameterDirection.Input,
                        Value = fiscalYear
                    };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                    {
                        ParameterName = "@OrgId",
                        SqlDbType = SqlDbType.VarChar,
                        Direction = ParameterDirection.Input,
                        Value = orgId
                    };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UseDaFIS",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Input,
                    //Value =  (!"AAES".Equals(orgId))  // our local database has been timing out lately; therefore, use DaFIS for all Orgs.
                    Value =  1
                };
                command.Parameters.Add(parameter);

                command.Connection.Open();
                var reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        results.Add(new ExpiredAccount
                            {
                                Level3OrgCode = reader[0].ToString(),
                                ChartNum = reader[1].ToString(),
                                AccountNum = reader[2].ToString(),
                                ExpirationDate = (DateTime)reader[3],
                                Amount = (decimal)reader[4] ,
                                MgrId = reader[5].ToString(),
                                MgrName = reader[6].ToString()
                            }
                            );
                    }
                }

                reader.Close();
                command.Connection.Close();
                command.Dispose();

                ViewData.Model = results;   
            }
            return View();
        }
    }
}
