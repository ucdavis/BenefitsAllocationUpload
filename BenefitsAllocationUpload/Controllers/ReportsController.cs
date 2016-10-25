﻿using BenefitsAllocationUpload.Models;
using BenefitsAllocationUpload.Services;
using FileHelpers;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Web.ActionResults;
using UnitFile = BenefitsAllocation.Core.Domain.UnitFile;


namespace BenefitsAllocationUpload.Controllers
{
    //[Authorize]
    [Authorize(Roles = RoleNames.User)]
    public class ReportsController : ApplicationController
    {
        readonly DataClasses _objData;
        readonly IDataExtractionService _dataExtractionService;
        private readonly ISftpService _sftpService;
        private readonly IRepository<UnitFile> _unitFileRepository;
    
        public ReportsController(IRepository<UnitFile> unitFileRepository, ISftpService sftpService, IDataExtractionService dataExtractionService)
        {
            _objData = new DataClasses();
            _dataExtractionService = dataExtractionService;
            _sftpService = sftpService;
            _unitFileRepository = unitFileRepository;
        }
 
        //
        // GET: /Reports/
        public ActionResult Index()
        {
            //var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
            var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
            var unit = user.Units.FirstOrDefault();
            var schoolCode = unit.DeansOfficeSchoolCode;
            var files = _objData.GetFiles(schoolCode).OrderByDescending(f => f.TimeStamp);
            ViewBag.Message = "Benefits Allocation Upload";
            return View(files);
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonNetResult Get(string id)
        {
            var retval = new UnitFileModel();

            if (!string.IsNullOrEmpty(id))
            {
                var unitFile = _unitFileRepository.Queryable.FirstOrDefault(u => u.Filename.Equals(id));
                if (unitFile != null)
                {
                    retval = new UnitFileModel(unitFile);
                }
            }

            return new JsonNetResult(retval);
        }

        public ActionResult Details()
        {
            //var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
            var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
            var unit = user.Units.FirstOrDefault();
            var schoolCode = unit.DeansOfficeSchoolCode;

            var userFiles = _unitFileRepository.GetAll().Where(file => file.SchoolCode.Equals("00") || file.SchoolCode.Equals(schoolCode)).ToList();
            ViewBag.Message = "Detailed file information as recorded in database";
            return View(userFiles);
        }
       
        public FileResult Download(string id)
        {
            int fid = Convert.ToInt32(id);
            var files = _objData.GetFiles();
            string filename = (from f in files
                               where f.FileId == fid
                               select f.FileName).First();

            string filePathAndFilename = (from f in files
                               where f.FileId == fid
                               select f.FilePath).First();
            const string contentType = "application/text";
            //Parameters to file are
            //1. The File Path on the File Server
            //2. The content type MIME type
            //3. The parameter for the file save by the browser
            return File(filePathAndFilename, contentType, filename);
        }

        public ActionResult DownloadAsExcel(string id)
        {
            int fid = Convert.ToInt32(id);
            var files = _objData.GetFiles();
            var file = (files.Where(f => f.FileId == fid)).First() ;

            var created = file.TimeStamp;
            var filenameWithExtension = file.FileName;
            var filePathAndFilename = file.FilePath;
            var filename = filenameWithExtension.Substring(0, filenameWithExtension.LastIndexOf('.'));

            var streamReader = new StreamReader(filePathAndFilename);

            var engine = new FileHelperEngine<FeederSystemFixedLengthRecord>();

            var result = engine.ReadStream(streamReader);

            var transactions = result.ToList();

            try
            {
                // Opening the Excel template...
                var templateFileStream = new FileStream(Server.MapPath(@"\Files\RevisedScrubberWithoutData.xlsx"),
                    FileMode.Open, FileAccess.Read);

                // Getting the complete workbook...
                var templateWorkbook = new XSSFWorkbook(templateFileStream);

                // Getting the worksheet by its name...
                var sheet = templateWorkbook.GetSheet("Sheet1");

                // We need this so the date will be formatted correctly; otherwise, the date format gets all messed up.
                var dateCellStyle = templateWorkbook.CreateCellStyle();
                var format = templateWorkbook.CreateDataFormat();
                dateCellStyle.DataFormat = format.GetFormat("[$-809]m/d/yyyy;@");

                // Here's another to ensure we get a number with 2 decimal places:
                var twoDecimalPlacesCellStyle = templateWorkbook.CreateCellStyle();
                format = templateWorkbook.CreateDataFormat();
                twoDecimalPlacesCellStyle.DataFormat = format.GetFormat("#0.##");
                var i = 0;
                foreach (var transaction in transactions)
                {
                    i++;
                    // Getting the row... 0 is the first row.
                    var dataRow = sheet.GetRow(i);
                    dataRow.CreateCell(0).SetCellValue(transaction.FiscalYear);
                    dataRow.CreateCell(1).SetCellValue(transaction.ChartNum);
                    dataRow.CreateCell(2).SetCellValue(transaction.Account);
                    dataRow.CreateCell(3).SetCellValue(transaction.SubAccount);
                    dataRow.CreateCell(4).SetCellValue(transaction.ObjectCode);
                    dataRow.CreateCell(5).SetCellValue(transaction.SubObjectCode);
                    dataRow.CreateCell(6).SetCellValue(transaction.BalanceType);
                    dataRow.CreateCell(7).SetCellValue(transaction.ObjectType.Trim());
                    dataRow.CreateCell(8).SetCellValue(transaction.FiscalPeriod);
                    dataRow.CreateCell(9).SetCellValue(transaction.DocumentType);
                    dataRow.CreateCell(10).SetCellValue(transaction.OriginCode);
                    dataRow.CreateCell(11).SetCellValue(transaction.DocumentNumber);
                    dataRow.CreateCell(12).SetCellValue(transaction.LineSequenceNumber);
                    dataRow.CreateCell(13).SetCellValue(transaction.TransactionDescription);

                    var cell = dataRow.CreateCell(14);
                    cell.CellStyle = twoDecimalPlacesCellStyle;  
                    cell.SetCellValue(Convert.ToDouble(transaction.Amount.Trim()));
                               
                    dataRow.CreateCell(15).SetCellValue(transaction.DebitCreditCode.Trim());

                    cell = dataRow.CreateCell(16);
                    cell.CellStyle = dateCellStyle;
                    cell.SetCellValue(transaction.TransactionDate);

                    dataRow.CreateCell(17).SetCellValue(transaction.OrganizationTrackingNumber);
                    dataRow.CreateCell(18).SetCellValue(transaction.ProjectCode);
                    dataRow.CreateCell(19).SetCellValue(transaction.OrganizationReferenceId.Trim());
                    dataRow.CreateCell(20).SetCellValue(transaction.ReferenceTypeCode.Trim());
                    dataRow.CreateCell(21).SetCellValue(transaction.ReferenceOriginCode.Trim());
                    dataRow.CreateCell(22).SetCellValue(transaction.ReferenceNumber.Trim());
                    dataRow.CreateCell(23).SetCellValue(transaction.ReversalDate.Trim());
                    dataRow.CreateCell(24).SetCellValue(transaction.TransactionEncumbranceUpdateCode.Trim());
                }
               
                // Forcing formula recalculation...
                sheet.ForceFormulaRecalculation = true;

                var ms = new MemoryStream();

                // Writing the workbook content to the FileStream...
                templateWorkbook.Write(ms);

                TempData["Message"] = "Excel report created successfully!";

                // Sending the server processed data back to the user computer...
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename + ".xlsx");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Opps!  Something went wrong.";

                return RedirectToAction("Index");
            }
        }

