using System;
using System.ComponentModel.DataAnnotations;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace BenefitsAllocation.Core.Domain
{
    public class ExpiredAccount : DomainObjectWithTypedId<String>
    {
        [Display(Name = "Chart")]
        public virtual string ChartNum { get; set; }

        [Display(Name = "Account")]
        public virtual string AccountNum { get; set; }

        [Display(Name = "Expired")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public virtual DateTime ExpirationDate { get; set; }

        [Display(Name = "Amount")]
        public virtual decimal Amount { get; set; }

        [Display(Name = "Org 3")]
        public virtual string Level3OrgCode { get; set; }

        [Display(Name = "Mgr ID")]
        public virtual string MgrId { get; set; }

        [Display(Name = "Mgr Name")]
        public virtual string MgrName { get; set; }
    }

    public class ExpiredAccountMap : ClassMap<ExpiredAccount>
    {
        public ExpiredAccountMap()
        {
            Table("ExpiredAccountsV");

            Id(x => x.Id, "AccountNum")
                .UnsavedValue(0)
                .GeneratedBy.Assigned();
            Map(x => x.ChartNum);
            Map(x => x.AccountNum);
            Map(x => x.ExpirationDate);
            Map(x => x.Amount);
            Map(x => x.Level3OrgCode, "Level3_OrgCode");
            Map(x => x.MgrId);
            Map(x => x.MgrName);
        }
    }
}
