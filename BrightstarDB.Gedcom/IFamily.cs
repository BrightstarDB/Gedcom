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
        IIndividual Husband { get; set; }
        IIndividual Wife { get; set; }
        ICollection<IIndividual> Children { get; set; }
    }
}
