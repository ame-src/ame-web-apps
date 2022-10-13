using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AmeWebApps.Models;
using System.Data.SqlClient;
using AmeWebApps.Models.TicketNotesDSTableAdapters;
using AmeWebApps.Models.TicketStepsDSTableAdapters;

namespace AmeWebApps.Controllers
{
    public class TicketsController : Controller
    {
        private AmeTicketsEntities1 db = new AmeTicketsEntities1();

        //
        // GET: /Tickets/

        public ActionResult Index()
        {
            return View(db.AME_TICKET.OrderByDescending(x=> x.DATE_OPENED).ToList());
        }

        public ActionResult FindReceiver()
        {
            return View();
        }


        //
        // GET: /Tickets/Details/5

        public ActionResult Details(int id = 0)
        {
            AME_TICKET ame_ticket = db.AME_TICKET.Find(id);
            if (ame_ticket == null)
            {
                return HttpNotFound();
            }
            return View(ame_ticket);
        }

        //
        // GET: /Tickets/Create

        public ActionResult Create(string COMPUTER_NAME)
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

        //
        // POST: /Tickets/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AME_TICKET ame_ticket)
        {
            if (ModelState.IsValid)
            {
                db.AME_TICKET.Add(ame_ticket);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ame_ticket);
        }

        //
        // GET: /Tickets/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AME_TICKET ame_ticket = db.AME_TICKET.Find(id);

            TicketNotesDS.AME_TICKET_NOTEDataTable TicketNoteDS = new TicketNotesDS.AME_TICKET_NOTEDataTable();
            AME_TICKET_NOTETableAdapter TicketNoteTA = new AME_TICKET_NOTETableAdapter();
            TicketNoteTA.Fill(TicketNoteDS);

            TicketStepsDS.AME_TICKET_STEPSDataTable TicketStepsDS = new TicketStepsDS.AME_TICKET_STEPSDataTable();
            AME_TICKET_STEPSTableAdapter TicketStepsTA = new AME_TICKET_STEPSTableAdapter();
            TicketStepsTA.Fill(TicketStepsDS);

            if (ame_ticket == null)
            {
                return HttpNotFound();
            }

            ViewBag.tNotes = TicketNoteDS.Where(x => x.TICKET_NOTE_ID == ame_ticket.TICKET_ID).OrderByDescending(x=>x.DATE_MODIFIED);
            ViewBag.tSteps = TicketStepsDS.Where(x => x.TICKET_ID == ame_ticket.TICKET_ID).FirstOrDefault();

            return View(ame_ticket);
        }

        //
        // POST: /Tickets/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AME_TICKET ame_ticket, string oNotes, bool cb_exp = false, bool cb_pwr = false, bool cb_cdtray = false, bool cb_cables = false, bool cb_amp = false)
        {
            TicketNotesDS.AME_TICKET_NOTEDataTable TicketNoteDS = new TicketNotesDS.AME_TICKET_NOTEDataTable();
            AME_TICKET_NOTETableAdapter TicketNoteTA = new AME_TICKET_NOTETableAdapter();
            TicketNoteTA.Fill(TicketNoteDS);

            TicketStepsDS.AME_TICKET_STEPSDataTable TicketStepsDS = new TicketStepsDS.AME_TICKET_STEPSDataTable();
            AME_TICKET_STEPSTableAdapter TicketStepsTA = new AME_TICKET_STEPSTableAdapter();
            TicketStepsTA.Fill(TicketStepsDS);
            var tSteps = TicketStepsDS.Where(x => x.TICKET_ID == ame_ticket.TICKET_ID).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (tSteps == null)
                {
                    var newTSteps = TicketStepsDS.NewAME_TICKET_STEPSRow();
                    newTSteps.TICKET_ID = ame_ticket.TICKET_ID;
                    newTSteps.chk_expiration = cb_exp;
                    newTSteps.chk_power = cb_pwr;
                    newTSteps.chk_cd_tray = cb_cdtray;
                    newTSteps.chk_cables = cb_cables;
                    newTSteps.chk_amp = cb_amp;
                    TicketStepsDS.AddAME_TICKET_STEPSRow(newTSteps);
                }
                else
                {
                    tSteps.chk_amp = cb_amp;
                    tSteps.chk_cables = cb_cables;
                    tSteps.chk_cd_tray = cb_cdtray;
                    tSteps.chk_expiration = cb_exp;
                    tSteps.chk_power = cb_pwr;
                    TicketStepsTA.Update(tSteps);
                }
            }
            TicketStepsTA.Update(TicketStepsDS);


            if (!(String.IsNullOrEmpty(oNotes)))
            {
                var newTNote = TicketNoteDS.NewAME_TICKET_NOTERow();
                newTNote.TICKET_NOTE_ID = ame_ticket.TICKET_ID;
                newTNote.TICKET_NOTE = oNotes;
                newTNote.DATE_MODIFIED = DateTime.Now;
                newTNote.ADDED_BY = User.Identity.Name.Substring(4);
                TicketNoteDS.AddAME_TICKET_NOTERow(newTNote);
            }

            TicketNoteTA.Update(TicketNoteDS);

            if (ModelState.IsValid)
            {
                db.Entry(ame_ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            

            return View(ame_ticket);
        }

        //
        // GET: /Tickets/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AME_TICKET ame_ticket = db.AME_TICKET.Find(id);
            if (ame_ticket == null)
            {
                return HttpNotFound();
            }
            return View(ame_ticket);
        }

        //
        // POST: /Tickets/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AME_TICKET ame_ticket = db.AME_TICKET.Find(id);
            db.AME_TICKET.Remove(ame_ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}