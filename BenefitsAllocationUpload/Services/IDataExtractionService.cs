using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using BenefitsAllocationUpload.Models;
using UCDArch.Core.PersistanceSupport;

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

        //Old method prior to modification for multi-college use.
        ///// <summary>
        ///// Default method For extracting AAES data using CAES Local FIS DataMart 
        ///// formats data from database and creates a new flat file for review and subsequent upload.
        ///// </summary>
        ///// <param name="fiscalYear">Current fiscal year</param>
        ///// <param name="fiscalPeriod">Period in which adjustments are to be applied</param>
        ///// <param name="transDescription">Some kind of meaningful description. 40 characters max</param>
        ///// <param name="orgDocNumber">Optional: An Organizational Document Number for all the transactions overall.  10 characters max.</param>
        ///// <param name="orgRefId">An Organizational Reference ID for the transactions overall. 8 characters max.</param>
        ///// <param name="transDocNumberSequence">Change the ### to something meaningful. A unique sequence number for the group of transactions. Must be 3 characters!</param>
        ///// <returns>Name of newly created flat file</returns>
        //public string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId, string transDocNumberSequence)
        //{
        //    using (var context = new FISDataMartEntities())
        //    {
        //        var transactions =
        //            from t in context.udf_GetBudgetAdjustmentUploadData(fiscalYear, fiscalPeriod, transDescription, orgDocNumber, orgRefId, transDocNumberSequence) 
        //            select new
        //            {
        //                t.UNIV_FISCAL_YEAR,
        //                t.FIN_COA_CD,
        //                t.ACCOUNT_NBR,
        //                t.SUB_ACCT_NBR,
        //                t.FIN_OBJECT_CD,
        //                t.FIN_SUB_OBJ_CD,
        //                t.FIN_BALANCE_TYP_CD,
        //                t.FIN_OBJ_TYP_CD,
        //                t.UNIV_FISCAL_PRD_CD,
        //                t.FDOC_TYP_CD,
        //                t.FS_ORIGIN_CD,
        //                t.FDOC_NBR,
        //                t.TRN_ENTR_SEQ_NBR,
        //                t.TRN_LDGR_ENTR_DESC,
        //                t.TRN_LDGR_ENTR_AMT,
        //                t.TRN_DEBIT_CRDT_CD,
        //                t.TRANSACTION_DT,
        //                t.ORG_DOC_NBR,
        //                t.PROJECT_CD,
        //                t.ORG_REFERENCE_ID,
        //                t.FDOC_REF_TYP_CD,
        //                t.FS_REF_ORIGIN_CD,
        //                t.FDOC_REF_NBR,
        //                t.FDOC_REVERSAL_DT,
        //                t.TRN_ENCUM_UPDT_CD
        //            };

        //        using (
        //            var file =
        //                new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
        //        {
        //            foreach (var trans in transactions)
        //            {
        //                file.WriteLine(
        //                    "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}{21}{22}{23}{24}",

        //                    trans.UNIV_FISCAL_YEAR,
        //                    trans.FIN_COA_CD,
        //                    trans.ACCOUNT_NBR,
        //                    trans.SUB_ACCT_NBR,
        //                    trans.FIN_OBJECT_CD,
        //                    trans.FIN_SUB_OBJ_CD,
        //                    trans.FIN_BALANCE_TYP_CD,
        //                    trans.FIN_OBJ_TYP_CD,
        //                    trans.UNIV_FISCAL_PRD_CD,
        //                    trans.FDOC_TYP_CD,
        //                    trans.FS_ORIGIN_CD,
        //                    trans.FDOC_NBR,
        //                    trans.TRN_ENTR_SEQ_NBR,
        //                    trans.TRN_LDGR_ENTR_DESC,
        //                    trans.TRN_LDGR_ENTR_AMT,
        //                    trans.TRN_DEBIT_CRDT_CD,
        //                    trans.TRANSACTION_DT,
        //                    trans.ORG_DOC_NBR,
        //                    trans.PROJECT_CD,
        //                    trans.ORG_REFERENCE_ID,
        //                    trans.FDOC_REF_TYP_CD,
        //                    trans.FS_REF_ORIGIN_CD,
        //                    trans.FDOC_REF_NBR,
        //                    trans.FDOC_REVERSAL_DT,
        //                    trans.TRN_ENCUM_UPDT_CD
        //                    );
        //            }

        //            file.Flush();
        //            file.Close();
        //            file.Dispose();

        //        }  // end using (var file = new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
        //    } // end using (var context = new FISDataMartEntities())
        //    return _fileName;
        //} // end public void CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgRefId, string transDocNumberSequence)

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
        public string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId,
                                 string transDocNumberSequence, string orgId, string transDocOriginCode, bool useDaFIS)
        {
            _fileName = GetFilenameForOrgId(orgId, transDocOriginCode);

            using (var context = new FISDataMartEntities())
            {
                // 2013-05-22 by kjt: Revised to use lower level approach to
                // allow manual setting of command time-out because query
                // can run longer than default time-out of 30 seconds.

                //var transactions = new List<BudgetAdjustmentUploadDataResults>();
                var command = context.Database.Connection.CreateCommand();
                // Set the command timeout because query can run longer than 
                // default time of 30 seconds:
                command.CommandTimeout = 60;
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
                    using (
                        var file =
                            new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
                    {
                        while (reader.Read())
                        {
                            file.WriteLine(
                                "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}{21}{22}{23}{24}",
                                reader[0].ToString(),
                                reader[1].ToString(),
                                reader[2].ToString(),
                                reader[3].ToString(),
                                reader[4].ToString(),
                                reader[5].ToString(),
                                reader[6].ToString(),
                                reader[7].ToString(),
                                reader[8].ToString(),
                                reader[9].ToString(),
                                reader[10].ToString(),
                                reader[11].ToString(),
                                reader[12].ToString(),
                                reader[13].ToString(),
                                reader[14].ToString(),
                                reader[15].ToString(),
                                reader[16].ToString(),
                                reader[17].ToString(),
                                reader[18].ToString(),
                                reader[19].ToString(),
                                reader[20].ToString(),
                                reader[21].ToString(),
                                reader[22].ToString(),
                                reader[23].ToString(),
                                reader[24].ToString()
                                );
                        }

                        file.Flush();
                        file.Close();
                        file.Dispose();
                    } // end using (var file = new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
                } // end if (reader.HasRows) 

                reader.Close();
                command.Connection.Close();
                command.Dispose();
              
                // Changed to more lower-level implementation, above, to allow setting of command time-out,
                // because query can run over 30 seconds.
                //var transactions =
                //    context.usp_GetBudgetAdjustmentUploadDataForOrg(fiscalYear, fiscalPeriod, transDescription,
                //                                                    orgDocNumber, orgRefId, transDocNumberSequence,
                //                                                    orgId, transDocOriginCode, useDaFIS, false).ToList();
                // Don't need to explicitly define select list as we're asking for all columns 
                // that are already in correct order.
                           //.Select(t => new
                           //    {
                           //        t.UNIV_FISCAL_YEAR,
                           //        t.FIN_COA_CD,
                           //        t.ACCOUNT_NBR,
                           //        t.SUB_ACCT_NBR,
                           //        t.FIN_OBJECT_CD,
                           //        t.FIN_SUB_OBJ_CD,
                           //        t.FIN_BALANCE_TYP_CD,
                           //        t.FIN_OBJ_TYP_CD,
                           //        t.UNIV_FISCAL_PRD_CD,
                           //        t.FDOC_TYP_CD,
                           //        t.FS_ORIGIN_CD,
                           //        t.FDOC_NBR,
                           //        t.TRN_ENTR_SEQ_NBR,
                           //        t.TRN_LDGR_ENTR_DESC,
                           //        t.TRN_LDGR_ENTR_AMT,
                           //        t.TRN_DEBIT_CRDT_CD,
                           //        t.TRANSACTION_DT,
                           //        t.ORG_DOC_NBR,
                           //        t.PROJECT_CD,
                           //        t.ORG_REFERENCE_ID,
                           //        t.FDOC_REF_TYP_CD,
                           //        t.FS_REF_ORIGIN_CD,
                           //        t.FDOC_REF_NBR,
                           //        t.FDOC_REVERSAL_DT,
                           //        t.TRN_ENCUM_UPDT_CD
                           //    });

                //using (
                //    var file =
                //        new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
                //{
                //    foreach (var trans in transactions)
                //    {
                //        file.WriteLine(
                //            "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}{21}{22}{23}{24}",

                //            trans.UNIV_FISCAL_YEAR,
                //            trans.FIN_COA_CD,
                //            trans.ACCOUNT_NBR,
                //            trans.SUB_ACCT_NBR,
                //            trans.FIN_OBJECT_CD,
                //            trans.FIN_SUB_OBJ_CD,
                //            trans.FIN_BALANCE_TYP_CD,
                //            trans.FIN_OBJ_TYP_CD,
                //            trans.UNIV_FISCAL_PRD_CD,
                //            trans.FDOC_TYP_CD,
                //            trans.FS_ORIGIN_CD,
                //            trans.FDOC_NBR,
                //            trans.TRN_ENTR_SEQ_NBR,
                //            trans.TRN_LDGR_ENTR_DESC,
                //            trans.TRN_LDGR_ENTR_AMT,
                //            trans.TRN_DEBIT_CRDT_CD,
                //            trans.TRANSACTION_DT,
                //            trans.ORG_DOC_NBR,
                //            trans.PROJECT_CD,
                //            trans.ORG_REFERENCE_ID,
                //            trans.FDOC_REF_TYP_CD,
                //            trans.FS_REF_ORIGIN_CD,
                //            trans.FDOC_REF_NBR,
                //            trans.FDOC_REVERSAL_DT,
                //            trans.TRN_ENCUM_UPDT_CD
                //            );
                //    }

                //    file.Flush();
                //    file.Close();
                //    file.Dispose();

                //}  // end using (var file = new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
            } // end using (var context = new FISDataMartEntities())
            return _fileName;
        }
    }
}