using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marketplace.Interfaces;

namespace Marketplace.BusinessLogic
{
    public class Eligible : IEligible
    {

        public string Message
        {
            get;
            set;
        }

        public bool IsEligible
        {
            get;
            set;
        }
    }
}
