using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BrightstarDB.Gedcom
{
    public class GedcomImporter
    {
        public static void Import(string fileName, string connectionString)
        {
            // parse the gedcom
            var gedcomParser = new GedcomReader(fileName);
            var d = XDocument.Load(gedcomParser);

            // create a context and create the entities.
            var ctx = new GedComContext(connectionString);

            var lookups = new Dictionary<string, object>();

            foreach (var elem in d.Root.Elements())
            {
                if (elem.Name.LocalName.Equals("INDI"))
                {
                    var id = TryGetValue(elem, "id");
                    var individual = ctx.Indivduals.Create();
                    if (id != null) lookups[id] = individual;

                    foreach (var elem1 in elem.Elements())
                    {
                        if (elem1.Name.LocalName.Equals("NAME"))
                        {
                            var name = TryGetValue(elem1, "value");
                            if (name != null) individual.Name = name;
                        }

                        if (elem1.Name.LocalName.Equals("SEX"))
                        {
                            var sex = TryGetValue(elem1, "value");
                            if (sex != null) individual.Sex = sex;
                        }

                        if (elem1.Name.LocalName.Equals("BIRT"))
                        {
                            var birth = ctx.BirthEvents.Create();
                            individual.BirthEvent = birth;
                            foreach (var elem2 in elem1.Elements())
                            {
                                if (elem2.Name.LocalName.Equals("PLAC"))
                                {
                                    var place = TryGetValue(elem2, "value");
                                    if (place != null) birth.Place = place;
                                }

                                if (elem2.Name.LocalName.Equals("DATE"))
                                {
                                    var date = TryGetValue(elem2, "value");
                                    if (date != null) birth.Date = date;
                                }
                            }
                        }

                        if (elem1.Name.LocalName.Equals("DEAT"))
                        {
                            var death = ctx.DeathEvents.Create();
                            individual.DeathEvent = death;
                            foreach (var elem2 in elem1.Elements())
                            {
                                if (elem2.Name.LocalName.Equals("PLAC"))
                                {
                                    var place = TryGetValue(elem2, "value");
                                    if (place != null) death.Place = place;
                                }

                                if (elem2.Name.LocalName.Equals("DATE"))
                                {
                                    var date = TryGetValue(elem2, "value");
                                    if (date != null) death.Date = date;
                                }
                            }
                        }
                    }

                }

                if (elem.Name.LocalName.Equals("FAM"))
                {
                    var id = TryGetValue(elem, "id");
                    var family = ctx.Families.Create();
                    if (id != null) lookups[id] = family;

                    foreach (var elem1 in elem.Elements())
                    {
                        if (elem1.Name.LocalName.Equals("MARR"))
                        {
                            var marriage = ctx.MarriageEvents.Create();
                            family.MarriageEvent = marriage;
                            foreach (var elem2 in elem1.Elements())
                            {
                                if (elem2.Name.LocalName.Equals("PLAC"))
                                {
                                    var place = TryGetValue(elem2, "value");
                                    if (place != null) marriage.Place = place;
                                }

                                if (elem2.Name.LocalName.Equals("DATE"))
                                {
                                    var date = TryGetValue(elem2, "value");
                                    if (date != null) marriage.Date = date;
                                }
                            }
                        }

                        if (elem1.Name.LocalName.Equals("HUSB"))
                        {
                            var idref = TryGetValue(elem1, "idref");
                            if (idref != null)
                            {
                                var husb = lookups[idref] as IIndivdual;
                                if (husb != null) family.Husband = husb;
                            }
                        }

                        if (elem1.Name.LocalName.Equals("WIFE"))
                        {
                            var idref = TryGetValue(elem1, "idref");
                            if (idref != null)
                            {
                                var wife = lookups[idref] as IIndivdual;
                                if (wife != null) family.Wife = wife;
                            }
                        }

                        if (elem1.Name.LocalName.Equals("CHIL"))
                        {
                            var idref = TryGetValue(elem1, "idref");
                            if (idref != null)
                            {
                                var child = lookups[idref] as IIndivdual;
                                if (child != null) family.Children.Add(child);
                            }
                        }
                    }

                }
            }
            ctx.SaveChanges();
        }

        private static string TryGetValue(XElement elem, string attributeName)
        {
            if (elem.Attribute(attributeName) != null)
            {
                return elem.Attribute(attributeName).Value;
            }
            return null;
        }
    }
}
