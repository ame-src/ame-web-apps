using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AmeWebApps.Controllers
{
    public class VideoReceiversController : Controller
    {
        //
        // GET: /VideoReceivers/
        string connString = "server=AME-DATA-02; database=AmeMaster; uid=sa; password=topdog;";


        public ActionResult Index()
        {
            List<string[]> computerNames = new List<string[]>();
            List<string[]> computerStatus = new List<string[]>();
            var count = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string sqlQry = "select * from RPM_CLIENT_SYSTEM where VIDEO_CAPS > 0";
                using (SqlCommand cmd = new SqlCommand(sqlQry, conn))
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        
                        while (reader.Read())
                        {

                            if (!reader.IsDBNull(reader.GetOrdinal("computer_name")))
                            {
                                string[] recInfo = new string[3];
                                string Name = reader.GetString(reader.GetOrdinal("computer_name"));
                                string Client = reader.GetInt32(reader.GetOrdinal("client_id")).ToString();
                                string Profile = reader.GetInt32(reader.GetOrdinal("profile_id")).ToString();
                                recInfo[0] = Name;
                                recInfo[1] = Client;
                                recInfo[2] = Profile;
                                computerNames.Add(recInfo);
                                count++;
                            }
                        }
                    }
                }
            }
            for (var i = 0; i < count; i++)
            {
                var filepath = @"\\Amecomm\ame\Logs\";
                var fileName = @"*video-player.log.1.gz";
                string fullPath;
                string[] computerStat = new string[5];
                computerStat[0] = computerNames[i][0];
                computerStat[1] = computerNames[i][1];
                computerStat[2] = computerNames[i][2];
                computerStat[3] = "No logs";
                computerStat[4] = "No logs";

                fullPath = filepath + computerNames[i][0] + "\\";
                List<string> allMatches = Directory.EnumerateFiles(fullPath, fileName).ToList();

                if (allMatches.Count > 0)
                {
                    var lastLog = allMatches.Last();
                    FileStream dest2 = System.IO.File.OpenRead(lastLog);
                    GZipStream decompFile = new GZipStream(dest2, CompressionMode.Decompress);
                    if(!Directory.Exists(@"C:\tmp\")){Directory.CreateDirectory(@"C:\tmp\");};
                    FileStream extractedFile = System.IO.File.Create(@"C:\tmp\tmpVPlog"+i.ToString());
                    int mySecondByte = decompFile.ReadByte();

                    while (mySecondByte != -1)
                    {
                        extractedFile.WriteByte((byte)mySecondByte);
                        mySecondByte = decompFile.ReadByte();
                    }

                    extractedFile.Close();

                    string text = System.IO.File.ReadAllText(@"C:\tmp\tmpVPlog" + i.ToString());
                    string HDMI1 = "";
                    string HDMI2 = "";
                    int lastInstanceDRM = text.LastIndexOf("DRM CHANGE");

                    if (text.LastIndexOf("card0-HDMI-A-1 seen as connected") > lastInstanceDRM)
                    {
                        HDMI1 = "connected";
                    }
                    if (text.LastIndexOf("card0-HDMI-A-2 seen as connected") > lastInstanceDRM)
                    {
                        HDMI2 = "connected";
                    }
                    if (text.LastIndexOf("card0-HDMI-A-3 seen as connected") > lastInstanceDRM)
                    {
                        HDMI2 = "connected";
                    }
                    computerStat[3] = HDMI1;
                    computerStat[4] = HDMI2;
                }
                computerStatus.Add(computerStat);
            }

            return View(computerStatus);
        }
    }
}
