# TODO

## Bugs

* Scores should be the same no matter what side I am. Verify.

## Improvements

I need to start disrupting before my opponent finishes tracks

Example - seed=-8220911213405463000 (battle 153)

At move 7 he starts a path from 2 to 1. I place some in there on turn 8, then by turn 9 give up because it's clear I won't have enough.

Solution: Start logging desire paths and seeing how many tracks each of us have on it. Score region strength based on that.

Questions: Do we reuse desire paths from creating tracks? That won't cover all paths, just the ones I think are good. 
           Maybe that's fine for now...

## Useful seeds

seed=8074289755211185000

4 to 8 and 3 to 4 are good examples of why the shortest path isn't always best. They go through mountains and rivers, so are very expensive

# Progress

f05509c54374211605954d8148a2ac5e7b7692ad

	Submitted: 2026-09-08 08:07pm
	Rank: 180

bfba5ccdb1cbd010a43fd7c17db200300a088ac5

	Changes: Look at multiple desire paths to use up all action points
	Submitted: 2026-09-08 10:37pm
	Rank: 159

533cdc9a4d822145dd374a11b10a048afa76f187

	Changes: Finish disrupting a region if it's started
	Submitted: 2026-09-08 11:02pm
	Rank: 145

43ca8362dd57bf6e24c9920273336d398dedc7ff

	Changes: Use active connections to disrupt regions
	Submitted: 2026-09-09 09:43pm
	Rank: (168) 180 

	NOTE: I found a bug which is causing me to try to place track where it already is

fd97a90c59e8b94b64c70d1a7d5bef24a8048041

	Changes: Fix bug where tracks were being placed on top of existing tracks
	Submitted: 2026-09-09 10:10pm
	Rank: (198) to 166

ea9d068376cdd5fbabbce068d9447454a3ebae1e

	Changes: Fix bug in calculating region score
	Submitted: 2026-09-09 11:00pm
	Rank: (166) to 176

f644c7bc61f750d7455aa62319c64d50f63588af

	Changes: Fix another bug in calculating region score
	Submitted: 2026-09-09 11:27pm
	Rank: (176) to 200

f644c7bc61f750d7455aa62319c64d50f63588af

	Changes: Experiment with excluding instability regions from desire paths.
	Submitted: 2026-09-10 12:22am
	Rank: (188) to 134

2aa91764f71fbdb0a802bad5dccd820a042ee97a

	Changes: First pass at counting who owns most tiles in a path before painting
	Submitted: 2026-09-10 09:32pm
	Rank: (134) to 194

c640216ee416fc8027159efe8b48c62824edd8b2

	Changes: Do a second check for paths if the first gives nothing
	Submitted: 2026-09-10 10:05pm
	Rank: (189) to 158

8d10c46f7a1feb6aae24bac3ea94753b8c4fc083

	Changes: Prioritise disrupting regions with more enemy tracks
			 Fixed bug in pointing cells
	Submitted: 2026-09-10 11:27pm
	Rank: (164) to 124

415e39b7dcf439f83505ad5374c8dd28a83d309a

	Changes: Fix remaining points bug
	Submitted: 2026-09-11 12:09am
	Rank: (124) to 151

405574ea4f28a1d89abf109579b63c907bbca5e0

	Changes: Start using up leftover paint points
	Submitted: 2026-09-11 12:29am
	Rank: (151) to -

4656f92cb4de3d5e1abc9fa2fab597f13a0ea1c4

	Changes: Don't place tracks on towns
	Submitted: 2026-09-11 12:49am
	Rank: (151) to 152

915b4397069c06ca3ec9c10ff583fa437fc48cf2

	Changes: Implement cell scoring
	Submitted: 2026-09-12 12:08am
	Rank: (157) to 155

376cb99be7332d79a08791ffdb139cb9ee4c8f77

	Changes: Swap ordering
	Submitted: 2026-09-12 12:34am
	Rank: (155) to 133

fb5a9bf3f0a37d6fa330ee9007c81a6273b7b918

	Changes: Swap ordering
	Submitted: 2026-09-12 02:53pm
	Rank: (137) to 146

de9246b52c67268569cc932279b0189e8400ea47

	Changes: If we can complete a path this turn do it
	Submitted: 2026-09-12 04:04pm
	Rank: (147) to 149

