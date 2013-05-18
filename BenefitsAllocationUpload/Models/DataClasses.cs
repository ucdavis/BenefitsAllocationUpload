using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;

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
            var user = User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
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
                var unitFiles = db.UnitFiles.ToList();
                foreach (var file in lstFiles)
                {
                    var unitFileResult = unitFiles.FirstOrDefault(u => u.Filename.Equals(file.FileName));
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
            //return GetFiles().Where(file => file.SchoolCode.Equals("00") || file.SchoolCode.Equals(schoolCode)).ToList();
            var files = GetFiles();

            var retval =
                files.Where(
                    file =>
                    file.SchoolCode != null && (file.SchoolCode.Equals("00") || file.SchoolCode.Equals(schoolCode)))
                     .ToList();

            return retval;
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

    public partial class User
    {
        private string _fullName;

        public virtual string FullName
        {
            get { return (String.IsNullOrEmpty(_fullName) ? LastName + ", " + FirstName : _fullName); }
            set { _fullName = value; }
        }

        public virtual IList<Role> Roles { get; set; }

        public virtual IList<Unit> Units { get; set; }

        public static User FindByLoginId(string loginId)
        {
            using (var db = new CATBERT3Entities1())
            {
                var userResult = db.udf_Catbert3_vUsers(ConfigurationManager.AppSettings["applicationsAbbr"]).FirstOrDefault(r => r.LoginID == loginId);
                if (userResult != null)
                {
                    var userId = userResult.UserID;

                    var units = db.Database.SqlQuery<Unit>("SELECT unit.* FROM catbert3.dbo.Unit unit" +
                                                           " INNER JOIN catbert3.dbo.udf_Catbert3_vUserUnits('" +
                                                           ConfigurationManager.AppSettings["applicationsAbbr"] +
                                                           "') uu ON unit.UnitID = uu.UnitID WHERE uu.UserID = " +
                                                           userId);
                    
                      
                    var roles = db.Database.SqlQuery<Role>("SELECT roles.RoleID, roles.Role AS Role1, roles.Inactive FROM catbert3.dbo.Roles roles" +
                                                           " INNER JOIN catbert3.dbo.udf_Catbert3_vUserRoles('" +
                                                           ConfigurationManager.AppSettings["applicationsAbbr"] +
                                                           "') ur ON roles.RoleID = ur.RoleID WHERE ur.UserID = " +
                                                           userId);
            
                    var user = new User
                        {
                            UserID = userResult.UserID,
                            LoginID = userResult.LoginID,
                            FirstName = userResult.FirstName,
                            LastName = userResult.LastName,
                            EmployeeID = userResult.EmployeeID,
                            StudentID = userResult.StudentID,
                            UserImage = userResult.UserImage,
                            SID = userResult.SID,
                            UserKey = userResult.UserKey,
                            Email = userResult.Email,
                            Phone = userResult.Phone,
                            Units = units.ToList(),
                            Roles = roles.ToList()
                        };

                    return user;
                }
                    return null;
            }
        }
    }

    public class TransDocOriginCode
    {
        public virtual string FsOriginCode { get; set; }
    }
}
