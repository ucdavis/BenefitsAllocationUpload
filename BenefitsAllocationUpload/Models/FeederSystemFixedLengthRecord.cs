using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq.Mapping;
using System.Globalization;
using FileHelpers;

namespace BenefitsAllocationUpload.Models
{
    /// <summary>
    /// A class containing the fields and formatting required for writing a fixed-length row, i.e. record, to be written to a KFS scrubber document. 
    /// The private fields are necessary for the FileHelpers class to parse or create the fixed-length file.  The corresponding public properties are 
    /// used by @Html extensions for determining the label names, etc.  
    /// </summary>
    [FixedLengthRecord]
    public class FeederSystemFixedLengthRecord
    {
        public const string Dashes = "----------";

        /// <summary>
        /// Fiscal year nnnn - fiscal year 2003-04 would be 2004
        /// </summary>
        [FieldFixedLength(4)]
        private int _fiscalYear;
        [Column(Name = "UNIV_FISCAL_YEAR")]
        [Display(Name = "Year")]
        public int FiscalYear { get { return _fiscalYear; } set { _fiscalYear = value; } }

        /// <summary>
        /// FIS Chart of Accounts:
        /// 3 - Davis campus - not hospital, not plant
        /// L - DANR - not plant
        /// P - Davis plant; N - DANR Plant; M - Hospital Plant
        /// S - School of Medicine
        /// H – hospital
        /// </summary>
        [FieldFixedLength(2)]
        [FieldAlign(AlignMode.Left)]
        private string _chartNum;

        [Column(Name = "FIN_COA_CD")]
        [Display(Name = "Chart")]
        public string ChartNum { get { return _chartNum; } set { _chartNum = value; } }

        /// <summary>
        /// The KFS account number, i.e. "ASTDONR"
        /// </summary>
        [FieldFixedLength(7)]
        private string _account;

        [Column(Name = "ACCOUNT_NBR")]
        [Display(Name = "Account")]
        public string Account
        {
            get
            {
                if (_account != null)
                {
                    return _account.ToUpper();
                }
                else
                {
                    return null;
                }

            }
            set { _account = value; }
        }

        /// <summary>
        ///  The KFS sub-account number
        ///  (if blank, then has to be '-----').
        /// </summary>
        [FieldConverter(typeof(NoSubAccountValueConverter))]
        [FieldFixedLength(5)]
        [FieldTrim(TrimMode.Right)]
        private string _subAccount;

