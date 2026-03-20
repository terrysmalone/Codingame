# Current Submission

## Commit in Arena

cd1e4b0bca5646b3d4cc8b1f865924d2d0b49e1b

## Changes since last Test in Arena
- Implement bullying ( On new branch)

# TO DO

## Specific

- Don't go for a head clash if the opponent can not take it, and I'd end up trapping myself in
- Don't count tails of opponent if they can get a powerup next move

## General

1. Take more care not to get blocked
2. How can I attack better
3. How can I be more aware of my own snakes positions

Example of 1 and 3
Example 
- seed=-1417240227969381000
- Me player 1, boss player 2
- Move 87 I get tangled and a snake starts dying

------------------------------------------------------------

seed=1870029513596017000
player 1 arena player 2 ide
On move 35 my own snakes clash. This is a good place to test best combos

IDE 1, arena 2
Move 18 I pick the wrong attack
seed=3478893539146914000

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


