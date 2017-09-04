namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module FollowingTests =

    let startFollowingFrom state =
        Following.StartFollowing(
              state
            , StateTestUtilities.cameraAtOrigin
            , fun state -> state
            , fun state -> state
            , StateTestUtilities.measurePositioner
        )

    [<Test>]
    let ``Can be reached from the Starting state``() = 
        let (startingState, _) = StateTestUtilities.initialState ()   
        let stateTransition = startFollowingFrom startingState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the Measuring state``() = 
        let (measuringState, _) = StateTestUtilities.measuringState ()      
        let stateTransition = startFollowingFrom measuringState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the StoppedFollowing state``() = 
        let (stoppedFollowingState, _) = StateTestUtilities.stoppedFollowingState ()      
        let stateTransition = startFollowingFrom stoppedFollowingState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the PathSimplified state``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()      
        let stateTransition = startFollowingFrom pathSimplifiedState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the SolutionFound state``() = 
        let (solutionFoundState, _) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = startFollowingFrom solutionFoundState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the WaitingToReplay state``() = 
        let (waitingToReplayState, _) = StateTestUtilities.waitingToReplayState ()   
        let stateTransition = startFollowingFrom waitingToReplayState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the Replaying state``() = 
        let (replayingState, _) = StateTestUtilities.replayingState ()   
        let stateTransition = startFollowingFrom replayingState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Initializes initial path to measure's position and camera's Y``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()
        beforeState.Measure.transform.position <- Vector(5.0f, 4.0f, 3.0f)
        let stateTransition = startFollowingFrom beforeState

        let stateAfterTransition = stateTransition.NewState
        let initialPath = stateAfterTransition.InitialPath.path |> List.ofSeq

        test <@ List.length initialPath = 1 @>
        test <@ List.head initialPath = Breadcrumb(Vector(5.0f, 4.0f, 3.0f), StateTestUtilities.zeroRotation, 0.0f) @>

    [<Test>]
    let ``Has no current path step or first step``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()
        let stateTransition = startFollowingFrom beforeState

        let followingState = stateTransition.NewState
        test <@ followingState.CurrentPathStep = MaybePathStep.None @>
        test <@ GameState.FirstStep(followingState) = MaybePathStep.None @>

    [<Test>]
    let ``Has four side effects``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()  
        let stateTransition = startFollowingFrom beforeState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq

        test <@ List.length sideEffects = 4 @>

    [<Test>]
    let ``Disables bounding box as first side effect``() = 
        let mutable boundingBoxDisabled = false

        let stateTransition = 
            Following.StartFollowing(
                  StateTestUtilities.measuringState () |> fst
                , StateTestUtilities.cameraAtOrigin
                , fun state -> boundingBoxDisabled <- true; state
                , fun state -> state
                , StateTestUtilities.measurePositioner
            )
        let stateAfterTransition = stateTransition.NewState
        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head

        let stateAfterSideEffect = firstSideEffect.Invoke(stateAfterTransition)

        test <@ boundingBoxDisabled = true @>
        test <@ stateAfterSideEffect = stateAfterTransition @>

    [<Test>]
    let ``Starts spatial observer as second side effect``() = 
        let mutable spatialMappingObserverStarted = false

        let stateTransition = 
            Following.StartFollowing(
                  StateTestUtilities.measuringState () |> fst
                , StateTestUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> spatialMappingObserverStarted <- true; state
                , StateTestUtilities.measurePositioner
            )
        let stateAfterTransition = stateTransition.NewState
        let secondSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1

        let stateAfterSideEffect = secondSideEffect.Invoke(stateAfterTransition)

        test <@ spatialMappingObserverStarted = true @>
        test <@ stateAfterSideEffect = stateAfterTransition @>

    [<Test>]
    let ``Moves measure as third side effect and puts the position and rotation in the Initial Path``() = 

        let stateTransition = 
            Following.StartFollowing(
                  StateTestUtilities.measuringState () |> fst
                , StateTestUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , StateTestUtilities.measurePositioner
            )

        let thirdSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 2
        let postMoveState = thirdSideEffect.Invoke(stateTransition.NewState)

        // We have cameraAtOrigin, and the position we want relative to the camera is CarryPositionRelativeToOneUnitInFrontOfCamera
        // (which is down 0.2 on the Y axis and out 1 on the Z axis.
        let expectedNewPosition = Vector(0.0f, -0.2f, 1.0f)
        let expectedCameraY = 0.0f

        test <@ postMoveState.Measure.transform.position = expectedNewPosition @>
        test <@ postMoveState.Measure.transform.rotation = StateTestUtilities.facingCameraRotation @>

        let initialPath = postMoveState.InitialPath.path |> List.ofSeq
        test <@ List.length initialPath = 2 @>
        test <@ List.head initialPath = Breadcrumb(StateTestUtilities.origin, StateTestUtilities.zeroRotation, 0.0f) @>
        test <@ List.item 1 initialPath = Breadcrumb(expectedNewPosition, StateTestUtilities.facingCameraRotation, expectedCameraY) @>

    [<Test>]
    let ``Speaks the state in the fourth side effect``() = 
        let (measuringState, spokenStateRef) = StateTestUtilities.measuringState () 

        let stateTransition = 
            Following.StartFollowing(
                  measuringState
                , StateTestUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , StateTestUtilities.measurePositioner
            )

        let fourthSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 3
        fourthSideEffect.Invoke(stateTransition.NewState) |> ignore

        test <@ !spokenStateRef = "I'm following you. Go to where you want to move the object. " + 
                "Say 'Put it down' when you have arrived at the place where you want to move the object." @>

    [<Test>]
    let ``Allows for steps along the path to be added and moves the measure to that location``() = 
        let (measuringState, _) = StateTestUtilities.measuringState ()      
        let stateTransition = startFollowingFrom measuringState
        let measureInitializingSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1
        let initializedMeasureState = measureInitializingSideEffect.Invoke(stateTransition.NewState)

        let cameraPositionAfterMotion = Vector(0.0f, 1.0f, 3.0f)
        let cameraRotationAfterMotion = Rotation(15.0f, 90.0f, 30.0f, 60.0f) // Our dummy rotation represents Euler angles
        let cameraLocationAfterMotion = Situation(cameraPositionAfterMotion, cameraRotationAfterMotion, Vector(0.0f, 0.0f, 1.0f))

        let stateTransitionTriggeredByMotion =
            Following.KeepFollowing(initializedMeasureState, cameraLocationAfterMotion, StateTestUtilities.measurePositioner)

        let newState = stateTransitionTriggeredByMotion.NewState
        test <@ newState.Mode = GameMode.Following @>
        test <@ newState.InitialPath.path.Count = 1 @>

        let sideEffects = stateTransitionTriggeredByMotion.SideEffects
        test <@ sideEffects.Count = 1 @>

        let measureMovingSideEffect = sideEffects |> List.ofSeq |> List.head
        let newStateAfterSideEffects = measureMovingSideEffect.Invoke(newState)

        let expectedNewPosition = Vector(0.0f, 0.8f, 4.0f) // Camera position is not measure position
        let expecteNewRotation = Rotation(0.0f, -90.0f, 0.0f, 0.0f) // Rotation facing camera rotation, only rotate about Y; others stay 0
        test <@ newStateAfterSideEffects.Measure.transform.position = expectedNewPosition @>
        test <@ newStateAfterSideEffects.Measure.transform.rotation = expecteNewRotation @>

        let currentPath = newStateAfterSideEffects.InitialPath.path
        test <@ currentPath.Count = 2 @>
        test <@ currentPath.First.Value = Breadcrumb(StateTestUtilities.origin, StateTestUtilities.zeroRotation, 0.0f) @>
        test <@ currentPath.Last.Value = Breadcrumb(expectedNewPosition, expecteNewRotation, 1.0f) @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.measuringState () 
        let followingState = (startFollowingFrom beforeState).NewState

        let expectedSpokenState = 
            "Right now I am following you. You can " + 
            "Say 'Put it down' when you have arrived at the place where you want to move the object."

        GameState.SayStatus(followingState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef followingState
