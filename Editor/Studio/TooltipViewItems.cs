using System;
using Veneer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BasicTooltip
{
    /// <summary>
    /// The pieces of the <see cref="TooltipView"/> Inspector that are computed or imperative, registered with Veneer so the default
    /// layout (and a user's copy of it) can place them: the state pill, the Play Mode strip, the "no parts" hint (in the Parts card) and
    /// three hints drawn under the tabs (no Canvas, inactive without a popup, no Content Size Fitter). They can be moved or hidden in
    /// the Inspector Designer, not changed.
    /// </summary>
    internal static class TooltipViewItems
    {
        /// <summary>Id of the header state pill (header badge).</summary>
        public const string StateId = "tooltip.view.state";

        /// <summary>Id of the Play Mode strip under the header (header badge slot, see <see cref="BuildLive"/>).</summary>
        public const string LiveId = "tooltip.view.live";

        /// <summary>Id of the "no Title, Body or Icon" hint (card item).</summary>
        public const string NoPartsId = "tooltip.view.no-parts";

        /// <summary>Id of the "not under a Canvas" error (footer, drawn under the tabs).</summary>
        public const string NoCanvasId = "tooltip.view.no-canvas";

        /// <summary>Id of the "inactive and nothing opens it" warning (footer, drawn under the tabs).</summary>
        public const string InactiveId = "tooltip.view.inactive";

        /// <summary>Id of the "no Content Size Fitter" hint (footer, drawn under the tabs).</summary>
        public const string NoFitterId = "tooltip.view.no-fitter";

        private const string NoPartsText =
            "No Title, Body or Icon is set, so the box would show nothing. Easy UI's Tooltip roles wire them for you.";

        private const string NoCanvasText =
            "The box is not under a Canvas, so it cannot be placed. Put it under your UI canvas.";

        private const string InactiveText =
            "The box is inactive and nothing (like a UniMVC TooltipPopup) opens it, so it never registers. Keep it active: it hides itself.";

        private const string NoFitterText =
            "No Content Size Fitter (Preferred Size) on the box. Add one, with a Vertical Layout Group, so it sizes itself to its texts.";

        private const string CardClass = "vn-card";

        /// <summary>Registers the custom items of <see cref="TooltipView"/>. Safe to call again (items are replaced).</summary>
        public static void Register()
        {
            Type type = typeof(TooltipView);

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = StateId,
                Label = "State",
                Icon = "check-circle",
                Description = "A pill in the header: Ready in the editor, Shown or Hidden in Play Mode.",
                Slot = VeneerCustomItemSlot.HeaderBadge,
                Build = BuildState,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = LiveId,
                Label = "Play Mode strip",
                Icon = "eye",
                Description = "A strip under the header, in Play Mode only: which object the box is shown for, or hidden.",
                Slot = VeneerCustomItemSlot.HeaderBadge,
                Build = BuildLive,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = NoPartsId,
                Label = "Parts check",
                Icon = "warning",
                Description = "A warning while Title, Body and Icon are all empty.",
                Slot = VeneerCustomItemSlot.Item,
                Build = BuildNoParts,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = NoCanvasId,
                Label = "Canvas check",
                Icon = "error",
                Description = "An error under the tabs while the box is not under a Canvas.",
                Slot = VeneerCustomItemSlot.Footer,
                Build = BuildNoCanvas,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = InactiveId,
                Label = "Inactive check",
                Icon = "warning",
                Description = "A warning under the tabs while the box is inactive and nothing opens it.",
                Slot = VeneerCustomItemSlot.Footer,
                Build = BuildInactive,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = NoFitterId,
                Label = "Size-to-text check",
                Icon = "info",
                Description = "A hint under the tabs while the box has no Content Size Fitter.",
                Slot = VeneerCustomItemSlot.Footer,
                Build = BuildNoFitter,
            });
        }

        private static TooltipView FirstView(VeneerCustomItemContext context)
        {
            return context.SerializedObject.targetObject as TooltipView;
        }

        private static VisualElement BuildState(VeneerCustomItemContext context)
        {
            TooltipView view = FirstView(context);
            context.Builder.StatePill(() => PillState(view));
            return null;
        }

        private static (string text, string kind) PillState(TooltipView view)
        {
            if (view == null)
            {
                return (string.Empty, "idle");
            }

            if (!Application.isPlaying)
            {
                return ("Ready", "info");
            }

            return view.IsShown ? ("Shown", "ok") : ("Hidden", "idle");
        }

        // The strip is a header-badge item so it is built right after the header, and is then moved from the header's right end to just
        // under the header card, where the strip always was. If the header card cannot be found (a header drawn over a banner picture
        // is not in the tree yet) the strip simply stays at the header's right end.
        private static VisualElement BuildLive(VeneerCustomItemContext context)
        {
            TooltipView view = FirstView(context);
            VisualElement strip = context.Builder.Live(() => LiveText(view));
            VisualElement card = strip.parent;
            while (card != null && !card.ClassListContains(CardClass))
            {
                card = card.parent;
            }

            VisualElement host = card != null ? card.parent : null;
            if (host != null)
            {
                host.Insert(host.IndexOf(card) + 1, strip);
            }

            return null;
        }

        private static string LiveText(TooltipView view)
        {
            if (view == null)
            {
                return string.Empty;
            }

            RectTransform shownFor = view.Target;
            return shownFor != null ? "for " + shownFor.name : "hidden";
        }

        private static VisualElement BuildNoParts(VeneerCustomItemContext context)
        {
            TooltipView view = FirstView(context);
            SerializedObject serializedObject = context.SerializedObject;
            context.Builder.ConditionalHint(
                () => view != null && view.gameObject != null && NoParts(serializedObject) ? NoPartsText : null,
                "warn");
            return null;
        }

        private static bool NoParts(SerializedObject serializedObject)
        {
            SerializedProperty title = serializedObject.FindProperty("title");
            SerializedProperty body = serializedObject.FindProperty("body");
            SerializedProperty icon = serializedObject.FindProperty("icon");
            return title != null && body != null && icon != null &&
                   title.objectReferenceValue == null && body.objectReferenceValue == null && icon.objectReferenceValue == null;
        }

        private static VisualElement BuildNoCanvas(VeneerCustomItemContext context)
        {
            TooltipView view = FirstView(context);
            context.Builder.ConditionalHint(
                () => view != null && view.GetComponentInParent<Canvas>(true) == null ? NoCanvasText : null,
                "error",
                true);
            return null;
        }

        private static VisualElement BuildInactive(VeneerCustomItemContext context)
        {
            TooltipView view = FirstView(context);
            context.Builder.ConditionalHint(
                () => view != null && !view.gameObject.activeSelf && view.GetComponent<ITooltipVisibility>() == null ? InactiveText : null,
                "warn",
                true);
            return null;
        }

        private static VisualElement BuildNoFitter(VeneerCustomItemContext context)
        {
            TooltipView view = FirstView(context);
            context.Builder.ConditionalHint(
                () => view != null && view.GetComponent<ContentSizeFitter>() == null ? NoFitterText : null,
                "info",
                true);
            return null;
        }
    }
}
