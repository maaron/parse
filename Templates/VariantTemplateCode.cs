using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Templates
{
    public partial class VariantTemplate
    {
        public int Count { get; private set; }

        public IEnumerable<int> Indeces
        {
            get { return Enumerable.Range(1, Count); }
        }

        public string TypeList
        {
            get
            {
                return String.Join(",",
                    Indeces.Select(i => "T" + i));
            }
        }

        public string VisitFuncParams
        {
            get
            {
                return String.Join(",",
                    Indeces.Select(i => String.Format("System.Func<T{0}, T> f{0}", i)));
            }
        }

        public string VisitActionParams
        {
            get
            {
                return String.Join(",",
                    Indeces.Select(i => String.Format("System.Action<T{0}> f{0}", i)));
            }
        }

        public VariantTemplate(int count)
        {
            Count = count;
        }
    }
}
