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
                lstFiles.Add(new FileNames()
                    {

                        FileId = i + 1,
                        FileName = item.Name,
                        TimeStamp = item.CreationTime,
                        FilePath = dirInfo.FullName + @"\" + item.Name
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
        /// <summary>
        /// The system generated ID of the file
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// The Name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The Time Stamp when the file was created (or last modified)
        /// </summary>
        public DateTimeOffset? TimeStamp { get; set; }
        // public DateTime TimeStamp { get; set; }

        /// <summary>
        /// The File Path of the file
        /// </summary>
        public string FilePath { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Uploaded { get; set; }
        public string UploadedBy { get; set; }
        public string SchoolCode { get; set; }

        /// <summary>
        /// The Length of the file in bytes.
        /// </summary>
        public long Length { get; set; }
    }

    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
        public const string User = "User";
        public const string EmulationUser = "EmulationUser";
        public const string UploadFile = "UploadFile";
        public const string EditReimbursableAccounts = "EditReimbursableAccounts";
    }

    public partial class Unit
    {
        private string _deansOfficeSchoolCode;

        public virtual string DeansOfficeSchoolCode
        {
            get
            {
                _deansOfficeSchoolCode = SchoolCode;
                string[] inPpsCodes = {"065040", "065025", "065130"};
                string[] notInPpsCodes = {"036000", "036005"};

                var ppsCode = (!String.IsNullOrEmpty(PPS_Code) ? PPS_Code.Trim() : string.Empty);
                if ((ppsCode.StartsWith("030") || (inPpsCodes.Contains(ppsCode) && !notInPpsCodes.Contains(ppsCode))))
                    _deansOfficeSchoolCode = "01";

                return _deansOfficeSchoolCode;
            }
            set { _deansOfficeSchoolCode = value; }
        }
    }

    public partial class User
    {
        private static readonly string ApplicationsAbbr = ConfigurationManager.AppSettings["applicationsAbbr"];
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
            using (var db = new CATBERT3Entities())
            {
                var user =
                    db.Database.SqlQuery<User>(
                        "SELECT DISTINCT users.* " +
                        "FROM Catbert3.dbo.Users AS users " +
                        "INNER JOIN Catbert3.dbo.Permissions permissions ON permissions.UserID = users.UserID " +
                        "INNER JOIN Catbert3.dbo.Applications AS apps ON permissions.ApplicationID = apps.ApplicationID " +
                        "WHERE apps.Abbr LIKE '" + ApplicationsAbbr +
                        "' AND permissions.Inactive = 0 ").FirstOrDefault(u => u.LoginID.Equals(loginId));

                if (user != null)
                {
                    user.Units = db.Database.SqlQuery<Unit>("SELECT unit.* " +
                                                            "FROM catbert3.dbo.Unit unit " +
                                                            "INNER JOIN Catbert3.dbo.UnitAssociations AS unitAssociations ON unit.UnitID = unitAssociations.UnitID " +
                                                            "INNER JOIN Catbert3.dbo.Applications AS applications ON unitAssociations.ApplicationID = applications.ApplicationID " +
                                                            "INNER JOIN Catbert3.dbo.Users AS users ON unitAssociations.UserID = users.UserID " +
                                                            "WHERE (applications.Abbr LIKE '" + ApplicationsAbbr + "') " +
                                                            "    AND (unitAssociations.Inactive = 0) " +
                                                            "    AND users.LoginID = '" + loginId + "'").ToList();

                    user.Roles =
                        db.Database.SqlQuery<Role>("SELECT roles.RoleID, roles.Role AS Role1, permissions.Inactive " +
                                                   "FROM catbert3.dbo.Roles roles " +
                                                   "INNER JOIN Catbert3.dbo.Permissions AS permissions ON roles.RoleID = permissions.RoleID " +
                                                   "INNER JOIN Catbert3.dbo.Applications AS applications ON permissions.ApplicationID = applications.ApplicationID " +
                                                   "INNER JOIN Catbert3.dbo.Users AS users ON permissions.UserID = users.UserID " +
                                                   "WHERE applications.Abbr LIKE '" + ApplicationsAbbr + "' " +
                                                   "  AND permissions.Inactive = 0 " +
                                                   "  AND users.LoginID = '" + loginId + "'").ToList();
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

    public class FiscalPeriod
    {
        public string Period { get; set; }
        public string Name { get; set; }
    }
}
