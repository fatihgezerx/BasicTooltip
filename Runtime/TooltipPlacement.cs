using UnityEngine;

namespace BasicTooltip
{
    /// <summary>The side of its object a tooltip goes on.</summary>
    public enum TooltipSide
    {
        Right,
        Left,
        Below,
        Above
    }

    /// <summary>
    /// Puts a tooltip next to its object, never off the canvas: on the preferred side if it fits there, else on
    /// the opposite one, else on the other two; with no side wide enough, on the one with the most room. Along
    /// the side it lines up with the object's top (or left) edge, and is then pushed back inside the canvas -
    /// so it always shows whole wherever there is room, however close to an edge the object is.
    /// </summary>
    public static class TooltipPlacement
    {
        private static readonly Vector3[] Corners = new Vector3[4];

        /// <param name="tooltip">The tooltip, laid out at its final size; its pivot is set to its top-left corner.</param>
        /// <param name="target">What it is shown for.</param>
        /// <param name="preferred">The side tried first.</param>
        /// <param name="gap">Space between the tooltip and the object.</param>
        /// <param name="margin">Space kept free between the tooltip and the canvas's edges.</param>
        public static void Place(RectTransform tooltip, RectTransform target, TooltipSide preferred, float gap, float margin)
        {
            var parent = tooltip.parent as RectTransform;
            var canvas = tooltip.GetComponentInParent<Canvas>(true);
            if (parent == null || canvas == null || target == null)
            {
                return;
            }

            // Everything in the parent's space, where the tooltip's local position lives. Its own scale is left
            // out (an opening animation may be zooming it), so it is placed for its full size.
            var bounds = RectIn(parent, (RectTransform)canvas.rootCanvas.transform);
            bounds = Rect.MinMaxRect(bounds.xMin + margin, bounds.yMin + margin, bounds.xMax - margin, bounds.yMax - margin);
            var around = RectIn(parent, target);
            var size = tooltip.rect.size;

            var side = preferred;
            var found = false;
            for (var i = 0; i < 4 && !found; i++)
            {
                side = Candidate(preferred, i);
                found = Room(side, around, bounds, size, gap) >= 0f;
            }

            if (!found)
            {
                var best = float.MinValue;
                for (var i = 0; i < 4; i++)
                {
                    var candidate = Candidate(preferred, i);
                    var room = Room(candidate, around, bounds, size, gap);
                    if (room > best)
                    {
                        best = room;
                        side = candidate;
                    }
                }
            }

            // The top-left corner, pushed inside the bounds (to their top-left when it is larger than them).
            var corner = Corner(side, around, size, gap);
            corner.x = Mathf.Max(Mathf.Min(corner.x, bounds.xMax - size.x), bounds.xMin);
            corner.y = Mathf.Min(Mathf.Max(corner.y, bounds.yMin + size.y), bounds.yMax);

            tooltip.pivot = new Vector2(0f, 1f);
            tooltip.localPosition = new Vector3(corner.x, corner.y, tooltip.localPosition.z);
        }

        // The preferred side, then its opposite, then the two across.
        private static TooltipSide Candidate(TooltipSide preferred, int index)
        {
            var horizontal = preferred is TooltipSide.Right or TooltipSide.Left;
            return index switch
            {
                0 => preferred,
                1 => Opposite(preferred),
                2 => horizontal ? TooltipSide.Below : TooltipSide.Right,
                _ => horizontal ? TooltipSide.Above : TooltipSide.Left
            };
        }

        private static TooltipSide Opposite(TooltipSide side) => side switch
        {
            TooltipSide.Right => TooltipSide.Left,
            TooltipSide.Left => TooltipSide.Right,
            TooltipSide.Below => TooltipSide.Above,
            _ => TooltipSide.Below
        };

        // What is left over on that side once the tooltip is there: negative when it doesn't fit.
        private static float Room(TooltipSide side, Rect around, Rect bounds, Vector2 size, float gap) => side switch
        {
            TooltipSide.Right => bounds.xMax - (around.xMax + gap + size.x),
            TooltipSide.Left => around.xMin - gap - size.x - bounds.xMin,
            TooltipSide.Below => around.yMin - gap - size.y - bounds.yMin,
            _ => bounds.yMax - (around.yMax + gap + size.y)
        };

        // The tooltip's top-left corner on that side (y up, as a RectTransform counts).
        private static Vector2 Corner(TooltipSide side, Rect around, Vector2 size, float gap) => side switch
        {
            TooltipSide.Right => new Vector2(around.xMax + gap, around.yMax),
            TooltipSide.Left => new Vector2(around.xMin - gap - size.x, around.yMax),
            TooltipSide.Below => new Vector2(around.xMin, around.yMin - gap),
            _ => new Vector2(around.xMin, around.yMax + gap + size.y)
        };

        // `rect`'s area in `space`'s local coordinates.
        private static Rect RectIn(RectTransform space, RectTransform rect)
        {
            rect.GetWorldCorners(Corners);
            Vector2 min = space.InverseTransformPoint(Corners[0]);
            Vector2 max = min;
            for (var i = 1; i < 4; i++)
            {
                Vector2 point = space.InverseTransformPoint(Corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
