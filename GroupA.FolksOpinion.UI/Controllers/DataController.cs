using GroupA.FolksOpinion.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroupA.FolksOpinion.UI.Controllers
{
    public class DataController : Controller
    {
        // GET: Data/Opinion/<UrlEncodedSearchTerm>
        public JsonResult Opinion(string q)
        {
            var result = Json("", JsonRequestBehavior.AllowGet);
            var subject = "";

            // Decode query string form URL encoding.
            // TODO: Decode query string form URL encoding.
            subject = q;

            // Get opinions for subject.
            var opinions = new WorldOpinions(subject);

            // Create Json from opinions data.
            result = Json(JsonConvert.SerializeObject(opinions), JsonRequestBehavior.AllowGet);

            return result;
        }
    }
}