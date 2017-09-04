namespace MovingSofaProblemTests.State

open Swensen.Unquote

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

type CSharpFunc<'a, 'b> = System.Func<'a, 'b>
type CSharpList<'a> = System.Collections.Generic.List<'a>

module StateTestUtilities =

(**********************************************************************************************************************)
(************************************************ OBJECT CONSTRUCTORS *************************************************)
(**********************************************************************************************************************)

    let origin = Vector(0.0f, 0.0f, 0.0f)
    let forwardZ = Vector(0.0f, 0.0f, 1.0f)

    let zeroRotation = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let facingCameraRotation = Rotation(0.0f, -180.0f, 0.0f, 0.0f)

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

    let measurePositioner = 
        System.Func<Measure, PositionAndRotation, PositionAndRotation>(
            fun measure positionAndRotation -> 
                measure.transform.position <- positionAndRotation.Position
                measure.transform.rotation <- positionAndRotation.Rotation
                positionAndRotation
        )

(**********************************************************************************************************************)
(************************************************* STATE CONSTRUCTORS *************************************************)
(**********************************************************************************************************************)

    let initialState () = 
        let spokenState = ref ""
        let startingTransition = Starting.Start(fun text -> spokenState := text)
        (startingTransition.NewState, spokenState)

    let measuringState () = 
        let (beforeState, spokenStateRef) = initialState ()
        let stateTransition =
            Measuring.StartMeasuring(beforeState, cameraAtOrigin, dummyMeasureCreator)

        let measureCreatingSideEffect = stateTransition.SideEffects |> List.ofSeq |>  List.head 
        let measuringState = measureCreatingSideEffect.Invoke(stateTransition.NewState)

        (measuringState, spokenStateRef)

    let followingState () = 
        let (beforeState, spokenStateRef) = measuringState ()
        let afterState =
            Following.StartFollowing(
                beforeState
                , cameraAtOrigin
                , fun state -> state
                , fun state -> state
                , measurePositioner
            ).NewState
        (afterState, spokenStateRef)

    let stoppedFollowingState () =
        let (beforeState, spokenStateRef) = followingState ()
        let afterState =
            StoppedFollowing.StopFollowing(
                beforeState
                , cameraAtOrigin
                , fun gameObject -> ()
                , fun state -> state
            ).NewState
        (afterState, spokenStateRef)

    let pathSimplifiedState () =
        let (beforeState, spokenStateRef) = stoppedFollowingState ()
        let afterState = 
            PathSimplified.SimplifyPath(
                beforeState
                , fun state -> state
            ).NewState
        (afterState, spokenStateRef)

(**********************************************************************************************************************)
(************************************************* COMPLEX ASSERTIONS *************************************************)
(**********************************************************************************************************************)

    let testSingleSideEffectSpeaks (spokenText: string) (spokenStateRef: string ref) (currentState: GameState) (sideEffects: CSharpList<CSharpFunc<GameState,GameState>>) =
        let sideEffectList = List.ofSeq sideEffects
        test <@ List.length sideEffectList = 1 @>

        let sideEffect = List.head sideEffectList
        let stateAfterSideEffect = sideEffect.Invoke(currentState)
        test <@ stateAfterSideEffect.Mode = currentState.Mode @>
        test <@ !spokenStateRef = spokenText @>
