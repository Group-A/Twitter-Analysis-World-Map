﻿using GroupA.FolksOpinion.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GroupA.FolksOpinion.UI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var trendingTopics = new TrendingTopics();
            return View(trendingTopics);
        }
    }
}