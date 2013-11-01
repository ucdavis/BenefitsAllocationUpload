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
        private readonly IRepositoryWithTypedId<ReimbursableBenefitsAccount, ReimbursableBenefitsAccountId> _reimbursableBenefitsAccountRepository;

        public ReimbursableBenefitsAccountController(
            IRepositoryWithTypedId<ReimbursableBenefitsAccount, ReimbursableBenefitsAccountId> reimbursableBenefitsAccountRepository)
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
            var orgId = GetOrgIdForCurrentUser();
            if (!String.IsNullOrEmpty(orgId))
            {
                var results =
                    _reimbursableBenefitsAccountRepository.Queryable.Where(x => x.OrgId.Equals(orgId)).ToList();

                ViewData.Model = results;
            }
            else
            {
                Message = "Error: No Organization found for current user!";
            }

            return View();
        }

        //
        // GET: /ReimbursableBenefitsAccount/Details/5
        // Passing the individual fields that makeup the ReimbursableBenefitsAccountId,
        // i.e. OrgId=SSCI&Chart=3&Account=ANTGENA, etc.,
        // in the query string will also allow proper binding of the Id object.
        public ActionResult Details(ReimbursableBenefitsAccountId id)
        {
            var results = _reimbursableBenefitsAccountRepository.GetNullableById(id);
            ViewData.Model = results;
            return View();
        }

        //
        // GET: /ReimbursableBenefitsAccount/Create
        public ActionResult Create()
        {
            var model = new ReimbursableBenefitsAccount();

            var orgId = GetOrgIdForCurrentUser();
            if (!String.IsNullOrEmpty(orgId))
            {
                model = new ReimbursableBenefitsAccount() {OrgId = orgId, Chart = "3", IsActive = true};
            }
            else
            {
                Message = "Error: No Organization found for current user!";
            }

            ViewData.Model = model;    
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

                    // Optional way if an additional method is implemented in the datamap.
                    //if (_reimbursableBenefitsAccountRepository.Queryable.Any(x => x.IdString.Equals(newAccount.IdString)))
                    if (_reimbursableBenefitsAccountRepository.GetNullableById(newAccount.Id) != null)
                    {
                        Message = "Account \"" + newAccount.Id + "\" already exists!";

                        return RedirectToAction("Index");
                    }

                    _reimbursableBenefitsAccountRepository.EnsurePersistent(newAccount);

                    Message = "New account \"" + newAccount.Id + "\" has been created.";

                    return RedirectToAction("Index");
                }
                return View(collection);
            }
            catch
            {
                 return View(collection);
            }
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult Edit(ReimbursableBenefitsAccountId id, bool isActive)
        {
            Result res;
            if (id != null)
            {
                res = new Result {Text = "Received and processed " + id + " successfully."};
                var result =
                    _reimbursableBenefitsAccountRepository.GetNullableById(id);

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
        // POST: /ReimbursableBenefitsAccount/Delete/5
        [System.Web.Mvc.HttpPost]
        public ActionResult Delete(ReimbursableBenefitsAccountId id)
        {
            if (id != null)
            {
                try
                {
                    var result = _reimbursableBenefitsAccountRepository.GetNullableById(id);
                    if (result != null)
                    {
                        _reimbursableBenefitsAccountRepository.Remove(result);
                        
                        Message = "Delete succeeded: Reimbursable Benefits Account \"" + id + "\" has been deleted.";
                    }
                    else
                    {
                        Message = "Delete failed: Received and unable to delete \"" + id + "\"!";
                    }
                }
                catch (Exception ex)
                {
                    Message = "Unable to delete account: " + ex.Message;
                }
            }
            else
            {
                Message = "Delete failed: Id was not privided!";
            }

           return RedirectToAction("Index");
        }

        //
        // POST: /ReimbursableBenefitsAccount/Delete/5
        [System.Web.Mvc.HttpPost]
        public JsonResult DeleteJson(ReimbursableBenefitsAccountId id)
        {
            Result res;
            if (id != null)
            {
                try
                {
                    var result = _reimbursableBenefitsAccountRepository.GetNullableById(id);
                    if (result != null)
                    {
                        _reimbursableBenefitsAccountRepository.Remove(result);

                        res = new Result
                            {
                                Text = "Delete succeeded: Reimbursable Benefits Account \"" + id + "\" has been deleted.",
                                Success = true
                            };
                    }
                    else
                    {
                        res = new Result
                            {
                                Text = "Delete failed: Received and unable to delete \"" + id + "\"!",
                                Success = false
                            };
                    }
                }
                catch (Exception ex)
                {
                    res = new Result
                            {
                                Text = "Unable to delete account: " + ex.Message,
                                Success = false
                            };
                }
            }
            else
            {
                res = new Result
                            {
                                Text = "Delete failed: Id was not privided!",
                                Success = false
                            };
            }

            return Json(res);
        }

        protected string GetOrgIdForCurrentUser()
        {
            var retval = string.Empty;

            using (var db = new FISDataMartEntities())
            {
                var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
                var unit = user.Units.FirstOrDefault();
                var schoolCodeParameter = new SqlParameter("schoolCode", unit.SchoolCode);

                retval = db.Database.SqlQuery<string>(
                    "SELECT dbo.udf_GetOrgIdForSchoolCode(@schoolCode)", schoolCodeParameter).FirstOrDefault();
            }
            return retval;
        } 
    }

    public class Result
    {
        public string Text { get; set; }
        public bool Success { get; set; }
    }
}
