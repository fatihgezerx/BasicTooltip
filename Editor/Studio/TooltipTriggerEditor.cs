using Veneer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BasicTooltip
{
    /// <summary>
    /// Studio-look Inspector for <see cref="TooltipTrigger"/>, which has no settings of its own: a header, how it
    /// gets its content, hints for what it needs (an <see cref="ITooltipSource"/>, a raycast target, a Selectable for
    /// gamepad navigation) and, in Play Mode, a button that asks the source again.
    /// </summary>
    [CustomEditor(typeof(TooltipTrigger))]
    [CanEditMultipleObjects]
    internal sealed class TooltipTriggerEditor : VeneerInspector<TooltipTrigger>
    {
        /// <inheritdoc />
        protected override VeneerBrand Brand => TooltipBrand.Value;

        /// <inheritdoc />
        protected override void Build(VeneerInspectorBuilder b)
        {
            b.Header("Tooltip Trigger", "Shows a tooltip on hover or selection");
            b.StatePill(PillState);

            b.Card("Content", "text", () =>
            {
                b.Hint("Asks the ITooltipSource on this object each time the pointer comes in or the object is selected. There is nothing to set here.");
                b.ConditionalHint(
                    () => Target != null && Target.GetComponent<ITooltipSource>() == null
                        ? "Nothing on this object implements ITooltipSource, so no tooltip will ever show. Add your component (for example an inventory slot) next to this one."
                        : null,
                    "warn",
                    true);
                var refresh = b.Button(
                    "Refresh now",
                    "refresh",
                    () =>
                    {
                        foreach (Object target in targets)
                        {
                            if (target is TooltipTrigger trigger && trigger != null)
                            {
                                trigger.Refresh();
                            }
                        }
                    },
                    "Asks the source again and shows or hides the tooltip.");
                b.VisibleWhen(refresh, () => Application.isPlaying);
            });

            b.Card("Needs", "check-circle", () =>
            {
                b.ConditionalHint(
                    () =>
                    {
                        if (Target == null)
                        {
                            return null;
                        }

                        Graphic graphic = Target.GetComponent<Graphic>();
                        return graphic == null || !graphic.raycastTarget
                            ? "No raycast target: hover needs a Graphic (Image, Text...) on this object with Raycast Target ticked."
                            : null;
                    },
                    "warn",
                    true);
                b.ConditionalHint(
                    () => Target != null && Target.GetComponent<Selectable>() == null
                        ? "Hover works, but a gamepad or keyboard player can only reach it when this is a Selectable (for example a Button)."
                        : null,
                    "info",
                    true);
                b.Hint("The tooltip box itself is a TooltipView somewhere in the canvas.");
            });
        }

        private (string text, string kind) PillState()
        {
            if (Target == null)
            {
                return (string.Empty, "idle");
            }

            return Target.GetComponent<ITooltipSource>() != null ? ("Source found", "ok") : ("No source", "warn");
        }
    }
}
