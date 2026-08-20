# Character Creation — Personality Scenarios

Eight scenes from the character's past, answered before the game starts.
No answer is right or wrong: the choices only describe who the character already is.

Flow: **Player Name → Village Name → 8 scenarios → trait summary → game starts.**

## Scoring

Every choice adds to five dimensions (Big Five / OCEAN):

| Key | Dimension | Reads as |
|-----|-----------|----------|
| C | Conscientiousness | discipline, order, planning |
| E | Extraversion | boldness, directness, social ease |
| A | Agreeableness | honesty, kindness, mercy |
| O | Openness | curiosity, adaptability, unorthodoxy |
| S | Stability | calm, control — **Neuroticism, reverse scored** |

Neuroticism is stored already reversed as `Stability`, so in every dimension a
higher number means "more of this".

## From scores to a trait

`PersonalityResolver` sums the eight answers and takes the strongest dimension:

| Dominant | Trait |
|----------|-------|
| Conscientiousness | Ambitious |
| Extraversion | Proud |
| Agreeableness | Honest Nature |
| Openness | Risk Seeker |
| Stability | Calm Mind |

When the two strongest dimensions are within `BlendThreshold` (1 point) of each
other, the pair gets its own trait:

| Pair | Trait |
|------|-------|
| Conscientiousness + Agreeableness | Kind But Unyielding |
| Conscientiousness + Stability | Cold Pragmatist |
| Agreeableness + Stability | Hidden Mercy |

Any other close pair keeps the dominant dimension's trait.

## Where the result goes

`GameManager.ApplyPersonality` writes onto the new save:

- `PlayerData.Personality` — the raw Big Five scores
- `PlayerData.PersonalityTrait` — the trait id
- `PlayerData.ActiveTraitTags` — the trait id is appended, so trait driven systems see it

Passive bonuses live in `PersonalityTraits` (`JobRewardPercent`,
`CraftQualityPercent`, `TradePricePercent`, `EventRollBonus`,
`RestRecoveryPercent`) and are read with `PersonalityTraits.GetById(pd.PersonalityTrait)`.
The values are placeholders to tune, and can be swapped for TraitCatalog entries
without touching the scenario or scoring code.

## Wiring the screen (Inspector)

On the personality panel object, add `PersonalityQuestionPanel` and assign:

| Field | What it is |
|-------|------------|
| `titleText` | scenario title |
| `storyText` | scenario text |
| `progressText` | shows "Your Story: 3/8" |
| `choiceContainer` | vertical layout group the choice buttons are spawned into |
| `choiceButtonPrefab` | a button with a TMP label; one is spawned per choice |
| `backButton` | optional; goes to the previous scenario, and out of the first one |
| `resultPanel` / `resultTraitText` / `resultDescriptionText` / `resultContinueButton` | optional summary shown after the last scenario |

Then on `GameManager` assign `personalityQuestions` and `personalityPanel`.
If `personalityQuestions` is left empty the game starts straight after the
village name, exactly like before.

## Editing the questions

All eight scenarios, their text and their scores live in
`PersonalityScenarioCatalog.Build()`. Adding or removing a scenario needs no
other change: the progress counter and the resolver both read `Count`.
