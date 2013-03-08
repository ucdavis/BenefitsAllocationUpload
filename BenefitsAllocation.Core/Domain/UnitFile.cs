using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCDArch.Core.DomainModel;
using FluentNHibernate.Mapping;

namespace BenefitsAllocation.Core.Domain
{
    public class UnitFile : DomainObject
    {
        [Display(Name = "Unit ID")]
        public virtual int UnitId { get; set; }

        [Display(Name = "File Name")]
        public virtual string Filename { get; set; }

        [Display(Name = "School Code")]
        public virtual string SchoolCode { get; set; }

        [Display(Name = "Created")]
        public virtual DateTime? Created { get; set; }

        [Display(Name = "Created By")]
        public virtual string CreatedBy { get; set; }

         [Display(Name = "Uploaded")]
        public virtual DateTime? Uploaded { get; set; }

        [Display(Name = "Uploaded By")]
        public virtual string UploadedBy { get; set; }
    }

    public class UnitFileMap : ClassMap<UnitFile>
    {
        public UnitFileMap()
        {
            Table("UnitFiles");

            Id(x => x.Id, "Id")
                .UnsavedValue(0)
                .GeneratedBy.Increment();
            Map(x => x.UnitId);
            Map(x => x.Filename);
            Map(x => x.SchoolCode);
            Map(x => x.Created);
            Map(x => x.CreatedBy);
            Map(x => x.Uploaded);
            Map(x => x.UploadedBy);
        }
    }
}
