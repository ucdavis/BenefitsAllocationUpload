using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using BenefitsAllocation.Core.Domain;
using BenefitsAllocationUpload.Models;
using UCDArch.Core.PersistanceSupport;

namespace BenefitsAllocationUpload.Controllers
{
    public class ReimbursableBenefitsAccountController : ApplicationController
    {
        private readonly IRepositoryWithTypedId<ReimbursableBenefitsAccount, string> _reimbursableBenefitsAccountRepository;

        public ReimbursableBenefitsAccountController(
            IRepositoryWithTypedId<ReimbursableBenefitsAccount, string> reimbursableBenefitsAccountRepository)
        {
            _reimbursableBenefitsAccountRepository = reimbursableBenefitsAccountRepository;
        }
        //
        // GET: /ReimbursableBenefitsAccount/

        /// <summary>
        /// Return a list of reimburseable benefits accounts for the logged in user's Org. 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            using (var db = new FISDataMartEntities())
            {
                var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
                var unit = user.Units.FirstOrDefault();
                var orgId = string.Empty;

                var schoolCodeParameter = new SqlParameter("schoolCode", unit.SchoolCode);

                orgId = db.Database.SqlQuery<string>(
                    "SELECT dbo.udf_GetOrgIdForSchoolCode(@schoolCode)", schoolCodeParameter).FirstOrDefault();

                var results = _reimbursableBenefitsAccountRepository.Queryable.Where(x => x.OrgId.Equals(orgId)).ToList();
                ViewData.Model = results;  
            }

            return View();
        }

        //
        // GET: /ReimbursableBenefitsAccount/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /ReimbursableBenefitsAccount/Create

        public ActionResult Create()
        {
            using (var db = new FISDataMartEntities())
            {
                var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
                var unit = user.Units.FirstOrDefault();
                var orgId = string.Empty;

                var schoolCodeParameter = new SqlParameter("schoolCode", unit.SchoolCode);

                orgId = db.Database.SqlQuery<string>(
                    "SELECT dbo.udf_GetOrgIdForSchoolCode(@schoolCode)", schoolCodeParameter).FirstOrDefault();

                var model = new ReimbursableBenefitsAccount() {OrgId = orgId, Chart = "3", IsActive = true};
                ViewData.Model = model;
            }

            return View();
        }

        //
        // POST: /ReimbursableBenefitsAccount/Create

        [System.Web.Mvc.HttpPost]
        public ActionResult Create(ReimbursableBenefitsAccount collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newAccount = new ReimbursableBenefitsAccount()
                        {
                            OrgId = collection.OrgId,                                
                            Chart = collection.Chart,
                            Account = collection.Account,
                            IsActive = collection.IsActive
                        };

                    if (_reimbursableBenefitsAccountRepository.Queryable.Any(x => x.IdString.Equals(newAccount.IdString)))
                    {
                        Message = "Account \"" + newAccount.IdString + "\" already exists!";

                        return RedirectToAction("Index");
                    }

                    _reimbursableBenefitsAccountRepository.EnsurePersistent(newAccount);

                    Message = "New account \"" + newAccount.IdString + "\" has been created.";

                    return RedirectToAction("Index");
                }
                return View(collection);
            }
            catch
            {
                 return View(collection);
            }
        }

        //
        // GET: /ReimbursableBenefitsAccount/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /ReimbursableBenefitsAccount/Edit/5

        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        
        [System.Web.Mvc.HttpPost]
        public JsonResult Edit(string id, bool isActive)
        {
            Result res;
            if (!string.IsNullOrEmpty(id))
            {
                res = new Result {Text = "Received and processed " + id + " successfully."};
                var myId = new ReimbursableBenefitsAccountId(id);

                var result =
                    _reimbursableBenefitsAccountRepository.Queryable.FirstOrDefault(
                        x => x.IdString.Equals(myId.ToString()));

                if (result != null)
                {
                    result.IsActive = isActive;
                    _reimbursableBenefitsAccountRepository.EnsurePersistent(result);

                    res = new Result {Text = "Update succeeded: Received and processed " + id + "."};
                }
                else
                {
                    res = new Result { Text = "Update failed: Received and unable to update " + id + "!" };
                }
            }
            else
            {
                res = new Result { Text = "Update failed: Id was not privided!" };
            }
            
            return Json(res);
        }

        //
        // GET: /ReimbursableBenefitsAccount/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /ReimbursableBenefitsAccount/Delete/5

        [System.Web.Mvc.HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }

    public class Result
    {
        public string Text { get; set; }
    }
}
