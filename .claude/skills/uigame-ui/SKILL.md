---
name: uigame-ui
description: Measured facts about this project's Unity UI — the real canvas size, the font scale the scene actually uses, art aspect ratios, and where things get attached. Use this skill before writing or editing ANY UI code, editor tool, prefab builder, or layout in this repo, and before answering questions about why a panel looks wrong, is empty, or does nothing when clicked. Also use it before adding a component or setting a Rect Transform, and whenever a screen "compiles but doesn't work" — this project's defining failure is code that is attached to nothing or asked at the wrong moment.
---

# UIGame UI

Every number here was measured in this repo. Re-measure rather than trust
memory if something looks off, but do not guess: guessing at layout has cost
this project several wasted round trips, because the person who can see the
screen is the user, not you.

## The canvas is not the reference resolution

`Main.unity`'s CanvasScaler reads `1920 x 1080`, match `0.5`. That is *not* the
size anything is laid out in. On the OnePlus 6T the project targets (1080x2340)
the canvas RectTransform measures **978 x 2120**, scale factor **1.104** —
confirmed in the Inspector, not inferred.

At match 0.5 the scaler reduces to:

```
scaleFactor = sqrt( (screenW x screenH) / (refW x refH) )
```

which depends only on the *product* of the reference. `1920x1080` and
`1080x1920` give an identical result, so swapping them changes nothing at all.
Changing `match` does change everything (match 0 gives a 1080x2340 canvas, about
10% larger) and would reflow every hand-built screen, so leave it alone.

The practical consequence: **a number that looks right against a 1080x1920
mental model is wrong here.** Read the canvas rect at build time if a tool needs
real units.

## Never write a pixel offset for layout

Anchors are already fractions of the parent, so a fractional spec needs no
arithmetic and is correct on any handset. Express position as `top`, `bottom`,
`left`, `right`, `height`, `width` in fractions and push them straight into
`anchorMin` / `anchorMax` with zero offsets.

This is not style. The first creation panel wrote `top: 380, bottom: 190`,
which reads fine against a 1200-tall screen and bunched every element into the
top third of the real 2120-tall canvas.

When a rect has a fixed size, **say which edge it is measured from**. `bottom`
plus `height` and `top` plus `height` are indistinguishable if the anchor is
implied, because an unspecified `top` and a `top` of zero are the same number —
a back button asking to sit above the bottom edge silently pinned itself to the
ceiling.

`Assets/Editor/UIScreen/` builds screens from JSON specs this way. Prefer
extending a spec over writing another builder.

## Font scale

The scene's own type sizes, counted:

| Size | Uses | Role |
|---|---|---|
| **36** | 56 | headings, primary values |
| **30** | 36 | body, most labels |
| 25 | 16 | secondary |
| 24 | 15 | small print |
| 50, 72 | 18 | display, title bars |

Generated content written at 13–22 read as roughly two and a half times too
small beside hand-laid panels. If you are choosing a size, choose from this
table.

Vertical layout groups take their height from the text, so type rarely clips
there. **Fixed cells do clip** — a `GridLayoutGroup` cell of 32 units cannot
hold 24 point type. Scale cell sizes and icon heights along with the font.

## Art aspect

| Sprite | Size | Aspect | What it is |
|---|---|---|---|
| `Backgrounds/STARTING BG.png` | 2160x3840 | 0.5625 | full-screen portrait |
| `Backgrounds/INVENTORY BG.png`, `QUEST BACKGROUND.png` | 1080x1920 | 0.5625 | full-screen portrait |
| `Backgrounds/PARCHMENT BACKGROUND.png` | 820x1200 | 0.68 | **a panel, not a background** |

The canvas is 0.46, so **even a purpose-made 0.5625 background does not fit**.
Use `AspectRatioFitter` in `EnvelopeParent` and let a RectMask2D crop it.
Stretching is what made a parchment scroll illustration look squashed across a
whole screen.

