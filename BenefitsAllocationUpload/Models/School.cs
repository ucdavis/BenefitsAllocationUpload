//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BenefitsAllocationUpload.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class School
    {
        public School()
        {
            this.Units = new HashSet<Unit>();
        }
    
        public string SchoolCode { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string Abbreviation { get; set; }
    
        public virtual ICollection<Unit> Units { get; set; }
    }
}
