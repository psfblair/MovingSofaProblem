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
            , StateUtilities.cameraAtOrigin
            , fun gameObject -> ()
            , fun state -> state
        )

    [<Test>]
    let ``Can be reached from the Following state``() = 
        let (beforeState, _) = StateUtilities.followingState ()   
        let stateTransition = stopFollowingFrom beforeState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.StoppedFollowing @>

    [<Test>]
    let ``Cannot be reached from the Starting state``() = 
        let (startingState, spokenStateRef) = StateUtilities.initialState ()   
        let stateTransition = stopFollowingFrom startingState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.Starting @>

        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 1 @>

        let sideEffect = List.head sideEffects
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.Starting @>
        test <@ !spokenStateRef = "I can't put it down because I'm not carrying anything right now." @>


    [<Test>]
    let ``Cannot be reached from the Measuring state``() = 
        let (measuringState, _) = StateUtilities.measuringState ()    
        let stateTransition = stopFollowingFrom measuringState

        let newState = stateTransition.NewState
        test <@ newState.Mode = GameMode.Measuring @>

        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 1 @>

        let sideEffect = List.head sideEffects
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Has three side effects``() = 
        let (beforeState, _) = StateUtilities.followingState ()   
        let stateTransition = stopFollowingFrom beforeState

        let newState = stateTransition.NewState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 3 @>

    [<Test>]
    let ``Releases the measure in the first side effect``() = 
        let (beforeState, spokenStateRef) = StateUtilities.followingState ()
        let mutable measureReleased = false  
        let stateTransition = 
            StoppedFollowing.StopFollowing(
                  beforeState
                , StateUtilities.cameraAtOrigin
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
        let (beforeState, spokenStateRef) = StateUtilities.followingState ()
        let stateTransition = stopFollowingFrom beforeState

        let newState = stateTransition.NewState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        let sideEffect = List.item 1 sideEffects
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.StoppedFollowing @>

        test <@ !spokenStateRef = "Simplifying the route." @>

    [<Test>]
    let ``Stops the spatial mapping observer in the third side effect``() = 
        let (beforeState, spokenStateRef) = StateUtilities.followingState ()
        let mutable spatialMappingObserverStopped = false  
        let stateTransition = 
            StoppedFollowing.StopFollowing(
                  beforeState
                , StateUtilities.cameraAtOrigin
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
        let (beforeState, spokenStateRef) = StateUtilities.followingState ()
        let stateTransition = stopFollowingFrom beforeState
        let stopFollowingState = stateTransition.NewState

        let sideEffects = GameState.SayStatus(stopFollowingState) |> List.ofSeq 
        test <@ List.length sideEffects = 1 @>

        let newState = (List.head sideEffects).Invoke(stopFollowingState)

        test <@ newState = stopFollowingState @>
        test <@ !spokenStateRef = "Right now I have stopped following you and am simplifying the route." @>