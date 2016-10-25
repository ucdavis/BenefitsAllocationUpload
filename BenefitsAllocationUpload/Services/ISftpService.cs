using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using Renci.SshNet;

namespace BenefitsAllocationUpload.Services
{
    public interface ISftpService
    {
        /// <summary>
        /// Upload file to Accounting and Financial Services host for import into General Ledger Current Budget for Benefits Allocation.
        /// </summary>
        /// <param name="filename">Name of file to be uploaded</param>
        /// <param name="schoolCode">School Code of corresponding school</param>
        void UploadFile(string filename, string schoolCode);

            /// <summary>
        /// Upload/write a stream to the Accounting and Financial Services SFTP server
        /// </summary>
        /// <param name="dataStream">The data stream to be uploaded.</param>
        /// <param name="filename">The filename of the be assigned</param>
        /// <param name="schoolCode">The school code, defaults to "01" (CAES).</param>
        void UploadStream(Stream dataStream, string filename, string schoolCode);
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
            using (var sftp = GetSftpClient(schoolCode))
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

        /// <summary>
        /// Upload/write a stream to the Accounting and Financial Services SFTP server
        /// </summary>
        /// <param name="dataStream">The data stream to be uploaded.</param>
        /// <param name="filename">The filename of the be assigned</param>
        /// <param name="schoolCode">The school code, defaults to "O1" (CAES).</param>
        public void UploadStream(Stream dataStream, string filename, string schoolCode)
        {
            using (var sftp = GetSftpClient(schoolCode))
            {
                sftp.Connect();

                dataStream.Seek(0, SeekOrigin.Begin);
                sftp.UploadFile(dataStream, _uploadDirectory + filename);

                sftp.Disconnect();
                sftp.Dispose();
            }
        }

        private SftpClient GetSftpClient(string schoolCode = null)
        {
            SftpUsername = ConfigurationManager.AppSettings["SftpUsername01"]; // SFTP user name. 
            SftpPassword = ConfigurationManager.AppSettings["SftpPassword01"]; // SFTP password. May be replaced by key file at future date.

            if (!string.IsNullOrEmpty(schoolCode) && !schoolCode.Equals("01")) // Default CAES school code hard coded because this should never change.
            {
                SftpUsername = ConfigurationManager.AppSettings["SftpUsername" + schoolCode];
                SftpPassword = ConfigurationManager.AppSettings["SftpPassword" + schoolCode];
            }

            return new SftpClient(SftpHostName, SftpPortNumber, SftpUsername, SftpPassword);
        }
    }
}