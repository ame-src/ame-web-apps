using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmeWebApps.Models
{
    public class PendingReturnModel
    {
        public string PRINT_DATE { get; set; }
        public int CLIENT_ID { get; set; }
        public string COMPANY_NAME { get; set; }
        public string BRANCH { get; set; }
        public string REPLACING_NAME { get; set; }
        public string RETURN_SLIP_TRACKING_NUM { get; set; }
        public string PRINTED_BY { get; set; }
    }
}
