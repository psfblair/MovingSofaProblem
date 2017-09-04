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
    let ``Initializes the initial path with the point at which the measure was dropped``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst

        let positionWhenStopped = Vector(1.0f, 2.0f, 3.0f)
        let stoppedFollowingInitialPath = PathHolder()
        stoppedFollowingInitialPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        stoppedFollowingInitialPath.Add(positionWhenStopped, StateTestUtilities.zeroRotation, -0.2f)
        stoppedFollowingState.InitialPath <- stoppedFollowingInitialPath

        let positionWhenDropped = Vector(3.0f, 4.0f, 5.0f)
        stoppedFollowingState.Measure.transform.position <- positionWhenDropped
        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState

        let expectedInitialPath = PathHolder()
        expectedInitialPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        expectedInitialPath.Add(positionWhenStopped, StateTestUtilities.zeroRotation, -0.2f)
        expectedInitialPath.Add(positionWhenDropped, StateTestUtilities.zeroRotation, -0.2f)

        test <@ pathSimplifiedState.InitialPath = expectedInitialPath @>

    [<Test>]
    let ``Initializes the path to replay with the simplified initial path``() = 
        let stoppedFollowingState = StateTestUtilities.stoppedFollowingState () |> fst

        let stoppedFollowingInitialPath = PathHolder()
        stoppedFollowingInitialPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        stoppedFollowingInitialPath.Add(Vector(1.0f, 0.0f, 1.0f), StateTestUtilities.zeroRotation, -0.2f)
        stoppedFollowingInitialPath.Add(Vector(2.0f, 0.0f, 2.0f), StateTestUtilities.zeroRotation, -0.2f)
        stoppedFollowingInitialPath.Add(Vector(3.0f, 0.0f, 3.0f), StateTestUtilities.zeroRotation, -0.2f)
        stoppedFollowingState.InitialPath <- stoppedFollowingInitialPath

        let positionAfterDropped = Vector(4.0f, 0.0f, 4.0f)
        stoppedFollowingState.Measure.transform.position <- positionAfterDropped
        let pathSimplifiedState = (simplifyPathFrom stoppedFollowingState).NewState

        let expectedPathToReplay = PathHolder()
        expectedPathToReplay.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        expectedPathToReplay.Add(positionAfterDropped, StateTestUtilities.zeroRotation, -0.2f)

        test <@ pathSimplifiedState.PathToReplay = expectedPathToReplay @>

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