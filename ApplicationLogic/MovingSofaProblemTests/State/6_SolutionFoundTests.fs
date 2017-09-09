namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State

module SolutionFoundTests =

    [<Test>]
    let ``Initializes the SolutionFound state with the supplied solution``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState
        pathSimplifiedState.PathToReplay <- pathSimplifiedState.InitialPath

        let solutionFoundState = 
            SolutionFound.HasFoundSolution(pathSimplifiedState, StateTestUtilities.solution ())

        test <@ solutionFoundState.PathToReplay = StateTestUtilities.solution () @>

    [<Test>]
    let ``Does not change the initial path``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState

        let solutionFoundState = 
            SolutionFound.HasFoundSolution(pathSimplifiedState, StateTestUtilities.solution ())

        test <@ solutionFoundState.InitialPath = pathSimplifiedState.InitialPath @>

    [<Test>]
    let ``First step is first step of solution``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState

        let solutionFoundState = 
            SolutionFound.HasFoundSolution(pathSimplifiedState, StateTestUtilities.solution ())

        test <@ GameState.FirstStep(solutionFoundState).Value.StartNode.Value = 
                    Breadcrumb(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f) @>
        test <@ GameState.FirstStep(solutionFoundState).Value.EndNode.Value = 
                    Breadcrumb(StateTestUtilities.solutionSecondPosition
                              , StateTestUtilities.ninetyDegreesAroundZ
                              , -0.2f) @>

    [<Test>]
    let ``Has no current path step``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState

        let solutionFoundState = 
            SolutionFound.HasFoundSolution(pathSimplifiedState, StateTestUtilities.solution ())

        test <@ solutionFoundState.CurrentPathStep = MaybePathStep.None @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (pathSimplifiedState, spokenStateRef) = StateTestUtilities.pathSimplifiedState ()
        let solutionFoundState = 
            SolutionFound.HasFoundSolution(pathSimplifiedState, StateTestUtilities.solution ())

        let expectedSpokenState = "Right now I have figured out a solution. You can Say 'Replay solution' to see it."

        GameState.SayStatus(solutionFoundState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef solutionFoundState

(******************************* STATE TRANSITION ACCESSIBILITY TESTS *******************************)

    [<Test>]
    let ``Can be reached from the PathSimplified state``() = 
        let (beforeState, _) = StateTestUtilities.pathSimplifiedState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())

        test <@ newState.Mode = GameMode.SolutionFound @>

    [<Test>]
    let ``Cannot be reached from the Starting state``() = 
        let (beforeState, _) = StateTestUtilities.initialState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())
        test <@ newState.Mode = GameMode.Starting @>

    [<Test>]
    let ``Cannot be reached from the Measuring state``() = 
        let (beforeState, _) = StateTestUtilities.measuringState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())
        test <@ newState.Mode = GameMode.Measuring @>

    [<Test>]
    let ``Cannot be reached from the Following state``() = 
        let (beforeState, _) = StateTestUtilities.followingState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())
        test <@ newState.Mode = GameMode.Following @>

    [<Test>]
    let ``Cannot be reached from the StoppedFollowing state``() = 
        let (beforeState, _) = StateTestUtilities.stoppedFollowingState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())
        test <@ newState.Mode = GameMode.StoppedFollowing @>

    [<Test>]
    let ``Cannot be reached from the WaitingToReplay state``() = 
        let (beforeState, _) = StateTestUtilities.waitingToReplayState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())
        test <@ newState.Mode = GameMode.WaitingToReplay @>

    [<Test>]
    let ``Cannot be reached from the Replaying state``() = 
        let (beforeState, _) = StateTestUtilities.replayingState ()   
        let newState = SolutionFound.HasFoundSolution(beforeState, PathHolder())
        test <@ newState.Mode = GameMode.Replaying @>

    [<Test>]
    let ``Cannot be reached from the FinishedReplaying state``() = 
        let (finishedReplayingState, _) = StateTestUtilities.finishedReplayingState ()   
        let newState = SolutionFound.HasFoundSolution(finishedReplayingState, PathHolder())
        test <@ newState.Mode = GameMode.FinishedReplaying @>

