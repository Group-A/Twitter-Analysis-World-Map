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
            var result = "";
            
            var opinionator = new Opinionator(subject);
            var worldOpinion = opinionator.WorldSubjectOpinion;

            result = JsonConvert.SerializeObject(worldOpinion);

            return Json(result, JsonRequestBehavior.AllowGet); ;
        }
    }
}