## TODO

 - Ordered list of priorities
	- This will mean a struct to keep track of actions
	- As a start use the struct to 
	  - Make sure I have enough protein. Otherwise chosse another move
	  - Make sure I'm not going to the same place with 2 organisms
 - More aggressive tentacles. Now they just go for close organs (within 2 spaces. Do an A* search and start attacking sooner)
 - Try iterative deepening for A* (it'll probably be needed for building priority lists)

## Test seeds

Lots of Protein near the original root 
seed=-8773989171102688000

No walls
seed=-5857682269723621000

It creates a spore and then blocks it straight away
seed=8116729794577537000

On move 14 I should buld a harvester on 3,6
seed=-9015237039626058000

Lots of Protein all over a big map. My code times out pretty quickly on this
seed=-1361847611390843400

Starting with a spore here would be good
seed=-7134633389896397000

I block myself in pretty early here
seed=-3605073768287950000

Pretty nice battle I should be able to test lots on
seed=8520556949647951000

Lots of proteins, not many walls
seed=-6630469394645055000

A D protein out of reach. I should prioritise getting to it
seed=6911191266081234000

BUG: Unnecessarily destroys harvested spawns
I need a flood fill in there to help me understand when I should leave the harvesters alone
seed=2134750476394718000

BUG: 2 of my organisms go for the same spot on move 8
seed=-6143774278780553000

BUG: Timeout (It no longer times out but this is a good test of sporing. It spores on turns 7 and 8 then runs out of proteins to use them sensibly)
seed=-6809659612317972000

Lots of proteins at the spawn point
seed=5422269799341382000

BUG: On turn 2 I don't "see" a B harvest in 2 moves because of the CONSUME in 1
seed=4756974866352816000