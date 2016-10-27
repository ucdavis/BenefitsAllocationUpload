using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Util;
using BenefitsAllocationUpload.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using FileHelpers;
using NHibernate.Mapping;
using NPOI.SS.Formula.Functions;

namespace BenefitsAllocationUpload.Services
{
    public interface IDataExtractionService
    {
        //string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId, string transDocNumberSequence);
        string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId, string transDocNumberSequence, string orgId, string transDocOriginCode, bool useDaFIS);
    }

    /// <summary>
    /// Contains methods for extracting formatted data from database and creating a new flat file for review and upload.
    /// </summary>
    public class DataExtractionService : IDataExtractionService
    {
        private static readonly string FileTimeStampFormat = ConfigurationManager.AppSettings["FileTimeStampFormat"]; // File name timestamp format.
        private readonly string _storageLocation = ConfigurationManager.AppSettings["StorageLocation"]; // Directory where the files are created and stored on the server or file system.
        private static readonly int _commandTimeout = int.Parse(ConfigurationManager.AppSettings["CommandTimeout"]);
        private const string FilenamePrefix = "journal.";
        private const string FilenameExtension = ".txt";
        private string _fileName = String.Empty; // Name for new file when created.
       
        public static readonly string DefaultCollegeLevelOrg = "AAES";
        public static readonly string DefaultDivisionLevelOrgs = string.Empty;

        public string CollegeLevelOrg { get; set; }
        public string DivisionLevelOrgs { get; set; }

        protected string GetFilenameForOrgId(string orgId, string transDocOriginCode="")
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

                var transactions = new List<FeederSystemFixedLengthRecord>();
                var command = context.Database.Connection.CreateCommand();
                // Set the command timeout because query can run longer than 
                // default time of 30 seconds:
                command.CommandTimeout = _commandTimeout;
                command.CommandText = "dbo.usp_GetBudgetAdjustmentUploadDataForOrg";
                command.CommandType = CommandType.StoredProcedure;

                var parameter = new SqlParameter
                {
                    ParameterName = "@FiscalYear",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = fiscalYear
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@FiscalPeriod",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = fiscalPeriod
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@TransDescription",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = transDescription
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@OrgDocNumber",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = orgDocNumber
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@OrgRefId",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = orgRefId
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@TransDocNumberSequence",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = transDocNumberSequence
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@OrgId",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = orgId
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@TransDocOriginCode",
                    SqlDbType = SqlDbType.VarChar,
                    Direction = ParameterDirection.Input,
                    Value = transDocOriginCode
                };
                command.Parameters.Add(parameter);

                parameter = new SqlParameter
                {
                    ParameterName = "@UseDaFIS",
                    SqlDbType = SqlDbType.Bit,
                    Direction = ParameterDirection.Input,
                    Value = useDaFIS
                };
                command.Parameters.Add(parameter);

                command.Connection.Open();
                
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var transaction = new FeederSystemFixedLengthRecord()
                        {
                            FiscalYear                      = Convert.ToInt32(reader[0].ToString()),
                            ChartNum                        = reader[1].ToString(),
                            Account                         = reader[2].ToString(),
                            SubAccount                      = reader[3].ToString(),
                            ObjectCode                      = reader[4].ToString(),
                            SubObjectCode                   = reader[5].ToString(),
                            BalanceType                     = reader[6].ToString(),
                            ObjectType                      = reader[7].ToString(),
                            FiscalPeriod                    = reader[8].ToString(),
                            DocumentType                    = reader[9].ToString(),
                            OriginCode                      = reader[10].ToString(),
                            DocumentNumber                  = reader[11].ToString(),
                            LineSequenceNumber              = reader[12].ToString(),
                            TransactionDescription          = reader[13].ToString(),
                            Amount                          = reader[14].ToString(),
                            DebitCreditCode                 = reader[15].ToString(),
                            TransactionDate                 = DateTime.ParseExact(reader[16].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture),
                            OrganizationTrackingNumber      = reader[17].ToString(),
                            ProjectCode                     = reader[18].ToString(),
                            OrganizationReferenceId         = reader[19].ToString(),
                            ReferenceTypeCode               = reader[20].ToString(),
                            ReferenceOriginCode             = reader[21].ToString(),
                            ReferenceNumber                 = reader[22].ToString(),
                            ReversalDate                    = reader[23].ToString(),
                            TransactionEncumbranceUpdateCode= reader[24].ToString()
                        };
                        transactions.Add(transaction);
                    }
                } // end if (reader.HasRows) 

                reader.Close();
                command.Connection.Close();
                command.Dispose();

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