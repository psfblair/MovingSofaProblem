namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module MeasuringTests =
    [<Test>]
    let ``Initializes the Measuring state but does not immediately transition to Measuring``() = 
        let (spokenState, startingState) = StateUtilities.initialState
        let cameraLocation = StateUtilities.cameraAtOrigin
        let startMeasuringStateTransition =
             Measuring.StartMeasuring(startingState, cameraLocation, fun measure positionAndRotation -> measure)

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
        let (spokenState, startingState) = StateUtilities.initialState
        let cameraLocation = StateUtilities.cameraAtOrigin
        let startMeasuringStateTransition =
             Measuring.StartMeasuring(
                startingState, 
                cameraLocation, 
                fun measure positionAndRotation -> 
                    StateUtilities.measureAt positionAndRotation.Position positionAndRotation.Rotation
            )

        let sideEffects = startMeasuringStateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 2 @>

        let sideEffect = List.head sideEffects 
        let newState = sideEffect.Invoke(startMeasuringStateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

        let initialPositionRelativeToCamera = Vector(0.0f, -0.2f, 1.0f)
        test <@ newState.Measure.transform.position = initialPositionRelativeToCamera @>
        test <@ newState.Measure.transform.rotation = StateUtilities.zeroRotation @>


    [<Test>]
    let ``Speaks the state in side effects``() = 
        let (whatSheSaidRef, startingState) = StateUtilities.initialState
        let cameraLocation = StateUtilities.cameraAtOrigin
        let startMeasuringStateTransition =
             Measuring.StartMeasuring(startingState, cameraLocation, fun measure positionAndRotation -> measure)

        let sideEffects = startMeasuringStateTransition.SideEffects |> List.ofSeq

        let measureCreatingSideEffect = List.head sideEffects
        let measuringState = measureCreatingSideEffect.Invoke(startMeasuringStateTransition.NewState)

        let speakingSideEffect = List.tail sideEffects |> List.head
        speakingSideEffect.Invoke(measuringState) |> ignore
        test <@ !whatSheSaidRef = "You can measure an object now. Say 'Come with me' when " + 
                "you have finished and are ready to indicate where you want to move the object." @>
