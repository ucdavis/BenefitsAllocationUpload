using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.IO;
using BenefitsAllocationUpload.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using FileHelpers;
using Dapper;


namespace BenefitsAllocationUpload.Services
{
    public interface IDataExtractionService
    {
        //string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId, string transDocNumberSequence);
        string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber,
            string orgRefId, string transDocNumberSequence, string orgId, string transDocOriginCode, bool useDaFIS);
    }

    /// <summary>
    /// Contains methods for extracting formatted data from database and creating a new flat file for review and upload.
    /// </summary>
    public class DataExtractionService : IDataExtractionService
    {
        private static readonly string FileTimeStampFormat = ConfigurationManager.AppSettings["FileTimeStampFormat"];
            // File name timestamp format.

        private readonly string _storageLocation = ConfigurationManager.AppSettings["StorageLocation"];
            // Directory where the files are created and stored on the server or file system.

        private static readonly int CommandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeout"]);
        private const string FilenamePrefix = "journal.";
        private const string FilenameExtension = ".txt";
        private string _fileName = String.Empty; // Name for new file when created.

        public static readonly string DefaultCollegeLevelOrg = "AAES";
        public static readonly string DefaultDivisionLevelOrgs = string.Empty;

        public string CollegeLevelOrg { get; set; }
        public string DivisionLevelOrgs { get; set; }

        protected string GetFilenameForOrgId(string orgId, string transDocOriginCode = "")
        {
            var stringBuilder = new StringBuilder(FilenamePrefix);

            if (string.IsNullOrEmpty(transDocOriginCode))
            {
                using (var context = new FISDataMartEntities())
                {
                    var orgIdParameter = new SqlParameter("orgId", orgId);
                    transDocOriginCode = context.Database.SqlQuery<string>(
                        "SELECT dbo.udf_GetTransDocOriginCodeForOrg(@orgId)", orgIdParameter).FirstOrDefault();
                }
            }

            stringBuilder.Append(transDocOriginCode);
            stringBuilder.Append(".");
            stringBuilder.Append((DateTime.Now).ToString(FileTimeStampFormat));
            stringBuilder.Append(FilenameExtension);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Extract the Benefits Allocation Data from the database and populates the FeederSystemFixedLengthRecords array. 
        /// </summary>
        /// <param name="fiscalYear"></param>
        /// <param name="fiscalPeriod"></param>
        /// <param name="transDescription"></param>
        /// <param name="orgDocNumber"></param>
        /// <param name="orgRefId"></param>
        /// <param name="transDocNumberSequence"></param>
        /// <param name="orgId"></param>
        /// <param name="transDocOriginCode"></param>
        /// <param name="useDaFIS"></param>
        /// <returns></returns>
        private IEnumerable<FeederSystemFixedLengthRecord> GetTransactions(string fiscalYear, string fiscalPeriod,
            string transDescription, string orgDocNumber, string orgRefId,
            string transDocNumberSequence, string orgId, string transDocOriginCode, bool useDaFIS)
        {
            using (var context = new FISDataMartEntities())
            {
                // 2013-05-22 by kjt: Revised to use lower level approach to
                // allow manual setting of command time-out because query
                // can run longer than default time-out of 30 seconds.
                var results = new List<BudgetAdjustmentUploadDataResults>();
                using (var command = context.Database.Connection.CreateCommand())
                {
                    // Set the command timeout because query can run longer than 
                    // default time of 30 seconds:
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = "dbo.usp_GetBudgetAdjustmentUploadDataForOrg";
                    command.CommandType = CommandType.StoredProcedure;

                    var parameters = new DynamicParameters();
                    parameters.Add("@FiscalYear", fiscalYear, DbType.String, ParameterDirection.Input);
                    parameters.Add("@FiscalPeriod", fiscalPeriod, DbType.String, ParameterDirection.Input);
                    parameters.Add("@TransDescription", transDescription, DbType.String, ParameterDirection.Input);
                    parameters.Add("@OrgDocNumber", orgDocNumber, DbType.String, ParameterDirection.Input);
                    parameters.Add("@OrgRefId", orgRefId, DbType.String, ParameterDirection.Input);
                    parameters.Add("@TransDocNumberSequence", transDocNumberSequence, DbType.String,
                        ParameterDirection.Input);
                    parameters.Add("@OrgId", orgId, DbType.String, ParameterDirection.Input);
                    parameters.Add("@TransDocOriginCode", transDocOriginCode, DbType.String, ParameterDirection.Input);
                    parameters.Add("@UseDaFIS", useDaFIS, DbType.Boolean, ParameterDirection.Input);

                    command.Connection.Open();

                    //Dapper.SqlMapper.SetTypeMap(
                    //typeof(FeederSystemFixedLengthRecord),
                    //new CustomPropertyTypeMap(
                    //    typeof(FeederSystemFixedLengthRecord),
                    //    (type, columnName) =>
                    //        type.GetProperties().FirstOrDefault(prop =>
                    //            prop.GetCustomAttributes(false)
                    //                .OfType<ColumnAttribute>()
                    //                .Any(attr => attr.Name == columnName))));

                    
                    results = command.Connection.Query<BudgetAdjustmentUploadDataResults>(command.CommandText, parameters,
                        transaction: null, buffered: true, commandTimeout: command.CommandTimeout,
                        commandType: CommandType.StoredProcedure).ToList();

                }

                var transactions = new List<FeederSystemFixedLengthRecord>();
                foreach (var result in results)
                {
                    transactions.Add(new FeederSystemFixedLengthRecord(result));
                }
                return transactions;
            }
        }


        /// <summary>
        /// Alternate method for extracting Benefits Allocation data using CAES Local FIS DataMart (for AAES only) OR
        /// Campus Data Warehouse for AAES --OR-- any other college/division that we've configured the program to work for. 
        /// Extracts formatted data from database and creates a new flat file for review and subsequent upload.
        /// </summary>
        /// <param name="fiscalYear">The current fiscal year</param>
        /// <param name="fiscalPeriod">Period in which adjustments are to be applied</param>
        /// <param name="transDescription">Some kind of meaningful description. 40 characters max.</param>
        /// <param name="orgDocNumber">Optional: An Organizational Document Number for all the transactions overall.  10 characters max.</param>
        /// <param name="orgRefId">An Organizational Reference ID for the transactions overall. 8 characters max.</param>
        /// <param name="transDocNumberSequence">Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!</param>
        /// <param name="orgId">Kuali level 40 (College level) or level 5 (Division Level) OrgId</param>
        /// <param name="transDocOriginCode">Unique origin code identifier provided by AFS for particular college</param>
        /// <param name="useDaFIS">Set this to 1 for any org other than AAES or if you want to get the data directly from DaFIS instead of our local FISDataMart.</param>
        /// <returns>Name of newly created flat file</returns>
        public string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber,
           string orgRefId,
           string transDocNumberSequence, string orgId, string transDocOriginCode, bool useDaFIS)
        {
            var transactions = GetTransactions(fiscalYear, fiscalPeriod,
             transDescription, orgDocNumber, orgRefId,
             transDocNumberSequence, orgId, transDocOriginCode, useDaFIS);

            var transactionsArray = transactions as FeederSystemFixedLengthRecord[] ?? transactions.ToArray();
            if (transactionsArray.Any())
            {
                _fileName = GetFilenameForOrgId(orgId, transDocOriginCode);
                var filenameAndPath = String.Format(@"{0}\{1}", HostingEnvironment.MapPath(_storageLocation), _fileName);

                using (var docStream = new MemoryStream())
                {
                    using (var flatFileWriter = new StreamWriter(docStream, Encoding.ASCII))
                    {
                        var fileWriterEngine = new FileHelperEngine(typeof(FeederSystemFixedLengthRecord));
                        fileWriterEngine.WriteStream(flatFileWriter, transactionsArray);

                        // Flush contents of fileWriterStream to underlying docStream:
                        flatFileWriter.Flush();

                        using (var file = new FileStream(filenameAndPath, FileMode.Create, FileAccess.Write))
                        {
                            docStream.WriteTo(file);
                        }
                    }
                }
            }
            return _fileName;
        }
    }
}