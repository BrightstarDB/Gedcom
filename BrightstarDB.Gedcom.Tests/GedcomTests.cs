using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrightstarDB.Gedcom.Tests
{
    [TestClass]
    public class GedcomTests
    {
        private const string DataFolder = "C:\\work\\github\\Gedcom\\Data\\";

        [TestMethod]
        public void TestParser()
        {
            var gedcomParser = new GedcomReader(DataFolder + "simple.ged");
            var d = XDocument.Load(gedcomParser);
            var xml = d.ToString();
            Console.WriteLine(xml);
        }

        [TestMethod]
        public void TestImporter()
        {
            var storeId = Guid.NewGuid().ToString();
            GedcomImporter.Import(DataFolder + "simple.ged", "type=embedded;storesdirectory=c:\\brightstar;storename=" + storeId);

            var ctx = new GedComContext("type=embedded;storesdirectory=c:\\brightstar;storename=" + storeId);

            Assert.AreEqual(3, ctx.Individuals.Count());
            Assert.AreEqual(1, ctx.Families.Count());

            var family = ctx.Families.ToList()[0];

            Assert.IsNotNull(family.Husband);
            Assert.AreEqual("1 JAN 1899", family.Husband.BirthEvent.Date);
            Assert.AreEqual("M", family.Husband.Sex);
            Assert.AreEqual(family, family.Husband.SpouseFamilies().ToList()[0]);

            Assert.IsNotNull(family.Wife);
            Assert.AreEqual("1 JAN 1899", family.Wife.BirthEvent.Date);
            Assert.AreEqual("F", family.Wife.Sex);
            
            Assert.AreEqual(1, family.Children.Count());

            Assert.AreEqual("marriage place", family.MarriageEvent.Place);
        }

    }
}
