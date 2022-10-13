using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AmeWebApps.Models
{
    public class BuildSheetModel
    {
        public string INSTALL_DATE { get; set; }
        public string SHIP_BY_DATE { get; set; }
        public string PRINT_DATE { get; set; }
        public int PROFILE_SIZE { get; set; }
        public int CLIENT_ID { get; set; }
        public int NUM_ZONES { get; set; }
        public string COMPANY_NAME { get; set; }
        public int SYSTEM_ID { get; set; }
        public string BRANCH_NAME { get; set; }
        public bool USE_REFURB { get; set; }
        public bool IS_REPLACEMENT { get; set; }
        public int? REPLACING_TYPE { get; set; }
        public string COMM_TYPE { get; set; }
        public string ARRIVE_BY_DATE { get; set; }
        public string PACK_TYPE { get; set; }
        public bool RETURN_LABEL { get; set; }
        public string SHIP_TO { get; set; }
        public string CUSTOM_ADDR_ATTN { get; set; }
        public string CUSTOM_ADDR_1 { get; set; }
        public string CUSTOM_ADDR_2 { get; set; }
        public string CUSTOM_ADDR_CITY { get; set; }
        public string CUSTOM_ADDR_STATE { get; set; }
        public string CUSTOM_ADDR_ZIP { get; set; }
        public string ADDITIONAL_INSTRUCTIONS { get; set; }
        public string REPLACING_NAME { get; set; }
        public string PROFILE_NAME { get; set; }
    }
    public class BuildSheetModelDA
    {
        public static BuildSheetModel GetModel(int systemID)
        {
            var bsModel = new BuildSheetModel();
            string connString = "server=192.168.0.24; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
            int locationID = 0;
            using (var con = new SqlConnection(connString))
            {
                string qry = "select * from RPM_CLIENT_SYSTEM rcs INNER JOIN RPM_CLIENT_LOCATION rcl ON rcs.LOCATION_ID = rcl.LOCATION_ID INNER JOIN RPM_CLIENT_ADDRESS rca ON rcl.ADDRESS_ID = rca.ADDRESS_ID INNER JOIN RPM_CLIENT_PROFILE rcp ON rcs.PROFILE_ID = rcp.PROFILE_ID where rcs.system_status IN ('RELEASED') and rcs.system_type = 'PRODUCTION' AND rcs.system_id = " + systemID + " order by rcs.SYSTEM_ID DESC ";
                using (var cmd = new SqlCommand(qry, con))
                {
                    con.Open();
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        bsModel.PROFILE_SIZE = reader.GetInt32(reader.GetOrdinal("MIN_4K_UNIT_STORAGE"));
                        switch (bsModel.PROFILE_SIZE)
                        {
                            case 36700160:
                                bsModel.PROFILE_SIZE = 160;
                                break;
                            case 26738688:
                                bsModel.PROFILE_SIZE = 120;
                                break;
                            case 17039360:
                                bsModel.PROFILE_SIZE = 80;
                                break;
                            case 12058624:
                                bsModel.PROFILE_SIZE = 60;
                                break;
                            case 8388608:
                                bsModel.PROFILE_SIZE = 40;
                                break;
                            case 4587520:
                                bsModel.PROFILE_SIZE = 20;
                                break;
                            case 4456448:
                                bsModel.PROFILE_SIZE = 20;
                                break;
                            case 3407872:
                                bsModel.PROFILE_SIZE = 15;
                                break;
                            case 3145728:
                                bsModel.PROFILE_SIZE = 14;
                                break;
                            case 2883584:
                                bsModel.PROFILE_SIZE = 13;
                                break;
                        }
                        bsModel.PROFILE_NAME = reader.GetString(reader.GetOrdinal("PROFILE_NAME"));
                        bsModel.CLIENT_ID = reader.GetInt32(reader.GetOrdinal("CLIENT_ID"));
                        bsModel.NUM_ZONES = reader.GetInt32(reader.GetOrdinal("ZONES"));
                        if (!reader.IsDBNull(reader.GetOrdinal("COMPANY_NAME")))
                        {
                            bsModel.COMPANY_NAME = reader.GetString(reader.GetOrdinal("COMPANY_NAME"));
                            bsModel.COMPANY_NAME = bsModel.COMPANY_NAME.Replace("'", "");
                        }
                        bsModel.SYSTEM_ID = reader.GetInt32(reader.GetOrdinal("SYSTEM_ID"));
                        bsModel.IS_REPLACEMENT = false;
                        bsModel.PACK_TYPE = "New";
                        bsModel.COMM_TYPE = "WAN";
                        bsModel.SHIP_TO = "SHIPPING";
                        if (!reader.IsDBNull(reader.GetOrdinal("BRANCH")))
                        {
                            bsModel.BRANCH_NAME = reader.GetString(reader.GetOrdinal("BRANCH"));
                            bsModel.BRANCH_NAME = bsModel.BRANCH_NAME.Replace("&", "And");
                        }
                        bsModel.COMPANY_NAME = bsModel.COMPANY_NAME.Replace("&", "And");
                        locationID = reader.GetInt32(reader.GetOrdinal("LOCATION_ID"));
                    }
                }
                qry = "select * from RPM_CLIENT_SYSTEM where LOCATION_ID=" + locationID + " and SYSTEM_STATUS in ('SHIPPED', 'INSTALLED', 'BUILT') and SYSTEM_TYPE = 'REPLACING' order by DATE_BUILT desc";
                using (var cmd = new SqlCommand(qry, con))
                {
                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        bsModel.IS_REPLACEMENT = true;
                        bsModel.REPLACING_NAME = reader.GetString(reader.GetOrdinal("COMPUTER_NAME"));
                        bsModel.REPLACING_TYPE = Convert.ToInt32(reader.GetString(reader.GetOrdinal("COMPUTER_NAME")).Substring(4, 1));
                        bsModel.RETURN_LABEL = true;
                        bsModel.SHIP_TO = "INSTALLED";
                        bsModel.USE_REFURB = true;
                        if (reader.IsDBNull(reader.GetOrdinal("COMM_METHOD")))
                        {
                            bsModel.COMM_TYPE = "WAN";
                        }
                        else
                        {
                            if ((reader.GetString(reader.GetOrdinal("COMM_METHOD"))) == "WAN")
                            {
                                bsModel.COMM_TYPE = "WAN";
                            }
                            else
                            {
                                bsModel.COMM_TYPE = "DIALUP";
                            }
                        }
                        bsModel.PACK_TYPE = "Replacement";
                    }
                }
            }
            return bsModel;
        }
    }
}