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
    let ninetyDegreesAroundZ = Rotation(0.0f, 0.0f, 90.0f, 0.0f)

    let cameraAtOrigin =
        let originPosition = origin
        let zeroRotation = zeroRotation
        Situation(originPosition, zeroRotation, forwardZ)

    let facingRotWithPosXYZ x y z = PositionAndRotation(Vector(x, y, z), facingCameraRotation)

    let initializedMeasurePositionAndRotation = facingRotWithPosXYZ 0.0f -0.2f 1.0f

    let measureWhenStartedFollowing = facingRotWithPosXYZ 1.0f 0.0f 1.0f

    let initialPathWhenStartedFollowing () = 
        let path = PathHolder()
        path.Add(measureWhenStartedFollowing, -0.2f)
        path

    let measureWhenStoppedFollowing = facingRotWithPosXYZ 2.0f 0.0f 2.0f

    let initialPathWhenStoppedFollowing () = 
        let path = PathHolder()
        path.Add(measureWhenStartedFollowing, -0.2f)
        path.Add(measureWhenStoppedFollowing, -0.2f)
        path.Add(facingRotWithPosXYZ 3.0f 0.0f 3.0f, -0.2f)
        path

    let measurePositionAndRotationAfterDropped = 
        PositionAndRotation(Vector(4.0f, 0.1f, 4.0f), ninetyDegreesAroundZ)

    let solutionSecondPosition = Vector(1.0f, 2.0f, 3.0f) // Distance from origin: 3.741657
    let solutionThirdPosition = Vector(5.0f, 2.0f, 7.0f)  // Distance from #2: 5.65685
    let solutionThirdRotation = Rotation(0.0f, 180.0f, 90.0f, 0.0f)

    let solution () = 
        let soln = PathHolder()
        soln.Add(PositionAndRotation(origin, zeroRotation), -0.2f)
        soln.Add(PositionAndRotation(solutionSecondPosition, ninetyDegreesAroundZ), -0.2f)
        soln.Add(PositionAndRotation(solutionThirdPosition, solutionThirdRotation), -0.2f)
        soln


(**********************************************************************************************************************)
(************************************************* DUMMY SIDE EFFECTS *************************************************)
(**********************************************************************************************************************)

    let measurePositioner = System.Action<PositionAndRotation>(fun positionAndRotation -> ())

(**********************************************************************************************************************)
(************************************************* STATE CONSTRUCTORS *************************************************)
(**********************************************************************************************************************)

    let replayingTranslationSpeed = 0.7f; // units/sec
    let replayingRotationSpeed = 20.0f; // degrees/sec

    let initialState () = 
        let spokenState = ref ""
        let startingTransition = 
            Starting.Start(
                cameraAtOrigin
                , Vector(0.0f, -0.2f, 0.0f)
                , replayingTranslationSpeed
                , replayingRotationSpeed
                , fun text -> spokenState := text
            )
        (startingTransition.NewState, spokenState)

    let measuringState () = 
        let (beforeState, spokenStateRef) = initialState ()
        let stateTransition =
            Measuring.StartMeasuring(beforeState, cameraAtOrigin, measurePositioner)

        let measureCreatingSideEffect = stateTransition.SideEffects |> List.ofSeq |>  List.head 
        let measuringState = measureCreatingSideEffect.Invoke(stateTransition.NewState)

        (measuringState, spokenStateRef)

    let followingState () = 
        let (beforeState, spokenStateRef) = measuringState ()
        let afterState =
            Following.StartFollowing(
                beforeState
                , measureWhenStartedFollowing
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
                , measureWhenStoppedFollowing
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
                , measurePositionAndRotationAfterDropped
                , fun state -> state
            ).NewState
        (afterState, spokenStateRef)

    let solutionFoundState () =
        let (beforeState, spokenStateRef) = pathSimplifiedState ()
        let afterState = SolutionFound.HasFoundSolution(beforeState, solution ())
        (afterState, spokenStateRef)

    let waitingToReplayState () =
        let (beforeState, spokenStateRef) = solutionFoundState ()
        let afterState = WaitingToReplay.StartReplaying(beforeState, measurePositioner).NewState
        (afterState, spokenStateRef)

    let replayingState () =
        let (beforeState, spokenStateRef) = waitingToReplayState ()
        let afterState = Replaying.PlayNextSegment(beforeState, 0.0f).NewState
        (afterState, spokenStateRef)

    let finishedReplayingState () =
        let (beforeState, spokenStateRef) = replayingState ()
        let afterState = FinishedReplaying.IsFinishedReplaying(beforeState)
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
