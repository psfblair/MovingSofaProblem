namespace MovingSofaProblem.State

open NUnit.Framework
open Swensen.Unquote

module FinishedReplayingTests =
    [<Test>]
    let ``Finds the truth``() = 
        let priorState = Starting.Start()
        let newState = FinishedReplaying.IsFinishedReplaying(priorState)
