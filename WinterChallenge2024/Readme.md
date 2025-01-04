## TODO

 - [x] Fix `ArgumentOutOfRangeException` bug listed below
 - [x] Fix bug where ene organism destroys a protein on the same move that another harvests it.
 - [ ] Fix bug where I don't get any tentacle moves at the start (because it can't see opponents 1 move away)
 - [ ] Harvest attacks shouldn't prioritise placing tentacles (add others and prioritise BASIC, unless we have a ton of tentacle stock)
 - [ ] Fix bug - Why don't I harvest the C proteins on turn 5
 - [ ] Fix bug - Unnecessarily destroys harvested spawns

## Seeds

### BUGS

BUG: One organism destroys a protein on the same move that another harvests it. 
I need to update check that we're not harvesting on the same spot....
A D protein out of reach. I should prioritise getting to it
seed=6911191266081234000

BUG: Why don't I get any tentacle moves at the start? (I can't "see" opponent organisms if they're right next to me)
seed=6928531867582551000

More tests of the above
seed=-5857682269723621000
seed=8520556949647951000

BUG: Why don't I harvest the C proteins on turn 5
seed=8116729794577537000

BUG: Why don't I harvest the D protein on turn 2
seed=5555532766085083000

BUG: Unnecessarily destroys harvested spawns
Organ 1 is trapped and starts destroying harvested proteins from the other organism.
I need a flood fill in there to help me understand when I should leave the harvesters alone
seed=2134750476394718000

BUG: I spore twice in one move
seed=5555532766085083000

### TESTING 

TEST: Good test of sporing early
seed=-6809659612317972000

TEST: Lots of proteins early start. Hard fought battle
I lost 69-68 to Boss 7 on move 100
seed=5422269799341382000

TEST: This is a fairly close won battle. We block off from each other early. Make sure I then use up all available space
seed=-6143774278780553000

TEST: Lots of proteins, not many walls. Test of battles
seed=-6630469394645055000

TEST: Lots of walls to navigate here. I lose this battle in testing
seed=-3605073768287950000

TEST: Early battle I lose badly
seed=-7134633389896397000