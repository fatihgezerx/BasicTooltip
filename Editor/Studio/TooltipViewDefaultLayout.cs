using System;
using Veneer;

namespace BasicTooltip
{
    /// <summary>
    /// The design of the <see cref="TooltipView"/> Inspector that Basic Tooltip ships: tab Placement (Side and Timing cards), tab Parts
    /// (Title, Body and Icon) and the system's own items (state pill, Play Mode strip, the "box cannot work" hints). With Veneer
    /// installed a project can copy it in the Inspector Designer and change the copy; this default is never saved as an asset.
    /// </summary>
    internal sealed class TooltipViewDefaultLayout : IVeneerDefaultLayoutProvider
    {
        /// <inheritdoc />
        public Type TargetType => typeof(TooltipView);

        /// <inheritdoc />
        public VeneerLayout CreateLayout()
        {
            VeneerLayout layout = new VeneerLayoutBuilder(typeof(TooltipView), "Tooltip Box", "The box every tooltip is shown in")
                .Page("Placement", "target")
                .Card("Side", "move")
                .Custom(TooltipViewItems.StateId)
                .Custom(TooltipViewItems.LiveId)
                .Enum("preferredSide", "Preferred side", "arrow-right,arrow-left,arrow-down,arrow-up")
                .Note("Tried first. When the box does not fit there it goes to the opposite side, then the other two, and is always pushed back inside the canvas.")
                .Field("gap", "Gap", unit: "px", widget: VeneerWidget.Number)
                .Field("margin", "Screen margin", unit: "px", widget: VeneerWidget.Number)
                .Card("Timing", "clock")
                .Field("showDelay", "Show delay", unit: "s", widget: VeneerWidget.Number)
                .Note("Real time, so tooltips also work in a paused game. Moving on to another object while one shows swaps it at once.")
                .Page("Parts", "layers")
                .Card("Parts", "layers")
                .Field("title", "Title", widget: VeneerWidget.Object)
                .Field("body", "Body", widget: VeneerWidget.Object)
                .Field("icon", "Icon", widget: VeneerWidget.Object)
                .Custom(TooltipViewItems.NoPartsId)
                .Note("Each part is optional; whichever has no content is hidden.")
                // The three hints below are footer items: they are drawn under the tabs whatever tab is open. A footer item needs an
                // entry in some card; this card has only footer entries, so it draws nothing itself.
                .Card("Checks under the tabs", "check-circle")
                .Custom(TooltipViewItems.NoCanvasId)
                .Custom(TooltipViewItems.InactiveId)
                .Custom(TooltipViewItems.NoFitterId)
                .Build();

            // The look of the system (accent, style sheet). UseThemeLogo stays off on purpose: the Inspector's builder already carries the
            // brand and shows its logo in the header, and the renderer would put a second logo next to it.
            layout.ThemeBrand = TooltipBrand.Value.Name;
            return layout;
        }

        /// <inheritdoc />
        public void RegisterItems()
        {
            TooltipViewItems.Register();
        }
    }
}
