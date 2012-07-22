using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightstarDB.EntityFramework;

namespace BrightstarDB.Gedcom
{
    [Entity]
    public interface IFamily
    {
        IMarriageEvent MarriageEvent { get; set; }
        IIndivdual Husband { get; set; }
        IIndivdual Wife { get; set; }
        ICollection<IIndivdual> Children { get; set; }
    }
}