Art lives under `Assets/UI Elements/`; item sprites follow `<SpriteBase>1..5`
where the trailing digit is a quality tier (Crude..Legendary).

## The failure that keeps happening

Code here compiles, is attached to nothing, and never runs. It has appeared as:
eight `GameSystemBase` subclasses missing from the scene including `TraitSystem`;
`ShopUI`, `QuestNoteView` and every profile binder built and unattached; origin
tags written and read by nobody; `SkillXpGain` promised by three traits and
applied at none of the three XP grant sites.

A grep for references proves nothing, because Unity wires MonoBehaviours through
the Inspector. **The only reliable test is the GUID**: read the script's `.meta`,
grep `Assets/Scenes` and `Assets/Prefabs` for it. `Tools > UIGame > Systems >
Report which systems are missing` does this for systems.

Before concluding a feature is unwritten, check whether it is merely unattached.

## Timing

`GameBootstrapper` registers systems in `Start` — it must, because at `Awake` no
scene object exists to find. Unity runs **every** `OnEnable` in a scene before
**any** `Start`.

So a panel left active in the scene asks for its system one phase too early,
gets null, and returns. It draws perfectly and does nothing, because no listener
was ever attached. Wait for the instance (a coroutine that yields until it
appears) rather than asking once.

## Say what went wrong, out loud

Silent failure is this project's signature, and it costs the user a Play cycle
every time. Two examples worth internalising: `StartNewGame` skipped character
creation without a word when its panel was unassigned, and `RequireRoot`
returned null whenever the selection had no RectTransform, so every profile tool
looked like it had never run — no log, no dialog, nothing.

When something cannot proceed, log which thing was missing and what to do about
it. A message that names the fix turns a debugging session into a glance.

**When a screen misbehaves, add a `Debug.Log` and ask the user to click it.**
Do not theorise from the code. You cannot see the screen; they can, in seconds.
Three consecutive wrong theories about a dark panel is what this rule is for.

## Reading the scene file

`Assets/Scenes/Main.unity` is the last **saved** state. If Unity's title bar
shows `Main*` there are unsaved changes and the file will disagree with what the
user sees. Never assert that a reference is assigned based on the file alone —
ask, or have the user check the Inspector.

Related: Unity discards unsaved scene changes when leaving Play mode. Any editor
tool that modifies the scene needs **run tool → Ctrl+S → Play**, in that order,
or the work evaporates.

## Verifying without Unity

You cannot run the editor. You can compile, and should, before every commit:

```bash
dotnet build Assembly-CSharp.csproj -v q --nologo
```

New files are absent from the generated `.csproj` until Unity refreshes; build a
temp copy with them added rather than skipping the check. **Gate the commit on
the build** — running them as independent commands has already pushed code that
did not compile.

A green build is not a working feature. Only the user's console proves that.

## Tools that already exist

Check here before writing another one.

| Menu | Does |
|---|---|
| `Tools > UIGame > UI Screens` | Builds a screen from a JSON spec in `Assets/Data/ui/` |
| `Tools > UIGame > Wiring` | Fills Inspector slots from `wiring.json`; handles prefab assets and hierarchy paths |
| `Tools > UIGame > Systems` | Reports and adds missing `GameSystemBase` systems |
| `Tools > UIGame > Profile Panel` | Wires the profile panel by object name; `keep my layout` and `Fill Skills and Traits only` leave hand-built content alone |
| `Tools > UIGame > Traits` / `Titles` / `Items` | Generate the SO databases from the code catalogs |

`ItemSOImporter` owns the sprite naming convention; call its `BuildSpriteIndex`
and `ResolveSprites` rather than restating it. Duplicating catalog data has
already gone wrong once — `TitleLadder` re-typed a ladder `TitleDatabaseSO`
already held, because only the top of that file was read.

## Division of labour

The user builds screens and does the Inspector work. Your side is asset
preparation, data catalogs, importers, editor tools, and the scripts behind
their panels. When a screen needs building, build the thing that builds it and
hand over the menu item.
