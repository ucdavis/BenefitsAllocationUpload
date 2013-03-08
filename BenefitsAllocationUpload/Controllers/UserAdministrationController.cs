using System.Web.Mvc;
using BenefitsAllocationUpload.Models;

namespace BenefitsAllocationUpload.Controllers
{ 
    /// <summary>
    /// Controller for the UserAdmin class
    /// </summary>
    [Authorize(Roles = RoleNames.Admin)]
    public class UserAdministrationController : ApplicationController
    {
        //
        // GET: /UserAdministration/

        // Note that a person needs both Admin, and ManageAll roles to be able to access the Admin IFrame from within this page!
        public ActionResult Index()
        {
            return View();
        }

    }
}
