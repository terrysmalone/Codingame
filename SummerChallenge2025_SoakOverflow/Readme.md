# TODO


## General 

* Remove currentMovePoints and use agent.MoveIntention.Nove
* pull move and action out of th same foreach
* Split move and action stuff into functions
* 
## Spreading

* Only spread out if we're actually close to another agent

## Bombing

* Can I dodge bombs actively?
* I should aim for the centre and not an edge
* Is it viable to bomb exactly where an enemy is standing?

## Shooting

# Tests

| Seed                      | player 1 | player 2 | Run 1  | Run 2  |
| ------------------------  | -------- | -------- | ------ | ------ |
| seed=3665131989781587000  | Arena    | IDE      | Arena  | Arena  |
|                           | IDE      | Arena    | Arena  | Arena  |
| seed=2409305125464446000  | Arena    | IDE      | IDE    | IDE    |
|                           | IDE      | Arena    | Arena  | Arena  |
| seed=-8859283512535219000 | Arena    | IDE      | IDE    | IDE	|
|                           | IDE      | Arena    | IDE    | IDE    |
| seed=-1157081042072761300 | Arena    | IDE      | Arena  | IDE    |
|                           | IDE      | Arena    | Arena  | Arena  |

# League boss analysis

seed=-3712306056014127000

On move 18 I lose 1, 629

# Legend boss fight analysis

&#x2611;

&#x2612;

| Seed                      | player | Early control | Bombs    | Shooting | Late control | Win      | Score      | Notes |
| ------------------------- | ------ | ------------- | -------- | -------- | ------------ | -------- | ---------- | ----- |
| seed=4030046837536737300  | 1      | &#x2612;      | N/A      | &#x2612; | N/A          | &#x2612; | 26 - 648   | My two range agent stayed the back and didn't bomb or shoot all game. So did his |
|                           | 2      | &#x2612;      | N/A      | &#x2612; | N/A          | &#x2612; | 648 - 26   | My two range agent stayed the back and didn't bomb or shoot all game. So did his |
| seed=-5938950415254304000 | 1      | -             | &#x2612; | &#x2612; | &#x2611;     | &#x2612; | 519 - 1135 | My two range agent stayed the back and didn't bomb or shoot all game             |

# Changes since last Test in Arenan

* Bomber doesn't dodged bombs


# TODO

## If I'm generating a higher score, and the agent is in cover, just stay still.

## I need to stop agents oscillating between two cells/states. For example, Advancing/Maximising Score/Advancing etc
Idea: Store repetition count. If it gets too high then just do something different to try to break the cycle

##  Add better early game pathfinding to spread them out 

Examples:
seed=-6630035130914770000 - I lose
seed=-6113375948142029000 - I win
seed=2117291074604353500 - This doesn't need it. Use this to verift we don't break the happy path

Idea: Save agents full paths eachturn. Pass these into the A* search and exclude them from being stepped on. If we can't find anything this way, then revert back to a normal search

TODO: Stop caring about this after the initial startup
