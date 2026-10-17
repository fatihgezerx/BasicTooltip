using Veneer;
using UnityEditor;

namespace BasicTooltip
{
    /// <summary>
    /// Studio-look Inspector for <see cref="TooltipTrigger"/>, which has no settings of its own, drawn from a Veneer layout: the
    /// project's own copy made in the Inspector Designer when there is one, otherwise <see cref="TooltipTriggerDefaultLayout"/>
    /// (a header, how it gets its content, hints for what it needs and, in Play Mode, a button that asks the source again). The state
    /// pill, the computed hints and the Refresh button are custom items (<see cref="TooltipTriggerItems"/>). A selection of several
    /// triggers shows the layout of the first one, as before.
    /// </summary>
    [CustomEditor(typeof(TooltipTrigger))]
    [CanEditMultipleObjects]
    internal sealed class TooltipTriggerEditor : VeneerLayoutDrivenInspector<TooltipTrigger>
    {
        /// <inheritdoc />
        protected override VeneerBrand Brand => TooltipBrand.Value;
    }
}