        [Column(Name = "SUB_ACCT_NBR")]
        [Display(Name = "Sub Acct")]
        public string SubAccount
        {
            get
            {
                if (_subAccount != null)
                {
                    return _subAccount.ToUpper();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _subAccount = value;
            }
        }

        /// <summary>
        /// The KFS financial object number, i.e. "SUB3", etc.
        /// </summary>
        [FieldFixedLength(4)]
        private string _objectCode;

        [Column(Name = "FIN_OBJECT_CD")]
        [Display(Name = "Obj Cons")]
        public string ObjectCode { get { return _objectCode; } set { _objectCode = value; } }

        /// <summary>
        /// The KFS financial sub-object code
        /// (if blank, then has to be '---').
        /// </summary>
        [FieldFixedLength(3)]
        [FieldTrim(TrimMode.Right)]
        [FieldAlign(AlignMode.Right, '-')]
        private string _subObjectCode;

        [Column(Name = "FIN_SUB_OBJ_CD")]
        [Display(Name = "Sub Obj")]
        public string SubObjectCode { get { return _subObjectCode; } set { _subObjectCode = value; } }

        /// <summary>
        /// The KFS balance type code, typically "CB" for current budget used with "GLCB" or "AC" for actuals used with GLJV.
        /// Designates the type of transactions summarized in the balance and transaction total amount fields ("AC" (actual), "CB" (budget), encumbrance).
        /// </summary>
        [FieldFixedLength(2)]
        private string _balanceType;

        [Column(Name = "FIN_BALANCE_TYP_CD")]
        [Display(Name = "Bal Type")]
        public string BalanceType { get { return _balanceType; } set { _balanceType = value; } }

        /// <summary>
        /// The KFS financial Object type code.
        /// Identifies the type of object for which transactions are being summarized as an asset, liability, expenditure, fund balance
        /// Note that this field is left blank and populated automatically by AFS load process.
        /// </summary>
        [FieldFixedLength(2)]
        private string _objectType;

        [Column(Name = "FIN_OBJ_TYP_CD")]
        [Display(Name = "Obj Type")]
        public string ObjectType { get { return _objectType; } set { _objectType = value; } }

        /// <summary>
        /// KFS Fiscal Period
        /// July=01... December=06... June=12 
        /// </summary>
        [FieldAlign(AlignMode.Right, '0')]
        [FieldFixedLength(2)]
        private string _fiscalPeriod;

        [Column(Name = "UNIV_FISCAL_PRD_CD")]
        [Display(Name = "Period")]
        public string FiscalPeriod { get { return _fiscalPeriod; } set { _fiscalPeriod = value; } }

        /// <summary>
        /// KFS Document Type "Number", i.e. Code.
        /// Identifies a Financial Information System (FIS) document type (i.e. Expense transfer, "GLCB" (budget adjustment), "GLJV" (journal vouchers), purchase requisitions, purchase orders, etc.).
        /// </summary>
        [FieldFixedLength(4)]
        private string _documentType;

        [Column(Name = "FDOC_TYP_CD")]
        [Display(Name = "Doc Type")]
        public string DocumentType { get { return _documentType; } set { _documentType = value; } }

        /// <summary>
        /// KFS Document Origin Code
        /// An identifier used to determine the source of a transaction. In TP, it is the server id, for the feeder systems, it is the service unit.
        /// 2 position alpha numeric code assigned by A&FS Operations.
        /// Typically "LB" (Lockbox), "AS" (Advancement Services), "CY" (CyberSource).
        /// </summary>
        [FieldFixedLength(2)]
        private string _originCode;

        [Column(Name = "FS_ORIGIN_CD")]
        [Display(Name = "Origin")]
        public string OriginCode { get { return _originCode; } set { _originCode = value; } }

        /// <summary>
        /// KFS Document Number
        /// System (either TP or service unit feeder) assigned unique FIS document number. 
        /// Must be unique across time - suggest fpxxxxxfy where fp is fiscal period 01-12 and fy is fiscal year ie 2004, xxxxx is alphanumeric value, no spaces
        /// </summary>
        [FieldFixedLength(9)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _documentNumber;

        [Column(Name = "FDOC_NBR")]
        [Display(Name = "Doc No")]
        public string DocumentNumber { get { return _documentNumber; } set { _documentNumber = value; } }

        /// <summary>
        /// Transaction Line Entry Sequence Number
        /// A unique identifier for each detail entry for a given document number
        /// Sequential number starting at 00001 for each document number
        /// </summary>
        [FieldFixedLength(5)]
        [FieldAlign(AlignMode.Right,'0')]
        [FieldTrim(TrimMode.Left)]
        private string _lineSequenceNumber;

        [Column(Name = "TRN_ENTR_SEQ_NBR")]
        [Display(Name = "Seq No")]
        public string LineSequenceNumber { get { return _lineSequenceNumber; } set { _lineSequenceNumber = value; } }

        /// <summary>
        /// Transaction Line Description
        /// The ledger description on a transaction
        /// Must be descriptive - cannot be N/A (see http://accounting.ucdavis.edu/rechargesolution.doc)
        /// </summary>
        [FieldFixedLength(40)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _transactionDescription;

        [Column(Name = "TRN_LDGR_ENTR_DESC")]
        [Display(Name = "Description")]
        public string TransactionDescription { get { return _transactionDescription; } set { _transactionDescription = value; } }

        /// <summary>
        /// Transaction Line Amount
        /// The Dollar Amount on a transaction , a signed field. (This field has had the appropriate sign set, when the balance type indicates that offsets are generated, based on a comparison of the debit/credit code in the original GL transaction to the normal debit/credit code indicated for the object type - if the two codes are not the same, the sign is reversed from what is carried in the actual transaction in the general ledger)
        /// Unsigned for GLJV/AC entries as DebitCreditCode is used, signed for GLCB/CB entries as DebitCreditCode is blank
        /// 99999999999.99 leading zeros optional
        /// </summary>
        [FieldFixedLength(14)]
        [FieldAlign(AlignMode.Right)]
        private string _amount;

        [Column(Name = "TRN_LDGR_ENTR_AMT")]
        [Display(Name = "Amount")]
        public string Amount { get { return _amount; } set { _amount = value; } }

        /// <summary>
        /// Transaction Debit/Credit Code.
        /// "D" if debit, "C" if credit (negative)
        /// Populated only for GLJV/AC transactions, blank otherwise, i.e., GLCB/CB transactions.
        /// </summary>
        [FieldFixedLength(1)]
        private string _debitCreditCode;

        [Column(Name = "TRN_DEBIT_CRDT_CD")]
        [Display(Name = "D/C")]
        public string DebitCreditCode { get { return _debitCreditCode; } set { _debitCreditCode = value; } }

        /// <summary>
        /// Transaction (Initiation) Date
        /// The ledger date on a transaction, i.e. not the actual date posted to the general ledger.
        /// format: yyyymmdd
        /// </summary>
        [FieldFixedLength(8)]
        //[FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        [FieldConverter(typeof(NoDateValueConverter))]
        private DateTime _transactionDate;

        [Column(Name = "TRANSACTION_DT")]
        [Display(Name = "Trans Date")]
        public DateTime TransactionDate
        {
            get { return _transactionDate;}
            set { _transactionDate = value; }
        }

        /// <summary>
        /// Organization Document (Tracking) Number, i.e., KFS Key
        /// An optional organization internal tracking number provided by the initiator for the whole document: a cross reference.
        /// blanks or data meaningful to you
        /// </summary>
        [FieldFixedLength(10)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _organizationTrackingNumber;

        [Column(Name = "ORG_DOC_NBR")]
        [Display(Name = "Org Doc No")]
        public string OrganizationTrackingNumber { get { return _organizationTrackingNumber; } set { _organizationTrackingNumber = value; } }

        /// <summary>
        /// KFS Transaction Line Project Number/Code.
        /// KFS project code or 10 dashes
        /// Not used by the GivingService process, so we'll ALWAYS be providing 10 dashes , i.e., "----------" as the project code.
        /// </summary>
        [FieldFixedLength(10)]
        [FieldConverter(typeof(NoProjectNumberValueConverter))]
        [FieldTrim(TrimMode.Right)]
        private string _projectCode;

        [Column(Name = "PROJECT_CD")]
        [Display(Name = "Project")]
        public string ProjectCode { get { return _projectCode; } set { _projectCode = value; } }

        /// <summary>
        /// Transaction Line Organization Reference Number/Id
        /// An optional organization internal tracking number provided by the initiator for an individual entry in the transaction: a cross reference.
        /// blanks or data meaningful to you
        /// </summary>
        [FieldFixedLength(8)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _organizationReferenceId;

        [Column(Name = "ORG_REFERENCE_ID")]
        [Display(Name = "Org Ref ID")]
        public string OrganizationReferenceId { get { return _organizationReferenceId; } set { _organizationReferenceId = value; } }

        /// <summary>
        /// Transaction Line Prior Document Type Number/Code
        /// The Document Type used to identify a related document; a cross reference.
        /// always blank
        /// </summary>
        [FieldFixedLength(4)]
        private string _referenceTypeCode;

        [Column(Name = "FDOC_REF_TYP_CD")]
        [Display(Name = "Ref Doc Type")]
        public string ReferenceTypeCode { get { return _referenceTypeCode; } set { _referenceTypeCode = value; } }

        /// <summary>
        /// Transaction Line Prior Document Origin Code
        /// The Document Origin Code used to identify a related document; a cross reference.
        /// always blank
        /// </summary>
        [FieldFixedLength(2)]
        private string _referenceOriginCode;

        [Column(Name = "FS_REF_ORIGIN_CD")]
        [Display(Name = "Ref Origin")]
        public string ReferenceOriginCode { get { return _referenceOriginCode; } set { _referenceOriginCode = value; } }

        /// <summary>
        /// Transaction Line Prior Document Number
        /// The Document Number used to identify a related document; a cross reference.
        /// always blank
        /// </summary>
        [FieldFixedLength(9)]
        private string _referenceNumber;

        [Column(Name = "FDOC_REF_NBR")]
        [Display(Name = "Ref Doc No")]
        public string ReferenceNumber { get { return _referenceNumber; } set { _referenceNumber = value; } }

        /// <summary>
        /// Transaction Reversal Date
        /// Used on selected documents to indicate a reversal date. On the Accrual Voucher or Journal Voucher, it identifies the date that the accounting entry will be automatically reversed.
        /// always blank
        /// </summary>
        [FieldFixedLength(8)]
        private string _reversalDate;

        [Column(Name = "FDOC_REVERSAL_DT")]
        [Display(Name = "Reverse Date")]
        public string ReversalDate { get { return _reversalDate; } set { _reversalDate = value; } }

        /// <summary>
        /// Transaction Encumbrance Update Code
        /// An indicator for the GLE to designate how and when to update open encumbrances
        /// always blank
        /// </summary>
        [FieldFixedLength(1)]
        private string _transactionEncumbranceUpdateCode;

        [Column(Name = "TRN_ENCUM_UPDT_CD")]
        [Display(Name = "Enc Update")]
        public string TransactionEncumbranceUpdateCode { get { return _transactionEncumbranceUpdateCode; } set { _transactionEncumbranceUpdateCode = value; } }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FeederSystemFixedLengthRecord()
        {

        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="record"></param>
        public FeederSystemFixedLengthRecord(FeederSystemFixedLengthRecord record)
        {
            FiscalYear = record.FiscalYear;
            ChartNum = record.ChartNum;
            Account = record.Account;
            SubAccount = record.SubAccount;
            ObjectCode = record.ObjectCode;
            SubObjectCode = record.SubObjectCode;
            BalanceType = record.BalanceType;
            ObjectType = record.ObjectType;
            FiscalPeriod = record.FiscalPeriod;
            DocumentType = record.DocumentType;
            OriginCode = record.OriginCode;
            DocumentNumber = record.DocumentNumber;
            LineSequenceNumber = record.LineSequenceNumber;
            TransactionDescription = record.TransactionDescription;
            Amount = record.Amount;
            DebitCreditCode = record.DebitCreditCode;
            TransactionDate = record.TransactionDate;
            OrganizationTrackingNumber = record.OrganizationTrackingNumber;
            ProjectCode = record.ProjectCode;
            OrganizationReferenceId = record.OrganizationReferenceId;
            ReferenceTypeCode = record.ReferenceTypeCode;
            ReferenceOriginCode = record.ReferenceOriginCode;
            ReferenceNumber = record.ReferenceNumber;
            ReversalDate = record.ReversalDate;
            TransactionEncumbranceUpdateCode = record.TransactionEncumbranceUpdateCode;
        }
    }

    /// <summary>
    /// Handles the proper setting of the date value should we
    /// return a null value from the database when creating the initial file.
    /// </summary>
    public class NoDateValueConverter : ConverterBase
    {
        private const string DateTimeFormatString = "{0:yyyyMMdd}";
        
        public override object StringToField(string from)
        {
            return DateTime.ParseExact(from, "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null || (DateTime)fieldValue == DateTime.MinValue)
            {
                return String.Format(DateTimeFormatString, DateTime.Now);
            }

            return String.Format(DateTimeFormatString, fieldValue);
        }
    }

    /// <summary>
    /// Generic method for handling padding of strings that are less that the necessary number of characters.
    /// </summary>
    static class NoStringValueConverter
    {
        public static string StringToField(string from, int fieldLength)
        {
            return from.PadRight(fieldLength, ' ');
        }

        public static string FieldToString(string fieldValue, int fieldLength)
        {
            if (!string.IsNullOrWhiteSpace(fieldValue))
                return fieldValue;

            return FeederSystemFixedLengthRecord.Dashes.Substring(0, fieldLength);
        }
    }

    /// <summary>
    /// Handle formatting project numbers that are either blank or less that 10 characters. 
    /// </summary>
    public class NoProjectNumberValueConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            return NoStringValueConverter.StringToField(from, 10);
        }

        public override string FieldToString(object fieldValue)
        {
            return NoStringValueConverter.FieldToString(fieldValue as string, 10);
        }
    }

    /// <summary>
    /// Handle formatting sub account that are either blank or less that 5 characters.
    /// </summary>
    public class NoSubAccountValueConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            return NoStringValueConverter.StringToField(from, 5);
        }

        public override string FieldToString(object fieldValue)
        {
            return NoStringValueConverter.FieldToString(fieldValue as string, 5);
        }
    }
}
