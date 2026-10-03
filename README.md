# BasicTooltip

A small, dependency-free tooltip for Unity's uGUI: hover an object, and a box with a title, a body and an
icon shows next to it - on the side you prefer when it fits there, otherwise wherever it does, and never
off the screen.

## Overview

One `TooltipView` in your canvas is the tooltip box. Any UI object can show it, with content your own system
gives - Basic Tooltip never needs to know that system:

- a `TooltipTrigger` on the object shows it on hover - and when the object is selected, so a gamepad or keyboard
  player navigating to it sees it too - filled by the `ITooltipSource` next to it (e.g. an inventory slot giving its
  item's name, description and icon); the object needs to be a Selectable (e.g. a Button) to be selected;
- or your own code calls `Tooltips.Show(new TooltipContent(title, body, icon), rectTransform)` and
  `Tooltips.Hide(rectTransform)`.

It works on its own. With [UniMVC](https://github.com/fatihgezerx/UniMVC) it opens and closes with UniMVC's
popup animations, with [Easy UI](https://github.com/fatihgezerx/EasyUI) it is designed there and set up by
roles, and with [LocalizationSystem](https://github.com/fatihgezerx/LocalizationSystem) Sync Project leaves its
labels alone - each optional.

## Features

- **Always on screen**: placed on the preferred side of its object (Right, Left, Below, Above), else on the
  opposite side, else on the other two; with no side wide enough, on the one with the most room - then
  pushed back inside the canvas, with a margin. Lined up with the object's top (or left) edge.
- **Anchored to the object**, not the pointer: it doesn't jitter while the pointer moves.
- **Title, body and icon**, each optional: whatever is empty is hidden.
- **Show delay** in real time (so it works in a paused game); moving on to another object while one shows
  swaps it at once.
- **Never in the way**: the box doesn't catch clicks or hover; a trigger disabled under the pointer (its
  window closed) takes its tooltip with it.
- **No `Update`, no coroutines**: event-driven. The only wait is the show delay, an `Awaitable` that runs
  only while a tooltip is waiting to show.
- **Modular**: the content comes from your system, through `ITooltipSource` - one small method.
- **No dependencies**. Optional: UniMVC (animations), Easy UI (roles), LocalizationSystem.

## Setup

### Requirements

- Unity 6 or newer (`Awaitable`)
- uGUI and TextMeshPro (`com.unity.ugui`), included by default

| Optional | Why |
|---|---|
| [UniMVC](https://github.com/fatihgezerx/UniMVC) | The tooltip as a popup, with UniMVC's open and close animations |
| [Easy UI](https://github.com/fatihgezerx/EasyUI) | Designing the tooltip and its triggers, set up by roles |
| [LocalizationSystem](https://github.com/fatihgezerx/LocalizationSystem) | Sync Project leaves the tooltip's labels alone |

### Installation

Clone or download this repository, then copy its contents into `Assets/Scripts/BasicTooltip/` (or anywhere
under `Assets/`). A small setup script offers to install the optional systems (you can decline them), and
keeps `HAS_BASIC_TOOLTIP` set while BasicTooltip is in the project, so code using it elsewhere compiles to
nothing once it is removed. With UniMVC, `Popups/TooltipPopup` (and, with Easy UI, its setup) is copied into
your MVC folder on its own, as with the other systems.

## Quick Start

### With Easy UI

1. In `Tools > Easy UI`, draw the tooltip: a box (an Empty or Image) with a Vertical Layout Group and a
   Content Size Fitter (Preferred Size), and in it a Text for the title, a Text for the body and, if you
   like, an Image for the icon.
2. Give them roles (**Add Role > Tooltip**): *Tooltip Box* on the box, *Tooltip Title*, *Tooltip Body* and
   *Tooltip Icon* on what is in it.
3. Mark what shows a tooltip with *Shows Tooltip* - in this panel or any other, e.g. an inventory's Slot
   Template (its slots give their items' tooltips).
4. Save, and build it with **GameObject > UI (Canvas) > Easy UI > your panel**. The box's `TooltipView` is
   wired to its parts, the box is moved to the canvas (drawn over everything), every element with Shows
   Tooltip gets a `TooltipTrigger` and a raycast target - and with UniMVC the box becomes a `TooltipPopup`,
   inactive, listed in the canvas's `UIManager`.

### By hand

1. Make the box under your canvas, with a `TooltipView` (it adds a `CanvasGroup`), and set its Title, Body
   and Icon fields. Keep it **active**: it hides itself, and registers with `Tooltips`, when the scene
   starts.
2. Put a `TooltipTrigger` and your `ITooltipSource` on any UI object with a raycast target (an Image, a
   Text...), or call `Tooltips` from your code.

With UniMVC, add `TooltipPopup` to the box instead of keeping it active: leave the box inactive and list the
popup in the `UIManager`'s Popups (it registers the view when initialized). Pick a fade or a pop as its
animation - the box is moved next to its object as it opens, so a slide doesn't suit it.

## Scripting

Give a hovered object its content - the `TooltipTrigger` next to it asks each time the pointer comes in:

```csharp
using BasicTooltip;

public sealed class SkillButton : MonoBehaviour, ITooltipSource
{
    public bool TryGetTooltip(out TooltipContent content)
    {
        content = new TooltipContent(skill.Name, skill.Description, skill.Icon);
        return true; // false: no tooltip right now
    }
}
```

Or show it yourself:

```csharp
// On hover (e.g. from IPointerEnterHandler / IPointerExitHandler):
Tooltips.Show(new TooltipContent(item.Name, $"Weight {item.Weight}", item.Icon), (RectTransform)transform);
Tooltips.Hide((RectTransform)transform); // only hides it if it is this object's
Tooltips.HideAny();                      // whatever it is shown for
```

`TooltipView` settings: **Preferred Side**, **Gap** (between the tooltip and its object), **Margin** (kept
free from the screen's edges) and **Show Delay** (seconds, real time).

Your own opening animation: implement `ITooltipVisibility` (`Show()`, `Hide()`) on a component next to the
`TooltipView` - it is then used instead of the CanvasGroup.

## Localization

The tooltip's labels are filled by code, so with LocalizationSystem, Sync Project never puts a `LocalizedText`
on them. Texts are shown as they are given: an `ITooltipSource` (or your call to `Tooltips.Show`) passes them
translated (`LocalizationRuntime.Get`) - as an inventory slot does with its item's name.

## License

[MIT License](LICENSE)
