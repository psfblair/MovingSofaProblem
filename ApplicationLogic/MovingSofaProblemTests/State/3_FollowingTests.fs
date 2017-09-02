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
            , StateUtilities.cameraAtOrigin
            , fun state -> state
            , fun state -> state
            , StateUtilities.dummyMeasurePositioner
        )

    [<Test>]
    let ``Can be reached from the Starting state``() = 
        let (startingState, _) = StateUtilities.initialState ()   
        let stateTransition = startFollowingFrom startingState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    [<Test>]
    let ``Can be reached from the Measuring state``() = 
        let (measuringState, _) = StateUtilities.measuringState ()      
        let stateTransition = startFollowingFrom measuringState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

    // TODO other states you can reach Following from

    [<Test>]
    let ``Initializes initial path to measure's position and camera's Y``() = 
        let (startingState, _) = StateUtilities.initialState ()
        startingState.Measure.transform.position <- Vector(5.0f, 4.0f, 3.0f)

        let cameraPosition = Situation(Vector(2.0f, 0.2f, 1.0f), StateUtilities.zeroRotation, StateUtilities.forwardZ)

        let stateTransition = 
            Following.StartFollowing(
                  startingState
                , cameraPosition
                , fun state -> state
                , fun state -> state
                , StateUtilities.dummyMeasurePositioner
            )

        let stateAfterTransition = stateTransition.NewState
        let initialPath = stateAfterTransition.InitialPath.path |> List.ofSeq

        test <@ List.length initialPath = 1 @>
        test <@ List.head initialPath = Breadcrumb(Vector(5.0f, 4.0f, 3.0f), StateUtilities.zeroRotation, 0.2f) @>

    [<Test>]
    let ``Has four side effects``() = 
        let (startingState, _) = StateUtilities.initialState ()  
        let stateTransition = startFollowingFrom startingState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq

        test <@ List.length sideEffects = 4 @>

    [<Test>]
    let ``Disables bounding box as first side effect``() = 
        let mutable boundingBoxDisabled = false

        let stateTransition = 
            Following.StartFollowing(
                  StateUtilities.initialState () |> fst
                , StateUtilities.cameraAtOrigin
                , fun state -> boundingBoxDisabled <- true; state
                , fun state -> state
                , StateUtilities.dummyMeasurePositioner
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
                  StateUtilities.initialState () |> fst
                , StateUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> spatialMappingObserverStarted <- true; state
                , StateUtilities.dummyMeasurePositioner
            )
        let stateAfterTransition = stateTransition.NewState
        let secondSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1

        let stateAfterSideEffect = secondSideEffect.Invoke(stateAfterTransition)

        test <@ spatialMappingObserverStarted = true @>
        test <@ stateAfterSideEffect = stateAfterTransition @>

    [<Test>]
    let ``Moves measure as third side effect and puts the position and rotation in the Initial Path``() = 

        let measurePositioner = 
            System.Func<Vector, System.Func<Measure, Situation, PositionAndRotation>>(
                fun relativePosition ->
                    System.Func<Measure, Situation, PositionAndRotation>(
                        fun measure situation -> 
                            let newPosition = situation.position + relativePosition
                            measure.transform.position <- newPosition
                            measure.transform.rotation <- situation.rotation
                            PositionAndRotation(newPosition, situation.rotation)
                    )
            )

        let stateTransition = 
            Following.StartFollowing(
                  StateUtilities.initialState () |> fst
                , StateUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , measurePositioner
            )

        let thirdSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 2
        let postMoveState = thirdSideEffect.Invoke(stateTransition.NewState)

        // We have cameraAtOrigin, and the position we want relative to the camera is CarryPositionRelativeToOneUnitInFrontOfCamera
        // (which is down 0.2 on the Y axis. Our dummy positioner adds the relative position to the camera position and
        // puts the measure there, and returns that position and rotation.
        let expectedNewPosition = Vector(0.0f, -0.2f, 0.0f)
        let expectedNewRotation = StateUtilities.zeroRotation
        let expectedCameraY = 0.0f

        test <@ postMoveState.Measure.transform.position = expectedNewPosition @>
        test <@ postMoveState.Measure.transform.rotation = expectedNewRotation @>

        let initialPath = postMoveState.InitialPath.path |> List.ofSeq
        test <@ List.length initialPath = 2 @>
        test <@ List.head initialPath = Breadcrumb(StateUtilities.origin, StateUtilities.zeroRotation, 0.0f) @>
        test <@ List.item 1 initialPath = Breadcrumb(expectedNewPosition, expectedNewRotation, expectedCameraY) @>

    [<Test>]
    let ``Speaks the state in the fourth side effect``() = 
        let (startingState, spokenStateRef) = StateUtilities.initialState () 

        let stateTransition = 
            Following.StartFollowing(
                  startingState
                , StateUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , StateUtilities.dummyMeasurePositioner
            )

        let fourthSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 3
        fourthSideEffect.Invoke(stateTransition.NewState) |> ignore

        test <@ !spokenStateRef = "I'm following you. Go to where you want to move the object. " + 
                "Say 'Put it down' when you have arrived at the place where you want to move the object." @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (startingState, spokenStateRef) = StateUtilities.initialState () 

        let followingState = 
            Following.StartFollowing(
                  startingState
                , StateUtilities.cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , StateUtilities.dummyMeasurePositioner
            ).NewState

        let sideEffects = GameState.SayStatus(followingState) |> List.ofSeq 
        test <@ List.length sideEffects = 1 @>

        let newState = (List.head sideEffects).Invoke(followingState)
        test <@ newState = followingState @>

        test <@ !spokenStateRef = "Right now I am following you. You can " + 
                "Say 'Put it down' when you have arrived at the place where you want to move the object." @>