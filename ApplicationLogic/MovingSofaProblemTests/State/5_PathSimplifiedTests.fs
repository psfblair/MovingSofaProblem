namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module PathSimplifiedTests =

    let simplifyPathFrom state = 
        PathSimplified.SimplifyPath(state
                                   , StateTestUtilities.measurePositionAndRotationAfterDropped
                                   , fun state -> state)

    [<Test>]
    let ``Initializes the initial path with simplified path ending in the point at which the measure was dropped``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst
        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState

        let expectedInitialPath = PathHolder()
        expectedInitialPath.Add(StateTestUtilities.measureWhenStartedFollowing, 0.0f)
        expectedInitialPath.Add(StateTestUtilities.measurePositionAndRotationAfterDropped, 0.1f)

        test <@ pathSimplifiedState.InitialPath = expectedInitialPath @>

    [<Test>]
    let ``Initializes the path to replay with the simplified initial path``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst
        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState

        let expectedPathToReplay = PathHolder()
        expectedPathToReplay.Add(StateTestUtilities.measureWhenStartedFollowing, 0.0f)
        expectedPathToReplay.Add(StateTestUtilities.measurePositionAndRotationAfterDropped, 0.1f)

        test <@ pathSimplifiedState.PathToReplay = expectedPathToReplay @>

    [<Test>]
    let ``First step is first step of simplified path``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst

        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState
        let first = StateTestUtilities.measureWhenStartedFollowing
        test <@ GameState.FirstStep(pathSimplifiedState).Value.StartNode.Value = 
                    Breadcrumb(first.Position, first.Rotation, 0.0f) @>

        let second = StateTestUtilities.measurePositionAndRotationAfterDropped
        test <@ GameState.FirstStep(pathSimplifiedState).Value.EndNode.Value = 
                    Breadcrumb(second.Position, second.Rotation, 0.1f) @>

    [<Test>]
    let ``Has no current path step``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst

        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState
        test <@ pathSimplifiedState.CurrentPathStep = MaybePathStep.None @>

    [<Test>]
    let ``Has three side effects``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = simplifyPathFrom beforeState

        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 3 @>

    [<Test>]
    let ``Speaks the state in the first side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        let sideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.PathSimplified @>

        test <@ !spokenStateRef = "Path simplified. Finding solution." @>

    [<Test>]
    let ``Starts finding the solution and then transitions to SolutionFound in the second side effect``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()

        let mutable solutionFinderCalled = false
        let stateTransition = 
            PathSimplified.SimplifyPath(beforeState
                                       , StateTestUtilities.measurePositionAndRotationAfterDropped
                                       , fun state -> solutionFinderCalled <- true; state)

        let newState = stateTransition.NewState
        let sideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1
        let stateAfterSideEffect = sideEffect.Invoke(newState)

        test <@ solutionFinderCalled = true @>
        test <@ stateAfterSideEffect.Mode = GameMode.SolutionFound @>

    [<Test>]
    let ``Speaks the new state in the third side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        let stateChangingSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1
        let stateAfterSideEffect = stateChangingSideEffect.Invoke(newState)

        let secondSpeakingSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 2
        let stateAfterSideEffect = secondSpeakingSideEffect.Invoke(stateAfterSideEffect)

        test <@ !spokenStateRef = "Finished figuring out a solution. Say 'Replay solution' to see it." @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.stoppedFollowingState ()   
        let pathSimplifiedState = (simplifyPathFrom beforeState).NewState

        let expectedSpokenState = "Right now I have simplified the path and am figuring out a solution."

        GameState.SayStatus(pathSimplifiedState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef pathSimplifiedState

(******************************* STATE TRANSITION ACCESSIBILITY TESTS *******************************)

    [<Test>]
    let ``Can be reached from the StoppedFollowing state``() = 
        let (beforeState, _) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Starting state``() = 
        let (beforeState, _) = StateTestUtilities.initialState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Measuring state``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the SolutionFound state``() = 
        let (beforeState, _) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the WaitingToReplay state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let stateTransition = simplifyPathFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the FinishedReplaying state``() = 
        let (finishedReplayingState, _) = StateTestUtilities.finishedReplayingState ()   
        let stateTransition = simplifyPathFrom finishedReplayingState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>
    
