using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OurUmbraco.MarketPlace.Interfaces;

namespace OurUmbraco.MarketPlace.Providers
{
    class ProjectTag : IProjectTag

    {
        protected int _id;
        protected string _text;
        protected int _count;
        protected int _liveCount;

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public int LiveCount
        {
            get { return _liveCount; }
            set { _liveCount = value; }
        }

        public bool Equals(IProjectTag other)
        {
            return this.Id == other.Id;
        }
    }
}
