using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using BenefitsAllocationUpload.Models;
using UCDArch.Web.Attributes;
using UCDArch.Web.Controller;

namespace BenefitsAllocationUpload.Controllers
{
    [Version(MajorVersion = 4)]
    [Authorize]
    public class ApplicationController : SuperController
    {
        private static readonly string ApplicationsAbbr = ConfigurationManager.AppSettings["applicationsAbbr"];
        
        public ApplicationController()
        {
            //var user = System.Web.HttpContext.Current.User;
          
            //if (user != null)
            //{
            //    using (var db = new FISDataMartEntities())
            //{
            //    var userp = db.udf_Catbert3_vUsers(ApplicationsAbbr).FirstOrDefault(u => u.LoginID.ToLower() == user.Identity.Name.ToLower());
                
            //    if (userp != null)
            //    {
            //        var userRoles = db.udf_Catbert3_UserRoles(ApplicationsAbbr).Where(ur => ur.UserID == userp.UserID);
            //        var role = Roles.FirstOrDefault(r => r.Role.Equals("User"));
            //        bool isInRole = userRoles.Any(u => u.RoleID == role.RoleID);
            //        Console.WriteLine(userp.FirstName);
            //    }
            //}
            //}
            
            //using (var context = new FISDataMartEntities())
            //{
            //    var users =
            //        from u in
            //            context.udf_Catbert3_vUsers(ApplicationsAbbr)
            //        select new
            //        {
            //            u.LoginID,
            //            u.Email,
            //            u.FirstName,
            //            u.LastName
            //        };

            //    foreach (var usr in users)
            //        {
            //            Console.WriteLine(usr.LoginID);
            //            Console.WriteLine(usr.Email);
            //            Console.WriteLine(usr.FirstName);
            //            Console.WriteLine(usr.LastName);
            //        }

            //} // end using (var context = new FISDataMartEntities())
        }

        public string ErrorMessage
        {
            get { return TempData[TEMP_DATA_ERROR_MESSAGE_KEY] as string; }
            set { TempData[TEMP_DATA_ERROR_MESSAGE_KEY] = value; }
        }

        private const string TEMP_DATA_ERROR_MESSAGE_KEY = "ErrorMessage";
        private const string UserKey = "UserKey";

        //public IList<udf_Catbert3_vRoles_Result>  Roles
        //{
        //    get
        //    {
        //        using (var db = new FISDataMartEntities())
        //        {
        //            return db.udf_Catbert3_vRoles(ApplicationsAbbr).ToList();
        //        }
        //    }
        //} 
    }
}
