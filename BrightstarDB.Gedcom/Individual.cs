using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrightstarDB.Gedcom
{
    public partial class Individual
    {
        public IEnumerable<IFamily> SpouseFamilies()
        {
            var ctx =  this.Context as GedComContext;
            if (this.Sex.Equals("M"))
            {
                return ctx.Families.Where(f => f.Husband.Id.Equals(this.Id)).ToList();
            }
            else
            {
                return ctx.Families.Where(f => f.Wife.Id.Equals(this.Id)).ToList();
            }
        }
    }
}
