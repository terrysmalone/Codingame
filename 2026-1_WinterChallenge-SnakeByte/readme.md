# Current Submission

## Commit in Arena

35e497605bacc59114856d45629e223bcaa7dcd8

## Changes since last Test in Arena

- Fixed bug where bot would keep searching after finding a head clash move 9b354cec8a958379aa503a87e6179481ba81581e

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


