using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping;
using NHibernate.Criterion;
using NHibernate.Transform;
using UCDArch.Core.DomainModel;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Core.Utils;
using UCDArch.Data.NHibernate;


namespace BenefitsAllocation.Core.Domain
{
    public class User: DomainObject
    {
        public virtual string LoginID { get; set; }

        public virtual string Email { get; set; }

        public virtual string Phone { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        private string _fullName;

        public virtual string FullName
        {
            get { return (String.IsNullOrEmpty(_fullName) ? LastName + ", " + FirstName : _fullName); }
            set { _fullName = value; }
        }

        public virtual string EmployeeID { get; set; }

        public virtual string StudentID { get; set; }

        public virtual string UserImage { get; set; }

        public virtual string SID { get; set; }

        //private bool _Inactive;

        //public virtual bool Inactive
        //{
        //    get { return _Inactive; }
        //    set { _Inactive = value; }
        //}

        public virtual Guid UserKey { get; set; }

        public virtual IList<Unit> Units { get; set; }

        public virtual IList<Roles> Roles { get; set; }

        /// <summary>
        /// This is in order to demo an "entitled" user as a department user.
        /// </summary>
        public virtual bool IsDepartmentUser { get; set; }

        public static List<string> FindUCDKerberosIDs(string NameToMatch)
        {
            throw new System.NotImplementedException();
        }

        public User()
        {
            IsDepartmentUser = false;
        }

        public static User GetByLoginId(IRepository repository, string loginId)
        {
            Check.Require(repository != null, "Repository must be supplied");

            return repository.OfType<User>().
                              Queryable.FirstOrDefault(r => r.LoginID == loginId);
        }

        public static User GetByEmployeeId(IRepository repository, string employeeId)
        {
            Check.Require(repository != null, "Repository must be supplied");

            return repository.OfType<User>().
               Queryable.
               Where(r => r.EmployeeID == employeeId)
               .FirstOrDefault();
        }

        public static IList<User> GetAll(IRepository repository, User user, bool isDepartmentUser)
        {
            Check.Require(repository != null, "Repository must be supplied");

            // var units = new List<Unit>();
            List<int> unitIds;
            var users = new List<User>();

            if (isDepartmentUser)
            {
                // Get list of all user's departments assigned in Catbert:
                //units = user.Units.ToList();
                unitIds = user.Units.Select(unit => unit.Id).ToList();
            }
            else
            {
                // Get distinct list of user's deans office schools based on Catbert school code(s):
                var schoolsForUser = user.Units.Select(x => x.DeansOfficeSchoolCode).Distinct().ToArray();

                // Get list of all departments in the user's deans office school(s):
                //units = repository.OfType<Unit>().Queryable.Where(x => schoolsForUser.Contains(x.DeansOfficeSchoolCode)).ToList();
                unitIds = repository.OfType<Unit>().Queryable.Where(x => schoolsForUser.Contains(x.DeansOfficeSchoolCode)).Select(x => x.Id).ToList();
            }

            // we have to get the all users associated with those units:
            //TODO: Try implementing with LINQ and lambda expressions

            var criteria = NHibernateSessionManager.Instance.GetSession().CreateCriteria(typeof(User));

            criteria.CreateAlias("Units", "Units")
                .AddOrder(Order.Asc("LastName")).AddOrder(Order.Asc("FirstName"))
                .SetResultTransformer(new DistinctRootEntityResultTransformer());

            var conjunction = Restrictions.Conjunction();
            conjunction.Add(Restrictions.In("Units.Id", unitIds));
            criteria.Add(conjunction);

            return criteria.List<User>().ToList();

            //foreach (var unit in units)
            //{
            //    users.AddRange(unit.Users);

            //    //departments.AddRange(user.Units.Select(unit => repository.OfType<Department>()
            //    //    .Queryable
            //    //    .Where(d => d.Id.Equals(unit.PPSCode))
            //    //    .FirstOrDefault()));
            //}

            //return users.Distinct().OrderBy(x => x.FullName).ToList();
        }
    }

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Table("Catbert3_vUsers");

            Id(x => x.Id, "UserID")
                .UnsavedValue("0")
                .GeneratedBy.Identity();

            Map(x => x.LoginID);
            Map(x => x.Email);
            Map(x => x.Phone);
            Map(x => x.FirstName);
            Map(x => x.LastName);
            Map(x => x.EmployeeID);
            Map(x => x.StudentID);
            Map(x => x.UserImage);
            Map(x => x.SID);
            Map(x => x.UserKey);

            HasManyToMany(x => x.Units)
                .Table("Catbert3_vUserUnits")
                .AsBag()
                .ParentKeyColumn("UserID")
                .ChildKeyColumn("UnitID");

            HasManyToMany(x => x.Roles)
                .Table("Catbert3_vUserRoles")
                .AsBag()
                .ParentKeyColumn("UserID")
                .ChildKeyColumn("RoleID");
        }
    }
}
