using System;
using Veneer;

namespace BasicTooltip
{
    /// <summary>
    /// The design of the <see cref="TooltipTrigger"/> Inspector that Basic Tooltip ships: a Content card (what the trigger does, the
    /// missing-source hint and the Play Mode Refresh button) and a Needs card (raycast target and Selectable hints). The component has no
    /// fields, so everything is a note or one of the system's items (<see cref="TooltipTriggerItems"/>). With Veneer installed a project
    /// can copy it in the Inspector Designer and change the copy; this default is never saved as an asset.
    /// </summary>
    internal sealed class TooltipTriggerDefaultLayout : IVeneerDefaultLayoutProvider
    {
        /// <inheritdoc />
        public Type TargetType => typeof(TooltipTrigger);

        /// <inheritdoc />
        public VeneerLayout CreateLayout()
        {
            VeneerLayout layout = new VeneerLayoutBuilder(typeof(TooltipTrigger), "Tooltip Trigger", "Shows a tooltip on hover or selection")
                .Page("Trigger", "text")
                .Card("Content", "text")
                .Custom(TooltipTriggerItems.StateId)
                .Note("Asks the ITooltipSource on this object each time the pointer comes in or the object is selected. There is nothing to set here.")
                .Custom(TooltipTriggerItems.NoSourceId)
                .Custom(TooltipTriggerItems.RefreshId)
                .Card("Needs", "check-circle")
                .Custom(TooltipTriggerItems.NoRaycastId)
                .Custom(TooltipTriggerItems.NoSelectableId)
                .Note("The tooltip box itself is a TooltipView somewhere in the canvas.")
                .Build();

            // The look of the system (accent, style sheet). UseThemeLogo stays off on purpose: the Inspector's builder already carries the
            // brand and shows its logo in the header, and the renderer would put a second logo next to it.
            layout.ThemeBrand = TooltipBrand.Value.Name;
            return layout;
        }

        /// <inheritdoc />
        public void RegisterItems()
        {
            TooltipTriggerItems.Register();
        }
    }
}
