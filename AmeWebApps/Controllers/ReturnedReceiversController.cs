using AmeWebApps.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace AmeWebApps.Controllers
{
    public class ReturnedReceiversController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PendingReturns()
        {
            List<PendingReturnModel> PendingReturnsList = new List<PendingReturnModel>();
            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog;";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sqlQry = "sp_getPendingReturns";
                using (SqlCommand cmd = new SqlCommand(sqlQry, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            PendingReturnModel prm = new PendingReturnModel();
                        
                            if (!reader.IsDBNull(reader.GetOrdinal("BRANCH")))
                            prm.BRANCH = reader.GetString(reader.GetOrdinal("BRANCH"));
                            if (!reader.IsDBNull(reader.GetOrdinal("CLIENT_ID")))
                            prm.CLIENT_ID = reader.GetInt32(reader.GetOrdinal("CLIENT_ID"));
                            if (!reader.IsDBNull(reader.GetOrdinal("COMPANY_NAME")))
                            prm.COMPANY_NAME = reader.GetString(reader.GetOrdinal("COMPANY_NAME"));
                            if (!reader.IsDBNull(reader.GetOrdinal("PRINT_DATE")))
                            prm.PRINT_DATE = reader.GetString(reader.GetOrdinal("PRINT_DATE"));
                            if (!reader.IsDBNull(reader.GetOrdinal("PRINTED_BY")))
                            prm.PRINTED_BY = reader.GetString(reader.GetOrdinal("PRINTED_BY"));
                            if (!reader.IsDBNull(reader.GetOrdinal("REPLACING_NAME")))
                            prm.REPLACING_NAME = reader.GetString(reader.GetOrdinal("REPLACING_NAME"));
                            if (!reader.IsDBNull(reader.GetOrdinal("RETURN_SLIP_TRACKING_NUM")))
                            prm.RETURN_SLIP_TRACKING_NUM = reader.GetString(reader.GetOrdinal("RETURN_SLIP_TRACKING_NUM"));
                            PendingReturnsList.Add(prm);
                        }
                    }
                }
            }



            return View(PendingReturnsList);
        }

        public ActionResult AutoDiagnose(string tagNum)
        {
            if (tagNum.Contains("_"))
            {
                var filepath = "\\\\Amecomm\\ame\\Logs\\" + tagNum + "\\Faults.txt.gz";
                FileStream dest2 = System.IO.File.OpenRead(filepath);
                GZipStream myDecompressionStream = new GZipStream(dest2, CompressionMode.Decompress);
                FileStream ExtractedFile = System.IO.File.Create(@"C:\Faultss.txt");

                int mySecondByte = myDecompressionStream.ReadByte();

                while (mySecondByte != -1)
                {
                    ExtractedFile.WriteByte((byte)mySecondByte);
                    mySecondByte = myDecompressionStream.ReadByte();
                }

                ExtractedFile.Close();

                System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Faultss.txt");

                List<string> faultList = new List<string>();
                string line;

                while ((line = file.ReadLine()) != null)
                {
                    if (line.Substring((line.Length - 2), 2) == "ON")
                    {
                        string date = line.Substring((line.Length - 17), 14);
                        DateTime dt = DateTime.ParseExact(date,"yyyyMMddHHmmss",null);
                        date = dt.ToString();

                        line = line.Substring(3, (line.Length - 20)) + " " + date;
                        faultList.Add(line);
                    }
                }
                

                ViewBag.faultList = faultList;
                ViewBag.tagNum = tagNum;
            }

            if (!String.IsNullOrEmpty(tagNum))
            {
                string connString = "server=192.168.0.24; database=AmeMaster; uid=sa; password=topdog;";
                try
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string sqlQry = String.Format("select rcs.CLIENT_ID, rcs.DATE_LAST_COMM, rcs.SYSTEM_EXPIRES, rca.COMPANY_NAME, rca.BRANCH, rcs.COMPUTER_NAME, rcl.NOTES from RPM_CLIENT_SYSTEM rcs inner join RPM_CLIENT_LOCATION rcl on rcs.LOCATION_ID = rcl.LOCATION_ID inner join RPM_CLIENT_ADDRESS rca on rcl.ADDRESS_ID = rca.ADDRESS_ID where rcs.COMPUTER_NAME = '{0}'", tagNum);
                        using (SqlCommand cmd = new SqlCommand(sqlQry, conn))
                        {
                            conn.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    ViewData["COMPUTER_NAME"] = reader.GetString(reader.GetOrdinal("COMPUTER_NAME"));
                                    ViewData["CLIENT_ID"] = reader.GetInt32(reader.GetOrdinal("CLIENT_ID")).ToString();
                                    ViewData["COMPANY_NAME"] = reader.GetString(reader.GetOrdinal("COMPANY_NAME"));
                                    ViewData["BRANCH"] = reader.GetString(reader.GetOrdinal("BRANCH"));
                                    ViewData["DATE_LAST_COMM"] = reader.GetDateTime(reader.GetOrdinal("DATE_LAST_COMM")).ToString();
                                    ViewData["SYSTEM_EXPIRES"] = reader.GetDateTime(reader.GetOrdinal("SYSTEM_EXPIRES")).ToString();
                                    var notes = reader.GetString(reader.GetOrdinal("NOTES"));
                                    notes = notes.Replace(System.Environment.NewLine, @"<br/>");
                                    ViewData["NOTES"] = notes.ToString();
                                }
                            }
                            conn.Close();
                        }
                    }
                }
                catch (Exception e32)
                {
                    Console.Write(e32.ToString());
                }
            }

            return View();
        }

        public ActionResult InputReturn()
        {

            return View();
        }

        public ActionResult InputReturn2(string COMPUTER_NAME)
        {
            
            if (!String.IsNullOrEmpty(COMPUTER_NAME))
            {
                string connString = "server=192.168.0.24; database=AmeMaster; uid=sa; password=topdog;";
                try
                {
                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        if (COMPUTER_NAME.Length > 6)
                        {
                            COMPUTER_NAME = COMPUTER_NAME.Substring(4, 6);
                        }
                        string sqlQry = String.Format("select rcs.CLIENT_ID, rca.COMPANY_NAME, rca.BRANCH, rcs.COMPUTER_NAME, rcl.NOTES from RPM_CLIENT_SYSTEM rcs inner join RPM_CLIENT_LOCATION rcl on rcs.LOCATION_ID = rcl.LOCATION_ID inner join RPM_CLIENT_ADDRESS rca on rcl.ADDRESS_ID = rca.ADDRESS_ID where rcs.COMPUTER_NAME like 'AME_{0}'", COMPUTER_NAME);
                        using (SqlCommand cmd = new SqlCommand(sqlQry, conn))
                        {
                            conn.Open();
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    ViewData["COMPUTER_NAME"] = reader.GetString(reader.GetOrdinal("COMPUTER_NAME"));
                                    ViewData["CLIENT_ID"] = reader.GetInt32(reader.GetOrdinal("CLIENT_ID")).ToString();
                                    ViewData["COMPANY_NAME"] = reader.GetString(reader.GetOrdinal("COMPANY_NAME"));
                                    ViewData["BRANCH"] = reader.GetString(reader.GetOrdinal("BRANCH"));
                                    var notes = reader.GetString(reader.GetOrdinal("NOTES"));
                                    notes = notes.Replace(System.Environment.NewLine, @"<br/>");
                                    ViewData["NOTES"] = notes.ToString();
                                }
                            }
                            conn.Close();
                        } 
                    }
                }
                catch (Exception e32)
                {
                    Console.Write(e32.ToString());
                }
            }

            return View();
        }

        public ActionResult InputReturn3(string RETURN_DATE, int CLIENT_ID, string BRANCH, string COMPUTER_NAME, string RETURN_REASON, string REPLACED_BY)
        {
            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string insertStr = String.Format("INSERT INTO AME_SYSTEM_RETURNS (RETURN_DATE, CLIENT_ID, BRANCH, COMPUTER_NAME, RETURN_REASON, REPLACED_BY) values ('{0}', {1}, '{2}', '{3}', '{4}', '{5}')", RETURN_DATE, CLIENT_ID, BRANCH, COMPUTER_NAME, RETURN_REASON, REPLACED_BY);
                    using (var cmd = new SqlCommand(insertStr, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return View();
        }

        public ActionResult ViewReturns()
        {
            List<AME_SYSTEM_RETURNS> returnList = new List<AME_SYSTEM_RETURNS>();
            returnList = new AmeSystemReturnsEntities().AME_SYSTEM_RETURNS.OrderByDescending(x => x.RETURN_DATE).ToList();
            

            return View(returnList);
        }

        public ActionResult AddReturnIssue(int returnID)
        {
            AME_SYSTEM_RETURNS returnedReceiver = new AmeSystemReturnsEntities().AME_SYSTEM_RETURNS.Where(x => x.RETURN_ID == returnID).First();
            return View(returnedReceiver);
        }

        public ActionResult UpdateReturnIssue(int RETURN_ID, string ISSUES)
        {
            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string insertStr = String.Format("UPDATE AME_SYSTEM_RETURNS SET ISSUES = '{0}' WHERE RETURN_ID = {1}", ISSUES, RETURN_ID);
                    using (var cmd = new SqlCommand(insertStr, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            List<AME_SYSTEM_RETURNS> returnList = new List<AME_SYSTEM_RETURNS>();
            returnList = new AmeSystemReturnsEntities().AME_SYSTEM_RETURNS.ToList();

            return View("Index");
        }
    }
}
