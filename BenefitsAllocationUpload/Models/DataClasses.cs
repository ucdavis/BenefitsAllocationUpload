using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using BenefitsAllocation.Core.Domain;
using System.Data.Objects.DataClasses;
using System.Data.Objects;


namespace BenefitsAllocationUpload.Models
{
    //public static class EdmFunctions
    //{
    //    [EdmFunction("FISDataMartModel.Store", "udf_GetOrgIdForSchoolCode")]
    //    public static string udf_GetOrgIdForSchoolCode(string schoolCode)
    //    {
    //        throw new NotSupportedException("Direct calls are not supported.");
    //    }
    //}

    public class DataClasses
    {
        private readonly string _storageLocation = ConfigurationManager.AppSettings["StorageLocation"];
        public static readonly string TemplateFileExtension = ConfigurationManager.AppSettings["TemplateFileExtension"];

        public List<FileNames> GetFiles()
        {
            //var user = User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
            var lstFiles = new List<FileNames>();
            var dirInfo = new DirectoryInfo(HostingEnvironment.MapPath(_storageLocation));
            
            int i = 0;
            foreach (var item in dirInfo.GetFiles())
            {
                lstFiles.Add(new FileNames() { 

                FileId = i + 1, FileName = item.Name, TimeStamp = item.CreationTime, FilePath = dirInfo.FullName+@"\"+item.Name
                });
                i = i + 1;
            }
            using (var db = new FISDataMartEntities())
            {
                foreach (var file in lstFiles)
                {
                    var unitFileResult = db.UnitFiles.FirstOrDefault(u => u.Filename.Equals(file.FileName));
                    if (unitFileResult != null)
                    {
                        file.CreatedBy = unitFileResult.CreatedBy;
                        file.Uploaded = unitFileResult.Uploaded;
                        file.UploadedBy = unitFileResult.UploadedBy;
                        file.SchoolCode = unitFileResult.SchoolCode;
                    }
                }
            }

            return lstFiles; 
        }

        public List<FileNames> GetFiles(string schoolCode)
        {
            return GetFiles().Where(file => file.SchoolCode.Equals("00") || file.SchoolCode.Equals(schoolCode)).ToList();
        }
    }
 
    public class FileNames
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public DateTime TimeStamp { get; set; }
        public string FilePath { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Uploaded { get; set; }
        public string UploadedBy { get; set; }
        public string SchoolCode { get; set; }
    }

    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string User = "User";
        public const string EmulationUser = "EmulationUser";
        public const string UploadFile = "UploadFile";

    }

    public class User
    {
        public virtual string LoginID { get; set; }

        public virtual string Email { get; set; }

        public virtual string Phone { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        private string _fullName;

        public virtual string FullName
        {
            get { return (String.IsNullOrEmpty(_fullName) ? LastName + ", " + FirstName : _fullName); }
            set { _fullName = value; }
        }

        public virtual string EmployeeID { get; set; }

        public virtual string StudentID { get; set; }

        public virtual string UserImage { get; set; }

        public virtual string SID { get; set; }

        //private bool _Inactive;

        //public virtual bool Inactive
        //{
        //    get { return _Inactive; }
        //    set { _Inactive = value; }
        //}

        public virtual Guid UserKey { get; set; }

        public virtual IList<Unit> Units { get; set; }

        public virtual IList<Roles> Roles { get; set; }

        /// <summary>
        /// This is in order to demo an "entitled" user as a department user.
        /// </summary>
        public virtual bool IsDepartmentUser { get; set; }

        public static User FindByLoginId(string loginId)
        {
           // throw new System.NotImplementedException();
            using (var db = new FISDataMartEntities())
            {
                var userResult = db.udf_Catbert3_vUsers(ConfigurationManager.AppSettings["applicationsAbbr"]).FirstOrDefault(r => r.LoginID == loginId);
                
                if (userResult != null)
                {
                    var userUnitResults = db.udf_Catbert3_vUserUnits(ConfigurationManager.AppSettings["applicationsAbbr"])
                                    .Where(uu => uu.UserID == userResult.UserID);

                    var unitResults = db.udf_Catbert3_vUnit(ConfigurationManager.AppSettings["applicationsAbbr"])
                                .Where(u => userUnitResults.Any(uu => u.UnitID == uu.UnitId));

                    var units = new List<Unit>();
                    if (unitResults.Any())
                    {
                        units.AddRange(unitResults.Select(ur => new Unit()
                            {
                                ShortName = ur.ShortName, FullName = ur.FullName, FISCode = ur.FIS_Code, DeansOfficeSchoolCode = ur.DeansOfficeSchoolCode, UnitID = ur.UnitID, PPSCode = ur.PPS_Code, SchoolCode = ur.SchoolCode
                            }));
                    }
                    return new User()
                        {
                            FirstName = userResult.FirstName,
                            LastName = userResult.LastName,
                            LoginID = userResult.LoginID,
                            Email = userResult.Email,
                            EmployeeID = userResult.EmployeeID,
                            Phone = userResult.Phone,
                            Units =  units
                            
                        };
                }
                else return null;
            }
        }

        public User()
        {
            IsDepartmentUser = false;
        }
    }

    public class TransDocOriginCode
    {
        public virtual string FsOriginCode { get; set; }
    }
}
