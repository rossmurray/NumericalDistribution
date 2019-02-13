using Colourful;
using Colourful.Conversion;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumericalDistribution
{
    public class DistributionRenderer
    {
        public Bitmap RenderDistribution<T>(Distribution<T> distribution, int width = 900, int height = 400)
        {
            var total = distribution.Buckets.Sum(x => x.Collection.Length);
            if (width < 400 || height < 200) throw new ArgumentException("resolution");
            var bitmapRect = new Rectangle(0, 0, width, height);
            var bitmap = new Bitmap(width, height);
            FillArea(bitmap, bitmapRect, Color.Black);
            var leftBoxWidth = 60;
            var bottomBoxHeight = 50;
            var border = 1;
            var topPadding = 15;
            var rightPadding = 40;
            var leftBox = new Rectangle(border, topPadding + border, leftBoxWidth, height - bottomBoxHeight - border * 3 - topPadding);
            var bottomBox = new Rectangle(border, height - bottomBoxHeight - border, width - border * 2, bottomBoxHeight);
            var barBox = new Rectangle(leftBox.Right + border, topPadding + border, width - leftBox.Width - border * 3 - rightPadding, height - bottomBox.Height - border * 3 - topPadding);

            var topPaddingLeftBox = new Rectangle(border, border, leftBox.Right - border, topPadding);
            var topPaddingBarBox = new Rectangle(barBox.Left, border, barBox.Width, topPadding);
            var topPaddingRightSide = new Rectangle(width - rightPadding - border, border, rightPadding, bottomBox.Top - border * 2);
            FillArea(bitmap, topPaddingLeftBox, Color.White);
            FillArea(bitmap, topPaddingBarBox, Color.White);
            FillArea(bitmap, topPaddingRightSide, Color.White);
            FillOutBars(bitmap, distribution, barBox);
            FillOutYAxis(bitmap, distribution.MaxCount, total, leftBox, false);
            FillOutXAxis(bitmap, distribution, bottomBox, barBox.Left, barBox.Right);
            return bitmap;
        }

        private void FillOutBars<T>(Bitmap bitmap, Distribution<T> distribution, Rectangle box)
        {
            var buckets = distribution.Buckets;
            var n = buckets.Count;
            var colors = GetNColorsForBarGraph(n);
            var bgColor = Color.White;
            for (int i = 0; i < box.Width; i++)
            {
                var x = i + box.Left;
                var bucketIndex = (int)Math.Floor(((double)i / (box.Width)) * n);
                var bucket = buckets[bucketIndex];
                var barColor = colors[bucketIndex];
                var valueP = (double)bucket.Collection.Length / distribution.MaxCount;
                for (int j = 0; j < box.Height; j++)
                {
                    var y = box.Bottom - j - 1;
                    var yp = (double)j / box.Height;
                    var color = valueP >= yp
                        ? barColor
                        : bgColor;
                    bitmap.SetPixel(x, y, color);
                }
            }
        }

        private void FillOutYAxis(Bitmap bitmap, int maxValue, int totalCount, Rectangle drawbox, bool absoluteYAxis = false)
        {
            var segments = 5;
            var increment = 1.0 / segments;
            var lineCoords = Enumerable.Range(0, segments)
                .Select(x => new { percentHigh = 1.0 - x * increment, pixelCoord = (int)(x * increment * drawbox.Height + drawbox.Top) })
                .ToArray();

            FillArea(bitmap, drawbox, Color.White);
            
            var indent = 5;
            foreach (var segmentTop in lineCoords)
            {
                for (int i = drawbox.Left + indent; i < drawbox.Right; i++)
                {
                    bitmap.SetPixel(i, segmentTop.pixelCoord, Color.Black);
                    bitmap.SetPixel(i, segmentTop.pixelCoord + 1, Color.Black);
                }
                var rect = new Rectangle(drawbox.Left + indent, segmentTop.pixelCoord + indent, 80, 16);
                var g = Graphics.FromImage(bitmap);
                var segmentValue = absoluteYAxis
                    ? segmentTop.percentHigh * maxValue
                    : (double)(segmentTop.percentHigh * maxValue) / totalCount;
                var value = string.Format("{0:0.##}", segmentValue);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.DrawString(value, new Font("Verdana", 10), Brushes.Black, rect);
                g.Flush();
            }
        }

        private void FillOutXAxis<T>(Bitmap bitmap, Distribution<T> distribution, Rectangle drawbox, int barLeftStart, int barRightEnd)
        {
            var segments = distribution.Buckets.Count;
            var increment = 1.0 / segments;
            var indent = 3;
            var textWidth = 72;
            var textHeight = 16;
            var labelRects = Enumerable.Range(0, segments + 1)
                .Select(x => (int)(x * increment * (barRightEnd - barLeftStart) + barLeftStart))
                .Select((x, i) => new Rectangle(
                    x - textWidth / 2,
                    drawbox.Top + indent + (i % 2 == 0 ? 0 : textHeight),
                    textWidth,
                    textHeight))
                .ToArray();
            FillArea(bitmap, drawbox, Color.White);
            for (int i = 0; i < segments; i++)
            {
                var bucket = distribution.Buckets[i];
                var rect = labelRects[i];
                DrawNumber(bitmap, rect, bucket.MinRange);
            }
            var lastLabel = labelRects.Last();
            DrawNumber(bitmap, lastLabel, distribution.Buckets[distribution.Buckets.Count - 1].MaxRange);
        }

        private void DrawNumber(Bitmap bitmap, Rectangle rect, double value)
        {
            var g = Graphics.FromImage(bitmap);
            var segmentValue = string.Format("{0:0.##}", value);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            var sformat = new StringFormat(StringFormat.GenericDefault);
            sformat.Alignment = StringAlignment.Center;
            sformat.LineAlignment = StringAlignment.Center;
            g.DrawString(segmentValue, new Font("Verdana", 10), Brushes.Black, rect, sformat);
            g.Flush();
        }

        private Color[] GetNColorsForBarGraph(int n)
        {
            var converter = new ColourfulConverter();
            var start = (L: 65, a: 65, b: -15);
            var end = (L: 25, a: -40, b: -35);
            var delta = (L: end.L - start.L, a: end.a - start.a, b: end.b - start.b);
            var colors = Enumerable.Range(0, n)
                .Select(x => (double)x / (n - 1))
                .Select(x => (L: start.L + delta.L * x, a: start.a + delta.a * x, b: start.b + delta.b * x))
                .Select(x => new LabColor(x.L, x.a, x.b))
                .Select(x => converter.ToRGB(x).ToColor())
                .Reverse()
                .ToArray();
            return colors;
        }

        private void FillArea(Bitmap b, Rectangle area, Color color)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                for (int y = area.Top; y < area.Bottom; y++)
                {
                    b.SetPixel(x, y, color);
                }
            }
        }
    }
}
