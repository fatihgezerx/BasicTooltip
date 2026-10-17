# Changelog

## Unreleased

### Editor
- The `TooltipView` and `TooltipTrigger` Inspectors are now drawn from default Veneer layouts (`TooltipViewDefaultLayout`,
  `TooltipTriggerDefaultLayout`) that can be customised in the Inspector Designer; the look is unchanged. Pills, the Play Mode
  strip, the hints and the Refresh button are registered custom items (`TooltipViewItems`, `TooltipTriggerItems`).
- Studio-look Inspectors for `TooltipView` (tabs Placement and Parts, state pill, hints for a box that cannot work) and
  `TooltipTrigger` (hints for a missing `ITooltipSource`, raycast target or Selectable; Refresh button in Play Mode),
  built with Veneer in the new `BasicTooltip.Editor.Studio` assembly (`Editor/Studio`).
- Slate brand (`TooltipBrand`, `Editor/Brand/TooltipBrand.uss`, logo and banner).
- `DependencyGuard` offers the Veneer and sets `HAS_VENEER`; without the kit Unity's default
  Inspectors are used. The core editor assembly (setup, Easy UI roles) is unchanged and does not need the kit.
