# Current Submission

## Commit in Arena

070b318ae4652b056dfe3b0df60bd554980370a9

## Changes since last Test in Arena

# TO DO

I should add a heuristic to exclude some checks. (For exmple, if distance frm powersource to nearest platform is more
than snake length don't bother. Although I should exclude ones that are lower since gravity can help...

When I increase search distance I redo the previous checks

For head clash checking I need to make sure that I properly exclude my own snakes. THere are different rules.
For example, I don't want to exclude them for both snakes.

At some point, I'll want the pathfinder to be sensible about whether it counts a tail as something it can't move to.
It should move to it in most cases, but it can't if the snake is going to grow.


Implement attacking, especially when I don't have a power up to follow and I'm bigger. The below gives a good example of it at
around move 18. 

seed=1185552052233567700
player 1: ziemekb player 2: me - last battle in arena: 67


# Levels


