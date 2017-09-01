namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.State
type Vector = Domain.Vector

module StartingTests =
    [<Test>]
    let ``Initializes the state``() = 
        let startingTransition = Starting.Start(fun text -> ())
        let sideEffects = startingTransition.SideEffects

        let state = startingTransition.NewState
        test <@ state.CurrentPathStep = MaybePathStep.None @>
        test <@ state.Mode = GameMode.Starting @>
        test <@ state.Measure <> null @>
        test <@ state.InitialPath.path.Count = 0 @>
        test <@ GameState.FirstStep(state) = MaybePathStep.None @>

    [<Test>]
    let ``Has a speech side effect``() = 
        let mutable whatSheSays = ""
        let startingTransition = Starting.Start(fun text -> whatSheSays <- text)
        let sideEffects = startingTransition.SideEffects |> List.ofSeq

        test <@ List.length sideEffects = 1 @>

        let sideEffect = List.head sideEffects 
        sideEffect.Invoke(startingTransition.NewState) |> ignore
        test <@ whatSheSays = "Starting." @>
