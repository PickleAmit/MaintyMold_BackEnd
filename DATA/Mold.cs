//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DATA
{
    using System;
    using System.Collections.Generic;
    
    public partial class Mold
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Mold()
        {
            this.Errors = new HashSet<Error>();
        }
    
        public int MoldID { get; set; }
        public string MoldDescription { get; set; }
        public Nullable<System.DateTime> LastTreatmentDate { get; set; }
        public Nullable<int> HourOfLastTreatment { get; set; }
        public Nullable<int> LocationCode { get; set; }
        public string moldStatusAfterTreatment { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Error> Errors { get; set; }
        public virtual Location Location { get; set; }
    }
}
