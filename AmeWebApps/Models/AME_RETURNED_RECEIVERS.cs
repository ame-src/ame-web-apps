//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AmeWebApps.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AME_RETURNED_RECEIVERS
    {
        public int RETURN_ID { get; set; }
        public Nullable<System.DateTime> RETURN_DATE { get; set; }
        public Nullable<int> CLIENT_ID { get; set; }
        public string CLIENT_NAME { get; set; }
        public string BRANCH { get; set; }
        public string COMPUTER_NAME { get; set; }
        public string RETURN_REASON { get; set; }
        public string RETURNED_BY { get; set; }
        public string IS_LOST { get; set; }
        public string REASON_LOST { get; set; }
    }
}
