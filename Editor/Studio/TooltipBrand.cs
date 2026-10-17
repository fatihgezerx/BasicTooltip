using Veneer;
using UnityEngine;

namespace BasicTooltip
{
    /// <summary>The identity of Basic Tooltip in the shared editor look: name, slate accent, logo and banner.</summary>
    internal static class TooltipBrand
    {
        private const string BrandFolder = "Assets/Scripts/BasicTooltip/Editor/Brand/";

        /// <summary>The cached brand; its pictures load lazily from <c>Editor/Brand</c>.</summary>
        public static readonly VeneerBrand Value = VeneerBrand.FromAssets(
            name: "Basic Tooltip",
            subtitle: "Hover tooltips for uGUI",
            accent: new Color32(0x8B, 0x8D, 0x98, 255),
            logoPath: BrandFolder + "logo.png",
            bannerPath: BrandFolder + "banner.png",
            iconFolder: null,
            styleSheetPath: BrandFolder + "TooltipBrand.uss",
            accentClass: "vn-accent--slate");
    }
}
