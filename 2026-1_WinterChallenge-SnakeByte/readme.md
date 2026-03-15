# Current Submission

## Commit in Arena

6b5c53e740eb8ae4c9d410952fdfb07ce3fde9d0

## Changes since last Test in Arena

- Started template for gettting snakes to climb ledges


# TO DO

seed=-2697487452895781000
player 1 ide player 2 ide

On move 3 snake 5 and snake 6 both go for the power source on 16,7. My old implementation didn't do this
becuse it excluded previous moves from searches. I need to add multiple moves in find best paths and then
discourage snakes going for the same power source.

Note: The arena beats me in this game because of that.


------------------------------------------------------------

seed=1870029513596017000
player 1 arena player 2 ide
On move 35 my own snakes clash. This is a good place to test best combos

IDE 1, arena 2
Move 18 I pick the wrong attack
seed=3478893539146914000

------------------------------------

Scoring moves
Prefer getting power sources near the middle of the map
Prefer getting power sources when i'm guaranteed to win
Prefer not overlapping paths (this should encourage my snakes to sread out more

When calculating scores, take direction into account. For example, if 5 moves have UP as their next direction, and they all score 50, do we really want 
LEFT, which only has one score if it's scored 60? Maybe not, maybe we do. It's something to consider.

-------------------------------------
Before any moves, do a look ahead of all possible move combinations. 
For each one check for:
- Me getting blocked in
- opponent getting blocked in
- Head clashes

If any of the move sets increase or decrease from the current svore, these are classed as golden/killing movesets.
For these, find the common denominator move, and make that either forced, or score it insanely highly. The common denominator
means that if moving snake 1 results in a plus score no matter what we do with the others, force that one. If there's no commpn denominator, 
score all moves as golden/killing.

-------------------------------------

Test: Just do one look ahead and time it to see if it's feasible. 

If an opponent can get to a snake before me. Find another

At some point, I'll want the pathfinder to be sensible about whether it counts a tail as something it can't move to.
It should move to it in most cases, but it can't if the snake is going to grow.


Implement attacking, especially when I don't have a power up to follow and I'm bigger. The below gives a good example of it at
around move 18. 

seed=1185552052233567700
player 1: ziemekb player 2: me - last battle in arena: 67


# Levels


