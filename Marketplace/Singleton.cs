using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uProject
{
    public class Singleton<T> where T : class, new()
    {
        Singleton() { }

        private static readonly Lazy<T> instance = new Lazy<T>(() => new T());

        public static T UniqueInstance
        {
            get { return instance.Value; }
        }
    }
}
