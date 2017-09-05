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
    let ``Can move along the current segment from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let replayingState = 
            Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner).NewState
        test <@ replayingState.Mode = GameMode.Replaying @>
(*
    [<Test>]
    let ``Cannot play the next segment from the FinishedReplaying state``() = 
        let (beforeState, _) = StateTestUtilities.finishedReplayingState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.FinishedReplaying @>
        failwith "Need to indicate that the segment can't be replayed"

    [<Test>]
    let ``Can replay the current segment from the FinishedReplaying state``() = 
        let (beforeState, _) = StateTestUtilities.finishedReplayingState ()   
        let replayingState = Replaying.ReplayCurrentSegment(beforeState, 0.0f).NewState
        test <@ replayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Cannot move along the current segment from the FinishedReplaying state``() = 
        let (beforeState, _) = StateTestUtilities.finishedReplayingState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.FinishedReplaying @>
        test <@ stateTransition.SideEffects.Count = 0 @>

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
    let ``Cannot move along the current segment from the Starting state``() = 
        let (beforeState, _) = StateTestUtilities.initialState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Starting @>
        test <@ stateTransition.SideEffects.Count = 0 @>

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
    let ``Cannot move along the current segment from the Measuring state``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Measuring @>
        test <@ stateTransition.SideEffects.Count = 0 @>

    [<Test>]
    let ``Cannot play the next segment from the Following state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Following @>

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
    let ``Cannot move along the current segment from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.Following @>
        test <@ stateTransition.SideEffects.Count = 0 @>

    [<Test>]
    let ``Cannot play the next segment from the StoppedFollowing state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.StoppedFollowing @>

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
    let ``Cannot move along the current segment from the StoppedFollowing state``() = 
        let (beforeState, _) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.StoppedFollowing @>
        test <@ stateTransition.SideEffects.Count = 0 @>

    [<Test>]
    let ``Cannot play the next segment from the PathSimplified state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.pathSimplifiedState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.PathSimplified @>

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
    let ``Cannot move along the current segment from the PathSimplified state``() = 
        let (beforeState, _) = StateTestUtilities.pathSimplifiedState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.PathSimplified @>
        test <@ stateTransition.SideEffects.Count = 0 @>

    [<Test>]
    let ``Cannot play the next segment from the SolutionFound state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = Replaying.PlayNextSegment(beforeState, 0.0f)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.SolutionFound @>

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
    let ``Cannot move along the current segment from the SolutionFound state``() = 
        let (beforeState, _) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.SolutionFound @>
        test <@ stateTransition.SideEffects.Count = 0 @>

    [<Test>]
    let ``Cannot move along the current segment from the WaitingToReplay state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let stateTransition = Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)

        let replayingState = stateTransition.NewState
        test <@ replayingState.Mode = GameMode.WaitingToReplay @>
        test <@ stateTransition.SideEffects.Count = 0 @>

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
    let ``Moving along the current segment changes no paths in the state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let keepReplayingState = 
            Replaying.KeepReplaying(beforeState, 0.1f, StateTestUtilities.measurePositioner).NewState

        test <@ keepReplayingState.PathToReplay = beforeState.PathToReplay @>
        test <@ keepReplayingState.InitialPath = beforeState.InitialPath @>
        test <@ Replaying.FirstStep(keepReplayingState) = WaitingToReplay.FirstStep(beforeState) @>
        test <@ keepReplayingState.Measure.transform.position = beforeState.Measure.transform.position @>
        test <@ keepReplayingState.Measure.transform.rotation = beforeState.Measure.transform.rotation @>

    [<Test>]
    let ``Playing the first segment does not change the state's current step'``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        test <@ replayingState.CurrentPathStep = beforeState.CurrentPathStep @>

    [<Test>]
    let ``Playing the next segment after the first advances the state's current step'``() = 
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
    let ``Replaying the current segment after the first does not change the current step``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let replayingState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        let replayingCurrentState = Replaying.ReplayCurrentSegment(replayingState, 0.0f).NewState
        test <@ replayingCurrentState.CurrentPathStep = replayingState.CurrentPathStep @>

    [<Test>]
    let ``Moving along the current segment does not change the state's current step``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let keepReplayingStateTransition = 
            Replaying.KeepReplaying(beforeState, 0.1f, StateTestUtilities.measurePositioner)
        let newState = keepReplayingStateTransition.NewState
        test <@ newState.CurrentPathStep = beforeState.CurrentPathStep @>

        let sideEffect = keepReplayingStateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingMeasure = sideEffect.Invoke(newState)

        test <@ stateAfterMovingMeasure.CurrentPathStep = beforeState.CurrentPathStep @>
    
    [<Test>]
    let ``Moving along the current segment has one side effect``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let keepReplayingStateTransition = 
            Replaying.KeepReplaying(beforeState, 0.1f, StateTestUtilities.measurePositioner)
        test <@ keepReplayingStateTransition.SideEffects.Count = 1 @>

    [<Test>]
    let ``Doesn't move the measure along the current segment if no time has elapsed since playing started``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let keepReplayingStateTransition = 
            Replaying.KeepReplaying(beforeState, 0.0f, StateTestUtilities.measurePositioner)
        let newState = keepReplayingStateTransition.NewState

        let sideEffect = keepReplayingStateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingMeasure = sideEffect.Invoke(newState)

        test <@ stateAfterMovingMeasure.Measure.transform.position = 
                 beforeState.Measure.transform.position @>
        test <@ stateAfterMovingMeasure.Measure.transform.rotation = 
                 beforeState.Measure.transform.rotation @>

    [<Test>]
    let ``Moves the measure along the current segment in proportion to the time elapsed since playing started``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()
        let keepReplayingStateTransition = 
            Replaying.KeepReplaying(beforeState, 1.0f, StateTestUtilities.measurePositioner)
        let newState = keepReplayingStateTransition.NewState

        let sideEffect = keepReplayingStateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingMeasure = sideEffect.Invoke(newState)

        // Translation Speed is 0.7 units per second; the solution moves toward (1,2,3) 
        // The triple (0.1870829,0.3741657,0.5612485) is 0.7 units long.
        test <@ stateAfterMovingMeasure.Measure.transform.position = Vector(0.1870829f,0.3741657f,0.5612485f) @>

        // Rotation Speed is 20 degrees per second; the solution moves toward (0,0,90,0) with our dummy rotation 
        test <@ stateAfterMovingMeasure.Measure.transform.rotation = Rotation(0.0f, 0.0f, 20.0f, 0.0f) @>

    [<Test>]
    let ``Leaves the measure at the end of the current segment if the entire translation and rotation have been traversed``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()
        // Translation Speed is 0.7 units per second; the solution moves to (1,2,3) 
        // The square root of 14 units (3.74) should thus take a bit over 5 seconds. 
        // Rotation speed is 20 degrees per second; moving toward (0,0,90,0) should take 4.5 seconds
        let keepReplayingStateTransition = 
            Replaying.KeepReplaying(beforeState, 6.0f, StateTestUtilities.measurePositioner)
        let newState = keepReplayingStateTransition.NewState

        let sideEffect = keepReplayingStateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingMeasure = sideEffect.Invoke(newState)

        test <@ stateAfterMovingMeasure.Measure.transform.position = StateTestUtilities.solutionSecondPosition @>
        test <@ stateAfterMovingMeasure.Measure.transform.rotation = StateTestUtilities.ninetyDegreesAroundZ @>

    [<Test>]
    let ``Continues translating but not rotating if the entire rotation has been traversed``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()
        let keepReplayingStateTransition = 
            Replaying.KeepReplaying(beforeState, 5.0f, StateTestUtilities.measurePositioner)
        let newState = keepReplayingStateTransition.NewState

        let sideEffect = keepReplayingStateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingMeasure = sideEffect.Invoke(newState)

        // The triple (0.9354143,1.870829,2.806243) is 3.5 units long - 0.7 * 5 sec. 
        test <@ stateAfterMovingMeasure.Measure.transform.position = Vector(0.9354143f,1.870829f,2.806243f) @>
        test <@ stateAfterMovingMeasure.Measure.transform.rotation = StateTestUtilities.ninetyDegreesAroundZ @>

    [<Test>]
    let ``Continues rotating but not translating if the entire rotation has been traversed``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()
        // Make sure Measure is positioned at the end of the first leg
        let keepReplayingFirstLegTransition = 
            Replaying.KeepReplaying(beforeState, 6.0f, StateTestUtilities.measurePositioner)
        let firstLegSideEffect = keepReplayingFirstLegTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingFirstLeg = firstLegSideEffect.Invoke(keepReplayingFirstLegTransition.NewState)

        // Traverse the second segment; reset clock at 0
        let playingSecondSegmentState = Replaying.PlayNextSegment(stateAfterMovingFirstLeg, 0.0f).NewState

        // The second segment traversal is 5.6 units long, will be covered in 8.1 seconds
        // The second segment rotation is 180 degrees about Y, requires 9 seconds
        let keepReplayingSecondLegTransition = 
            Replaying.KeepReplaying(playingSecondSegmentState, 8.5f, StateTestUtilities.measurePositioner)
        let newState = keepReplayingSecondLegTransition.NewState

        let secondLegSideEffect = keepReplayingSecondLegTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterMovingSecondLeg = secondLegSideEffect.Invoke(newState)

        test <@ stateAfterMovingSecondLeg.Measure.transform.position = StateTestUtilities.solutionThirdPosition @>
        test <@ stateAfterMovingSecondLeg.Measure.transform.rotation = Rotation(0.0f, 170.0f, 90.0f, 0.0f) @>

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
