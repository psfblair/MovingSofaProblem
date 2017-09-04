namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module WaitingToReplayTests =

    [<Test>]
    let ``Can be reached from the SolutionFound state``() = 
        let (beforeState, _) = StateTestUtilities.solutionFoundState ()   
        let newState = WaitingToReplay.StartReplaying(beforeState).NewState
        test <@ newState.Mode = GameMode.WaitingToReplay @>

    [<Test>]
    let ``Can be reached from the PathSimplified state``() = 
        let (beforeState, _) = StateTestUtilities.pathSimplifiedState ()   
        let newState = WaitingToReplay.StartReplaying(beforeState).NewState
        test <@ newState.Mode = GameMode.WaitingToReplay @>

    let invalidTransitionMessage = "I can't replay the solution because I have no solution to replay."

    [<Test>]
    let ``Cannot be reached from the Starting state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.initialState ()   
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)
        let newState = stateTransition.NewState

        test <@ newState.Mode = GameMode.Starting @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Cannot be reached from the Measuring state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.measuringState ()   
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)
        let newState = stateTransition.NewState

        test <@ newState.Mode = GameMode.Measuring @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Cannot be reached from the Following state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.followingState ()   
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)
        let newState = stateTransition.NewState

        test <@ newState.Mode = GameMode.Following @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Cannot be reached from the StoppedFollowing state``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.stoppedFollowingState ()   
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)
        let newState = stateTransition.NewState

        test <@ newState.Mode = GameMode.StoppedFollowing @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState
