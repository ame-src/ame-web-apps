using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AmeWebApps.Models;

namespace AmeWebApps.Controllers
{
    public class NewCategoriesController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NewCategories(string selcat = "AMEV2")
        {
            List<AME_CATEGORY_TREE> categoryList = new List<AME_CATEGORY_TREE>();
            List<RPM_MUSIC_CATEGORY> musicCategories = new List<RPM_MUSIC_CATEGORY>();
            List<RPM_MUSIC> musicList = new List<RPM_MUSIC>();

            NewCatModels db = new NewCatModels();
            categoryList = db.AME_CATEGORY_TREE.ToList();
            musicCategories = db.RPM_MUSIC_CATEGORY.ToList();
            musicList = db.RPM_MUSIC.ToList();

            var topCategory = categoryList.Where(x => x.PARENT_CATEGORY_ID == null).FirstOrDefault();
            var categoryContent = musicCategories.Where(x => x.CATEGORY_ID == selcat);

            var categoryMusic = from item1 in categoryContent
                                join item2 in musicList on item1.MUSIC_ID equals item2.MUSIC_ID
                                select item2;
            

            SetChildren(topCategory, categoryList);

            ViewData["selcat"] = selcat;
            ViewData["totalNew"] = categoryContent.Count();
            ViewData["totalSongs"] = musicCategories.Count;
            ViewData["catCont"] = categoryMusic;

            return View(topCategory);
        }

        private void SetChildren(AME_CATEGORY_TREE model, List<AME_CATEGORY_TREE> categoryList)
        {
            var children = categoryList.Where(x => x.PARENT_CATEGORY_ID == model.CATEGORY_ID).ToList();
            
            if (children.Count > 0)
            {
                foreach (var child in children)
                {
                    SetChildren(child, categoryList);
                    model.Children.Add(child);
                }
            }
        }

        public ActionResult NewCategoriesPerSong(string sArtist, string sTitle)
        {
            NewCatModels db = new NewCatModels();
            List<RPM_MUSIC> rpmMusicList = db.RPM_MUSIC.ToList();
            List<RPM_MUSIC_CATEGORY> rpmMusicCatList = db.RPM_MUSIC_CATEGORY.ToList();
            List<AME_CATEGORY_TREE> ameCatTreeList = db.AME_CATEGORY_TREE.Where(x=>x.PARENT_CATEGORY_ID != "AMEV1").ToList();
            List<SongByCategoriesModel> songList = new List<SongByCategoriesModel>();

            var songCategories = from cat in ameCatTreeList
                                 join rCat in rpmMusicCatList on cat.CATEGORY_ID equals rCat.CATEGORY_ID
                                 join rMusic in rpmMusicList on rCat.MUSIC_ID equals rMusic.MUSIC_ID
                                 select new { MUSIC_ID = rMusic.MUSIC_ID, ARTIST = rMusic.DISPLAY_ARTIST, TITLE = rMusic.TITLE, CATEGORY = cat.DESCRIPTION, DATE_MODIFIED = rCat.DATE_MODIFIED };

            foreach (var row in songCategories)
            {
                if (songList.Exists(x => x.MUSIC_ID == row.MUSIC_ID))
                {
                    var songAlready = songList.Find(x => x.MUSIC_ID == row.MUSIC_ID);
                    songAlready.CATEGORIES.Add(row.CATEGORY);
                }
                else
                {
                    SongByCategoriesModel newSong = new SongByCategoriesModel();
                    newSong.ARTIST = row.ARTIST;
                    newSong.MUSIC_ID = row.MUSIC_ID;
                    newSong.TITLE = row.TITLE;
                    newSong.DATE_MODIFIED = Convert.ToDateTime(row.DATE_MODIFIED).ToString("yyyy/MM/dd");
                    newSong.CATEGORIES.Add(row.CATEGORY);

                    songList.Add(newSong);
                }
            }

            songList = songList.OrderByDescending(x => x.DATE_MODIFIED).ToList();

            return View(songList);
        }
    }
}
