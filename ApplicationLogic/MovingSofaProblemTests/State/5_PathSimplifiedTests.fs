namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module PathSimplifiedTests =

    let simplifyPathFrom state = PathSimplified.SimplifyPath(state, fun state -> state)

    [<Test>]
    let ``Can be reached from the StoppedFollowing state``() = 
        let (beforeState, _) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Starting state``() = 
        let (beforeState, _) = StateTestUtilities.initialState ()   
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Measuring state``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()   
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.PathSimplified @>

    [<Test>]
    let ``Can be reached from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.PathSimplified @>
    
    [<Test>]
    let ``Initializes the initial path with simplified path ending in the point at which the measure was dropped``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst
        StateTestUtilities.setInitialPathAndMeasurePosition stoppedFollowingState

        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState

        let expectedInitialPath = PathHolder()
        expectedInitialPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        expectedInitialPath.Add(StateTestUtilities.measurePositionAfterDropped
                               , StateTestUtilities.zeroRotation
                               , -0.2f)

        test <@ pathSimplifiedState.InitialPath = expectedInitialPath @>

    [<Test>]
    let ``Initializes the path to replay with the simplified initial path``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst
        StateTestUtilities.setInitialPathAndMeasurePosition stoppedFollowingState

        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState

        let expectedPathToReplay = PathHolder()
        expectedPathToReplay.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        expectedPathToReplay.Add(StateTestUtilities.measurePositionAfterDropped
                                , StateTestUtilities.zeroRotation
                                , -0.2f)

        test <@ pathSimplifiedState.PathToReplay = expectedPathToReplay @>

    [<Test>]
    let ``First step is first step of simplified path``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst
        StateTestUtilities.setInitialPathAndMeasurePosition stoppedFollowingState

        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState
        test <@ GameState.FirstStep(pathSimplifiedState).Value.StartNode.Value = 
                    Breadcrumb(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f) @>
        test <@ GameState.FirstStep(pathSimplifiedState).Value.EndNode.Value = 
                    Breadcrumb(StateTestUtilities.measurePositionAfterDropped
                              , StateTestUtilities.zeroRotation
                              , -0.2f) @>

    [<Test>]
    let ``Has no current path step``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst
        StateTestUtilities.setInitialPathAndMeasurePosition stoppedFollowingState

        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState
        test <@ pathSimplifiedState.CurrentPathStep = MaybePathStep.None @>

    [<Test>]
    let ``Has two side effects``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = simplifyPathFrom beforeState

        let newState = stateTransition.NewState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 2 @>

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
    let ``Starts finding the solution in the second side effect``() = 
        let mutable solutionFinderCalled = false
        let (beforeState, _) = StateTestUtilities.followingState ()
        let stateTransition = PathSimplified.SimplifyPath(beforeState, fun state -> solutionFinderCalled <- true; state)

        let newState = stateTransition.NewState
        let sideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1
        let stateAfterSideEffect = sideEffect.Invoke(newState)

        test <@ solutionFinderCalled = true @>
        test <@ stateAfterSideEffect.Mode = GameMode.SolutionFound @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = simplifyPathFrom beforeState
        let pathSimplifiedState = stateTransition.NewState

        let sideEffects = GameState.SayStatus(pathSimplifiedState) |> List.ofSeq 
        test <@ List.length sideEffects = 1 @>

        let newState = (List.head sideEffects).Invoke(pathSimplifiedState)

        test <@ newState = pathSimplifiedState @>
        test <@ !spokenStateRef = "Right now I have simplified the path and am figuring out a solution." @>