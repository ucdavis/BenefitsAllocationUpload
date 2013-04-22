using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace BenefitsAllocationUpload.Models
{

    public class UnitFileModel
    {
        public virtual int UnitId { get; set; }

        public virtual string Filename { get; set; }

        public virtual string SchoolCode { get; set; }

        public virtual string Created { get; set; }

        public virtual string CreatedBy { get; set; }

        public virtual string Uploaded { get; set; }

        public virtual string UploadedBy { get; set; }

        public UnitFileModel(BenefitsAllocation.Core.Domain.UnitFile unitFile)
        {
            Created = (unitFile.Created != null ? unitFile.Created.Value.ToString("M/d/yyyy hh:mm:ss tt") : String.Empty);
            CreatedBy = unitFile.CreatedBy;
            Filename = unitFile.Filename;
            SchoolCode = unitFile.SchoolCode;
            UnitId = unitFile.Id;
            Uploaded = (unitFile.Uploaded != null ? unitFile.Uploaded.Value.ToString("M/d/yyyy hh:mm:ss tt") : String.Empty);
            UploadedBy = unitFile.UploadedBy;
        }

        public UnitFileModel()
        {
        }
    }

    public class CreateModel
    {

        public CreateModel()
        {
            // Set default to source our own FISDataMart:
            UseDaFIS = TrueFalse.False;

            // set the default period:
            var month = DateTime.Now.Month;

            _fiscalPeriod = Convert.ToString(month > 6 ? month - 6 : month + 6);
            _fiscalPeriod = _fiscalPeriod.Length == 1 ? "0" + _fiscalPeriod : _fiscalPeriod;

            var year = DateTime.Now.Year;
            FiscalYear = Convert.ToString(month > 6 ? year + 1 : year);
        }

        public bool EnableUseDaFisSelection { get; set; }

        public enum TrueFalse { False, True }

        [Display(Name = "Get Data from DaFIS?")]
        public TrueFalse UseDaFIS { get; set; }

        private string _fiscalPeriod;

        [Required]
        [Display(Name = "Fiscal Period")]
        //[StringLength(2, ErrorMessage = "The {0} must be (2) characters long.", MinimumLength = 2)]
        public string FiscalPeriod
        {
            get { return _fiscalPeriod; }
            set { _fiscalPeriod = value; }
        }

        public IList<string> FiscalPeriods
        {
            get
            {
                var retval = new List<string>()
                    {
                        "01",
                        "02",
                        "03",
                        "04",
                        "05",
                        "06",
                        "07",
                        "08",
                        "09",
                        "10",
                        "11",
                        "12",
                        "13"
                    };
                return retval;
            }
        }

        [Required, Display(Name = "Fiscal Year")]
        public string FiscalYear { get; set; }

        public string CurrentFiscalPeriod
        {
            get
            {
                var month = DateTime.Now.Month;
                var retval = (month > 6 ? month - 6 : month + 6);

                return retval.ToString();
            }
        }

        public IList<string> FiscalYears
        {
            get
            {
                var currentYear = DateTime.Now.Year;
                var priorYear = currentYear - 1;

                var retval = new List<string>()
                    {
                        Convert.ToString(currentYear),
                        Convert.ToString(currentYear + 1)
                    };
                return retval;
            }
        }

        [Required]
        [Display(Name = "Transaction Description")]
        [StringLength(40, ErrorMessage = "The {0} should be at least (10) and no longer that (40) characters.", MinimumLength = 10)]
        public string TransDescription { get; set; }

        [Display(Name = "Organization Document Number (Optional)")]
        [StringLength(10, ErrorMessage = "The {0} can be no longer than (10) characters.", MinimumLength = 0)]
        public string OrgDocNumber { get; set; }

        [Required]
        [Display(Name = "Organization Reference ID")]
        [StringLength(8, ErrorMessage = "The {0} should be at least (5) and no longer than (8) characters.", MinimumLength = 5)]
        public string OrgRefId { get; set; }

        [Required]
        [Display(Name = "Transaction Document Number Sequence No")]
        //[StringLength(3, ErrorMessage = "The {0} must be (3) characters long.", MinimumLength = 3)]
        public string TransDocNumberSequence { get; set; }

        public IList<string> TransDocNumberSequences
        {
            get
            {
                var retval = new List<string>()
                    {
                        "001",
                        "002",
                        "003",
                        "004",
                        "005",
                        "006",
                        "007",
                        "008",
                        "009",
                        "010",
                        "011",
                        "012",
                        "013",
                        "014",
                        "015",
                        "016",
                        "017",
                        "018",
                        "019",
                        "020",
                    };
                return retval;
            }
        }
    }
}