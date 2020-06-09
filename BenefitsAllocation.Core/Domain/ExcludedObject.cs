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
    public class ExcludedObject : DomainObjectWithTypedId<string>
    {
        private ExcludedObjectId _id = new ExcludedObjectId();
        public new virtual ExcludedObjectId Id
        {
            get
            {
                return _id;
            }

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

        private string _objectNum;
        [Display(Name = "Object Num")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter an Object Num.")]
        [StringLength(4, ErrorMessage = "{0} must be four (4) characters long.", MinimumLength = 4)]
        public virtual string ObjectNum
        {
            get
            {
                return _objectNum;
            }
            set 
            {
                _objectNum = value;
                _id.ObjectNum = _objectNum;
            }
        }

        [Display(Name = "Is Excluded?")]
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

    public class ExcludedObjectId
    {
        public virtual string OrgId { get; set; }
       
        public virtual string ObjectNum { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            var t = obj as ExcludedObjectId;
            if (t == null)
                return false;
            if (OrgId == t.OrgId && ObjectNum == t.ObjectNum)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return (OrgId + "|" + ObjectNum).GetHashCode();
        }

        public override string ToString()
        {
            return (OrgId + "|" + ObjectNum);
        } 
    }

    public class ExcludedObjectMap : ClassMap<ExcludedObject>
    {
        public ExcludedObjectMap()
        {
            Table("ExcludedObjects");
            CompositeId(x => x.Id)
                .KeyProperty(x => x.OrgId, "OrgID")
                .KeyProperty(x => x.ObjectNum, "ObjectNum");

            Map(x => x.OrgId)
                .Not.Update()
                .Not.Insert();
            Map(x => x.ObjectNum)
                .Not.Update()
                .Not.Insert();
            Map(x => x.IsActive);
        }
    }
}
