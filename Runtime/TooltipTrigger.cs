using UnityEngine;
using UnityEngine.EventSystems;

namespace BasicTooltip
{
    /// <summary>
    /// What a hovered object shows in its tooltip. Implemented by a component of your own system on the same object
    /// as its <see cref="TooltipTrigger"/> - e.g. an inventory slot giving its item's name, description and icon - so
    /// Basic Tooltip never needs to know that system.
    /// </summary>
    public interface ITooltipSource
    {
        /// <summary>The content to show right now; false for none (e.g. an empty slot, or while dragging).</summary>
        bool TryGetTooltip(out TooltipContent content);
    }

    /// <summary>
    /// Shows a tooltip while the pointer is over this UI object, filled by the <see cref="ITooltipSource"/> on it,
    /// asked each time the pointer comes in. It needs a raycast target to be hovered - a Graphic on it (an Image, a
    /// Text...) with Raycast Target ticked. Added by Easy UI's Shows Tooltip role; or call
    /// <see cref="Tooltips.Show"/> from your own code instead.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ITooltipSource _source;
        private RectTransform _rect;
#if UNITY_EDITOR
        private bool _warned;
#endif

        private RectTransform Rect => _rect != null ? _rect : _rect = (RectTransform)transform;

        public void OnPointerEnter(PointerEventData eventData) => Refresh();

        public void OnPointerExit(PointerEventData eventData) => Tooltips.Hide(Rect);

        // Hidden or destroyed under the pointer (e.g. its window closed): no exit comes, so its tooltip goes now.
        private void OnDisable() => Tooltips.Hide(Rect);

        /// <summary>Shows what its source gives now - e.g. after it changed while the tooltip was shown - or hides it when nothing.</summary>
        public void Refresh()
        {
            _source ??= GetComponent<ITooltipSource>();
            if (_source != null && _source.TryGetTooltip(out var content))
            {
                Tooltips.Show(content, Rect);
                return;
            }

            Tooltips.Hide(Rect);
#if UNITY_EDITOR
            if (_source == null && !_warned)
            {
                _warned = true;
                Debug.LogWarning($"[Basic Tooltip] '{name}' has a Tooltip Trigger but nothing on it gives its content: add a " +
                                 "component implementing ITooltipSource next to it.", this);
            }
#endif
        }
    }
}
