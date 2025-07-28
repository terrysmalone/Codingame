# Implementation

## High level overview

For every game move the following functions are called

GetCommands

&nbsp;&nbsp;&nbsp; UpdateScores

&nbsp;&nbsp;&nbsp; UpdateMoveLists

&nbsp;&nbsp;&nbsp; // Initialise maps and calculators

&nbsp;&nbsp;&nbsp; UpdatePriorities

&nbsp;&nbsp;&nbsp; GetMoveCommands

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; for each agent

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; if MoveRepetitionDetected

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetSpreadMove

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; if current round score is below -40

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetScoreMaximisingMove

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; if priority is DodgingBombs

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetBombDodgeMove

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetSpreadMove (This is only called if GetBombDodgeMove gives no move)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; if current round score is below -20

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetScoreMaximisingMove

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; if priority is FindBestAttackPosition (except for the bomber agent)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetBestAttackPosition

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; if current round score is below -0

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetScoreMaximisingMove

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetBestAdvancingMove

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; UpdateForCollisions


&nbsp;&nbsp;&nbsp; GetActionCommands

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; for each agent

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetThrowAction

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; GetShootAction

&nbsp;&nbsp;&nbsp; GetCommandStrings

&nbsp;&nbsp;&nbsp; ResetIntentions


Generally, all of the "Get" functions check to see if move/action has laready been found, and if so, they do nothing.

My initial plan was to have a more sophisticated priority system, where each agent had its own ordered list of priorities that could be updated as new information was found. In the end though, A mix of a couple of important overriding priorities and the fall-back method above, was enough.




