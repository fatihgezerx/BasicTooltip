using Veneer;
using UnityEditor;

namespace BasicTooltip
{
    /// <summary>
    /// Studio-look Inspector for <see cref="TooltipView"/>, drawn from a Veneer layout: the project's own copy made in the Inspector
    /// Designer when there is one, otherwise <see cref="TooltipViewDefaultLayout"/> (tabs Placement and Parts). The state pill, the
    /// Play Mode strip and the hints for a box that cannot work (no Canvas above it, inactive without a popup to open it, no parts, no
    /// size-to-text layout) are custom items (<see cref="TooltipViewItems"/>). A selection of several boxes shows the layout of the
    /// first one, as before.
    /// </summary>
    [CustomEditor(typeof(TooltipView))]
    [CanEditMultipleObjects]
    internal sealed class TooltipViewEditor : VeneerLayoutDrivenInspector<TooltipView>
    {
        /// <inheritdoc />
        protected override VeneerBrand Brand => TooltipBrand.Value;
    }
}
