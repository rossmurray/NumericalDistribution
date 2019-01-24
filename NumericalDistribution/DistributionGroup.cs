using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumericalDistribution
{
    public class DistributionGroup<T>
    {
        public double MinRange { get; set; }
        public double MaxRange { get; set; }
        public T[] Collection { get; set; }

        public DistributionGroup(double minRange, double maxRange, T[] items)
        {
            this.Collection = items;
            this.MinRange = minRange;
            this.MaxRange = maxRange;
        }
    }
}
