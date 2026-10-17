using System;
using Veneer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace BasicTooltip
{
    /// <summary>
    /// The pieces of the <see cref="TooltipTrigger"/> Inspector that are computed or imperative, registered with Veneer so the default
    /// layout (and a user's copy of it) can place them: the state pill, three hints (no <see cref="ITooltipSource"/>, no raycast target,
    /// no Selectable) and the Play Mode Refresh button. They can be moved or hidden in the Inspector Designer, not changed.
    /// </summary>
    internal static class TooltipTriggerItems
    {
        /// <summary>Id of the header state pill (header badge).</summary>
        public const string StateId = "tooltip.trigger.state";

        /// <summary>Id of the "nothing implements ITooltipSource" hint (card item).</summary>
        public const string NoSourceId = "tooltip.trigger.no-source";

        /// <summary>Id of the Refresh button, shown in Play Mode only (card item).</summary>
        public const string RefreshId = "tooltip.trigger.refresh";

        /// <summary>Id of the "no raycast target" hint (card item).</summary>
        public const string NoRaycastId = "tooltip.trigger.no-raycast";

        /// <summary>Id of the "not a Selectable" hint (card item).</summary>
        public const string NoSelectableId = "tooltip.trigger.no-selectable";

        private const string NoSourceText =
            "Nothing on this object implements ITooltipSource, so no tooltip will ever show. Add your component (for example an inventory slot) next to this one.";

        private const string NoRaycastText =
            "No raycast target: hover needs a Graphic (Image, Text...) on this object with Raycast Target ticked.";

        private const string NoSelectableText =
            "Hover works, but a gamepad or keyboard player can only reach it when this is a Selectable (for example a Button).";

        /// <summary>Registers the custom items of <see cref="TooltipTrigger"/>. Safe to call again (items are replaced).</summary>
        public static void Register()
        {
            Type type = typeof(TooltipTrigger);

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = StateId,
                Label = "State",
                Icon = "check-circle",
                Description = "A pill in the header: Source found or No source.",
                Slot = VeneerCustomItemSlot.HeaderBadge,
                Build = BuildState,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = NoSourceId,
                Label = "ITooltipSource check",
                Icon = "warning",
                Description = "A warning while nothing on this object implements ITooltipSource.",
                Slot = VeneerCustomItemSlot.Item,
                Build = BuildNoSource,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = RefreshId,
                Label = "Refresh now button",
                Icon = "refresh",
                Description = "Play Mode only: asks the source again and shows or hides the tooltip.",
                Slot = VeneerCustomItemSlot.Item,
                Build = BuildRefresh,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = NoRaycastId,
                Label = "Raycast target check",
                Icon = "warning",
                Description = "A warning while the object has no Graphic with Raycast Target ticked.",
                Slot = VeneerCustomItemSlot.Item,
                Build = BuildNoRaycast,
            });

            VeneerCustomItems.Register(type, new VeneerCustomItem
            {
                Id = NoSelectableId,
                Label = "Selectable check",
                Icon = "info",
                Description = "A hint while the object is not a Selectable (gamepad and keyboard cannot reach it).",
                Slot = VeneerCustomItemSlot.Item,
                Build = BuildNoSelectable,
            });
        }

        private static TooltipTrigger FirstTrigger(VeneerCustomItemContext context)
        {
            return context.SerializedObject.targetObject as TooltipTrigger;
        }

        private static VisualElement BuildState(VeneerCustomItemContext context)
        {
            TooltipTrigger trigger = FirstTrigger(context);
            context.Builder.StatePill(() => PillState(trigger));
            return null;
        }

        private static (string text, string kind) PillState(TooltipTrigger trigger)
        {
            if (trigger == null)
            {
                return (string.Empty, "idle");
            }

            return trigger.GetComponent<ITooltipSource>() != null ? ("Source found", "ok") : ("No source", "warn");
        }

        private static VisualElement BuildNoSource(VeneerCustomItemContext context)
        {
            TooltipTrigger trigger = FirstTrigger(context);
            context.Builder.ConditionalHint(
                () => trigger != null && trigger.GetComponent<ITooltipSource>() == null ? NoSourceText : null,
                "warn",
                true);
            return null;
        }

        private static VisualElement BuildRefresh(VeneerCustomItemContext context)
        {
            UnityEditor.SerializedObject serializedObject = context.SerializedObject;
            VisualElement refresh = context.Builder.Button(
                "Refresh now",
                "refresh",
                () => RefreshTargets(serializedObject),
                "Asks the source again and shows or hides the tooltip.");
            context.Builder.VisibleWhen(refresh, () => Application.isPlaying);
            return null;
        }

        private static void RefreshTargets(UnityEditor.SerializedObject serializedObject)
        {
            foreach (UnityEngine.Object target in serializedObject.targetObjects)
            {
                if (target is TooltipTrigger trigger && trigger != null)
                {
                    trigger.Refresh();
                }
            }
        }

        private static VisualElement BuildNoRaycast(VeneerCustomItemContext context)
        {
            TooltipTrigger trigger = FirstTrigger(context);
            context.Builder.ConditionalHint(
                () =>
                {
                    if (trigger == null)
                    {
                        return null;
                    }

                    Graphic graphic = trigger.GetComponent<Graphic>();
                    return graphic == null || !graphic.raycastTarget ? NoRaycastText : null;
                },
                "warn",
                true);
            return null;
        }

        private static VisualElement BuildNoSelectable(VeneerCustomItemContext context)
        {
            TooltipTrigger trigger = FirstTrigger(context);
            context.Builder.ConditionalHint(
                () => trigger != null && trigger.GetComponent<Selectable>() == null ? NoSelectableText : null,
                "info",
                true);
            return null;
        }
    }
}
