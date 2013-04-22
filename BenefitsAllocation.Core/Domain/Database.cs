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
    public class Database : DomainObjectWithTypedId<string>
    {
        [Display(Name = "Database Name")]
        public virtual string DatabaseName { get; set; }

        [Display(Name = "Business Purpose")]
        public virtual string BusinessPurpose { get; set; }

        [Display(Name = "Comments")]
        public virtual string Comments { get; set; }
    }

    //public class DatabaseMap : ClassMap<Database>
    //{
    //    public DatabaseMap()
    //    {
    //        Id(x => x.Id, "DatabaseName")
    //            .UnsavedValue("empty")
    //            .GeneratedBy.Assigned();
    //        Map(x => x.BusinessPurpose);
    //        Map(x => x.Comments);
    //    }
    //}
}
