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
using ExcludedObject = BenefitsAllocation.Core.Domain.ExcludedObject;

namespace BenefitsAllocationUpload.Controllers
{
    public class ExcludedObjectController : ApplicationController
    {
        private readonly IRepositoryWithTypedId<ExcludedObject, ExcludedObjectId> _excludedObjectRepository;

        public ExcludedObjectController(
            IRepositoryWithTypedId<ExcludedObject, ExcludedObjectId> excludedObjectRepository)
        {
            _excludedObjectRepository = excludedObjectRepository;
        }

        //
        // GET: /ExcludedObject/

        /// <summary>
        /// Return a list of reimbursable excluded objects for the logged in user's Org. 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var orgId = GetOrgIdForCurrentUser();
            if (!String.IsNullOrEmpty(orgId))
            {
                var results =
                    _excludedObjectRepository.Queryable.Where(x => x.OrgId.Equals(orgId)).ToList();

                ViewData.Model = results;
            }
            else
            {
                Message = "Error: No Organization found for current user!";
            }

            return View();
        }

        //
        // GET: /ExcludedObject/Details/5
        // Passing the individual fields that makeup the ExcludedObjectId,
        // i.e. OrgId=AAES&ObjectNum=8570, etc.,
        // in the query string will also allow proper binding of the Id object.
        public ActionResult Details(ExcludedObjectId id)
        {
            var results = _excludedObjectRepository.GetNullableById(id);
            ViewData.Model = results;
            return View();
        }

        //
        // GET: /ExcludedObject/Create
        public ActionResult Create()
        {
            var model = new ExcludedObject();

            var orgId = GetOrgIdForCurrentUser();
            if (!String.IsNullOrEmpty(orgId))
            {
                model = new ExcludedObject() {OrgId = orgId, IsActive = true};
            }
            else
            {
                Message = "Error: No Organization found for current user!";
            }

            ViewData.Model = model;    
            return View();
        }

        //
        // POST: /ExcludedObject/Create
        [System.Web.Mvc.HttpPost]
        public ActionResult Create(ExcludedObject collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newExcludedObject = new ExcludedObject()
                        {
                            OrgId = collection.OrgId,                                
                            ObjectNum = collection.ObjectNum,
                            IsActive = collection.IsActive
                        };

                    // Optional way if an additional method is implemented in the datamap.
                    //if (_excludedObjectRepository.Queryable.Any(x => x.IdString.Equals(newExcludedObject.IdString)))
                    if (_excludedObjectRepository.GetNullableById(newExcludedObject.Id) != null)
                    {
                        Message = "Object Num \"" + newExcludedObject.Id + "\" already exists!";

                        return RedirectToAction("Index");
                    }

                    _excludedObjectRepository.EnsurePersistent(newExcludedObject);

                    Message = "New excluded object \"" + newExcludedObject.Id + "\" has been created.";

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
        public JsonResult EditJson(ExcludedObjectId id, bool isActive)

        {
            Result res;
            if (id != null)
            {
                res = new Result { Text = "Received and processed \"" + id + "\" successfully.", Success = true };
                var result =
                    _excludedObjectRepository.GetNullableById(id);

                if (result != null)
                {
                    result.IsActive = isActive;
                    _excludedObjectRepository.EnsurePersistent(result);

                    res = new Result { Text = "Update succeeded: Received and processed \"" + id + "\".  Changed \"Is Excluded?\" to " + isActive.ToString() + ".", Success = true };
                }
                else
                {
                    res = new Result { Text = "Update failed: Received and unable to update \"" + id + "\"!", Success = false };
                }
            }
            else
            {
                res = new Result { Text = "Update failed: Id was not provided!", Success = false };
            }
            
            return Json(res);
        }

        //
        // POST: /ExcludedObject/Delete/5
        [System.Web.Mvc.HttpPost]
        public ActionResult Delete(ExcludedObjectId id)
        {
            if (id != null)
            {
                try
                {
                    var result = _excludedObjectRepository.GetNullableById(id);
                    if (result != null)
                    {
                        _excludedObjectRepository.Remove(result);
                        
                        Message = "Delete succeeded: Excluded Object \"" + id + "\" has been deleted.";
                    }
                    else
                    {
                        Message = "Delete failed: Received and unable to delete \"" + id + "\"!";
                    }
                }
                catch (Exception ex)
                {
                    Message = "Unable to delete excluded object: " + ex.Message;
                }
            }
            else
            {
                Message = "Delete failed: Id was not privided!";
            }

           return RedirectToAction("Index");
        }

        //
        // POST: /ExcludedObject/Delete/5
        [System.Web.Mvc.HttpPost]
        public JsonResult DeleteJson(ExcludedObjectId id)
        {
            Result res;
            if (id != null)
            {
                try
                {
                    var result = _excludedObjectRepository.GetNullableById(id);
                    if (result != null)
                    {
                        _excludedObjectRepository.Remove(result);

                        res = new Result
                            {
                                Text = "Delete succeeded: Excluded Object \"" + id + "\" has been deleted.",
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
                                Text = "Unable to delete excluded object: " + ex.Message,
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

//    public class Result
//    {
//        public string Text { get; set; }
//        public bool Success { get; set; }
//    }
}
