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

# Pre gold refactor sanity checks

Test the Aren against the IDE in a number of specific tests. Results should not change

| Seed                      | player 1 | player 2 | Pre refactor results |
| ------------------------  | -------- | -------- | -------------------- |
| seed=3665131989781587000  | Arena    | IDE      |                      |
|                           | IDE      | Arena    |                      |
| seed=2409305125464446000  | Arena    | IDE      |                      |
|                           | IDE      | Arena    |                      |
| seed=-8859283512535219000 | Arena    | IDE      |                      |
|                           | IDE      | Arena    |                      |
| seed=-1157081042072761300 | Arena    | IDE      |                      |
|                           | IDE      | Arena    |                      |


seed=-5987309072221063000

Move 23: I lose 603 - 2



