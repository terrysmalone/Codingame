## TODO

 - Ordered list of priorities
	- This will mean a struct to keep track of actions
 - More aggressive tentacles. Now they just go for close organs (within 2 spaces. Do an A* search and start attacking sooner)
	

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

BUG: When trying to unblock myself I often make a very silly
decision and destroy harvesters that don't let me unblock.
I need a flood fill in there to help me understand when I should leave the harvesters alone
seed=2134750476394718000