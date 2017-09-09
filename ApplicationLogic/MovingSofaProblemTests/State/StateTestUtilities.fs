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


    let measureAt position rotation =
        // When we instantiate the Measure in Unity we just provide position and
        // rotation, not forward.
        let situation = Situation(position, rotation, forwardZ)
        Measure(situation)

    let measurePositionAfterDropped = Vector(4.0f, 0.0f, 4.0f)

    let initialPathWhenStoppedFollowing () = 
        let path = PathHolder()
        path.Add(origin, zeroRotation, -0.2f)
        path.Add(Vector(1.0f, 0.0f, 1.0f), zeroRotation, -0.2f)
        path.Add(Vector(2.0f, 0.0f, 2.0f), zeroRotation, -0.2f)
        path.Add(Vector(3.0f, 0.0f, 3.0f), zeroRotation, -0.2f)
        path

    let solutionSecondPosition = Vector(1.0f, 2.0f, 3.0f) // Distance from origin: 3.741657
    let solutionThirdPosition = Vector(5.0f, 2.0f, 7.0f)  // Distance from #2: 5.65685
    let solutionThirdRotation = Rotation(0.0f, 180.0f, 90.0f, 0.0f)

    let solution () = 
        let soln = PathHolder()
        soln.Add(origin, zeroRotation, -0.2f)
        soln.Add(solutionSecondPosition, ninetyDegreesAroundZ, -0.2f)
        soln.Add(solutionThirdPosition, solutionThirdRotation, -0.2f)
        soln


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

    let solutionFoundState () =
        let (beforeState, spokenStateRef) = pathSimplifiedState ()
        let afterState = SolutionFound.HasFoundSolution(beforeState, solution ())
        (afterState, spokenStateRef)

    let waitingToReplayState () =
        let (beforeState, spokenStateRef) = solutionFoundState ()
        let afterState = WaitingToReplay.StartReplaying(beforeState).NewState
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
(************************************************* STATE INITIALIZERS *************************************************)
(**********************************************************************************************************************)

    let setInitialPathAndMeasurePosition (state: GameState) =
        state.InitialPath <- initialPathWhenStoppedFollowing ()
        state.Measure.transform.position <- measurePositionAfterDropped

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
