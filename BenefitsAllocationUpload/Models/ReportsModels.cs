using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
            // Set default to source Campus Data Warehouse:
            UseDaFIS = YesNo.Yes;

            // Set the default fiscal year and period:
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var day = DateTime.Now.Day;
            // Use current year for periods 7 thru 13.
            var fiscalYear = year;
            // Use current month + 6 for periods 7 thru 13:
            var fiscalPeriod = month + 6;
            
            // Use next year and month - 6 for periods 1 thru 6:
            if ((month >= 8) || (month == 7 && day > 15))
            {
                fiscalYear = fiscalYear + 1;
                fiscalPeriod = month - 6;
            }

            // Apply the adjustments to the previous period that has just passed, but not yet closed.
            // Applies to all periods where the day of the month is <= 5
            if (day <= 5)
            {
                fiscalPeriod = fiscalPeriod - 1;
            }

            var fiscalPeriodString = fiscalPeriod.ToString();
            // Add a leading "0" for periods 1-9
            if (fiscalPeriod < 10)
                fiscalPeriodString = "0" + fiscalPeriodString;

            // Set the default fiscal period with the information just determined:
            FiscalPeriod = FiscalPeriods.AsQueryable().FirstOrDefault(p => p.Period.Equals(fiscalPeriodString));
            FiscalYear = fiscalYear.ToString();
        }

        public bool EnableUseDaFisSelection { get; set; }

        public enum TrueFalse { False, True  }

        public enum YesNo { No, Yes }

        [Display(Name = "Get Data from Campus Data Warehouse?")]
        public YesNo UseDaFIS { get; set; }

        //private string _fiscalPeriod;

        [Required, Display(Name = "Fiscal Period")]
        public FiscalPeriod FiscalPeriod { get; set; }

        public IList<FiscalPeriod> FiscalPeriods
        {
            get { 
                var retval = new List<FiscalPeriod>
                    {
                        new FiscalPeriod() {Period = "01", Name = "July (01)"},
                        new FiscalPeriod() {Period = "02", Name = "August (02)"},
                        new FiscalPeriod() {Period = "03", Name = "September (03)"},
                        new FiscalPeriod() {Period = "04", Name = "October (04)"},
                        new FiscalPeriod() {Period = "05", Name = "November (05)"},
                        new FiscalPeriod() {Period = "06", Name = "December (06)"},
                        new FiscalPeriod() {Period = "07", Name = "January (07)"},
                        new FiscalPeriod() {Period = "08", Name = "February (08)"},
                        new FiscalPeriod() {Period = "09", Name = "March (09)"},
                        new FiscalPeriod() {Period = "10", Name = "April (10)"},
                        new FiscalPeriod() {Period = "11", Name = "May (11)"},
                        new FiscalPeriod() {Period = "12", Name = "June (12)"},
                        new FiscalPeriod() {Period = "13", Name = "June Final (13)"},
                    };

                return retval;
            }
        }

        //public IList<String> FiscalPeriods
        //{
        //    get
        //    {
        //        var retval = new List<string>()
        //            {
        //                "01",
        //                "02",
        //                "03",
        //                "04",
        //                "05",
        //                "06",
        //                "07",
        //                "08",
        //                "09",
        //                "10",
        //                "11",
        //                "12",
        //                "13"
        //            };
        //        return retval;
        //    }
        //}

        [Required, Display(Name = "Fiscal Year")]
        public string FiscalYear { get; set; }

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
        [StringLength(3, ErrorMessage = "The {0} must be (3) characters long.", MinimumLength = 0)]
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