namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module StoppedFollowingTests =

    let stopFollowingFrom state =
        StoppedFollowing.StopFollowing(
              state
            , StateTestUtilities.cameraAtOrigin
            , fun gameObject -> ()
            , fun state -> state
        )

    [<Test>]
    let ``Can be reached from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = stopFollowingFrom beforeState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.StoppedFollowing @>

    let invalidTransitionMessage = "I can't put it down because I'm not carrying anything right now."

    [<Test>]
    let ``Cannot be reached from the Starting state``() = 
        let (startingState, spokenStateRef) = StateTestUtilities.initialState ()   
        let stateTransition = stopFollowingFrom startingState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.Starting @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Cannot be reached from the Measuring state``() = 
        let (measuringState, spokenStateRef) = StateTestUtilities.measuringState ()    
        let stateTransition = stopFollowingFrom measuringState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.Measuring @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Cannot be reached from the PathSimplified state``() = 
        let (pathSimplifiedState, spokenStateRef) = StateTestUtilities.pathSimplifiedState ()    
        let stateTransition = stopFollowingFrom pathSimplifiedState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.PathSimplified @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Cannot be reached from the SolutionFound state``() = 
        let (solutionFoundState, spokenStateRef) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = stopFollowingFrom solutionFoundState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.SolutionFound @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Initializes the initial path with the point at which it stopped following``() = 
        let positionWhenStopped = Vector(1.0f, 2.0f, 3.0f)
        let (beforeState, _) = StateTestUtilities.followingState ()
        beforeState.Measure.transform.position <- positionWhenStopped

        let expectedPathBeforeStopped = PathHolder()
        expectedPathBeforeStopped.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, 0.0f)

        test <@ beforeState.InitialPath = expectedPathBeforeStopped @>

        let newState = (stopFollowingFrom beforeState).NewState

        let expectedInitialPath = PathHolder()
        expectedInitialPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, 0.0f)
        expectedInitialPath.Add(positionWhenStopped, StateTestUtilities.zeroRotation, 0.0f)
          
        test <@ newState.InitialPath = expectedInitialPath @>

    [<Test>]
    let ``Does not initialize the path to replay``() = 
        let positionWhenStopped = Vector(1.0f, 2.0f, 3.0f)
        let (beforeState, _) = StateTestUtilities.followingState ()
        beforeState.Measure.transform.position <- positionWhenStopped

        test <@ beforeState.PathToReplay = PathHolder() @>

        let newState = (stopFollowingFrom beforeState).NewState
        test <@ beforeState.PathToReplay = PathHolder() @>

    [<Test>]
    let ``Has no current path step or first step``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()
        let stateTransition = stopFollowingFrom beforeState

        let stoppedFollowingState = stateTransition.NewState
        test <@ stoppedFollowingState.CurrentPathStep = MaybePathStep.None @>
        test <@ GameState.FirstStep(stoppedFollowingState) = MaybePathStep.None @>

    [<Test>]
    let ``Has three side effects``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = stopFollowingFrom beforeState

        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 3 @>

    [<Test>]
    let ``Releases the measure in the first side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()
        let mutable measureReleased = false  
        let stateTransition = 
            StoppedFollowing.StopFollowing(
                  beforeState
                , StateTestUtilities.cameraAtOrigin
                , fun gameObject -> measureReleased <- true; ()
                , fun state -> state
            )

        let newState = stateTransition.NewState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        let sideEffect = List.item 0 sideEffects
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.StoppedFollowing @>

        test <@ measureReleased = true @>

    [<Test>]
    let ``Speaks the state in the second side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()
        let stateTransition = stopFollowingFrom beforeState

        let newState = stateTransition.NewState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        let sideEffect = List.item 1 sideEffects
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.StoppedFollowing @>

        test <@ !spokenStateRef = "Simplifying the route." @>

    [<Test>]
    let ``Stops the spatial mapping observer in the third side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()
        let mutable spatialMappingObserverStopped = false  
        let stateTransition = 
            StoppedFollowing.StopFollowing(
                  beforeState
                , StateTestUtilities.cameraAtOrigin
                , fun gameObject -> ()
                , fun state -> spatialMappingObserverStopped <- true; state
            )

        let newState = stateTransition.NewState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        let sideEffect = List.item 2 sideEffects
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.StoppedFollowing @>

        test <@ spatialMappingObserverStopped = true @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()
        let stateTransition = stopFollowingFrom beforeState
        let stopFollowingState = stateTransition.NewState

        let sideEffects = GameState.SayStatus(stopFollowingState) |> List.ofSeq 
        test <@ List.length sideEffects = 1 @>

        let newState = (List.head sideEffects).Invoke(stopFollowingState)

        test <@ newState = stopFollowingState @>
        test <@ !spokenStateRef = "Right now I have stopped following you and am simplifying the route." @>