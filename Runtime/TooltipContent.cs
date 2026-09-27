using UnityEngine;

namespace BasicTooltip
{
    /// <summary>
    /// What a tooltip shows: a title, a body and an icon, each optional - whatever is empty is hidden. Pass the
    /// texts as they are to be read (already translated, if your game is).
    /// </summary>
    public readonly struct TooltipContent
    {
        public readonly string Title;
        public readonly string Body;
        public readonly Sprite Icon;

        public TooltipContent(string title, string body = null, Sprite icon = null)
        {
            Title = title;
            Body = body;
            Icon = icon;
        }

        /// <summary>Whether there is nothing to show.</summary>
        public bool IsEmpty => string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Body) && Icon == null;
    }
}
