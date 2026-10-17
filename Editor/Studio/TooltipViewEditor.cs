using Veneer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BasicTooltip
{
    /// <summary>
    /// Studio-look Inspector for <see cref="TooltipView"/>: placement and timing on the first tab, the Title / Body /
    /// Icon parts on the second, a state pill (Play Mode: Shown / Hidden) and hints for a box that cannot work
    /// (no Canvas above it, inactive without a popup to open it, no parts, no size-to-text layout).
    /// </summary>
    [CustomEditor(typeof(TooltipView))]
    [CanEditMultipleObjects]
    internal sealed class TooltipViewEditor : VeneerInspector<TooltipView>
    {
        /// <inheritdoc />
        protected override VeneerBrand Brand => TooltipBrand.Value;

        /// <inheritdoc />
        protected override void Build(VeneerInspectorBuilder b)
        {
            b.Header("Tooltip Box", "The box every tooltip is shown in");
            b.StatePill(PillState);
            b.Live(LiveText);

            VisualElement[] tabs = b.Tabs("BasicTooltip.ViewTab", ("Placement", "target"), ("Parts", "layers"));

            using (b.Into(tabs[0]))
            {
                b.Card("Side", "move", () =>
                {
                    b.Enum(
                        "preferredSide",
                        new[] { "arrow-right", "arrow-left", "arrow-down", "arrow-up" },
                        new[] { "Right", "Left", "Below", "Above" },
                        "Preferred side");
                    b.Hint("Tried first. When the box does not fit there it goes to the opposite side, then the other two, and is always pushed back inside the canvas.");
                    b.Number("gap", "Gap", "px");
                    b.Number("margin", "Screen margin", "px");
                });

                b.Card("Timing", "clock", () =>
                {
                    b.Number("showDelay", "Show delay", "s");
                    b.Hint("Real time, so tooltips also work in a paused game. Moving on to another object while one shows swaps it at once.");
                });
            }

            using (b.Into(tabs[1]))
            {
                b.Card("Parts", "layers", () =>
                {
                    b.Object("title", "Title");
                    b.Object("body", "Body");
                    b.Object("icon", "Icon");
                    b.ConditionalHint(
                        () => Target != null && Target.gameObject != null && NoParts()
                            ? "No Title, Body or Icon is set, so the box would show nothing. Easy UI's Tooltip roles wire them for you."
                            : null,
                        "warn");
                    b.Hint("Each part is optional; whichever has no content is hidden.");
                });
            }

            b.ConditionalHint(
                () => Target != null && Target.GetComponentInParent<Canvas>(true) == null
                    ? "The box is not under a Canvas, so it cannot be placed. Put it under your UI canvas."
                    : null,
                "error",
                true);
            b.ConditionalHint(
                () => Target != null && !Target.gameObject.activeSelf && Target.GetComponent<ITooltipVisibility>() == null
                    ? "The box is inactive and nothing (like a UniMVC TooltipPopup) opens it, so it never registers. Keep it active: it hides itself."
                    : null,
                "warn",
                true);
            b.ConditionalHint(
                () => Target != null && Target.GetComponent<ContentSizeFitter>() == null
                    ? "No Content Size Fitter (Preferred Size) on the box. Add one, with a Vertical Layout Group, so it sizes itself to its texts."
                    : null,
                "info",
                true);
        }

        private bool NoParts()
        {
            SerializedProperty title = serializedObject.FindProperty("title");
            SerializedProperty body = serializedObject.FindProperty("body");
            SerializedProperty icon = serializedObject.FindProperty("icon");
            return title != null && body != null && icon != null &&
                   title.objectReferenceValue == null && body.objectReferenceValue == null && icon.objectReferenceValue == null;
        }

        private (string text, string kind) PillState()
        {
            if (Target == null)
            {
                return (string.Empty, "idle");
            }

            if (!Application.isPlaying)
            {
                return ("Ready", "info");
            }

            return Target.IsShown ? ("Shown", "ok") : ("Hidden", "idle");
        }

        private string LiveText()
        {
            if (Target == null)
            {
                return string.Empty;
            }

            RectTransform shownFor = Target.Target;
            return shownFor != null ? "for " + shownFor.name : "hidden";
        }
    }
}
