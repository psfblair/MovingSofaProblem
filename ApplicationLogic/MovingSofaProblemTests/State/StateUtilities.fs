namespace MovingSofaProblemTests.State

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module StateUtilities =

(**********************************************************************************************************************)
(************************************************ OBJECT CONSTRUCTORS *************************************************)
(**********************************************************************************************************************)

    let origin = Vector(0.0f, 0.0f, 0.0f)
    let zeroRotation = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let forwardZ = Vector(0.0f, 0.0f, 1.0f)

    let cameraAtOrigin =
        let originPosition = origin
        let zeroRotation = zeroRotation
        Situation(originPosition, zeroRotation, forwardZ)


    let measureAt position rotation =
        // When we instantiate the Measure in Unity we just provide position and
        // rotation, not forward.
        let situation = Situation(position, rotation, forwardZ)
        Measure(situation)


(**********************************************************************************************************************)
(************************************************* DUMMY SIDE EFFECTS *************************************************)
(**********************************************************************************************************************)

    let dummyMeasureCreator = 
        System.Func<Measure, PositionAndRotation, Measure>(
            fun measure positionAndRotation -> measure
        )

    let dummyMeasurePositioner = 
        System.Func<Vector, System.Func<Measure, Situation, PositionAndRotation>>(
            fun relativePosition ->
                System.Func<Measure, Situation, PositionAndRotation>(
                    fun measure situation -> PositionAndRotation(situation.position, situation.rotation)
                )
        )

(**********************************************************************************************************************)
(************************************************* STATE CONSTRUCTORS *************************************************)
(**********************************************************************************************************************)

    let initialState () = 
        let spokenState = ref ""
        let startingTransition = Starting.Start(fun text -> spokenState := text)
        (startingTransition.NewState, spokenState)

    let measuringState () = 
        let beforeState = initialState ()
        let stateTransition =
            Measuring.StartMeasuring(fst beforeState, cameraAtOrigin, dummyMeasureCreator)

        let measureCreatingSideEffect = stateTransition.SideEffects |> List.ofSeq |>  List.head 
        let measuringState = measureCreatingSideEffect.Invoke(stateTransition.NewState)

        (measuringState, snd beforeState)

    let followingState () = 
        let beforeState = measuringState ()
        let afterState =
            Following.StartFollowing(
                fst beforeState
                , cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , dummyMeasurePositioner
            ).NewState
        (afterState, snd beforeState)
