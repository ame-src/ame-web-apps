using AmeWebApps.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Office = Microsoft.Office.Core;
using Word = Microsoft.Office.Interop.Word;

namespace AmeWebApps.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return View();
        }


        public ActionResult UpdatedCharts()
        {
            string connString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=\\\\ame-file-01\\Apps\\Music\\Music.mdb";
            List<ChartEntry> ChartEntries = new List<ChartEntry>();
            var qrySQL = "select * from Music where [KEY 4] = 'CHRT' and [Key 5] is not null order by [Key 5]";

            using (OleDbConnection con = new OleDbConnection(connString))
            {
                using (OleDbCommand cmd = new OleDbCommand(qrySQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    con.Open();

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartEntry ce = new ChartEntry();
                            ce.artist = reader["ARTIST"].ToString();
                            ce.title = reader["TITLE"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("LABEL")))
                                ce.label = reader["LABEL"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("FIRST")))
                                ce.first = reader["FIRST"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("KEY 5")))
                            {
                                ce.rank = reader["KEY 5"].ToString();
                                ce.chart = "AC";
                                ce.disc = reader["TH DISC"].ToString() + " - " + reader["TH TRK#"].ToString();
                            }
                            ChartEntries.Add(ce);
                        }
                    }
                }

                qrySQL = "select * from Music where [KEY 4] = 'CHRT' and [Key 6] is not null order by [Key 6]";
                using (OleDbCommand cmd = new OleDbCommand(qrySQL, con))
                {
                    cmd.CommandType = CommandType.Text;

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartEntry ce = new ChartEntry();
                            ce.artist = reader["ARTIST"].ToString();
                            ce.title = reader["TITLE"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("LABEL")))
                                ce.label = reader["LABEL"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("FIRST")))
                                ce.first = reader["FIRST"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("KEY 6")))
                            {
                                ce.rank = reader["KEY 6"].ToString();
                                ce.chart = "CHR";
                                ce.disc = reader["TH DISC"].ToString() + " - " + reader["TH TRK#"].ToString();
                            }
                            ChartEntries.Add(ce);
                        }
                    }
                }

                qrySQL = "select * from Music where [KEY 4] = 'CHRT' and [Key 7] is not null order by [Key 7]";
                using (OleDbCommand cmd = new OleDbCommand(qrySQL, con))
                {
                    cmd.CommandType = CommandType.Text;

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartEntry ce = new ChartEntry();
                            ce.artist = reader["ARTIST"].ToString();
                            ce.title = reader["TITLE"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("LABEL")))
                                ce.label = reader["LABEL"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("FIRST")))
                                ce.first = reader["FIRST"].ToString();

                            ce.rank = reader["KEY 7"].ToString();
                            ce.chart = "CTRY";
                            ce.disc = reader["TH DISC"].ToString() + " - " + reader["TH TRK#"].ToString();

                            ChartEntries.Add(ce);
                        }
                    }
                }

                qrySQL = "select * from Music where [KEY 4] = 'CHRT' and [Key 8] is not null order by [Key 8]";
                using (OleDbCommand cmd = new OleDbCommand(qrySQL, con))
                {
                    cmd.CommandType = CommandType.Text;

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartEntry ce = new ChartEntry();
                            ce.artist = reader["ARTIST"].ToString();
                            ce.title = reader["TITLE"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("LABEL")))
                                ce.label = reader["LABEL"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("FIRST")))
                                ce.first = reader["FIRST"].ToString();

                            ce.rank = reader["KEY 8"].ToString();
                            ce.chart = "URB";
                            ce.disc = reader["C DISC"].ToString() + " - " + reader["C TRK#"].ToString();

                            ChartEntries.Add(ce);
                        }
                    }
                }

                qrySQL = "select * from Music where [KEY 4] = 'CHRT' and [Key 9] is not null order by [Key 9]";
                using (OleDbCommand cmd = new OleDbCommand(qrySQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartEntry ce = new ChartEntry();
                            ce.artist = reader["ARTIST"].ToString();
                            ce.title = reader["TITLE"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("LABEL")))
                                ce.label = reader["LABEL"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("FIRST")))
                                ce.first = reader["FIRST"].ToString();

                            ce.rank = reader["KEY 9"].ToString();
                            ce.chart = "RK";
                            ce.disc = reader["C DISC"].ToString() + " - " + reader["C TRK#"].ToString();

                            ChartEntries.Add(ce);
                        }
                    }
                }

                qrySQL = "select * from Music where [KEY 4] = 'CHRT' and [Key 10] is not null order by [Key 10]";
                using (OleDbCommand cmd = new OleDbCommand(qrySQL, con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ChartEntry ce = new ChartEntry();
                            ce.artist = reader["ARTIST"].ToString();
                            ce.title = reader["TITLE"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("LABEL")))
                                ce.label = reader["LABEL"].ToString();

                            if (!reader.IsDBNull(reader.GetOrdinal("FIRST")))
                                ce.first = reader["FIRST"].ToString();

                            ce.rank = reader["KEY 10"].ToString();
                            ce.chart = "ALT";
                            ce.disc = reader["C DISC"].ToString() + " - " + reader["C TRK#"].ToString();

                            ChartEntries.Add(ce);
                        }
                    }
                }

            }
            return View(ChartEntries);
        }
    }
}
