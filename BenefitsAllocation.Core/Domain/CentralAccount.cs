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
    public class CentralAccount : DomainObjectWithTypedId<string>
    {
        [Display(Name = "Org Id")]
        public virtual string OrgId { get; set; }

        [Display(Name = "School Code")]
        public virtual string SchoolCode { get; set; }

        [Display(Name = "Chart")]
        public virtual string Chart { get; set; }

        [Display(Name = "Account")]
        public virtual string Account { get; set; }

        [Display(Name = "Sub Account")]
        public virtual string SubAccount { get; set; }

        [Display(Name = "Object Consolidation")]
        public virtual string ObjectConsolidation { get; set; }

        [Display(Name = "Funding Object Consolidation")]
        public virtual string FundingObjectConsolidation { get; set; }

        [Display(Name = "Sub Object")]
        public virtual string SubObject { get; set; }

        [Display(Name = "Function Code")]
        public virtual string FunctionCode { get; set; }

        [Display(Name = "Trans Doc Origin Code")]
        public virtual string TransDocOriginCode { get; set; }

        [Display(Name = "Op Fund")]
        public virtual string OpFund { get; set; }
    }

    public class CentralAccountMap : ClassMap<CentralAccount>
    {
        public CentralAccountMap()
        {
            Table("CentralAccounts");

            Id(x => x.Id, "PK_CentralAccounts")
                .GeneratedBy.Assigned();
            Map(x => x.OrgId);
            Map(x => x.SchoolCode);
            Map(x => x.Chart);
            Map(x => x.Account);
            Map(x => x.SubAccount);
            Map(x => x.ObjectConsolidation);
            Map(x => x.FundingObjectConsolidation);
            Map(x => x.SubObject);
            Map(x => x.FunctionCode);
            Map(x => x.TransDocOriginCode);
            Map(x => x.OpFund);
        }
    }
}