        public ActionResult Upload(string id)
        {
            int fid = Convert.ToInt32(id);
            var files = _objData.GetFiles();
            string filename = (from f in files
                               where f.FileId == fid
                               select f.FileName).First();

            //var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
            var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);
            var unit = user.Units.FirstOrDefault();
            var schoolCode = unit.DeansOfficeSchoolCode;

            _sftpService.UploadFile(filename, schoolCode);

            var unitFile = _unitFileRepository.Queryable.FirstOrDefault(x => x.Filename == filename);

            if (unitFile == null)
            {
                //var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
                //var unit = user.Units.FirstOrDefault();
                unitFile = new UnitFile()
                    {
                        Filename = filename,
                        SchoolCode = unit.DeansOfficeSchoolCode,
                        UnitId = unit.UnitID
                    };
            }

            unitFile.Uploaded = DateTime.Now;
            unitFile.UploadedBy = User.Identity.Name;

            _unitFileRepository.EnsurePersistent(unitFile);

            Message = "File \"" + filename + "\" has been uploaded.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string id)
        {
            //var deleteSuccess = false; 

            int fid = Convert.ToInt32(id);
            var files = _objData.GetFiles();
            string fullPath = (from f in files
                               where f.FileId == fid
                               select f.FilePath).First();

            var file = new FileInfo(fullPath);

            if (file.Exists)
            {
                file.Delete();
                //deleteSuccess = true;
            } 

            Message = "File \"" + file.Name + "\" has been deleted.";
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            var model = new CreateModel()
                {
                    UseDaFIS = CreateModel.YesNo.Yes,
                    EnableUseDaFisSelection = false
                };

            //var unit = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name).Units.FirstOrDefault();
            var unit = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name).Units.FirstOrDefault();
            if (unit != null)
            {
                var schoolCode = unit.SchoolCode;
                if (!string.IsNullOrEmpty(schoolCode) && schoolCode.Equals("01"))
                {
                    model.UseDaFIS = CreateModel.YesNo.No;
                    model.EnableUseDaFisSelection = true;
                }
            }

