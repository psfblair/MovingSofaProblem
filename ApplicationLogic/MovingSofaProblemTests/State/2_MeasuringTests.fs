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
            , StateUtilities.cameraAtOrigin
            , fun measure positionAndRotation -> measure
        )

    [<Test>]
    let ``Initializes the Measuring state but does not immediately transition to Measuring``() = 
        let (startingState, _) = StateUtilities.initialState ()
        let startMeasuringStateTransition = startMeasuringFrom startingState

        let state = startMeasuringStateTransition.NewState
        test <@ state.CurrentPathStep = MaybePathStep.None @>
        test <@ state.Mode = GameMode.Starting @>

        test <@ state.Measure.transform.position = StateUtilities.origin @>
        test <@ state.Measure.transform.rotation = StateUtilities.zeroRotation @>
        test <@ state.Measure.transform.forward = StateUtilities.origin @>
        test <@ state.InitialPath.path.Count = 0 @>
        test <@ GameState.FirstStep(state) = MaybePathStep.None @>

    [<Test>]
    let ``Creates a measure and transitions to the Measuring state in the first side effect``() = 
        let (startingState, _) = StateUtilities.initialState ()
        let measureCreator = 
            System.Func<Measure, PositionAndRotation, Measure>(
                fun measure positionAndRotation -> 
                    StateUtilities.measureAt positionAndRotation.Position positionAndRotation.Rotation
            )

        let startMeasuringStateTransition = 
            Measuring.StartMeasuring(startingState, StateUtilities.cameraAtOrigin, measureCreator)

        let sideEffects = startMeasuringStateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 2 @>

        let sideEffect = List.head sideEffects 
        let newState = sideEffect.Invoke(startMeasuringStateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

        let initialPositionRelativeToCamera = Vector(0.0f, -0.2f, 1.0f)
        test <@ newState.Measure.transform.position = initialPositionRelativeToCamera @>
        test <@ newState.Measure.transform.rotation = StateUtilities.zeroRotation @>


    [<Test>]
    let ``Speaks the state in the second side effect``() = 
        let (startingState, spokenStateRef) = StateUtilities.initialState ()
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
        let (measuringState, spokenStateRef) = StateUtilities.measuringState ()

        let sayStatusSideEffects = GameState.SayStatus(measuringState) |> List.ofSeq
        test <@ List.length sayStatusSideEffects = 1 @>

        let newState = (List.head sayStatusSideEffects).Invoke(measuringState)
        test <@ newState = measuringState @>
        test <@ !spokenStateRef = 
                    "Right now I can measure an object that you select. You can " +
                    "Say 'Come with me' when you have finished and are ready to " + 
                    "indicate where you want to move the object."     @>
