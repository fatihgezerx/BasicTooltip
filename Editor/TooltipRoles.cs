#if HAS_EASYUI
using System.Collections.Generic;
using EasyUI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BasicTooltip
{
    /// <summary>
    /// The roles an Easy UI panel's elements can have for Basic Tooltip, picked in Easy UI (Add Role, at the top of
    /// an element's inspector): the tooltip box and what is in it, and the objects that show a tooltip on hover -
    /// filled by their own scripts (an <see cref="ITooltipSource"/>, e.g. an inventory slot).
    /// </summary>
    public static class TooltipRoles
    {
        public const string Box = "tooltip.box";
        public const string Title = "tooltip.title";
        public const string Body = "tooltip.body";
        public const string Icon = "tooltip.icon";
        public const string Trigger = "tooltip.trigger";

        // Localization System's Localized Text, by id: the tooltip's texts are filled by code, and a
        // LocalizedText would overwrite them with one line.
        internal const string LocalizedText = "localization.localized-text";
    }

    /// <summary>Lists <see cref="TooltipRoles"/> in Easy UI's Add Role menu, under Tooltip.</summary>
    internal sealed class TooltipRoleProvider : IEasyUIRoleProvider
    {
        private static readonly string[] FilledByCode = { TooltipRoles.LocalizedText };

        public IEnumerable<EasyUIRole> GetRoles()
        {
            yield return new EasyUIRole(TooltipRoles.Box, "Tooltip/Tooltip Box",
                "The tooltip: holds the Title, Body and Icon, shown next to the hovered object wherever it fits on screen. " +
                "Give it a Vertical Layout Group and a Content Size Fitter (Preferred) to size it to its texts.",
                EasyUIElementType.EmptyObject, EasyUIElementType.Image)
            {
                Component = typeof(TooltipView)
            };

            yield return new EasyUIRole(TooltipRoles.Title, "Tooltip/Tooltip Title",
                "Inside the Tooltip Box: the title. Hidden when there is none.", EasyUIElementType.Text)
            {
                Conflicts = FilledByCode
            };

            yield return new EasyUIRole(TooltipRoles.Body, "Tooltip/Tooltip Body",
                "Inside the Tooltip Box: the text under the title. Hidden when there is none.", EasyUIElementType.Text)
            {
                Conflicts = FilledByCode
            };

            yield return new EasyUIRole(TooltipRoles.Icon, "Tooltip/Tooltip Icon",
                "Inside the Tooltip Box: an icon. Hidden when there is none.", EasyUIElementType.Image);

            yield return new EasyUIRole(TooltipRoles.Trigger, "Tooltip/Shows Tooltip",
                "Shows a tooltip while hovered, filled by the element's own script (e.g. an inventory slot's item). " +
                "Several elements can have it.", false,
                EasyUIElementType.EmptyObject, EasyUIElementType.Text, EasyUIElementType.Image, EasyUIElementType.RawImage,
                EasyUIElementType.Button, EasyUIElementType.Toggle, EasyUIElementType.Slider, EasyUIElementType.Dropdown,
                EasyUIElementType.InputField)
            {
                Component = typeof(TooltipTrigger)
            };
        }
    }

    /// <summary>
    /// Sets up a built panel with <see cref="TooltipRoles"/>: the box's view wired to its Title, Body and Icon, none
    /// of them in the pointer's way, and the box taken out to the canvas, drawn over everything, anchored to a point
    /// (it moves next to what it is shown for); every trigger made hoverable.
    /// </summary>
    internal sealed class TooltipBuilder : IEasyUIBuildHandler
    {
        private static readonly Color Transparent = new(0f, 0f, 0f, 0f);

        private readonly List<GameObject> _triggers = new();

        // Before others (e.g. a UniMVC popup made of the box), which find the box where it ends up.
        public int Order => -10;

        public void OnBuilt(EasyUIBuild build)
        {
            build.FindAll(TooltipRoles.Trigger, _triggers);
            foreach (var trigger in _triggers)
            {
                MakeHoverable(trigger);
            }

            var box = build.Find(TooltipRoles.Box);
            var titleObject = build.Find(TooltipRoles.Title);
            var bodyObject = build.Find(TooltipRoles.Body);
            var iconObject = build.Find(TooltipRoles.Icon);
            if (box == null)
            {
                var part = titleObject != null ? titleObject : bodyObject != null ? bodyObject : iconObject;
                if (part != null)
                {
                    Debug.LogWarning("[Basic Tooltip] The Tooltip Title, Body and Icon show in a Tooltip Box: give that role to the box holding them.", part);
                }

                return;
            }

            var view = box.GetComponent<TooltipView>();
            var fields = new SerializedObject(view);
            fields.FindProperty("title").objectReferenceValue = Part<TMP_Text>(titleObject, box, "Tooltip Title");
            fields.FindProperty("body").objectReferenceValue = Part<TMP_Text>(bodyObject, box, "Tooltip Body");
            fields.FindProperty("icon").objectReferenceValue = Part<Image>(iconObject, box, "Tooltip Icon");
            fields.ApplyModifiedPropertiesWithoutUndo();

            foreach (var graphic in box.GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }

            // Anchored to a point at its size: it is placed by code next to what it is shown for.
            var rect = (RectTransform)box.transform;
            var size = rect.rect.size;
            var center = rect.anchorMin == rect.anchorMax ? rect.anchorMin : new Vector2(0.5f, 0.5f);
            rect.anchorMin = rect.anchorMax = center;
            rect.sizeDelta = size;

            if (box != build.Root && build.Canvas != null)
            {
                build.MoveOut(box, build.Canvas.transform);
            }

            box.transform.SetAsLastSibling();
        }

        // The part's component, when it is inside the box; null (and a warning) when it is elsewhere.
        private static T Part<T>(GameObject part, GameObject box, string role) where T : Component
        {
            if (part == null)
            {
                return null;
            }

            if (!part.transform.IsChildOf(box.transform))
            {
                Debug.LogWarning($"[Basic Tooltip] The {role} isn't inside the Tooltip Box, so it is never shown.", part);
                return null;
            }

            return part.GetComponent<T>();
        }

        // A trigger is hovered through a raycast target: one without a Graphic gets a see-through Image.
        private static void MakeHoverable(GameObject trigger)
        {
            if (trigger.TryGetComponent<Graphic>(out var graphic))
            {
                graphic.raycastTarget = true;
                return;
            }

            trigger.AddComponent<Image>().color = Transparent;
        }
    }
}
#endif
