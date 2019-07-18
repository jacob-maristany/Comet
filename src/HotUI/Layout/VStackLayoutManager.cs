using System;
using System.Collections.Generic;

namespace HotUI.Layout
{
    public class VStackLayoutManager : ILayoutManager
    {
        private readonly HorizontalAlignment _defaultAlignment;
        private readonly float _spacing;
        
        public VStackLayoutManager(
            HorizontalAlignment alignment = HorizontalAlignment.Center, 
            float? spacing = null)
        {
            _defaultAlignment = alignment;
            _spacing = spacing ?? 4;
        }

        public SizeF Measure(AbstractLayout layout, SizeF available)
        {
            var index = 0;
            var width = 0f;
            var height = 0f;
            var spacerCount = 0;
            var lastWasSpacer = false;
            
            foreach (var view in layout)
            {
                var isSpacer = false;
                
                if (view is Spacer)
                {
                    spacerCount++;
                    isSpacer = true;
                }
                else
                {
                    var size = view.MeasuredSize;
                    if (!view.MeasurementValid)
                    {
                        view.MeasuredSize = size = view.Measure(available);
                        view.MeasurementValid = true;
                    }

                    var finalHeight = size.Height;
                    var finalWidth = size.Width;
                    
                    var padding = view.Padding;
                    finalHeight += padding.VerticalThickness;
                    finalWidth += padding.HorizontalThickness;

                    width = Math.Max(finalWidth, width);
                    height += finalHeight;
                }
                
                if (index > 0 && !lastWasSpacer && !isSpacer)
                    height += _spacing;

                lastWasSpacer = isSpacer;
                index++;
            }

            if (spacerCount > 0)
                height = available.Height;

            return new SizeF(width, height);
        }

        public void Layout(AbstractLayout layout, SizeF measured)
        {
            var width = 0f;

            var index = 0;
            var nonSpacerHeight = 0f;
            var spacerCount = 0;
            var sizes = new List<SizeF>();
            var lastWasSpacer = false;

            foreach (var view in layout)
            {
                var isSpacer = false;

                if (view is Spacer)
                {
                    spacerCount++;
                    isSpacer = true;
                    sizes.Add(new SizeF());
                }
                else
                {
                    var size = view.MeasuredSize;
                    sizes.Add(size);
                    width = Math.Max(size.Width, width);
                    nonSpacerHeight += size.Height + view.Padding.VerticalThickness;
                }
                
                if (index > 0 && !lastWasSpacer && !isSpacer)
                    nonSpacerHeight += _spacing;
                
                lastWasSpacer = isSpacer;
                index++;
            }

            nonSpacerHeight = Math.Min(nonSpacerHeight, measured.Height);

            var spacerHeight = 0f;
            if (spacerCount > 0)
            {
                var availableHeight = measured.Height - nonSpacerHeight;
                spacerHeight = availableHeight / spacerCount;
            }

            var x = 0f;
            var y = 0f;
            index = 0;
            foreach (var view in layout)
            {
                var isSpacer = false;
                
                SizeF size;
                if (view is Spacer)
                {
                    isSpacer = true;
                    size = new SizeF(width, spacerHeight);
                }
                else
                {
                    size = sizes[index];
                }

                var alignment = view.FrameConstraints?.Alignment?.Horizontal ?? _defaultAlignment;
                var alignedX = x;

                var padding = view.GetPadding();
                
                switch (alignment)
                {
                    case HorizontalAlignment.Center:
                        alignedX += (measured.Width - size.Width - padding.Left + padding.Right) / 2;
                        break;
                    case HorizontalAlignment.Trailing:
                        alignedX += measured.Width - size.Width - padding.Right;
                        break;
                    case HorizontalAlignment.Leading:
                        alignedX = padding.Left;
                        break;
                      default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (index > 0 && !lastWasSpacer && !isSpacer)
                    y += _spacing;
                
                y += padding.Top;

                view.Frame = new RectangleF(alignedX, y, size.Width, size.Height);
                
                y += size.Height;
                y += padding.Bottom;
                
                index++;
            }
        }
    }
}