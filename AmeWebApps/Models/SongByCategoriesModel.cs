using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AmeWebApps.Models;

namespace AmeWebApps.Models
{
    public class SongByCategoriesModel
    {
        public int MUSIC_ID { get; set; }
        public string TITLE { get; set; }
        public string ARTIST { get; set; }
        public string DATE_MODIFIED { get; set; }
        public List<String> CATEGORIES { get; set; }
        public SongByCategoriesModel()
        {
            CATEGORIES = new List<string>();
        }
    }
}