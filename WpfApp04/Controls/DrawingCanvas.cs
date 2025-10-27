using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp04.Controls
{
    /// <summary>
    /// Canvas с фунциями определения содержащихся рисунков по индексу, координатам, контуру
    /// </summary>
    public class DrawingCanvas : Canvas
    {

        private readonly List<Visual> _hits = new List<Visual>();
        protected readonly List<Visual> visuals = new List<Visual>();


        protected override Visual GetVisualChild(int index)
        {
            if (index < visuals.Count) return visuals[index];

            return base.GetVisualChild(index - visuals.Count);
        }
        protected override int VisualChildrenCount => visuals.Count + base.VisualChildrenCount;

        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);
            AddVisualChild(visual);
            AddLogicalChild(visual);
        }

        public void ClearVisuals()
        {
            foreach (var visual in visuals)
            {
                RemoveVisualChild(visual);
                RemoveLogicalChild(visual);
            }
            visuals.Clear();
        }

        public void RemoveVisual(Visual visual)
        {
            visuals.Remove(visual);

            RemoveVisualChild(visual);
            RemoveLogicalChild(visual);
        }

        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            return hitResult.VisualHit as DrawingVisual;
        }

        public List<Visual> GetVisuals(Geometry region)
        {
            _hits.Clear();
            var parameters = new GeometryHitTestParameters(region);
            VisualTreeHelper.HitTest(this, null, HitTestCallback, parameters);
            return _hits;
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            var geometryResult = (GeometryHitTestResult)result;
            var visual = result.VisualHit as DrawingVisual;
            if (visual != null &&
                geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                _hits.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        }

    }
}
