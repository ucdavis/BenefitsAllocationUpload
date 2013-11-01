using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Renci.SshNet;

namespace BenefitsAllocationUpload.Services
{
    public interface ISftpService
    {
        void UploadFile(string filename, string schoolCode);
    }

    public class SftpService : ISftpService
    {
        private static readonly string SftpHostName = ConfigurationManager.AppSettings["SftpHostName"]; // SFTP hostname, i.e. fis-depot-test.ucdavis.edu
        private static readonly int SftpPortNumber = Int16.Parse(ConfigurationManager.AppSettings["SftpPortNumber"]); // SFTP port number, typically 22
        private readonly string _storageLocation = ConfigurationManager.AppSettings["StorageLocation"]; // Directory where the files are created and stored on the server or file system.
        private readonly string _uploadDirectory = ConfigurationManager.AppSettings["UploadDirectory"]; // Directory where the files are to be uploaded to on the A&FS server, defaults to root "./"
        protected string SftpUsername { get; set; } // SFTP user name.  
        protected string SftpPassword { get; set; } // SFTP password. May be replaced by key file at future date.

        public SftpService()
        {

        }

        /// <summary>
        /// Upload file to Accounting and Financial Services host for import into General Ledger Current Budget for Benefits Allocation.
        /// </summary>
        /// <param name="filename">Name of file to be uploaded</param>
        /// <param name="schoolCode">School Code of corresponding school</param>
        public void UploadFile(string filename, string schoolCode)
        {
            SftpUsername = ConfigurationManager.AppSettings["SftpUsername01"]; // SFTP user name. 
            SftpPassword = ConfigurationManager.AppSettings["SftpPassword01"]; // SFTP password. May be replaced by key file at future date.

            if (!string.IsNullOrEmpty(schoolCode) && !schoolCode.Equals("01")) // Default CAES school code hard coded because this should never change.
            {
                SftpUsername = ConfigurationManager.AppSettings["SftpUsername" + schoolCode];
                SftpPassword = ConfigurationManager.AppSettings["SftpPassword" + schoolCode];
            }

            using (var sftp = new SftpClient(SftpHostName, SftpPortNumber, SftpUsername, SftpPassword))
            {
                sftp.Connect();
                //var handle = File.OpenRead(_storageLocation + filename);
                var handle = File.OpenRead(HostingEnvironment.MapPath(_storageLocation) + "\\" + filename);

                sftp.UploadFile(handle, _uploadDirectory + filename);

                handle.Close();
                handle.Dispose();

                sftp.Disconnect();
                sftp.Dispose();
            }
        }
    }
}