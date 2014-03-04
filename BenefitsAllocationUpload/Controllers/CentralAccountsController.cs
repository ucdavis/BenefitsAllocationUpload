using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BenefitsAllocation.Core.Domain;
using BenefitsAllocationUpload.Models;
using UCDArch.Core.PersistanceSupport;

namespace BenefitsAllocationUpload.Controllers
{
    public class CentralAccountsController : ApplicationController
    {

        private readonly IRepository<CentralAccount> _centralAccountRepository;

        public CentralAccountsController(IRepository<CentralAccount> centralAccountRepository)
        {
            _centralAccountRepository = centralAccountRepository;
        }

        //
        // GET: /CentralAccounts/

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
                var model = _centralAccountRepository.GetAll().Where(x => x.OrgId.Equals(orgId)).ToList();
                return View(model);
            }
        }

        //
        // GET: /CentralAccounts/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /CentralAccounts/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /CentralAccounts/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /CentralAccounts/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /CentralAccounts/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /CentralAccounts/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /CentralAccounts/Delete/5

        [HttpPost]
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
}