6b324e36f25a625827f31c43a88b0503ddf9da0c

	Changes: Make search for region more efficient
	Submitted: 2026-09-12 08:15pm
	Rank: (153) to 146

ad8a0df0ab3a9523d199372853870503e370be4c

	Changes: Don't go for quick wins
	Submitted: 2026-09-12 09:05pm
	Rank: (146) to 161

491a873a5ed2b40a8457bda8230a4659f1b4b7b6

	Changes: Don't place tracks in same town path
	Submitted: 2026-09-12 10:12pm
	Rank: (164) to 182

ace4e7bbcaafb27f79374427dd8623968a360cf4

	Changes: Reverted back to 6b324e36f25a625827f31c43a88b0503ddf9da0c. I'll need to bring in some of the above 2
	Submitted: 2026-09-12 11:55pm
	Rank: 165

ed4ec0caeb75c1b8b273e0c940de9f711ba0d97a

	Changes: Remove ordering by town count
	Submitted: 2026-09-13 08:07pm
	Rank: (150) to 191

55a927055ce70597cd443fd665d25bbbd5810cf0

	Changes: Prioritise short and long paths separately
             Run two passes through pathfinding
	Submitted: 2026-09-13 09:48pm
	Rank: (190) to 194

3c9d33374f42b758227bdf9df3365d1b645eaa29

	Changes: Remove complex ordering. Just focus on path length
	Submitted: 2026-09-13 10:37pm
	Rank: (195) to 215

NOTE: Testing ace4e7bbcaafb27f79374427dd8623968a360cf4 
	
	Submitted: 2026-09-13 11:37pm
	Rank: (215) to 188

---------------------

# Analysis on changes since best recent result

## ace4e7bbcaafb27f79374427dd8623968a360cf4 - 165

Tweak ordering - Put ActionCost before ShortestRemainingCount
Remove ordering by town count - Remove get town count from ordering *** NOTE: I don't have any verification that this was a good idea *** 

## ed4ec0caeb75c1b8b273e0c940de9f711ba0d97a - dropped 41 places to 191

Prioritise cells where there are no high action costs along the whole track (NOTE: This is later removed)
Prioritise short and long paths separately
Run two passes through pathfinding

## 55a927055ce70597cd443fd665d25bbbd5810cf0 - dropped 4 places to 194

Remove complex ordering. Just focus on path length - 

## 3c9d33374f42b758227bdf9df3365d1b645eaa29 - dropped 20 places to 215

-------------------

16e6bf3bfd21d4dd4b3345a3ee21256c6e5e2ad4

	Changes: Add town ordering count back in 
	Submitted: 2026-09-14 12:13am
	Rank: (188) to 252

Note: ace4e7bbcaafb27f79374427dd8623968a360cf4 is still better than the others by far.
      I'm going to revert back to that and try to incrementally add things in.

ace4e7bbcaafb27f79374427dd8623968a360cf4

	Changes: Revert
	Submitted: 2026-09-14 09:13am
	Rank: (255) to 188

36e6936021c4b785d42326f4a452736563555db1
	
	Changes: Add a fallback exclude points withoud unstable regions
	Submitted: 2026-09-14 11:41am
	Rank: (195) to 162	

b64f96412ff2e09493f522cf3f5e9a974d3a0f9e
	
	Changes: Add region unstability scorer
	Submitted: 2026-09-14 04:04m
	Rank: (172) to 152

Promoted to silver league 

9cddccd8eff9ceda858a05d255e0c28abf4e0028

	Changes: Track diff between path size and action cost
	Submitted: 2026-09-15 10:53m
	Rank:(43 silver) to 19

ac7c715012bb71023f8fe5794eb7fc4cc7e88728

	Changes: Path find using action cost
	Submitted: 2026-09-15 02:11pm
	Rank:(13) to 10

61a451eea29f837e901cc11ad1bd89e46dd3e56b

	Changes: No longer block multiple cells in the same region
	Submitted: 2026-09-15 08:42pm
	Rank:(14) to 10

Promoted to Gold league

c21a0bf405a1dcf8e7c3e5e12360fc027547d1f6

	Changes: Score incomplete desire paths
	Submitted: 2026-09-16 10:38pm
	Rank:(124) to 117	

a3bca54481acbf804940b865e2379e9b9385e220

	Changes: Order potential finishers by largest size first
	Submitted: 2026-09-17 11:02pm
	Rank:(118) to 122

(124)



