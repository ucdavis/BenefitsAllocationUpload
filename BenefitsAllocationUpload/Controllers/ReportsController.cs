using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Services.Description;
using BenefitsAllocationUpload.Models;
using BenefitsAllocationUpload.Services;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;
using UCDArch.Web.ActionResults;
using UnitFile = BenefitsAllocation.Core.Domain.UnitFile;

namespace BenefitsAllocationUpload.Controllers
{
    //[Authorize]
    [Authorize(Roles = RoleNames.User)]
    public class ReportsController : ApplicationController
    {
        DataClasses objData;
        IDataExtractionService _dataExtractionService;
        private ISftpService _sftpService;
        private readonly IRepository<UnitFile> _unitFileRepository;
 
        public ReportsController(IRepository<UnitFile> unitFileRepository)
        {
            objData = new DataClasses();
            _dataExtractionService = new DataExtractionService();
            _sftpService = new SftpService();
            _unitFileRepository = unitFileRepository;
        }
 
        //
        // GET: /Reports/
        public ActionResult Index()
        {
            var files = objData.GetFiles();
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
            var userFiles = _unitFileRepository.GetAll();
            ViewBag.Message = "Detailed file information as recorded in database";
            return View(userFiles);
        }
       
        public FileResult Download(string id)
        {
            int fid = Convert.ToInt32(id);
            var files = objData.GetFiles();
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

        public ActionResult Upload(string id)
        {
            int fid = Convert.ToInt32(id);
            var files = objData.GetFiles();
            string filename = (from f in files
                               where f.FileId == fid
                               select f.FileName).First();

            _sftpService.UploadFile(filename);

            var unitFile = _unitFileRepository.Queryable.FirstOrDefault(x => x.Filename == filename);

            if (unitFile == null)
            {
                var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
                var unit = user.Units.FirstOrDefault();
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
            var deleteSuccess = false; 

            int fid = Convert.ToInt32(id);
            var files = objData.GetFiles();
            string fullPath = (from f in files
                               where f.FileId == fid
                               select f.FilePath).First();

            var file = new FileInfo(fullPath);

            if (file.Exists)
            {
                file.Delete();
                deleteSuccess = true;
            } 

            Message = "File \"" + file.Name + "\" has been deleted.";
            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            var model = new CreateModel();
            ViewBag.Message = "Create a New Upload File";
            return View(model);
        }

        [HttpPost]
        //public ActionResult Create(string fiscalYear, string fiscalPeriod, string transDescription, string orgRefId, string transDocNumberSequence)
        public ActionResult Create(CreateModel m)
        {
            if (ModelState.IsValid)
            {
                var filename = _dataExtractionService.CreateFile(m.FiscalYear, m.FiscalPeriod, m.TransDescription, m.OrgDocNumber, m.OrgRefId, m.TransDocNumberSequence);
                var user = BenefitsAllocation.Core.Domain.User.GetByLoginId(Repository, User.Identity.Name);
                var unit = user.Units.FirstOrDefault();
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
    }
}

