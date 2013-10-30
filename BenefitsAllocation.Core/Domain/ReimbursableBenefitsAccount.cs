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
    public class ReimbursableBenefitsAccount : DomainObjectWithTypedId<string>
    {
        private ReimbursableBenefitsAccountId _id = new ReimbursableBenefitsAccountId();
        public new virtual ReimbursableBenefitsAccountId Id 
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _orgId;
        [Display(Name = "Org ID")]
        [Required]
        public virtual string OrgId {
            get
            {
                return _orgId;
            }
            set
            {
                _orgId = value;
                _id.OrgId = _orgId; 
            }
        }

        private string _chart;
        [Display(Name = "Chart")]
        [Required]
        public virtual string Chart
        {
            get { return _chart; }
            set
            {
                _chart = value;
                _id.Chart = _chart;
            }
        }

        private string _account;
        [Display(Name = "Account")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter an Account Number.")]
        [StringLength(7, ErrorMessage = "{0} must be seven (7) characters long.", MinimumLength = 7)]
        public virtual string Account
        {
            get
            {
                return _account;
            }
            set 
            { 
                _account = value;
                _id.Account = _account;
            }
        }

        [Display(Name = "Is Active")]
        [Required]
        public virtual bool IsActive { get; set; }

        //public override bool Equals(Object obj)
        //{
        //    return this.Id.Equals(obj);
        //}

        //public override int GetHashCode()
        //{
        //    return this.Id.GetHashCode();
        //}
    }

    public class ReimbursableBenefitsAccountId
    {
        public virtual string OrgId { get; set; }
        public virtual string Chart { get; set; }
        public virtual string Account { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            var t = obj as ReimbursableBenefitsAccountId;
            if (t == null)
                return false;
            if (OrgId == t.OrgId && Chart == t.Chart && Account == t.Account)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return (OrgId + "|" + Chart + "|" + Account).GetHashCode();
        }

        public override string ToString()
        {
            return (OrgId + "|" + Chart + "|" + Account);
        } 
    }

    public class ReimbursableBenefitsAccountMap : ClassMap<ReimbursableBenefitsAccount>
    {
        public ReimbursableBenefitsAccountMap()
        {
            Table("ReimbursableBenefitsAccounts");
            CompositeId(x => x.Id)
                .KeyProperty(x => x.OrgId, "OrgID")
                .KeyProperty(x => x.Chart, "Chart")
                .KeyProperty(x => x.Account, "Account");

            Map(x => x.OrgId)
                .Not.Update()
                .Not.Insert();
            Map(x => x.Chart)
                .Not.Update()
                .Not.Insert();
            Map(x => x.Account)
                .Not.Update()
                .Not.Insert();
            Map(x => x.IsActive);
        }
    }
}
