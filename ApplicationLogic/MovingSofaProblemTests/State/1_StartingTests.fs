namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.State
type Vector = Domain.Vector

module StartingTests =
    [<Test>]
    let ``Initializes the state``() = 
        let startingTransition = Starting.Start(StateTestUtilities.cameraAtOrigin, -0.2f, fun text -> ())
        let sideEffects = startingTransition.SideEffects

        let state = startingTransition.NewState
        test <@ state.CurrentPathStep = MaybePathStep.None @>
        test <@ state.Mode = GameMode.Starting @>
        test <@ state.MeasureLocation = StateTestUtilities.initializedMeasurePositionAndRotation @>
        test <@ state.InitialPath.path.Count = 0 @>
        test <@ GameState.FirstStep(state) = MaybePathStep.None @>

    [<Test>]
    let ``Has a speech side effect``() = 
        let mutable spokenState = ""
        let startingTransition = 
            Starting.Start(
                StateTestUtilities.cameraAtOrigin, 
                -0.2f, 
                fun text -> spokenState <- text
            )
        let sideEffects = startingTransition.SideEffects |> List.ofSeq

        test <@ List.length sideEffects = 1 @>

        let sideEffect = List.head sideEffects 
        sideEffect.Invoke(startingTransition.NewState) |> ignore
        test <@ spokenState = "Starting." @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let spokenStateRef = ref ""
        let startingTransition = 
            Starting.Start(
                StateTestUtilities.cameraAtOrigin, 
                -0.2f, 
                fun text -> spokenStateRef := text
            )
        let startingState = startingTransition.NewState

        let expectedSpokenState = "Right now I am starting."
        GameState.SayStatus(startingState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef startingState
