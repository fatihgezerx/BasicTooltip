using UnityEngine;

namespace BasicTooltip
{
    /// <summary>
    /// The way in: show a tooltip next to any UI object, from anywhere - a <see cref="TooltipTrigger"/> does it
    /// on hover, your own code (an inventory slot, a skill button) calls <see cref="Show"/> and
    /// <see cref="Hide"/> itself. It goes through the scene's <see cref="TooltipView"/>; without one, nothing
    /// shows and nothing breaks.
    /// </summary>
    public static class Tooltips
    {
        private static TooltipView _view;

        /// <summary>The tooltip in use: the one registered last, or null.</summary>
        public static TooltipView View => _view;

        /// <summary>The object the tooltip is shown (or about to be shown) for, or null.</summary>
        public static RectTransform Target => _view != null ? _view.Target : null;

        /// <summary>Makes <paramref name="view"/> the tooltip in use. A <see cref="TooltipView"/> does it itself when it wakes.</summary>
        public static void Register(TooltipView view)
        {
            if (view != null)
            {
                _view = view;
            }
        }

        public static void Unregister(TooltipView view)
        {
            if (_view == view)
            {
                _view = null;
            }
        }

        /// <summary>
        /// Shows <paramref name="content"/> next to <paramref name="target"/> - after the view's Show Delay, or at
        /// once when it is already showing for something else - wherever it fits on screen.
        /// </summary>
        public static void Show(in TooltipContent content, RectTransform target)
        {
            if (_view != null)
            {
                _view.Show(content, target);
            }
        }

        /// <summary>Hides the tooltip if it is shown (or about to be) for <paramref name="target"/>; for anything else it stays.</summary>
        public static void Hide(RectTransform target)
        {
            if (_view != null && _view.Target == target)
            {
                _view.Hide();
            }
        }

        /// <summary>Hides the tooltip, whatever it is shown for.</summary>
        public static void HideAny()
        {
            if (_view != null)
            {
                _view.Hide();
            }
        }

        // Play Mode without a domain reload keeps statics: start each session without the last one's view.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _view = null;
    }
}
