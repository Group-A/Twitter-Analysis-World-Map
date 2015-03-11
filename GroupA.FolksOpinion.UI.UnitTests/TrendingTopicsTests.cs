using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GroupA.FolksOpinion.UI.Models;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace GroupA.FolksOpinion.UI.UnitTests
{
    [TestClass]
    public class TrendingTopicsTests
    {
        [TestClass]
        public class TrendingTopicsConstructor
        {
            [TestMethod]
            public void TrendingTopicsConstructorDefault()
            {
                new TrendingTopics();
            }
        }

        [TestClass]
        public class CurrentTrendsProperty
        {
            [TestMethod]
            public void CurrentTrendsPropertyGet()
            {
                var t = new TrendingTopics();
                var c = t.CurrentTrends;
            }
        }
    }
}
