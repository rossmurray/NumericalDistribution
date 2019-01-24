using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumericalDistribution
{
    public class Distribution<T>
    {
        public IList<DistributionGroup<T>> Buckets { get; set; }
        public int MaxCount { get; set; }

        public Distribution(IList<DistributionGroup<T>> buckets)
        {
            var max = buckets.Max(x => x.Collection.Length);
            this.MaxCount = max;
            this.Buckets = buckets;
        }
    }
}
