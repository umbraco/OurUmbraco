using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IKarma
    {
        int ProjectId { get; set; }
        int Points { get; set; }
    }
}
