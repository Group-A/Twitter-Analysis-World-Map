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
        // GET: Data/Opinion?q=<query>
        public JsonResult Opinion(string q)
        {
            var subject = q;
            var opinionator = new Opinionator(subject);
            var worldOpinion = opinionator.WorldSubjectOpinion;

            return Json(worldOpinion, JsonRequestBehavior.AllowGet);
        }
    }
}