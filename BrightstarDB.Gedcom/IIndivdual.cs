using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightstarDB.EntityFramework;

namespace BrightstarDB.Gedcom
{
    [Entity]
    public interface IIndivdual
    {
        string Name { get; set; }
        string Sex { get; set; }
        IBirthEvent BirthEvent { get; set; }
        IDeathEvent DeathEvent { get; set; }
    }
}