            ViewBag.Message = "Create a New Upload File";
            return View(model);
        }

        [HttpPost]
        //public ActionResult Create(string fiscalYear, string fiscalPeriod, string transDescription, string orgRefId, string transDocNumberSequence)
        public ActionResult Create(CreateModel m)
        {
            if (ModelState.IsValid)
            {
                //var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
                var user = Models.User.FindByLoginId(System.Web.HttpContext.Current.User.Identity.Name);

                var unit = user.Units.FirstOrDefault();
                var orgId = string.Empty;
                var transDocOriginCode = string.Empty;
                using (var context = new FISDataMartEntities())
                {
                    var schoolCodeParameter = new SqlParameter("schoolCode", unit.SchoolCode);
                    orgId = context.Database.SqlQuery<string>(
                    "SELECT dbo.udf_GetOrgIdForSchoolCode(@schoolCode)", schoolCodeParameter).FirstOrDefault();
                    var orgIdParameter = new SqlParameter("orgId", orgId);
                    transDocOriginCode = context.Database.SqlQuery<string>(
                    "SELECT dbo.udf_GetTransDocOriginCodeForOrg(@orgId)", orgIdParameter).FirstOrDefault();
                }
  
                //var filename = _dataExtractionService.CreateFile(m.FiscalYear, m.FiscalPeriod, m.TransDescription, m.OrgDocNumber, m.OrgRefId, m.TransDocNumberSequence);
                var useDaFIS = (m.UseDaFIS == CreateModel.YesNo.Yes);
                var filename = _dataExtractionService.CreateFile(m.FiscalYear, m.FiscalPeriod.Period, m.TransDescription, m.OrgDocNumber, m.OrgRefId, m.TransDocNumberSequence, orgId, transDocOriginCode, useDaFIS);
                //var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
                //var unit = user.Units.FirstOrDefault();
                var unitFile = new UnitFile()
                    {
                        Filename = filename,
                        SchoolCode = unit.DeansOfficeSchoolCode,
                        UnitId = unit.UnitID,
                        Created = DateTime.Now,
                        CreatedBy = User.Identity.Name
                    };

                _unitFileRepository.EnsurePersistent(unitFile);

                Message = "File \"" + filename + "\" has been created.";

                return RedirectToAction("Index");
            }
            return View(m);
        }

        public ActionResult Display(string id)
        {
            int fid = Convert.ToInt32(id);
            var files = _objData.GetFiles();
            string filename = (from f in files
                               where f.FileId == fid
                               select f.FileName).First();

            string filePathAndFilename = (from f in files
                                          where f.FileId == fid
                                          select f.FilePath).First();

            var engine = new FileHelperEngine<FeederSystemFixedLengthRecord>();

            var streamReader = new StreamReader(filePathAndFilename);

            var result = engine.ReadStream(streamReader);

            var transactions = result.ToList();

            TempData["Message"] = "Now viewing \"" + filename + "\".";
            return View(transactions);
        }
    }
}

