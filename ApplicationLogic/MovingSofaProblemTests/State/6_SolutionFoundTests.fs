namespace MovingSofaProblemTests.State

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblemTests.Types

open MovingSofaProblem.Path
open MovingSofaProblem.State
open Domain

module SolutionFoundTests =

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
        let (beforeState, _) = StateTestUtilities.pathSimplifiedState ()

        let pathSimplifiedReplayPath = PathHolder()
        pathSimplifiedReplayPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        pathSimplifiedReplayPath.Add(Vector(1.0f, 0.0f, 3.0f), StateTestUtilities.zeroRotation, -0.2f)
        pathSimplifiedReplayPath.Add(Vector(6.0f, 0.0f, 3.0f), StateTestUtilities.zeroRotation, -0.2f)
        beforeState.PathToReplay <- pathSimplifiedReplayPath

        let solution = PathHolder()
        solution.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, 0.0f)
        solution.Add(Vector(1.0f, 2.0f, 3.0f), StateTestUtilities.zeroRotation, 2.0f)
        let newState = SolutionFound.HasFoundSolution(beforeState, solution)

        test <@ newState.PathToReplay = solution @>

    [<Test>]
    let ``Does not change the initial path``() = 
        let (beforeState, _) = StateTestUtilities.pathSimplifiedState ()

        let pathSimplifiedInitialPath = PathHolder()
        pathSimplifiedInitialPath.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        pathSimplifiedInitialPath.Add(Vector(1.0f, 0.0f, 3.0f), StateTestUtilities.zeroRotation, -0.2f)
        pathSimplifiedInitialPath.Add(Vector(6.0f, 0.0f, 3.0f), StateTestUtilities.zeroRotation, -0.2f)
        beforeState.InitialPath <- pathSimplifiedInitialPath

        let solution = PathHolder()
        solution.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, 0.0f)
        solution.Add(Vector(1.0f, 2.0f, 3.0f), StateTestUtilities.zeroRotation, 2.0f)
        let newState = SolutionFound.HasFoundSolution(beforeState, solution)

        test <@ newState.InitialPath = beforeState.InitialPath @>


