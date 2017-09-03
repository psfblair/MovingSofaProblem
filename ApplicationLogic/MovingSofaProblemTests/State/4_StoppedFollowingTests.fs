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

    [<Test>]
    let ``Cannot be reached from the Starting state``() = 
        let (startingState, spokenStateRef) = StateTestUtilities.initialState ()   
        let stateTransition = stopFollowingFrom startingState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.Starting @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks 
                "I can't put it down because I'm not carrying anything right now." spokenStateRef newState


    [<Test>]
    let ``Cannot be reached from the Measuring state``() = 
        let (measuringState, spokenStateRef) = StateTestUtilities.measuringState ()    
        let stateTransition = stopFollowingFrom measuringState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.Measuring @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks 
                "I can't put it down because I'm not carrying anything right now." spokenStateRef newState

    [<Test>]
    let ``Has three side effects``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let stateTransition = stopFollowingFrom beforeState

        let newState = stateTransition.NewState
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