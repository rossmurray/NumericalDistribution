using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumericalDistribution
{
    public static class StaticExtensions
    {
        private static DistributionRenderer renderer = new DistributionRenderer();

        public static Bitmap Render<T>(this Distribution<T> distribution, int width = 900, int height = 400)
        {
            return renderer.RenderDistribution<T>(distribution, width, height);
        }

        public static Distribution<T> ToDistribution<T>(this T[] source, Func<T, double> keySelector, double minRange, double maxRange, int bins = 10)
        {
            var keys = source.Select(x => keySelector(x)).ToArray();
            var result = ToDistributionImpl(source, keys, minRange, maxRange, bins);
            return result;
        }

        public static Distribution<T> ToDistribution<T>(this T[] source, Func<T, double> keySelector, int bins = 10)
        {
            var keys = source.Select(x => keySelector(x)).ToArray();
            var min = keys.Min();
            var max = keys.Max();
            var result = ToDistributionImpl(source, keys, min, max, bins);
            return result;
        }

        private static Distribution<T> ToDistributionImpl<T>(this T[] source, double[] keys, double minRange, double maxRange, int bins)
        {
            if (maxRange <= minRange) throw new ArgumentException("out of range range");
            var delta = maxRange - minRange;
            var binSize = delta / bins;
            var ordered = keys.Select((x, i) => new { x, i })
                .OrderBy(x => x.x)
                .Select(x => (key: x.x, value: source[x.i]))
                .Where(x => x.key >= minRange && x.key < maxRange);
            var binMaximums = Enumerable.Range(1, bins).Select(x => minRange + x * binSize);
            var lastMax = minRange;
            var buckets = new List<DistributionGroup<T>>();
            foreach (var binMax in binMaximums)
            {
                var items = ordered.TakeWhile(x => x.key < binMax)
                    .Select(x => x.value).ToArray();
                ordered = ordered.Skip(items.Length);
                var group = new DistributionGroup<T>(lastMax, binMax, items);
                buckets.Add(group);
                lastMax = binMax;
            }
            var upperExcluded = ordered.TakeWhile(x => x.key == maxRange).Select(x => x.value).ToArray();
            var lastBucket = buckets.Last();
            if (upperExcluded.Any())
            {
                lastBucket.Collection = lastBucket.Collection.Concat(upperExcluded).ToArray();
            }
            var result = new Distribution<T>(buckets.ToArray());
            return result;
        }

    }
}
