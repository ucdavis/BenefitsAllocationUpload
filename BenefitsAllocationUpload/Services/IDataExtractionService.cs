using System;
using System.Configuration;
using System.Linq;
using System.Web.Hosting;
using BenefitsAllocationUpload.Models;
using UCDArch.Core.PersistanceSupport;

namespace BenefitsAllocationUpload.Services
{
    public interface IDataExtractionService
    {
        string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId, string transDocNumberSequence);
    }

    /// <summary>
    /// Extract formatted data from database and create a new flat file for review and upload.
    /// </summary>
    public class DataExtractionService : IDataExtractionService
    {
        private static readonly string FileTimeStampFormat = ConfigurationManager.AppSettings["FileTimeStampFormat"]; // File name timestamp format.
        private readonly string _storageLocation = ConfigurationManager.AppSettings["StorageLocation"]; // Directory where the files are created and stored on the server or file system.
        private readonly string _fileName = "journal.AG." + (DateTime.Now).ToString(FileTimeStampFormat) + ".txt"; // Name for new file when created.
        public static readonly string CollegeLevelOrg = "AAES";
        public static readonly string DivisionLevelOrgs = string.Empty;

        public string CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgDocNumber, string orgRefId, string transDocNumberSequence)
        {
            using (var context = new FISDataMartEntities())
            {
                var transactions =
                    from t in context.udf_GetBudgetAdjustmentUploadData(fiscalYear, fiscalPeriod, transDescription, orgDocNumber, orgRefId, transDocNumberSequence) 
                    select new
                    {
                        t.UNIV_FISCAL_YEAR,
                        t.FIN_COA_CD,
                        t.ACCOUNT_NBR,
                        t.SUB_ACCT_NBR,
                        t.FIN_OBJECT_CD,
                        t.FIN_SUB_OBJ_CD,
                        t.FIN_BALANCE_TYP_CD,
                        t.FIN_OBJ_TYP_CD,
                        t.UNIV_FISCAL_PRD_CD,
                        t.FDOC_TYP_CD,
                        t.FS_ORIGIN_CD,
                        t.FDOC_NBR,
                        t.TRN_ENTR_SEQ_NBR,
                        t.TRN_LDGR_ENTR_DESC,
                        t.TRN_LDGR_ENTR_AMT,
                        t.TRN_DEBIT_CRDT_CD,
                        t.TRANSACTION_DT,
                        t.ORG_DOC_NBR,
                        t.PROJECT_CD,
                        t.ORG_REFERENCE_ID,
                        t.FDOC_REF_TYP_CD,
                        t.FS_REF_ORIGIN_CD,
                        t.FDOC_REF_NBR,
                        t.FDOC_REVERSAL_DT,
                        t.TRN_ENCUM_UPDT_CD
                    };

                using (
                    var file =
                        new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
                {
                    foreach (var trans in transactions)
                    {
                        file.WriteLine(
                            "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18}{19}{20}{21}{22}{23}{24}",

                            trans.UNIV_FISCAL_YEAR,
                            trans.FIN_COA_CD,
                            trans.ACCOUNT_NBR,
                            trans.SUB_ACCT_NBR,
                            trans.FIN_OBJECT_CD,
                            trans.FIN_SUB_OBJ_CD,
                            trans.FIN_BALANCE_TYP_CD,
                            trans.FIN_OBJ_TYP_CD,
                            trans.UNIV_FISCAL_PRD_CD,
                            trans.FDOC_TYP_CD,
                            trans.FS_ORIGIN_CD,
                            trans.FDOC_NBR,
                            trans.TRN_ENTR_SEQ_NBR,
                            trans.TRN_LDGR_ENTR_DESC,
                            trans.TRN_LDGR_ENTR_AMT,
                            trans.TRN_DEBIT_CRDT_CD,
                            trans.TRANSACTION_DT,
                            trans.ORG_DOC_NBR,
                            trans.PROJECT_CD,
                            trans.ORG_REFERENCE_ID,
                            trans.FDOC_REF_TYP_CD,
                            trans.FS_REF_ORIGIN_CD,
                            trans.FDOC_REF_NBR,
                            trans.FDOC_REVERSAL_DT,
                            trans.TRN_ENCUM_UPDT_CD
                            );
                    }

                    file.Flush();
                    file.Close();
                    file.Dispose();

                }  // end using (var file = new System.IO.StreamWriter(HostingEnvironment.MapPath(_storageLocation) + "\\" + _fileName))
            } // end using (var context = new FISDataMartEntities())
            return _fileName;
        } // end public void CreateFile(string fiscalYear, string fiscalPeriod, string transDescription, string orgRefId, string transDocNumberSequence)
    }
}