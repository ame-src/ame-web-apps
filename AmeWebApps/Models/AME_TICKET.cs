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
    
    public partial class AME_TICKET
    {
        public int TICKET_ID { get; set; }
        public string STATUS { get; set; }
        public int CLIENT_ID { get; set; }
        public string BRANCH { get; set; }
        public System.DateTime DATE_OPENED { get; set; }
        public string ASIGNEE { get; set; }
        public string RECIEVER_TAG { get; set; }
        public string REASON_FOR_CALL { get; set; }
        public string ESCALATED_TO { get; set; }
        public Nullable<System.DateTime> LAST_UPDATE { get; set; }
        public Nullable<int> PRIORITY { get; set; }
        public Nullable<System.DateTime> DATE_CLOSED { get; set; }
        public string REASON_CLOSED { get; set; }
        public string LOST { get; set; }
    }
}
