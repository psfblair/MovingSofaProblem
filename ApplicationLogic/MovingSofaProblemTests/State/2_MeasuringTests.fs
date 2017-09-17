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
            , StateTestUtilities.measurePositioner
        )

    [<Test>]
    let ``Initializes the Measuring state``() = 
        let (startingState, _) = StateTestUtilities.initialState ()
        let startMeasuringStateTransition = startMeasuringFrom startingState

        let state = startMeasuringStateTransition.NewState
        test <@ state.MeasureLocation = StateTestUtilities.initializedMeasurePositionAndRotation @>
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
    let ``Puts the measure at the current measure location and transitions to the Measuring state in the first side effect``() = 
        let (startingState, _) = StateTestUtilities.initialState ()
        // So we can tell it changed
        let mutable measureCreatedAtPositionAndRotation =
            PositionAndRotation(Vector(-1.0f, -1.0f, -1.0f), Rotation(-1.0f, -1.0f, -1.0f, -1.0f))

        let startMeasuringStateTransition = 
            Measuring.StartMeasuring(
                startingState, 
                StateTestUtilities.cameraAtOrigin, 
                fun positionAndRotation -> measureCreatedAtPositionAndRotation <- positionAndRotation ; ()
            )

        let sideEffects = startMeasuringStateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 2 @>

        let sideEffect = List.head sideEffects 
        let newState = sideEffect.Invoke(startMeasuringStateTransition.NewState)
        test <@ newState.Mode = GameMode.Measuring @>

        test <@ newState.MeasureLocation = StateTestUtilities.initializedMeasurePositionAndRotation @>
        test <@ measureCreatedAtPositionAndRotation = StateTestUtilities.initializedMeasurePositionAndRotation @>


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

        let expectedSpokenState = 
            "Right now I can measure an object that you select. You can " +
            "Say 'Come with me' when you have finished and are ready to " + 
            "indicate where you want to move the object."

        GameState.SayStatus(measuringState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef measuringState


(******************************* STATE TRANSITION ACCESSIBILITY TESTS *******************************)

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
    let ``Can be reached from the WaitingToReplay state``() = 
        let (waitingToReplayState, _) = StateTestUtilities.waitingToReplayState ()   
        let stateTransition = startMeasuringFrom waitingToReplayState
        test <@ stateTransition.NewState.Mode = GameMode.WaitingToReplay @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)

        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Can be reached from the Replaying state``() = 
        let (replayingState, _) = StateTestUtilities.replayingState ()   
        let stateTransition = startMeasuringFrom replayingState
        test <@ stateTransition.NewState.Mode = GameMode.Replaying @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)

        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Can be reached from the FinishedReplaying state``() = 
        let (beforeState, _) = StateTestUtilities.finishedReplayingState ()   
        let stateTransition = startMeasuringFrom beforeState
        test <@ stateTransition.NewState.Mode = GameMode.FinishedReplaying @>

        let firstSideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let newState = firstSideEffect.Invoke(stateTransition.NewState)

        test <@ newState.Mode = GameMode.Measuring @>
