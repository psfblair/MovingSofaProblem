namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module MeasuringTests =

    let startMeasuringFrom state =
        Measuring.StartMeasuring(
              state
            , StateTestUtilities.cameraAtOrigin
            , fun measure positionAndRotation -> measure
        )

    [<Test>]
    let ``Can be reached from the Starting state but does not immediately transition to Measuring``() = 
        let (startingState, _) = StateTestUtilities.initialState ()      
        let stateTransition = startMeasuringFrom startingState
        test <@ stateTransition.NewState.Mode = GameMode.Starting @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Can be reached from the Following state``() = 
        let (followingState, _) = StateTestUtilities.followingState ()      
        let stateTransition = startMeasuringFrom followingState
        test <@ stateTransition.NewState.Mode = GameMode.Following @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Can be reached from the StoppedFollowing state``() = 
        let (stoppedFollowingState, _) = StateTestUtilities.stoppedFollowingState ()      
        let stateTransition = startMeasuringFrom stoppedFollowingState
        test <@ stateTransition.NewState.Mode = GameMode.StoppedFollowing @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Can be reached from the PathSimplified state``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()      
        let stateTransition = startMeasuringFrom pathSimplifiedState
        test <@ stateTransition.NewState.Mode = GameMode.PathSimplified @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Can be reached from the SolutionFound state``() = 
        let (solutionFoundState, _) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = startMeasuringFrom solutionFoundState
        test <@ stateTransition.NewState.Mode = GameMode.SolutionFound @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)

        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Initializes the Measuring state``() = 
        let (startingState, _) = StateTestUtilities.initialState ()
        let startMeasuringStateTransition = startMeasuringFrom startingState

        let state = startMeasuringStateTransition.NewState
        test <@ state.Measure.transform.position = StateTestUtilities.origin @>
        test <@ state.Measure.transform.rotation = StateTestUtilities.zeroRotation @>
        test <@ state.Measure.transform.forward = StateTestUtilities.origin @>
        test <@ state.InitialPath.path.Count = 0 @>
        test <@ state.CurrentPathStep = MaybePathStep.None @>
        test <@ GameState.FirstStep(state) = MaybePathStep.None @>

    [<Test>]
    let ``Has two side effects``() = 
        let (beforeState, _) = StateTestUtilities.initialState ()  
        let stateTransition = startMeasuringFrom beforeState
        let sideEffects = stateTransition.SideEffects |> List.ofSeq

        test <@ List.length sideEffects = 2 @>

    [<Test>]
    let ``Creates a measure and transitions to the Measuring state in the first side effect``() = 
        let (startingState, _) = StateTestUtilities.initialState ()
        let measureCreator = 
            System.Func<Measure, PositionAndRotation, Measure>(
                fun measure positionAndRotation -> 
                    StateTestUtilities.measureAt positionAndRotation.Position positionAndRotation.Rotation
            )

        let startMeasuringStateTransition = 
            Measuring.StartMeasuring(startingState, StateTestUtilities.cameraAtOrigin, measureCreator)

        let sideEffects = startMeasuringStateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 2 @>

        let sideEffect = List.head sideEffects 
        let newState = sideEffect.Invoke(startMeasuringStateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

        let initialPositionRelativeToCamera = Vector(0.0f, -0.2f, 1.0f)
        test <@ newState.Measure.transform.position = initialPositionRelativeToCamera @>
        test <@ newState.Measure.transform.rotation = StateTestUtilities.facingCameraRotation @>


    [<Test>]
    let ``Speaks the state in the second side effect``() = 
        let (startingState, spokenStateRef) = StateTestUtilities.initialState ()
        let startMeasuringStateTransition = startMeasuringFrom startingState
        let sideEffects = startMeasuringStateTransition.SideEffects |> List.ofSeq

        let measureCreatingSideEffect = List.head sideEffects
        let measuringState = measureCreatingSideEffect.Invoke(startMeasuringStateTransition.NewState)

        let speakingSideEffect = List.tail sideEffects |> List.head
        speakingSideEffect.Invoke(measuringState) |> ignore
        test <@ !spokenStateRef = "You can measure an object now. Say 'Come with me' when " + 
                "you have finished and are ready to indicate where you want to move the object." @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (measuringState, spokenStateRef) = StateTestUtilities.measuringState ()

        let sayStatusSideEffects = GameState.SayStatus(measuringState) |> List.ofSeq
        test <@ List.length sayStatusSideEffects = 1 @>

        let newState = (List.head sayStatusSideEffects).Invoke(measuringState)
        test <@ newState = measuringState @>
        test <@ !spokenStateRef = 
                    "Right now I can measure an object that you select. You can " +
                    "Say 'Come with me' when you have finished and are ready to " + 
                    "indicate where you want to move the object."     @>
