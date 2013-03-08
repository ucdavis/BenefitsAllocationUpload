using System;
using System.Collections.Generic;
//using BenefitsAllocation.Core.Domain;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace BenefitsAllocation.Core.Domain
{
 public class Login : DomainObjectWithTypedId<string>
    {
     public virtual User User { get; set; }

     public Login()
        {
        }
    }

    public class LoginMap : ClassMap<Login>
    {
        public LoginMap()
        {
            Table("Catbert3_vLogin");

            Id(x => x.Id, "LoginID")
                .UnsavedValue("empty")
                .GeneratedBy.Assigned();

            References(x => x.User, "UserID")
                .Not.Nullable();
        }
    }

    [Serializable]
    public class Unit : DomainObject
    {
        public virtual string ShortName { get; set; }

        public virtual string FullName { get; set; }

        private string _ppsCode;

        public virtual string PPSCode
        {
            get
            {
                if (!String.IsNullOrEmpty(_ppsCode))
                    return _ppsCode.Trim();

                return _ppsCode;
            }
            set { _ppsCode = value; }
        }

        private string _fisCode;

        public virtual string FISCode
        {
            get
            {
                if (!String.IsNullOrEmpty(_fisCode))
                    return _fisCode.Trim();

                return _fisCode;
            }
            set { _fisCode = value; }
        }

        public virtual int UnitID { get; set; }

        public virtual string SchoolCode { get; set; }

        public virtual string DeansOfficeSchoolCode { get; set; }

        //private bool _Inactive;

        //public virtual bool Inactive
        //{
        //    get { return _Inactive; }
        //    set { _Inactive = value; }
        //}

        public virtual IList<User> Users { get; set; }

        public Unit()
        {
            Users = new List<User>();
        }
    }

    public class UnitMap : ClassMap<Unit>
    {
        public UnitMap()
        {
            Table("Catbert3_vUnit");

            Id(x => x.Id, "UnitID")
                .UnsavedValue(0)
                .GeneratedBy.Assigned();

            Map(x => x.UnitID);
            Map(x => x.FullName);
            Map(x => x.ShortName);
            Map(x => x.PPSCode).Column("PPS_Code");
            Map(x => x.FISCode).Column("FIS_Code");
            Map(x => x.SchoolCode);
            Map(x => x.DeansOfficeSchoolCode);

            HasManyToMany(x => x.Users)
               .Table("Catbert3_vUserUnit")
               .AsBag()
               .ParentKeyColumn("UnitID")
               .ChildKeyColumn("UserID");

            ReadOnly();
        }
    }

    [Serializable]
    public class Roles : DomainObject
    {
        public virtual string Role { get; set; }

        public virtual int RoleID { get; set; }

        public virtual bool Inactive { get; set; }

        public Roles()
        {
        }
    }

    public class RolesMap : ClassMap<Roles>
    {
        //public RolesMap()
        //{
        //    Table("Catbert3_vRoles");

        //    Id(x => x.Id, "RoleID")
        //        .UnsavedValue("0")
        //        .GeneratedBy.Assigned();

        //    Map(x => x.Role);
        //}
        public RolesMap()
        {
            Table("Roles");

            Id(x => x.Id, "RoleID")
                .UnsavedValue("0")
                .GeneratedBy.Assigned();

            Map(x => x.Role);
            Map(x => x.Inactive);
        }
    }

    [Serializable]
    public class School : DomainObjectWithTypedId<string>
    {
        public virtual string SchoolCode { get; set; }

        public virtual string ShortDescription { get; set; }

        public virtual string LongDescription { get; set; }

        public virtual string Abbreviation { get; set; }
    }

    public class SchoolMap : ClassMap<School>
    {
        public SchoolMap()
        {
            Table("Schools");

            Id(x => x.Id, "SchoolCode")
                .UnsavedValue("0")
                .GeneratedBy.Assigned();

            Map(x => x.ShortDescription);
            Map(x => x.LongDescription);
            Map(x => x.Abbreviation);
        }
    }
}