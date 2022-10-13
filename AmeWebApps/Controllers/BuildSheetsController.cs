using AmeWebApps.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Word = Microsoft.Office.Interop.Word;
//using Microsoft.Office.Tools.Word;

namespace AmeWebApps.Controllers
{
    public class BuildSheetsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestPrint(string SendName="shawn_connolly", string SendDomain="amemusic", string ToAddr="shawn_connolly@amemusic.com")
        {
                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("exchange.local.rpm.com");
                smtpServer.Credentials = new System.Net.NetworkCredential("SConnolly", "s1987Conno!!y");
                smtpServer.Port = 25;
                string SendAddr = SendName + "@" + SendDomain + ".com";
                mail.From = new MailAddress(SendAddr);
                mail.To.Add(ToAddr);
                mail.Subject = "Hey our test is starting up again!";
                mail.IsBodyHtml = true;
                mail.Body = "<html><head></head><body></body></html>";    

                smtpServer.Send(mail);

            return View();
        }

        public ActionResult DeleteEntry(int systemID)
        {
            if (systemID != null || systemID != 0)
            {
                string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
                using (var conn = new SqlConnection(connString))
                {
                    var deleteQry = String.Format("delete from AME_BUILD_SHEET where system_id = {0}", systemID);
                    using (var cmd = new SqlCommand(deleteQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
            }

            return View();
        }

        public ActionResult ViewShipped()
        {
            List<AME_BUILD_SHEET> bsList = new List<AME_BUILD_SHEET>();
            bsList = new AmeMasterEntities().AME_BUILD_SHEET.Where(x => x.TRACKING_NUM != null).ToList();

            foreach (var bs in bsList)
            {
                if (!String.IsNullOrEmpty(bs.DATE_SHIPPED))
                {
                    bs.DATE_SHIPPED = Convert.ToDateTime(bs.DATE_SHIPPED).ToString("yyyy/MM/dd");
                }
            }

            bsList = bsList.OrderByDescending(x => x.DATE_SHIPPED).ToList();

            return View(bsList);
        }

        public ActionResult PrintList()
        {
            List<BuildSheetListModel> bsList = new List<BuildSheetListModel>();
            bsList = BuildSheetListDA.GetBuildSheetList().Where(x => x.ALREADY_PRINTED == false).ToList();

            return View(bsList);
        }

        public ActionResult VerifyBuildSheet(int systemID)
        {
            BuildSheetModel bsModel = new BuildSheetModel();
            bsModel = BuildSheetModelDA.GetModel(systemID);
            if (!String.IsNullOrEmpty(bsModel.ADDITIONAL_INSTRUCTIONS))
            {
                bsModel.ADDITIONAL_INSTRUCTIONS = bsModel.ADDITIONAL_INSTRUCTIONS.Replace("'", "`");
                bsModel.ADDITIONAL_INSTRUCTIONS = bsModel.ADDITIONAL_INSTRUCTIONS.Replace("<", "");
            }
            if (!String.IsNullOrEmpty(bsModel.PROFILE_NAME))
            {
                bsModel.PROFILE_NAME = bsModel.PROFILE_NAME.Replace("'", "`");
                bsModel.PROFILE_NAME = bsModel.PROFILE_NAME.Replace("<", "");
            }
            if (!String.IsNullOrEmpty(bsModel.BRANCH_NAME))
            {
                bsModel.BRANCH_NAME = bsModel.BRANCH_NAME.Replace("'", "`");
                bsModel.BRANCH_NAME = bsModel.BRANCH_NAME.Replace("<", "");
            }
            if (!String.IsNullOrEmpty(bsModel.COMPANY_NAME))
            {
                bsModel.COMPANY_NAME = bsModel.COMPANY_NAME.Replace("'", "`");
                bsModel.COMPANY_NAME = bsModel.COMPANY_NAME.Replace("<", "");
            }
            if (!(String.IsNullOrEmpty(bsModel.REPLACING_NAME)))
            {
                bsModel.REPLACING_TYPE = Convert.ToInt32(bsModel.REPLACING_NAME.Substring(4, 1));
            }
            else
            {
                bsModel.REPLACING_TYPE = 8;
            }

            return View(bsModel);
        }

        public ActionResult EditList()
        {
            List<BuildSheetListModel> bsList = new List<BuildSheetListModel>();
            bsList = BuildSheetListDA.GetBuildSheetList().Where(x => x.ALREADY_PRINTED == true).ToList();

            return View(bsList);
        }

        public ActionResult EditBuildSheet(int systemID)
        {
            AmeMasterEntities AME = new AmeMasterEntities();
            AME_BUILD_SHEET bsModel = new AME_BUILD_SHEET();
            bsModel = AME.AME_BUILD_SHEET.Where(x => x.SYSTEM_ID == systemID).First();

            return View(bsModel);
        }

        public ActionResult AddShippingInfoList(int? showHidden)
        {
            List<AME_BUILD_SHEET> bsList = new List<AME_BUILD_SHEET>();
            AmeMasterEntities AME = new AmeMasterEntities();
            bsList = AME.AME_BUILD_SHEET.Where(x => String.IsNullOrEmpty(x.TRACKING_NUM)).OrderBy(x => x.SHIP_BY_DATE).ToList();
            if (showHidden == 1)
            {
                bsList = bsList.Concat(AME.AME_BUILD_SHEET.Where(x => x.TRACKING_NUM == "HIDDEN")).ToList();
                bsList.OrderBy(x => x.SHIP_BY_DATE);
            }
            return View(bsList);

        }

        public ActionResult HideBuildsheet(int systemID)
        {
            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
            using (var conn = new SqlConnection(connString))
            {
                var updateQry = String.Format("UPDATE AME_BUILD_SHEET set TRACKING_NUM = 'HIDDEN' WHERE SYSTEM_ID = {0}", systemID);
                using (var cmd = new SqlCommand(updateQry, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            List<AME_BUILD_SHEET> bsList = new List<AME_BUILD_SHEET>();
            AmeMasterEntities AME = new AmeMasterEntities();
            bsList = AME.AME_BUILD_SHEET.Where(x => String.IsNullOrEmpty(x.TRACKING_NUM)).OrderBy(x => x.SHIP_BY_DATE).ToList();

            return View("AddShippingInfoList", bsList);
        }

        public ActionResult PrintFinalBuildSheet(int systemID, string TRACKING_NUM, string RETURN_NUM, string SHIP_MTHD)
        {
            RPMModels RPM = new RPMModels();
            RPM_CLIENT_SYSTEM systemModel = new RPM_CLIENT_SYSTEM();
            systemModel = RPM.RPM_CLIENT_SYSTEM.Where(x => x.SYSTEM_ID == systemID).First();

            var DATE_BUILT = systemModel.DATE_BUILT.ToString();
            var DATE_SHIPPED = DateTime.Now.ToString();
            var TAG_NUMBER = systemModel.COMPUTER_NAME;

            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
            using (var conn = new SqlConnection(connString))
            {
                var updateQry = String.Format("UPDATE AME_BUILD_SHEET set DATE_BUILT = '{0}', DATE_SHIPPED = '{1}', TAG_NUMBER = '{2}', TRACKING_NUM = '{3}', RETURN_SLIP_TRACKING_NUM = '{4}', SHIPPING_METHOD = '{5}' where SYSTEM_ID = {6}", DATE_BUILT, DATE_SHIPPED, TAG_NUMBER, TRACKING_NUM, RETURN_NUM, SHIP_MTHD, systemID);
                using (var cmd = new SqlCommand(updateQry, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            string connString2 = "server=AME-DATA-02; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
            using (var conn = new SqlConnection(connString2))
            {
                var updateQry = String.Format("UPDATE RPM_CLIENT_SYSTEM set SYSTEM_STATUS = 'SHIPPED', DATE_SHIPPED = getdate() where system_id = {0} and SYSTEM_STATUS='BUILT'", systemID);
                using (var cmd = new SqlCommand(updateQry, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            string filePath = null;
            string newFilePath = @"\\AME-FILE-01\Apps\Buildsheets\" + systemID + "f.docx";
            if (System.IO.File.Exists(@"\\AME-FILE-01\Apps\Buildsheets\" + systemID + "e.docx"))
            {
                filePath = @"\\AME-FILE-01\Apps\Buildsheets\" + systemID + "e.docx";
            }
            else
            {
                filePath = @"\\AME-FILE-01\Apps\Buildsheets\" + systemID + ".docx";
            }

            System.IO.File.Copy(filePath, newFilePath, true);

            

            using (WordprocessingDocument doc = WordprocessingDocument.Open(newFilePath, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                Regex rxText = new Regex("");

                if (!String.IsNullOrEmpty(DATE_BUILT))
                {
                    rxText = new Regex("DATE_BUILT");
                    docText = rxText.Replace(docText, DATE_BUILT);
                }

                if (!String.IsNullOrEmpty(DATE_SHIPPED))
                {
                    rxText = new Regex("DATE_SHIP");
                    docText = rxText.Replace(docText, DATE_SHIPPED);
                }

                if (!String.IsNullOrEmpty(TRACKING_NUM))
                {
                    rxText = new Regex("TRACKING_NUM");
                    docText = rxText.Replace(docText, TRACKING_NUM);
                }
                if (!String.IsNullOrEmpty(TAG_NUMBER))
                {
                    rxText = new Regex("TAG_NUM");
                    docText = rxText.Replace(docText, TAG_NUMBER);
                }
                if (!String.IsNullOrEmpty(SHIP_MTHD))
                {
                    rxText = new Regex("SHIP_MTHD");
                    docText = rxText.Replace(docText, SHIP_MTHD);
                }
                if (!String.IsNullOrEmpty(RETURN_NUM))
                {
                    rxText = new Regex("RETURN_NUM");
                    docText = rxText.Replace(docText, RETURN_NUM);
                }
                using (StreamWriter sw = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                }
                doc.Close();
            }

            //-------------Print build sheet
            Word.Application wordApp = new Word.Application();
            Word.Document wordDoc = new Word.Document();
            wordDoc = wordApp.Documents.Open(newFilePath);
            object oMissing = System.Reflection.Missing.Value;
            wordDoc.Activate();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            //wordDoc.PrintOut(oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
            wordDoc.Close();
            wordApp.Quit();

            //Email?

            if (systemModel.CLIENT_ID == 2338)
            {
                AmeMasterEntities AME = new AmeMasterEntities();
                AME_BUILD_SHEET bsModel = new AME_BUILD_SHEET();
                bsModel = AME.AME_BUILD_SHEET.Where(x => x.SYSTEM_ID == systemModel.SYSTEM_ID).First();

                var filepath = "\\\\Amecomm\\ame\\Logs\\" + systemModel.COMPUTER_NAME + "\\IP Settings 1.txt.gz";
                FileStream dest2 = System.IO.File.OpenRead(filepath);
                GZipStream myDecompressionStream = new GZipStream(dest2, CompressionMode.Decompress);
                FileStream ExtractedFile = System.IO.File.Create(@"C:\Mac.txt");

                int mySecondByte = myDecompressionStream.ReadByte();

                while (mySecondByte != -1)
                {
                    ExtractedFile.WriteByte((byte)mySecondByte);
                    mySecondByte = myDecompressionStream.ReadByte();
                }

                ExtractedFile.Close();

                string text = System.IO.File.ReadAllText(@"C:\Mac.txt");
                string MAC = "";
                if (systemModel.COMPUTER_NAME.Contains("L"))
                {
                    int first = text.IndexOf("HWaddr ") + "HWaddr ".Length;
                    MAC = text.Substring(first, 17);
                    MAC = MAC.ToUpper();
                    MAC = MAC.Replace(":", "-");
                }
                if (systemModel.COMPUTER_NAME.Contains("_"))
                {
                    int first = text.IndexOf("Physical Address. . . . . . . . . : ") + "Physical Address. . . . . . . . . : ".Length;
                    MAC = text.Substring(first, 17);
                }

                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient("exchange.local.rpm.com");
                smtpServer.Credentials = new System.Net.NetworkCredential("SConnolly", "s1987Conno!!y");
                smtpServer.Port = 25;
                mail.From = new MailAddress("BuildSheetPrinter@amemusic.com");
                mail.To.Add("shawn_connolly@amemusic.com");
                mail.To.Add("paul_krikorian@amemusic.com");
                mail.To.Add("stephen_rurka@amemusic.com");
                mail.Subject = "Key Bank Shipping Info";
                mail.Body = bsModel.COMPANY_NAME + " " + bsModel.BRANCH_NAME + Environment.NewLine + "TAG: " + systemModel.COMPUTER_NAME.Substring(4, 6) + Environment.NewLine + "MAC: " + MAC + Environment.NewLine + "Tracking: " + TRACKING_NUM;

                smtpServer.Send(mail);
            }
            
            return View();
        }

        public ActionResult PrintFinalBuildSheetBatch(int systemIDstart, int systemIDstop, string TRACKING_NUM, string RETURN_NUM, string SHIP_MTHD)
        {
            RPMModels RPM = new RPMModels();
            RPM_CLIENT_SYSTEM systemModel = new RPM_CLIENT_SYSTEM();

            for (var systemID = systemIDstart; systemID <= systemIDstop; systemID++)
            {

                systemModel = RPM.RPM_CLIENT_SYSTEM.Where(x => x.SYSTEM_ID == systemID).First();

                var DATE_BUILT = systemModel.DATE_BUILT.ToString();
                var DATE_SHIPPED = DateTime.Now.ToString();
                var TAG_NUMBER = systemModel.COMPUTER_NAME;

                string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
                using (var conn = new SqlConnection(connString))
                {
                    var updateQry = String.Format("UPDATE AME_BUILD_SHEET set DATE_BUILT = '{0}', DATE_SHIPPED = '{1}', TAG_NUMBER = '{2}', TRACKING_NUM = '{3}', RETURN_SLIP_TRACKING_NUM = '{4}', SHIPPING_METHOD = '{5}' where SYSTEM_ID = {6}", DATE_BUILT, DATE_SHIPPED, TAG_NUMBER, TRACKING_NUM, RETURN_NUM, SHIP_MTHD, systemID);
                    using (var cmd = new SqlCommand(updateQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                string connString2 = "server=AME-DATA-02; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
                using (var conn = new SqlConnection(connString2))
                {
                    var updateQry = String.Format("UPDATE RPM_CLIENT_SYSTEM set SYSTEM_STATUS = 'SHIPPED', DATE_SHIPPED = getdate() where system_id = {0} and SYSTEM_STATUS='BUILT'", systemID);
                    using (var cmd = new SqlCommand(updateQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }

                string filePath = null;
                string newFilePath = @"\\AME-FILE-01\Apps\Buildsheets\" + systemID + "f.docx";
                if (System.IO.File.Exists(@"\\AME-FILE-01\Apps\Buildsheets\" + systemID + "e.docx"))
                {
                    filePath = @"\\AME-FILE-01\Apps\Buildsheets\" + systemID + "e.docx";
                }
                else
                {
                    filePath = @"\\AME-FILE-01\Apps\Buildsheets\" + systemID + ".docx";
                }

                System.IO.File.Copy(filePath, newFilePath, true);



                using (WordprocessingDocument doc = WordprocessingDocument.Open(newFilePath, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }

                    Regex rxText = new Regex("DATE_BUILT");
                    docText = rxText.Replace(docText, DATE_BUILT);

                    rxText = new Regex("DATE_SHIP");
                    docText = rxText.Replace(docText, DATE_SHIPPED);

                    rxText = new Regex("TRACKING_NUM");
                    docText = rxText.Replace(docText, TRACKING_NUM);

                    rxText = new Regex("TAG_NUM");
                    docText = rxText.Replace(docText, TAG_NUMBER);

                    rxText = new Regex("SHIP_MTHD");
                    docText = rxText.Replace(docText, SHIP_MTHD);

                    rxText = new Regex("RETURN_NUM");
                    docText = rxText.Replace(docText, RETURN_NUM);

                    using (StreamWriter sw = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create)))
                    {
                        sw.Write(docText);
                    }
                    doc.Close();
                }

                //-------------Print build sheet
                Word.Application wordApp = new Word.Application();
                Word.Document wordDoc = new Word.Document();
                wordDoc = wordApp.Documents.Open(newFilePath);
                object oMissing = System.Reflection.Missing.Value;
                wordDoc.Activate();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                //wordDoc.PrintOut(oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                wordDoc.Close();
                wordApp.Quit();
   
            }

            return View();
        }

        public ActionResult AddShippingInfo(int systemID)
        {
            RPMModels RPMDB = new RPMModels();
            List<RPM_CLIENT_ADDRESS> addrList = new List<RPM_CLIENT_ADDRESS>();

            ViewBag.systemID = systemID;

            try
            {
                RPM_CLIENT_SYSTEM rcsModel = new RPM_CLIENT_SYSTEM();
                rcsModel = RPMDB.RPM_CLIENT_SYSTEM.Where(x => x.SYSTEM_ID == systemID).First();

                RPM_CLIENT_LOCATION rclModel = new RPM_CLIENT_LOCATION();
                rclModel = RPMDB.RPM_CLIENT_LOCATION.Where(x => x.LOCATION_ID == rcsModel.LOCATION_ID).First();

                RPM_CLIENT_ADDRESS installedAtAddr = new RPM_CLIENT_ADDRESS();
                RPM_CLIENT_ADDRESS shipToAddr = new RPM_CLIENT_ADDRESS();

                installedAtAddr = RPMDB.RPM_CLIENT_ADDRESS.Where(x => x.ADDRESS_ID == rclModel.ADDRESS_ID).First();
                if (!(installedAtAddr == null))
                {
                    installedAtAddr.COMPANY_NAME = installedAtAddr.COMPANY_NAME.Replace("'", "`");
                    installedAtAddr.COMPANY_TYPE = "Installed At Address";
                    addrList.Add(installedAtAddr);
                }

                shipToAddr = RPMDB.RPM_CLIENT_ADDRESS.Where(x => x.ADDRESS_ID == rclModel.SHIP_TO_ADDRESS_ID).First();
                if (!(shipToAddr == null))
                {
                    shipToAddr.COMPANY_NAME = shipToAddr.COMPANY_NAME.Replace("'", "`");
                    shipToAddr.COMPANY_TYPE = "Ship To Address";
                    addrList.Add(shipToAddr);
                }

                return View(addrList);
            }
            catch { }

            return View();
        }

        public ActionResult UpdateBuildSheet(string INSTALL_DATE, string SHIP_BY_DATE, int PROFILE_SIZE, int CLIENT_ID, int NUM_ZONES, string COMPANY_NAME, int SYSTEM_ID, string BRANCH_NAME, string USE_REFURB, string REPLACING_TYPE, string COMM_TYPE, string ARRIVE_BY_DATE, string PACK_TYPE, string RETURN_LABEL, string SHIP_TO, string CUSTOM_ADDR_ATTN, string CUSTOM_ADDR_1, string CUSTOM_ADDR_2, string CUSTOM_ADDR_CITY, string CUSTOM_ADDR_STATE, string CUSTOM_ADDR_ZIP, string ADDITIONAL_INSTRUCTIONS, string REPLACING_NAME, string BUILD_SHEET_ID)
        {
            //------------Set and format variables
            var PRINTED_BY = User.Identity.Name.Substring(4);
            DateTime dt = DateTime.Now;
            var PRINT_DATE = dt.ToString("MM/dd/yyyy");
            if (String.IsNullOrEmpty(REPLACING_TYPE)) { REPLACING_TYPE = "NEW"; };

            if ((Convert.ToDateTime(INSTALL_DATE)) <= dt) { INSTALL_DATE = "ASAP"; };

            if (Convert.ToDateTime(SHIP_BY_DATE) <= dt) { SHIP_BY_DATE = "0"; }
            else { SHIP_BY_DATE = Convert.ToDateTime(SHIP_BY_DATE).ToString("MM/dd/yyyy"); };

            if (!String.IsNullOrEmpty(ARRIVE_BY_DATE))
            {
                if ((Convert.ToDateTime(ARRIVE_BY_DATE)) <= dt) { ARRIVE_BY_DATE = "ASAP"; }
                else { ARRIVE_BY_DATE = (Convert.ToDateTime(ARRIVE_BY_DATE)).ToString("MM/dd/yyyy"); };
            }
            else
            {
                ARRIVE_BY_DATE = "ASAP";
            }

            if (!String.IsNullOrEmpty(ADDITIONAL_INSTRUCTIONS))
            {
                ADDITIONAL_INSTRUCTIONS = ADDITIONAL_INSTRUCTIONS.Replace("'", "`");
            }

            var replacingName = REPLACING_NAME;

            if (String.IsNullOrWhiteSpace(REPLACING_NAME) || String.IsNullOrEmpty(REPLACING_NAME)) { REPLACING_NAME = "NULL"; }
            else { REPLACING_NAME = "'" + REPLACING_NAME + "'"; };

            //------------Update database
            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
            using (var conn = new SqlConnection(connString))
            {
                //try
                //{
                    var updateQry = String.Format("UPDATE AME_BUILD_SHEET set INSTALL_DATE = '{0}', SHIP_BY_DATE = '{1}', PROFILE_SIZE = {2}, CLIENT_ID = {3}, NUM_ZONES = {4}, COMPANY_NAME = '{5}', SYSTEM_ID = {6}, BRANCH_NAME = '{7}', USE_REFURB = '{8}', REPLACING_TYPE = '{9}', COMM_TYPE = '{10}', ARRIVE_BY_DATE = '{11}', PACKAGING_TYPE = '{12}', RETURN_LABEL = '{13}', SHIP_TO_TYPE = '{14}', CUSTOM_ADDR_ATTN = '{15}', CUSTOM_ADDR_1 = '{16}', CUSTOM_ADDR_2 = '{17}', CUSTOM_ADDR_CITY = '{18}', CUSTOM_ADDR_STATE = '{19}', CUSTOM_ADDR_ZIP = '{20}', ADDITIONAL_INSTRUCTIONS = '{21}' WHERE BUILD_SHEET_ID = {22}", INSTALL_DATE, SHIP_BY_DATE, PROFILE_SIZE, CLIENT_ID, NUM_ZONES, COMPANY_NAME, SYSTEM_ID, BRANCH_NAME, USE_REFURB, REPLACING_TYPE, COMM_TYPE, ARRIVE_BY_DATE, PACK_TYPE, RETURN_LABEL, SHIP_TO, CUSTOM_ADDR_ATTN, CUSTOM_ADDR_1, CUSTOM_ADDR_2, CUSTOM_ADDR_CITY, CUSTOM_ADDR_STATE, CUSTOM_ADDR_ZIP, ADDITIONAL_INSTRUCTIONS, BUILD_SHEET_ID);
                    using (var cmd = new SqlCommand(updateQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                //}
                //catch (Exception ex)
                //{
                //    Console.Write(ex);
                //}
            }

            //-----------Create build sheet document from the template
            System.IO.File.Copy(@"C:\Buildsheet.docx", @"\\AME-FILE-01\Apps\Buildsheets\" + SYSTEM_ID + "e.docx", true);
            using (WordprocessingDocument doc = WordprocessingDocument.Open(@"\\AME-FILE-01\Apps\Buildsheets\" + SYSTEM_ID + "e.docx", true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                Regex rxText = new Regex("PRINT_DATE");
                docText = rxText.Replace(docText, PRINT_DATE.ToString());

                rxText = new Regex("SYSTEM_ID");
                docText = rxText.Replace(docText, SYSTEM_ID.ToString());

                rxText = new Regex("INSTALL_DATE");
                docText = rxText.Replace(docText, INSTALL_DATE);

                if (SHIP_BY_DATE == "0") { SHIP_BY_DATE = "ASAP"; };
                rxText = new Regex("SHIP_BY_DATE");
                docText = rxText.Replace(docText, SHIP_BY_DATE);

                rxText = new Regex("PROFILE_SIZE");
                docText = rxText.Replace(docText, PROFILE_SIZE.ToString());

                rxText = new Regex("CLIENT_ID");
                docText = rxText.Replace(docText, CLIENT_ID.ToString());

                rxText = new Regex("NUM_ZONES");
                docText = rxText.Replace(docText, NUM_ZONES.ToString());

                rxText = new Regex("COMPANY_NAME");
                docText = rxText.Replace(docText, COMPANY_NAME);

                rxText = new Regex("BRANCH_NAME");
                docText = rxText.Replace(docText, BRANCH_NAME);

                rxText = new Regex("USE_REFURB");
                docText = rxText.Replace(docText, USE_REFURB);

                rxText = new Regex("REPLACING_TYPE");
                if (!(String.IsNullOrEmpty(replacingName))) { docText = rxText.Replace(docText, replacingName.ToString()); }
                if (replacingName == "NULL") { docText = rxText.Replace(docText, "NEW"); };

                rxText = new Regex("COMM_TYPE");
                docText = rxText.Replace(docText, COMM_TYPE);

                rxText = new Regex("ARRIVE_BY_DATE");
                docText = rxText.Replace(docText, ARRIVE_BY_DATE);

                rxText = new Regex("PACK_TYPE");
                docText = rxText.Replace(docText, PACK_TYPE);

                rxText = new Regex("RETURN_LABEL");
                docText = rxText.Replace(docText, RETURN_LABEL);

                rxText = new Regex("SHIP_TO");
                docText = rxText.Replace(docText, SHIP_TO);

                rxText = new Regex("CUSTOM_ADDR_ATTN");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_ATTN))) { docText = rxText.Replace(docText, CUSTOM_ADDR_ATTN); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_1");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_1))) { docText = rxText.Replace(docText, CUSTOM_ADDR_1); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_2");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_2))) { docText = rxText.Replace(docText, CUSTOM_ADDR_2); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_CITY");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_CITY))) { docText = rxText.Replace(docText, CUSTOM_ADDR_CITY); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_STATE");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_STATE))) { docText = rxText.Replace(docText, CUSTOM_ADDR_STATE); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_ZIP");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_ZIP))) { docText = rxText.Replace(docText, CUSTOM_ADDR_ZIP); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("ADDITIONAL_INSTRUCTIONS");
                docText = rxText.Replace(docText, ADDITIONAL_INSTRUCTIONS);

                rxText = new Regex("PRINTED_BY");
                docText = rxText.Replace(docText, ("REPRINT - " + User.Identity.Name.Substring(4)));

                using (StreamWriter sw = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                }
                doc.Close();
            }

            //-------------Print build sheet
            Word.Application wordApp = new Word.Application();
            Word.Document wordDoc = new Word.Document();
            var filePath = @"\\AME-FILE-01\Apps\Buildsheets\" + SYSTEM_ID + "e.docx";
            wordDoc = wordApp.Documents.Open(filePath);
            object oMissing = System.Reflection.Missing.Value;
            wordDoc.Activate();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            wordDoc.PrintOut(oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
            wordDoc.Close();
            wordApp.Quit();

            return View();
        }

        public ActionResult PrintBuildSheet(string PROFILE_NAME, string INSTALL_DATE, string SHIP_BY_DATE, int PROFILE_SIZE, int CLIENT_ID, int NUM_ZONES, string COMPANY_NAME, int SYSTEM_ID, string BRANCH_NAME, bool USE_REFURB, int REPLACING_TYPE, string COMM_TYPE, string ARRIVE_BY_DATE, string PACK_TYPE, bool RETURN_LABEL, string SHIP_TO, string CUSTOM_ADDR_ATTN, string CUSTOM_ADDR_1, string CUSTOM_ADDR_2, string CUSTOM_ADDR_CITY, string CUSTOM_ADDR_STATE, string CUSTOM_ADDR_ZIP, string ADDITIONAL_INSTRUCTIONS, string REPLACING_NAME)
        {
            //------------Set and format variables
            var PRINTED_BY = User.Identity.Name;
            var USE_REF = "No";
            var INC_RETURN = "No";
            DateTime dt = DateTime.Now;
            var PRINT_DATE = dt.ToString("MM/dd/yyyy");

            if (USE_REFURB) { USE_REF = "Yes"; }
            if (RETURN_LABEL) { INC_RETURN = "YES"; }
            if ((Convert.ToDateTime(INSTALL_DATE)) <= dt) { INSTALL_DATE = "ASAP"; };

            if (!String.IsNullOrEmpty(ADDITIONAL_INSTRUCTIONS))
            {
                ADDITIONAL_INSTRUCTIONS = ADDITIONAL_INSTRUCTIONS.Replace("'", "`");
                ADDITIONAL_INSTRUCTIONS = ADDITIONAL_INSTRUCTIONS.Replace("&", "and");
            }

            PROFILE_NAME = PROFILE_NAME.Replace("'", "`");
            COMPANY_NAME = COMPANY_NAME.Replace("'", "`");

            if (Convert.ToDateTime(SHIP_BY_DATE) <= dt) { SHIP_BY_DATE = "0"; }
            else { SHIP_BY_DATE = Convert.ToDateTime(SHIP_BY_DATE).ToString("MM/dd/yyyy"); };

            if ((Convert.ToDateTime(ARRIVE_BY_DATE)) <= dt) { ARRIVE_BY_DATE = "ASAP"; }
            else { ARRIVE_BY_DATE = (Convert.ToDateTime(ARRIVE_BY_DATE)).ToString("MM/dd/yyyy"); };
            
            //------------Insert and update database
            string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
            using (var conn = new SqlConnection(connString))
            {
                //try 
                //{
                    var buildSheetID = 0;
                    var selQry = "SELECT MAX(BUILD_SHEET_ID)+1 as SEQ_NUM from AME_BUILD_SHEET";
                    using (var cmd = new SqlCommand(selQry, conn))
                    {
                        conn.Open();
                        IDataReader reader = cmd.ExecuteReader();
                        while (reader.Read()) { buildSheetID = reader.GetInt32(reader.GetOrdinal("SEQ_NUM")); }
                        conn.Close();
                    };

                    var insQry = String.Format("INSERT INTO AME_BUILD_SHEET (INSTALL_DATE, SHIP_BY_DATE, PROFILE_SIZE, CLIENT_ID, NUM_ZONES, COMPANY_NAME, SYSTEM_ID, BRANCH_NAME, USE_REFURB, REPLACING_TYPE, COMM_TYPE, ARRIVE_BY_DATE, PACKAGING_TYPE, RETURN_LABEL, SHIP_TO_TYPE, CUSTOM_ADDR_ATTN, CUSTOM_ADDR_1, CUSTOM_ADDR_2, CUSTOM_ADDR_CITY, CUSTOM_ADDR_STATE, CUSTOM_ADDR_ZIP, ADDITIONAL_INSTRUCTIONS, PRINT_DATE, PRINTED_BY, BUILD_SHEET_ID, REPLACING_NAME) VALUES ('{0}', '{1}', {2}, {3}, {4}, '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}', {24}, '{25}')",
                                            INSTALL_DATE, SHIP_BY_DATE, PROFILE_SIZE, CLIENT_ID, NUM_ZONES, COMPANY_NAME, SYSTEM_ID, BRANCH_NAME, USE_REF, REPLACING_TYPE, COMM_TYPE, ARRIVE_BY_DATE, PACK_TYPE, INC_RETURN, SHIP_TO, CUSTOM_ADDR_ATTN, CUSTOM_ADDR_1, CUSTOM_ADDR_2, CUSTOM_ADDR_CITY, CUSTOM_ADDR_STATE, CUSTOM_ADDR_ZIP, ADDITIONAL_INSTRUCTIONS, PRINT_DATE, PRINTED_BY, buildSheetID, REPLACING_NAME);
                    using (var cmd = new SqlCommand(insQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                //}
                //catch (Exception ex)
                //{
                //    Console.Write(ex);
                //}
            }
            
            //-----------Create build sheet document from the template
            var copyLocation = @"\\AME-FILE-01\Apps\Buildsheets\" + SYSTEM_ID + ".docx";
            System.IO.File.Copy(@"C:\Buildsheet1.docx", copyLocation, true);

            using (WordprocessingDocument doc = WordprocessingDocument.Open(copyLocation, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                Regex rxText = new Regex("PRINT_DATE");
                docText = rxText.Replace(docText, PRINT_DATE.ToString());

                rxText = new Regex("SYSTEM_ID");
                docText = rxText.Replace(docText, SYSTEM_ID.ToString());

                rxText = new Regex("INSTALL_DATE");
                docText = rxText.Replace(docText, INSTALL_DATE);

                if (SHIP_BY_DATE == "0") { SHIP_BY_DATE = "ASAP"; };
                rxText = new Regex("SHIP_BY_DATE");
                docText = rxText.Replace(docText, SHIP_BY_DATE);

                rxText = new Regex("PROFILE_SIZE");
                docText = rxText.Replace(docText, PROFILE_SIZE.ToString());

                rxText = new Regex("PROFILE_NAME");
                docText = rxText.Replace(docText, PROFILE_NAME);

                rxText = new Regex("CLIENT_ID");
                docText = rxText.Replace(docText, CLIENT_ID.ToString());

                rxText = new Regex("NUM_ZONES");
                docText = rxText.Replace(docText, NUM_ZONES.ToString());

                rxText = new Regex("COMPANY_NAME");
                docText = rxText.Replace(docText, COMPANY_NAME);

                rxText = new Regex("BRANCH_NAME");
                docText = rxText.Replace(docText, BRANCH_NAME);

                rxText = new Regex("USE_REFURB");
                docText = rxText.Replace(docText, USE_REF);

                rxText = new Regex("REPLACING_TYPE");
                if (!String.IsNullOrEmpty(REPLACING_NAME))
                {
                    docText = rxText.Replace(docText, REPLACING_NAME);
                }
                else
                {
                    REPLACING_NAME = "NEW";
                    docText = rxText.Replace(docText, REPLACING_NAME);
                }
                Console.WriteLine("Made it to 3");
                rxText = new Regex("COMM_TYPE");
                docText = rxText.Replace(docText, COMM_TYPE);

                rxText = new Regex("ARRIVE_BY_DATE");
                docText = rxText.Replace(docText, ARRIVE_BY_DATE);

                rxText = new Regex("PACK_TYPE");
                docText = rxText.Replace(docText, PACK_TYPE);

                rxText = new Regex("RETURN_LABEL");
                docText = rxText.Replace(docText, INC_RETURN);

                rxText = new Regex("SHIP_TO");
                docText = rxText.Replace(docText, SHIP_TO);

                rxText = new Regex("CUSTOM_ADDR_ATTN");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_ATTN))) { docText = rxText.Replace(docText, CUSTOM_ADDR_ATTN); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_1");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_1))) { docText = rxText.Replace(docText, CUSTOM_ADDR_1); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_2");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_2))) { docText = rxText.Replace(docText, CUSTOM_ADDR_2); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_CITY");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_CITY))) { docText = rxText.Replace(docText, CUSTOM_ADDR_CITY); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_STATE");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_STATE))) { docText = rxText.Replace(docText, CUSTOM_ADDR_STATE); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("CUSTOM_ADDR_ZIP");
                if (!(String.IsNullOrEmpty(CUSTOM_ADDR_ZIP))) { docText = rxText.Replace(docText, CUSTOM_ADDR_ZIP); }
                else { docText = rxText.Replace(docText, " "); };

                rxText = new Regex("ADDITIONAL_INSTRUCTIONS");
                docText = rxText.Replace(docText, ADDITIONAL_INSTRUCTIONS);

                rxText = new Regex("PRINTED_BY");
                docText = rxText.Replace(docText, User.Identity.Name.Substring(4));

                using (StreamWriter sw = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create)))
                {
                    sw.Write(docText);
                }
                doc.Close();
            }

            //-------------Print build sheet
            Microsoft.Office.Interop.Word._Application wordApp = new Microsoft.Office.Interop.Word.Application();
            wordApp.Visible = false;
            Microsoft.Office.Interop.Word._Document wordDoc = new Word.Document();
            wordDoc = wordApp.Documents.Open(copyLocation);
            object oMissing = System.Reflection.Missing.Value;
            wordDoc.Activate();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            wordDoc.PrintOut(oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
            wordDoc.Close();
            wordApp.Quit();
            

            //-------------Send email
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient("exchange.local.rpm.com");
            smtpServer.Credentials = new System.Net.NetworkCredential("SConnolly", "s1987Conno!!y");
            smtpServer.Port = 25;
            mail.From = new MailAddress("BuildSheetPrinter@amemusic.com");
            mail.To.Add("shawn_connolly@amemusic.com");
            mail.CC.Add("kristine_rader-miller@amemusic.com");
            mail.CC.Add("paul_krikorian@amemusic.com");
            mail.To.Add("stephen_rurka@amemusic.com");
            
            mail.Subject = "Replacement build sheet printed.";
            if (CLIENT_ID == 1398 || CLIENT_ID == 903 || CLIENT_ID == 2656 || CLIENT_ID == 2147 || CLIENT_ID == 2466 || CLIENT_ID == 2422 || CLIENT_ID == 2434 || CLIENT_ID == 2464)
            {
                mail.CC.Add("mike_makhool@amemusic.com");
            }
            if (CLIENT_ID == 2147)
            {
                mail.CC.Add("ara_avedissian@amemusic.com");
            }
            if (String.IsNullOrEmpty(REPLACING_NAME))
            {
                mail.Subject = "New build sheet printed.";
            };
            mail.Body = "Client: "+ CLIENT_ID +": " + COMPANY_NAME +", Branch: "+ BRANCH_NAME +", System ID: "+SYSTEM_ID+", Printed By: "+PRINTED_BY+" Replacing: "+ REPLACING_NAME;

            smtpServer.Send(mail);

            return View();
        }

        public ActionResult PrintBatchBuildsheets(int startID, int endID)
        {
            for (var i = startID; i <= endID; i++)
            {
                BuildSheetModel bsm = BuildSheetModelDA.GetModel(i);
                var PRINTED_BY = User.Identity.Name.Substring(4);
                var USE_REF = "No";
                var INC_RETURN = "No";
                DateTime dt = DateTime.Now;
                var PRINT_DATE = dt.ToString("MM/dd/yyyy");
                var REPLACING_TYPE = "NEW";
                var INSTALL_DATE = "ASAP";
                var ADDITIONAL_INSTRUCTIONS = " ";
                var SHIP_BY_DATE = "ASAP";
                var ARRIVE_BY_DATE = "ASAP";
                var REPLACING_NAME = "NULL";
                var PROFILE_SIZE = bsm.PROFILE_SIZE;
                var CLIENT_ID = bsm.CLIENT_ID;
                var NUM_ZONES = bsm.NUM_ZONES;
                var COMPANY_NAME = bsm.COMPANY_NAME;
                var SYSTEM_ID = bsm.SYSTEM_ID;
                var BRANCH_NAME = bsm.BRANCH_NAME;
                var COMM_TYPE = "WAN";
                var PACK_TYPE = "NEW";
                var SHIP_TO = "Installed at Address";
                var CUSTOM_ADDR_ATTN = " ";
                var CUSTOM_ADDR_1 = " ";
                var CUSTOM_ADDR_2 = " ";
                var CUSTOM_ADDR_CITY = " ";
                var CUSTOM_ADDR_STATE = " ";
                var CUSTOM_ADDR_ZIP = " ";

                //------------Insert and update database
                string connString = "server=WEB-APPS-1; database=AmeMaster; uid=sa; password=topdog; MultipleActiveResultSets=true";
                using (var conn = new SqlConnection(connString))
                {
                    //try 
                    //{
                    var buildSheetID = 0;
                    //var selQry = "SELECT SEQ_NUM FROM RPM_SEQUENCING WHERE APPLICATION = 'AMECLIENTMGT' and TYPE = 'BUILD_SHEET_ID'";
                    //using (var cmd = new SqlCommand(selQry, conn))
                    //{
                    //    conn.Open();
                    //    IDataReader reader = cmd.ExecuteReader();
                    //    while (reader.Read()) { buildSheetID = reader.GetInt32(reader.GetOrdinal("SEQ_NUM")); }
                    //    conn.Close();
                    //};

                    var insQry = String.Format("INSERT INTO AME_BUILD_SHEET (INSTALL_DATE, SHIP_BY_DATE, PROFILE_SIZE, CLIENT_ID, NUM_ZONES, COMPANY_NAME, SYSTEM_ID, BRANCH_NAME, USE_REFURB, REPLACING_TYPE, COMM_TYPE, ARRIVE_BY_DATE, PACKAGING_TYPE, RETURN_LABEL, SHIP_TO_TYPE, CUSTOM_ADDR_ATTN, CUSTOM_ADDR_1, CUSTOM_ADDR_2, CUSTOM_ADDR_CITY, CUSTOM_ADDR_STATE, CUSTOM_ADDR_ZIP, ADDITIONAL_INSTRUCTIONS, PRINT_DATE, PRINTED_BY, BUILD_SHEET_ID, REPLACING_NAME) VALUES ('{0}', '{1}', {2}, {3}, {4}, '{5}', {6}, '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', '{20}', '{21}', '{22}', '{23}', {24}, {25})",
                                            INSTALL_DATE, SHIP_BY_DATE, PROFILE_SIZE, CLIENT_ID, NUM_ZONES, COMPANY_NAME, SYSTEM_ID, BRANCH_NAME, USE_REF, REPLACING_TYPE, COMM_TYPE, ARRIVE_BY_DATE, PACK_TYPE, INC_RETURN, SHIP_TO, 
                                            CUSTOM_ADDR_ATTN, CUSTOM_ADDR_1, CUSTOM_ADDR_2, CUSTOM_ADDR_CITY, CUSTOM_ADDR_STATE, CUSTOM_ADDR_ZIP, ADDITIONAL_INSTRUCTIONS, PRINT_DATE, PRINTED_BY, buildSheetID, REPLACING_NAME);
                    using (var cmd = new SqlCommand(insQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }

                 /*   buildSheetID = buildSheetID + 1;
                    var updQry = String.Format("UPDATE RPM_SEQUENCING SET SEQ_NUM = {0} WHERE APPLICATION = 'AMECLIENTMGT' AND TYPE = 'BUILD_SHEET'", buildSheetID);
                    using (var cmd = new SqlCommand(updQry, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    } */
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.Write(ex);
                    //}
                }

                //-----------Create build sheet document from the template
                var copyLocation = @"\\AME-FILE-01\Apps\Buildsheets\" + SYSTEM_ID + ".docx";
                System.IO.File.Copy(@"C:\Buildsheet1.docx", copyLocation, true);

                using (WordprocessingDocument doc = WordprocessingDocument.Open(copyLocation, true))
                {
                    string docText = null;
                    using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
                    {
                        docText = sr.ReadToEnd();
                    }

                    Regex rxText = new Regex("PRINT_DATE");
                    docText = rxText.Replace(docText, PRINT_DATE.ToString());

                    rxText = new Regex("SYSTEM_ID");
                    docText = rxText.Replace(docText, SYSTEM_ID.ToString());

                    rxText = new Regex("INSTALL_DATE");
                    docText = rxText.Replace(docText, INSTALL_DATE);

                    if (SHIP_BY_DATE == "0") { SHIP_BY_DATE = "ASAP"; };
                    rxText = new Regex("SHIP_BY_DATE");
                    docText = rxText.Replace(docText, SHIP_BY_DATE);

                    rxText = new Regex("PROFILE_SIZE");
                    docText = rxText.Replace(docText, PROFILE_SIZE.ToString());

                    rxText = new Regex("CLIENT_ID");
                    docText = rxText.Replace(docText, CLIENT_ID.ToString());

                    rxText = new Regex("NUM_ZONES");
                    docText = rxText.Replace(docText, NUM_ZONES.ToString());

                    rxText = new Regex("COMPANY_NAME");
                    docText = rxText.Replace(docText, COMPANY_NAME);

                    rxText = new Regex("BRANCH_NAME");
                    docText = rxText.Replace(docText, BRANCH_NAME);

                    rxText = new Regex("USE_REFURB");
                    docText = rxText.Replace(docText, USE_REF);

                    rxText = new Regex("REPLACING_TYPE");
                    if (!(String.IsNullOrEmpty(REPLACING_TYPE))) { docText = rxText.Replace(docText, "Model " + REPLACING_TYPE.ToString()); }
                    else { docText = rxText.Replace(docText, "NEW"); };

                    rxText = new Regex("COMM_TYPE");
                    docText = rxText.Replace(docText, COMM_TYPE);

                    rxText = new Regex("ARRIVE_BY_DATE");
                    docText = rxText.Replace(docText, ARRIVE_BY_DATE);

                    rxText = new Regex("PACK_TYPE");
                    docText = rxText.Replace(docText, PACK_TYPE);

                    rxText = new Regex("RETURN_LABEL");
                    docText = rxText.Replace(docText, INC_RETURN);

                    rxText = new Regex("SHIP_TO");
                    docText = rxText.Replace(docText, SHIP_TO);

                    rxText = new Regex("CUSTOM_ADDR_ATTN");
                    if (!(String.IsNullOrEmpty(CUSTOM_ADDR_ATTN))) { docText = rxText.Replace(docText, CUSTOM_ADDR_ATTN); }
                    else { docText = rxText.Replace(docText, " "); };

                    rxText = new Regex("CUSTOM_ADDR_1");
                    if (!(String.IsNullOrEmpty(CUSTOM_ADDR_1))) { docText = rxText.Replace(docText, CUSTOM_ADDR_1); }
                    else { docText = rxText.Replace(docText, " "); };

                    rxText = new Regex("CUSTOM_ADDR_2");
                    if (!(String.IsNullOrEmpty(CUSTOM_ADDR_2))) { docText = rxText.Replace(docText, CUSTOM_ADDR_2); }
                    else { docText = rxText.Replace(docText, " "); };

                    rxText = new Regex("CUSTOM_ADDR_CITY");
                    if (!(String.IsNullOrEmpty(CUSTOM_ADDR_CITY))) { docText = rxText.Replace(docText, CUSTOM_ADDR_CITY); }
                    else { docText = rxText.Replace(docText, " "); };

                    rxText = new Regex("CUSTOM_ADDR_STATE");
                    if (!(String.IsNullOrEmpty(CUSTOM_ADDR_STATE))) { docText = rxText.Replace(docText, CUSTOM_ADDR_STATE); }
                    else { docText = rxText.Replace(docText, " "); };

                    rxText = new Regex("CUSTOM_ADDR_ZIP");
                    if (!(String.IsNullOrEmpty(CUSTOM_ADDR_ZIP))) { docText = rxText.Replace(docText, CUSTOM_ADDR_ZIP); }
                    else { docText = rxText.Replace(docText, " "); };

                    rxText = new Regex("ADDITIONAL_INSTRUCTIONS");
                    docText = rxText.Replace(docText, ADDITIONAL_INSTRUCTIONS);

                    rxText = new Regex("PRINTED_BY");
                    docText = rxText.Replace(docText, User.Identity.Name.Substring(4));

                    using (StreamWriter sw = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create)))
                    {
                        sw.Write(docText);
                    }
                    doc.Close();
                }

                //-------------Print build sheet
                Microsoft.Office.Interop.Word._Application wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = false;
                Microsoft.Office.Interop.Word._Document wordDoc = new Word.Document();
                wordDoc = wordApp.Documents.Open(copyLocation);
                object oMissing = System.Reflection.Missing.Value;
                wordDoc.Activate();
                Thread.Sleep(TimeSpan.FromSeconds(1));
                wordDoc.PrintOut(oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                wordDoc.Close();
                wordApp.Quit();
                Thread.Sleep(1000);
            }

            return View();
        }
    }
}
