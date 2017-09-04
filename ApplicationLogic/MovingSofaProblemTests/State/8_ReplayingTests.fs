namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module ReplayingTests =

    [<Test>]
    let ``Can play next segment from the WaitingToReplay state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.Replaying @>

    [<Test>]
    let ``Can replay the current segment from the WaitingToReplay state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.Replaying @>
(*
    [<Test>]
    let ``Can play next segment from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.Replaying @>

    [<Test>]
    let ``Can replay the current segment from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.Replaying @>

    [<Test>]
    let ``Cannot play the next segment from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.finishedReplayingState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.FinishedReplaying @>
        failwith "Need to indicate that the segment can't be replayed"

    [<Test>]
    let ``Can replay the current segment from the FinishedReplaying state``() = 
        let (beforeState, _) = StateTestUtilities.finishedReplayingState ()   
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.FinishedReplaying @>
*)
    let invalidNextSegmentMessage = "I can't replay the solution because I have no solution to replay."
    let invalidReplayStepMessage = "I can't replay the current step because I currently don't have a step to replay."

    [<Test>]
    let ``Cannot play the next segment from the Starting state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.initialState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Starting @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidNextSegmentMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot replay the current segment from the Starting state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.initialState ()   
        let stateTransition = Replaying.ReplayCurrentSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Starting @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidReplayStepMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot play the next segment from the Measuring state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.measuringState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Measuring @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidNextSegmentMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot replay the current segment from the Measuring state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.measuringState ()   
        let stateTransition = Replaying.ReplayCurrentSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Measuring @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidReplayStepMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot play the next segment from the Following state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Following @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidNextSegmentMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot replay the current segment from the StoppedFollowing state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = Replaying.ReplayCurrentSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.StoppedFollowing @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidReplayStepMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot play the next segment from the StoppedFollowing state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.StoppedFollowing @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidNextSegmentMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot replay the current segment from the PathSimplified state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.pathSimplifiedState ()   
        let stateTransition = Replaying.ReplayCurrentSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.PathSimplified @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidReplayStepMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot play the next segment from the PathSimplified state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.pathSimplifiedState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.PathSimplified @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidNextSegmentMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot replay the current segment from the SolutionFound state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = Replaying.ReplayCurrentSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.SolutionFound @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidReplayStepMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot play the next segment from the SolutionFound state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.SolutionFound @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidNextSegmentMessage spokenStateRef replayingState

    [<Test>]
    let ``Cannot replay the current segment from the Following state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()   
        let stateTransition = Replaying.ReplayCurrentSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Following @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidReplayStepMessage spokenStateRef replayingState

    [<Test>]
    let ``Playing the next segment has no immediate side effects``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let sideEffects = Replaying.PlayNextSegment(beforeState, 0.0f).SideEffects
        test <@ sideEffects.Count = 0 @>

    [<Test>]
    let ``Replaying the current segment has no immediate side effects``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let sideEffects = Replaying.ReplayCurrentSegment(beforeState, 0.0f).SideEffects
        test <@ sideEffects.Count = 0 @>

    [<Test>]
    let ``Playing the next segment changes no paths in the state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState

        test <@ replayingState.PathToReplay = beforeState.PathToReplay @>
        test <@ replayingState.InitialPath = beforeState.InitialPath @>
        test <@ Replaying.FirstStep(replayingState) = WaitingToReplay.FirstStep(beforeState) @>
        test <@ replayingState.Measure.transform.position = beforeState.Measure.transform.position @>
        test <@ replayingState.Measure.transform.rotation = beforeState.Measure.transform.rotation @>

    [<Test>]
    let ``Replaying the current segment changes no paths in the state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState

        test <@ replayingState.PathToReplay = beforeState.PathToReplay @>
        test <@ replayingState.InitialPath = beforeState.InitialPath @>
        test <@ Replaying.FirstStep(replayingState) = WaitingToReplay.FirstStep(beforeState) @>
        test <@ replayingState.Measure.transform.position = beforeState.Measure.transform.position @>
        test <@ replayingState.Measure.transform.rotation = beforeState.Measure.transform.rotation @>

    [<Test>]
    let ``Playing the first segment does not change the state's current step'``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        test <@ replayingState.CurrentPathStep = beforeState.CurrentPathStep @>

    [<Test>]
    let ``Playing the next segment advances the state's current step'``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        let nextReplayingState = Replaying.PlayNextSegment(replayingState, 0.0f).NewState
        test <@ nextReplayingState.CurrentPathStep <> beforeState.CurrentPathStep @>
        test <@ nextReplayingState.CurrentPathStep.Value.StartNode =
                 beforeState.CurrentPathStep.Value.EndNode @>

    [<Test>]
    let ``Replaying the first segment does not change the current step``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState
        test <@ replayingState.CurrentPathStep = beforeState.CurrentPathStep @>

    [<Test>]
    let ``Replaying the current segment does not change the current step``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        let replayingCurrentState = Replaying.ReplayCurrentSegment(replayingState, 0.0f).NewState
        test <@ replayingCurrentState.CurrentPathStep = replayingState.CurrentPathStep @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.waitingToReplayState ()
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState

        let expectedSpokenState = 
            "Right now I am in the middle of replaying the solution. " + 
            "You can Say 'Next' to replay the next step, 'Again' to replay " + 
            "the current step, or 'Replay solution' to start over from the beginning."

        GameState.SayStatus(replayingState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef replayingState
