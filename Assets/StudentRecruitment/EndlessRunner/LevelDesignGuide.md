# Level Design Guide for Endless Runner

This guide provides concepts and principles for designing levels for the Endless Runner game.

## Core Level Concepts

### Track Types

1. **Straight Tracks**: Simple straight segments, ideal for tutorials and as breathers between complex sections.
2. **Curved Tracks**: Tracks with gentle turns, visually engaging but maintains similar gameplay to straights.
3. **Slope Tracks**: Uphill or downhill segments that can change player visibility and perception.
4. **Split Tracks**: Paths that diverge and merge, offering player choices with different challenges/rewards.

### Game Zones

1. **Tutorial Zone**: Simple obstacles and power-ups with instructional elements.
2. **Standard Zone**: Balanced challenge with mixed obstacles and occasional power-ups.
3. **Challenge Zone**: Difficult obstacle patterns with fewer power-ups.
4. **Reward Zone**: High concentration of collectibles with minimal obstacles.
5. **Boss Zone**: Intense obstacle patterns that build to a climax (e.g., boulder chase sequence).

## Obstacle Placement

### Pacing Guidelines

1. **Build-up**: Start with simple obstacles, gradually increasing complexity.
2. **Peak Challenge**: Create a challenging section near 60-70% of the level.
3. **Cool Down**: Reduce difficulty slightly before the finish to ensure satisfying completion.
4. **Breather Sections**: Include obstacle-free zones to let players recover between challenges.

### Obstacle Patterns

1. **Zigzag**: Alternate lane obstacles forcing player to weave between lanes.
2. **Tunnel**: Place walls in outer lanes, forcing center lane passage.
3. **Quick Switch**: Place two obstacles in quick succession that require lane changes.
4. **Jump Sequence**: Multiple jump obstacles in sequence with precise timing required.
5. **Slide Sequence**: Multiple slide obstacles requiring timed slides.
6. **Mixed Challenges**: Combine obstacle types (e.g., jump then immediately slide).

## Visual Progression

1. **Environment Changes**: Shift environments as player progresses (e.g., city to forest).
2. **Time of Day**: Transition lighting from day to sunset to night.
3. **Weather Effects**: Introduce weather effects (rain, fog) for visual interest and subtle challenge variation.
4. **Landmark Placement**: Add distinctive landmarks to give sense of progression.

## Collectible Placement

### Coin Patterns

1. **Trail Pattern**: Line of coins guiding player through optimal path.
2. **Curve Pattern**: Curved line encouraging smooth lane transition.
3. **Lane Indicator**: Coins indicate which lane will be safe from upcoming obstacles.
4. **Risk/Reward**: Place high-value coins in difficult-to-reach locations.

### Power-Up Placement

1. **Pre-Challenge**: Place before difficult sections to help players.
2. **Recovery**: Place after challenging sequences as reward.
3. **Alternative Paths**: Use to encourage exploration of riskier routes.

### Secret Collectibles

1. **Hidden Pages**: Place notebook pages in non-obvious locations.
2. **Unlockable Areas**: Create secret paths requiring specific actions to access.

## Level Flow Design

### Level Structure

1. **Introduction**: 10-15% of level with simple obstacles to establish rhythm.
2. **Early Challenge**: 15-30% introduce core obstacle patterns.
3. **Mid-game Escalation**: 30-60% ramp up challenge with combined obstacles.
4. **Peak Challenge**: 60-75% create most difficult sequence.
5. **Final Push**: 75-90% maintain challenge but ensure completability.
6. **Victory Lap**: 90-100% celebration section with collectibles and minimal threats.

### Challenge Curve

For each level, map out difficulty on a 1-10 scale:
- Level 1: Peak at 5/10 difficulty
- Level 2: Peak at 6-7/10 difficulty
- Level 3: Peak at 8/10 difficulty

### Player Testing

1. **Difficulty Assessment**: Track first-time player success rates for each section.
2. **Death Mapping**: Note where players fail most frequently and adjust difficulty.
3. **Path Preference**: Observe which routes players take when given choices.

## Implementation Guidelines

### Level Inspector Parameters

Use the level inspector to configure:
- Segment spawn probability tables
- Obstacle density settings
- Collectible frequency and placement
- Power-up rarity settings
- Boulder chase trigger points

### Difficulty Progression

1. **Level 1**: Focus on introduction of basic mechanics. Boulder appears at 75% of level.
2. **Level 2**: Introduce combined obstacles and short challenge sequences. Boulder at 60%.
3. **Level 3**: Introduce complex obstacle patterns, longer challenge sequences. Boulder at 50%.

### Example Level Layout

**Level 1 - City Escape**
1. Start with 3 straight segments with minimal obstacles
2. Introduce simple lane changes with zigzag obstacles
3. Present first jump obstacles individually
4. Mid-point breather with coin rewards
5. Introduce slide obstacles
6. First combined challenge (jump + lane change)
7. Boulder chase sequence
8. Final straight to finish line with coin rewards

## Theming & Storytelling

### Environmental Storytelling

1. **Level 1**: Corporate office environment transitioning to city streets
2. **Level 2**: City outskirts moving into industrial area
3. **Level 3**: Industrial zone leading to laboratory facility

### Narrative Elements

1. **Starting Cutscene**: Brief scene establishing context for the level.
2. **Collectible Notes**: Notebook pages provide story context when collected.
3. **Environmental Details**: Set pieces provide visual storytelling.

## Testing Checklist

Before finalizing a level:
- [ ] Playtest starting from each checkpoint
- [ ] Verify all collectibles are obtainable
- [ ] Check for unintentional shortcuts
- [ ] Validate difficulty curve matches target audience
- [ ] Ensure completion time meets target (1-3 minutes per level)
- [ ] Verify visual elements properly communicate gameplay requirements 