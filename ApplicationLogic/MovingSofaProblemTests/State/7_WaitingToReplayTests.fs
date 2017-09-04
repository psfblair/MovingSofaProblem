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

    [<Test>]
    let ``Cannot be reached from the Starting state``() = 
        let (beforeState, _) = StateTestUtilities.initialState ()   
        let newState = WaitingToReplay.StartReplaying(beforeState).NewState

        test <@ newState.Mode = GameMode.Starting @>

    [<Test>]
    let ``Cannot be reached from the Measuring state``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()   
        let newState = WaitingToReplay.StartReplaying(beforeState).NewState

        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Cannot be reached from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let newState = WaitingToReplay.StartReplaying(beforeState).NewState

        test <@ newState.Mode = GameMode.Following @>

    [<Test>]
    let ``Cannot be reached from the StoppedFollowing state``() = 
        let (beforeState, _) = StateTestUtilities.stoppedFollowingState ()   
        let newState = WaitingToReplay.StartReplaying(beforeState).NewState

        test <@ newState.Mode = GameMode.StoppedFollowing @>
