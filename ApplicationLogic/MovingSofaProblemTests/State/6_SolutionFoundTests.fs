namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module SolutionFoundTests =

    let solutionSecondPosition = Vector(1.0f, 2.0f, 3.0f)

    let solution () = 
        let soln = PathHolder()
        soln.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        soln.Add(solutionSecondPosition, StateTestUtilities.zeroRotation, -0.2f)
        soln

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
    let ``Initializes the SolutionFound state with the supplied solution``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState
        pathSimplifiedState.PathToReplay <- pathSimplifiedState.InitialPath

        let solutionFoundState = SolutionFound.HasFoundSolution(pathSimplifiedState, solution ())

        test <@ solutionFoundState.PathToReplay = solution () @>

    [<Test>]
    let ``Does not change the initial path``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState

        let solutionFoundState = SolutionFound.HasFoundSolution(pathSimplifiedState, solution ())

        test <@ solutionFoundState.InitialPath = pathSimplifiedState.InitialPath @>

    [<Test>]
    let ``First step is first step of solution``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState

        let solutionFoundState = SolutionFound.HasFoundSolution(pathSimplifiedState, solution ())

        test <@ GameState.FirstStep(solutionFoundState).Value.StartNode.Value = 
                    Breadcrumb(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f) @>
        test <@ GameState.FirstStep(solutionFoundState).Value.EndNode.Value = 
                    Breadcrumb(solutionSecondPosition, StateTestUtilities.zeroRotation, -0.2f) @>

    [<Test>]
    let ``Has no current path step``() = 
        let (pathSimplifiedState, _) = StateTestUtilities.pathSimplifiedState ()
        StateTestUtilities.setInitialPathAndMeasurePosition pathSimplifiedState

        let solutionFoundState = SolutionFound.HasFoundSolution(pathSimplifiedState, solution ())

        test <@ solutionFoundState.CurrentPathStep = MaybePathStep.None @>

