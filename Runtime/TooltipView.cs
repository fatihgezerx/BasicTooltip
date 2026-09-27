using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BasicTooltip
{
    /// <summary>
    /// Opens and closes the tooltip box - e.g. with an animation. Implemented by a component on the
    /// <see cref="TooltipView"/>'s object (e.g. a UniMVC popup); without one, the view shows and hides itself at
    /// once, with its CanvasGroup.
    /// </summary>
    public interface ITooltipVisibility
    {
        void Show();
        void Hide();
    }

    /// <summary>
    /// The tooltip box: a title, a body and an icon (each optional), shown through <see cref="Tooltips"/> next to
    /// the object it is for - on its <see cref="PreferredSide"/> when it fits there, else wherever it fits, never
    /// off the screen (see <see cref="TooltipPlacement"/>). It never catches the pointer.
    /// </summary>
    /// <remarks>
    /// Give the box a layout that sizes it to its texts - e.g. a Vertical Layout Group and a Content Size Fitter
    /// (Preferred Size) - and keep it under the canvas, above what it is shown over (it moves to the end of its
    /// parent when shown). Without an <see cref="ITooltipVisibility"/> it stays active and hides with its
    /// CanvasGroup, so it registers itself as soon as the scene starts; with one (e.g. a UniMVC popup, which is
    /// inactive while hidden) that component registers it. The Show Delay counts in real time, so tooltips work
    /// in a paused game too, and only while a tooltip is waiting to show.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class TooltipView : MonoBehaviour
#if HAS_LOCALIZATION_SYSTEM
        , LocalizationSystem.ILocalizedByCode
#endif
    {
        // Moving from one object to the next hides the tooltip (pointer exit) just before showing it again
        // (pointer enter): a show this soon after a hide skips the delay, as if it had stayed.
        private const float SwapGrace = 0.1f;

        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private Image icon;

        [Tooltip("The side of the object tried first; the others are tried when it doesn't fit there.")]
        [SerializeField] private TooltipSide preferredSide = TooltipSide.Right;

        [Tooltip("Space between the tooltip and the object.")]
        [Min(0f)] [SerializeField] private float gap = 8f;

        [Tooltip("Space kept free between the tooltip and the screen's edges.")]
        [Min(0f)] [SerializeField] private float margin = 8f;

        [Tooltip("Seconds the pointer rests on an object before its tooltip shows (real time). Moving on to another " +
                 "object while one shows swaps it at once.")]
        [Min(0f)] [SerializeField] private float showDelay = 0.3f;

        private RectTransform _rect;
        private CanvasGroup _group;
        private ITooltipVisibility _visibility;
        private bool _ready;
        private bool _shown;
        private float _hiddenAt = float.NegativeInfinity;

        // Bumped by every Show and Hide, so a delayed show that was overtaken does nothing.
        private int _request;
        private TooltipContent _content;

        /// <summary>The object it is shown (or about to be shown) for, or null.</summary>
        public RectTransform Target { get; private set; }

        /// <summary>Whether it is shown.</summary>
        public bool IsShown => _shown;

        public TooltipSide PreferredSide
        {
            get => preferredSide;
            set => preferredSide = value;
        }

        private void Awake()
        {
            EnsureReady();
            Tooltips.Register(this);
            if (!_shown && _visibility == null)
            {
                _group.alpha = 0f;
            }
        }

        private void OnDestroy() => Tooltips.Unregister(this);

        /// <summary>Shows <paramref name="content"/> next to <paramref name="target"/>; hides it when there is nothing to show.</summary>
        public void Show(in TooltipContent content, RectTransform target)
        {
            if (content.IsEmpty || target == null)
            {
                Hide();
                return;
            }

            _content = content;
            Target = target;
            var request = ++_request;

            if (_shown || showDelay <= 0f || Time.unscaledTime - _hiddenAt < SwapGrace)
            {
                Present(request);
            }
            else
            {
                _ = PresentLater(request);
            }
        }

        public void Hide()
        {
            _request++;
            Target = null;
            if (!_shown)
            {
                return;
            }

            _shown = false;
            _hiddenAt = Time.unscaledTime;
            if (_visibility != null)
            {
                _visibility.Hide();
            }
            else
            {
                _group.alpha = 0f;
            }
        }

        // Waits in real time (a paused game still shows tooltips), then shows - unless another Show or a Hide
        // came first. Runs only while a tooltip waits: a few frames, then done.
        private async Awaitable PresentLater(int request)
        {
            var at = Time.unscaledTime + showDelay;
            while (Time.unscaledTime < at)
            {
                await Awaitable.NextFrameAsync();
                if (this == null || request != _request)
                {
                    return;
                }
            }

            Present(request);
        }

        private void Present(int request)
        {
            if (this == null || request != _request || Target == null || !Target.gameObject.activeInHierarchy)
            {
                return;
            }

            EnsureReady();

            // Shown first: a layout only sizes an active box.
            if (_visibility != null)
            {
                _visibility.Show();
            }

            _shown = true;
            SetText(title, _content.Title);
            SetText(body, _content.Body);
            if (icon != null)
            {
                icon.sprite = _content.Icon;
                icon.gameObject.SetActive(_content.Icon != null);
            }

            _rect.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
            TooltipPlacement.Place(_rect, Target, preferredSide, gap, margin);

            if (_visibility == null)
            {
                _group.alpha = 1f;
            }
        }

        private static void SetText(TMP_Text label, string text)
        {
            if (label == null)
            {
                return;
            }

            var any = !string.IsNullOrEmpty(text);
            label.gameObject.SetActive(any);
            if (any)
            {
                label.SetText(text);
            }
        }

        // Awake doesn't run on a box that starts inactive (e.g. a UniMVC popup): set up on first use instead.
        private void EnsureReady()
        {
            if (_ready)
            {
                return;
            }

            _ready = true;
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;
            _visibility = GetComponent<ITooltipVisibility>();
        }

#if HAS_LOCALIZATION_SYSTEM
        // Filled by code with texts already in the current language: LocalizationSystem's "Sync Project" leaves
        // these labels alone.
        bool LocalizationSystem.ILocalizedByCode.Fills(Component text) => text != null && (text == title || text == body);
#endif
    }
}
