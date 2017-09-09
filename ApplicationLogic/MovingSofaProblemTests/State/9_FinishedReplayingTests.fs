namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote

open MovingSofaProblem.State

module FinishedReplayingTests =

    [<Test>]
    let ``Does not change the paths``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)

        test <@ finishedReplayingState.InitialPath = beforeState.InitialPath @>
        test <@ finishedReplayingState.PathToReplay = beforeState.PathToReplay @>

    [<Test>]
    let ``Current path step is last step of replaying state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState () // Should have 2 steps
        let replayingFirstStep = Replaying.PlayNextSegment(beforeState, 0.0f)
        let replayingSecondStep = Replaying.PlayNextSegment(replayingFirstStep.NewState, 0.2f)
        let secondStepComplete = Replaying.PlayNextSegment(replayingSecondStep.NewState, 0.4f)

        let newState = secondStepComplete.NewState
        test <@ newState.Mode = GameMode.FinishedReplaying @>
        test <@ newState.CurrentPathStep = replayingSecondStep.NewState.CurrentPathStep @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.replayingState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)

        let expectedSpokenState = 
            "Right now I have finished replaying my solution. You can " + 
            "Say 'Replay solution' to replay again from the beginning."

        GameState.SayStatus(finishedReplayingState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef finishedReplayingState

(******************************* STATE TRANSITION ACCESSIBILITY TESTS *******************************)

    [<Test>]
    let ``Can finish replaying from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Can finish replaying from the WaitingToReplay state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Can finish replaying from the Measuring state``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Can finish replaying from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Can finish replaying from the StoppedFollowing state``() = 
        let (beforeState, _) = StateTestUtilities.stoppedFollowingState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Can finish replaying from the PathSimplified state``() = 
        let (beforeState, _) = StateTestUtilities.pathSimplifiedState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>

    [<Test>]
    let ``Can finish replaying from the SolutionFound state``() = 
        let (beforeState, _) = StateTestUtilities.solutionFoundState ()   
        let finishedReplayingState = FinishedReplaying.IsFinishedReplaying(beforeState)
        test <@ finishedReplayingState.Mode = GameMode.FinishedReplaying @>


