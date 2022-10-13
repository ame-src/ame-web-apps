using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AmeWebApps.Models
{
    public class BuildSheetListModel
    {
        public int SYSTEM_ID { get; set; }
        public string COMPANY_NAME { get; set; }
        public string BRANCH { get; set; }
        public bool ALREADY_PRINTED { get; set; }
    }

    public class BuildSheetListDA
    {
        public static List<BuildSheetListModel> GetBuildSheetList()
        {
            string connString = "server=192.168.0.24; database=AmeMaster; uid=sa; password=topdog;";
            var returnList = new List<BuildSheetListModel>();
            using (var conn = new SqlConnection(connString))
            {
                string qry = "select rcs.SYSTEM_ID, rca.COMPANY_NAME, rca.BRANCH from RPM_CLIENT_SYSTEM rcs INNER JOIN RPM_CLIENT_LOCATION rcl ON rcs.LOCATION_ID = rcl.LOCATION_ID INNER JOIN RPM_CLIENT_ADDRESS rca ON rcl.ADDRESS_ID = rca.ADDRESS_ID INNER JOIN RPM_CLIENT_PROFILE rcp ON rcs.PROFILE_ID = rcp.PROFILE_ID where rcs.system_status = 'RELEASED' and rcs.system_type = 'PRODUCTION' order by rcs.SYSTEM_ID DESC ";
                using (var cmd = new SqlCommand(qry, conn))
                {
                    conn.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var buildSheet = new BuildSheetListModel();
                        buildSheet.SYSTEM_ID = reader.GetInt32(reader.GetOrdinal("SYSTEM_ID"));
                        buildSheet.COMPANY_NAME = reader.GetString(reader.GetOrdinal("COMPANY_NAME"));
                        if (!reader.IsDBNull(reader.GetOrdinal("BRANCH")))
                        {
                            buildSheet.BRANCH = reader.GetString(reader.GetOrdinal("BRANCH"));
                        }
                        buildSheet.ALREADY_PRINTED = false;

                        string connString2 = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog;";
                        var sqlQry = String.Format("SELECT * FROM AME_BUILD_SHEET WHERE SYSTEM_ID = {0}", buildSheet.SYSTEM_ID);
                        using (var con2 = new SqlConnection(connString2))
                        {
                            using (var cmd2 = new SqlCommand(sqlQry, con2))
                            {
                                con2.Open();
                                IDataReader reader2 = cmd2.ExecuteReader();
                                while (reader2.Read())
                                {
                                    if (!(reader2.IsDBNull(reader2.GetOrdinal("SYSTEM_ID"))))
                                    {
                                        buildSheet.ALREADY_PRINTED = true;
                                    }
                                }
                            }
                        }
                        returnList.Add(buildSheet);
                    }
                }
            }
            return returnList;
        }
    }
}