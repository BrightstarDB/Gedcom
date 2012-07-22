using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrightstarDB.EntityFramework;

namespace BrightstarDB.Gedcom
{
    [Entity]
    public interface IBirthEvent
    {
        string Place { get; set; }
        string Date { get; set; }
    }
}
