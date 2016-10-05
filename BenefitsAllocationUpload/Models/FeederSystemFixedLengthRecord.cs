using FileHelpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentToFormCreation.Models
{
    /// <summary>
    /// A class containing the fields and formatting required for writing a fixed-length row, i.e. record, to be written to a KFS scrubber document. 
    /// The private fields are necessary for the FileHelpers class to parse or create the fixed-length file.  The corresponding public properties are 
    /// used by @Html extensions for determining the label names, etc.  
    /// </summary>
    [FixedLengthRecord]
    public class FeederSystemFixedLengthRecord
    {
        /// <summary>
        /// Fiscal year nnnn - fiscal year 2003-04 would be 2004
        /// </summary>
        [FieldFixedLength(4)]
        private int _fiscalYear;

        [Display(Name = "Year")]
        public int FiscalYear { get { return _fiscalYear; } }

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

        [Display(Name = "Chart")]
        public string ChartNum { get { return _chartNum; } }

        /// <summary>
        /// The KFS account number, i.e. "ASTDONR"
        /// </summary>
        [FieldFixedLength(7)]
        private string _account;

        [Display(Name = "Account")]
        public string Account { get { return _account; } }

        /// <summary>
        ///  The KFS sub-account number
        ///  (if blank, then has to be '-----').
        /// </summary>
        [FieldFixedLength(5)]
        [FieldTrim(TrimMode.Right)]
        private string _subAccount;

        [Display(Name = "Sub-account")]
        public string SubAccount { get { return _subAccount; } }

        /// <summary>
        /// The KFS financial object number, i.e. "SUB3", etc.
        /// </summary>
        [FieldFixedLength(4)]
        private string _objectCode;

        [Display(Name = "Object Code")]
        public string ObjectCode { get { return _objectCode; } }

        /// <summary>
        /// The KFS financial sub-object code
        /// (if blank, then has to be '---').
        /// </summary>
        [FieldFixedLength(3)]
        [FieldTrim(TrimMode.Right)]
        private string _subObjectCode;

        [Display(Name = "Sub-object Code")]
        public string SubObjectCode { get { return _subObjectCode; } }

        /// <summary>
        /// The KFS balance type code, typically "CB" for current budget used with "GLCB" or "AC" for actuals used with GLJV.
        /// Designates the type of transactions summarized in the balance and transaction total amount fields ("AC" (actual), "CB" (budget), encumbrance).
        /// </summary>
        
        [FieldFixedLength(2)]
        private string _balanceType;

        [Display(Name = "Balance Type")]
        public string BalanceType { get { return _balanceType; } }

        /// <summary>
        /// The KFS financial Object type code.
        /// Identifies the type of object for which transactions are being summarized as an asset, liability, expenditure, fund balance
        /// Note that this field is left blank and populated automatically by AFS load process.
        /// </summary>
        [FieldFixedLength(2)]
        private string _objectType;

        [Display(Name = "Object Type")]
        public string ObjectType { get { return _objectType; } }

        /// <summary>
        /// KFS Fiscal Period
        /// July=01... December=06... June=12 
        /// </summary>
        [FieldFixedLength(2)]
        private string _fiscalPeriod;

        [Display(Name = "Period")]
        public string FiscalPeriod { get { return _fiscalPeriod; } }

        /// <summary>
        /// KFS Document Type "Number", i.e. Code.
        /// Identifies a Financial Information System (FIS) document type (i.e. Expense transfer, "GLCB" (budget adjustment), "GLJV" (journal vouchers), purchase requisitions, purchase orders, etc.).
        /// </summary>
        [FieldFixedLength(4)]
        private string _documentType;

        [Display(Name = "Doc Type")]
        public string DocumentType { get { return _documentType; } }

        /// <summary>
        /// KFS Document Origin Code
        /// An identifier used to determine the source of a transaction. In TP, it is the server id, for the feeder systems, it is the service unit.
        /// 2 position alpha numeric code assigned by A&FS Operations.
        /// Typically "LB" (Lockbox), "AS" (Advancement Services), "CY" (CyberSource).
        /// </summary>
        [FieldFixedLength(2)]
        private string _originCode;

        [Display(Name = "Doc Origin Code")]
        public string OriginCode { get { return _originCode; } }

        /// <summary>
        /// KFS Document Number
        /// System (either TP or service unit feeder) assigned unique FIS document number. 
        /// Must be unique across time - suggest fpxxxxxfy where fp is fiscal period 01-12 and fy is fiscal year ie 2004, xxxxx is alphanumeric value, no spaces
        /// </summary>
        [FieldFixedLength(9)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _documentNumber;

        [Display(Name = "Doc Number")]
        public string DocumentNumber { get { return _documentNumber; } }

        /// <summary>
        /// Transaction Line Entry Sequence Number
        /// A unique identifier for each detail entry for a given document number
        /// Sequential number starting at 00001 for each document number
        /// </summary>
        [FieldFixedLength(5)]
        [FieldAlign(AlignMode.Right)]
        [FieldTrim(TrimMode.Left)] 
        private string _lineSequenceNumber;

        [Display(Name = "Line Number")]
        public string LineSequenceNumber { get { return _lineSequenceNumber; } }

        /// <summary>
        /// Transaction Line Description
        /// The ledger description on a transaction
        /// Must be descriptive - cannot be N/A (see http://accounting.ucdavis.edu/rechargesolution.doc)
        /// </summary>
        [FieldFixedLength(40)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _transactionDescription;

        [Display(Name = "Description")]
        public string TransactionDescription {get { return _transactionDescription; } }

        /// <summary>
        /// Transaction Line Amount
        /// The Dollar Amount on a transaction , a signed field. (This field has had the appropriate sign set, when the balance type indicates that offsets are generated, based on a comparison of the debit/credit code in the original GL transaction to the normal debit/credit code indicated for the object type - if the two codes are not the same, the sign is reversed from what is carried in the actual transaction in the general ledger)
        /// Unsigned for GLJV/AC entries as DebitCreditCode is used, signed for GLCB/CB entries as DebitCreditCode is blank
        /// 99999999999.99 leading zeros optional
        /// </summary>
        [FieldFixedLength(14)]
        [FieldAlign(AlignMode.Right)]
        private string _amount;

        [Display(Name = "Amount")]
        public string Amount {get { return _amount; } }

        /// <summary>
        /// Transaction Debit/Credit Code.
        /// "D" if debit, "C" if credit (negative)
        /// Populated only for GLJV/AC transactions, blank otherwise, i.e., GLCB/CB transactions.
        /// </summary>
        [FieldFixedLength(1)]
        private string _debitCreditCode;

        [Display(Name = "Debit/Credit Code")]
        public string DebitCreditCode {get { return _debitCreditCode; } }

        /// <summary>
        /// Transaction (Initiation) Date
        /// The ledger date on a transaction, i.e. not the actual date posted to the general ledger.
        /// format: yyyymmdd
        /// </summary>
        [FieldFixedLength(8)]
        [FieldConverter(ConverterKind.Date, "yyyyMMdd")]
        private DateTime _transactionDate;

        [Display(Name = "Trans Date")]
        public DateTime TransactionDate { get { return _transactionDate; } }

        /// <summary>
        /// Organization Document (Tracking) Number, i.e., KFS Key
        /// An optional organization internal tracking number provided by the initiator for the whole document: a cross reference.
        /// blanks or data meaningful to you
        /// </summary>
        [FieldFixedLength(10)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _organizationTrackingNumber;

        [Display(Name = "Tracking Number(KFS Key)")]
        public string OrganizationTrackingNumber { get { return _organizationTrackingNumber; } }

        /// <summary>
        /// KFS Transaction Line Project Number/Code.
        /// KFS project code or 10 dashes
        /// Not used by the GivingService process, so we'll ALWAYS be providing 10 dashes , i.e., "----------" as the project code.
        /// </summary>
        [FieldFixedLength(10)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _projectCode;

        [Display(Name = "Project Code")]
        public string ProjectCode { get { return _projectCode; } }

        /// <summary>
        /// Transaction Line Organization Reference Number/Id
        /// An optional organization internal tracking number provided by the initiator for an individual entry in the transaction: a cross reference.
        /// blanks or data meaningful to you
        /// </summary>
        [FieldFixedLength(8)]
        [FieldAlign(AlignMode.Left)]
        [FieldTrim(TrimMode.Right)]
        private string _organizationReferenceId;

        [Display(Name = "Org Ref ID")]
        public string OrganizationReferenceId { get { return _organizationReferenceId; } }

        /// <summary>
        /// Transaction Line Prior Document Type Number/Code
        /// The Document Type used to identify a related document; a cross reference.
        /// always blank
        /// </summary>
        [FieldFixedLength(4)]
        private string _referenceTypeCode;

        [Display(Name = "Ref Doc Type Code")]
        public string ReferenceTypeCode { get { return _referenceTypeCode; } }

        /// <summary>
        /// Transaction Line Prior Document Origin Code
        /// The Document Origin Code used to identify a related document; a cross reference.
        /// always blank
        /// </summary>
        [FieldFixedLength(2)]
        private string _referenceOriginCode;

        [Display(Name = "Ref Doc Origin Code")]
        public string ReferenceOriginCode { get { return _referenceOriginCode; } }

        /// <summary>
        /// Transaction Line Prior Document Number
        /// The Document Number used to identify a related document; a cross reference.
        /// always blank
        /// </summary>
        [FieldFixedLength(9)]
        private string _referenceNumber;

        [Display(Name = "Ref Doc Number")]
        public string ReferenceNumber { get { return _referenceNumber; } }

        /// <summary>
        /// Transaction Reversal Date
        /// Used on selected documents to indicate a reversal date. On the Accrual Voucher or Journal Voucher, it identifies the date that the accounting entry will be automatically reversed.
        /// always blank
        /// </summary>
        [FieldFixedLength(8)]
        private string _reversalDate;

        [Display(Name = "Reversal Date (always blank)")]
        public string ReversalDate {get { return _reversalDate; } }

        /// <summary>
        /// Transaction Encumbrance Update Code
        /// An indicator for the GLE to designate how and when to update open encumbrances
        /// always blank
        /// </summary>
        [FieldFixedLength(1)]
        private string _transactionEncumbranceUpdateCode;

        [Display(Name = "Encumb Update Code (always blank)")]
        public string TransactionEncumbranceUpdateCode { get { return _transactionEncumbranceUpdateCode; } }

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
            _fiscalYear = record.FiscalYear;
            _chartNum = record.ChartNum;
            _account = record.Account;
            _subAccount = record.SubAccount;
            _objectCode = record.ObjectCode;
            _subObjectCode = record.SubObjectCode;
            _balanceType = record.BalanceType;
            _objectType = record.ObjectType;
            _fiscalPeriod = record.FiscalPeriod;
            _documentType = record.DocumentType;
            _originCode = record.OriginCode;
            _documentNumber = record.DocumentNumber;
            _lineSequenceNumber = record.LineSequenceNumber;
            _transactionDescription = record.TransactionDescription;
            _amount = record.Amount;
            _debitCreditCode = record.DebitCreditCode;
            _transactionDate = record.TransactionDate;
            _organizationTrackingNumber = record.OrganizationTrackingNumber;
            _projectCode = record.ProjectCode;
            _organizationReferenceId = record.OrganizationReferenceId;
            _referenceTypeCode = record.ReferenceTypeCode;
            _referenceOriginCode = record.ReferenceOriginCode;
            _referenceNumber = record.ReferenceNumber;
            _reversalDate = record.ReversalDate;
            _transactionEncumbranceUpdateCode = record.TransactionEncumbranceUpdateCode;
        }
    }
}