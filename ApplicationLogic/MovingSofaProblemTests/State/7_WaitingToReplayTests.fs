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

    [<Test>]
    let ``Cannot be reached if the solution has no first step``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.solutionFoundState ()   
        let emptySolution = PathHolder()
        emptySolution.Add(StateTestUtilities.origin, StateTestUtilities.zeroRotation, -0.2f)
        beforeState.PathToReplay <- emptySolution

        let stateTransition = WaitingToReplay.StartReplaying(beforeState)
        let newState = stateTransition.NewState

        test <@ newState.Mode = GameMode.SolutionFound @>

        stateTransition.SideEffects 
            |> StateTestUtilities.testSingleSideEffectSpeaks invalidTransitionMessage spokenStateRef newState

    [<Test>]
    let ``Has two side effects``() = 
        let (beforeState, _) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)

        let sideEffects = stateTransition.SideEffects |> List.ofSeq
        test <@ List.length sideEffects = 2 @>

    [<Test>]
    let ``Moves the measure to the starting point of the solution in the first side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.solutionFoundState ()
        let protoSolutionPath = (StateTestUtilities.solution ()).path
        let initialPosition = Vector(-1.0f, 0.0f, 2.0f)
        let initialRotation = Rotation(-10.0f, -10.0f, 0.0f, 0.0f)
        protoSolutionPath.AddFirst(Breadcrumb(initialPosition, initialRotation, -0.2f)) |> ignore
        let solution = PathHolder(protoSolutionPath)

        beforeState.PathToReplay <- solution
        beforeState.Measure.transform.position <- solution.path.Last.Value.Position
        beforeState.Measure.transform.rotation <- solution.path.Last.Value.Rotation

        test <@ solution.path.First.Value.Position <> solution.path.Last.Value.Position @>
        test <@ solution.path.First.Value.Rotation <> solution.path.Last.Value.Rotation @>
         
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)

        let newState = stateTransition.NewState
        let sideEffect = stateTransition.SideEffects |> List.ofSeq |> List.head
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.WaitingToReplay @>

        test <@ newState.Measure.transform.position = initialPosition @>
        test <@ newState.Measure.transform.rotation = initialRotation @>

    [<Test>]
    let ``Speaks the state in the second side effect``() = 
        let (beforeState, spokenStateRef) = StateTestUtilities.solutionFoundState ()   
        let stateTransition = WaitingToReplay.StartReplaying(beforeState)

        let newState = stateTransition.NewState
        let sideEffect = stateTransition.SideEffects |> List.ofSeq |> List.item 1
        let stateAfterSideEffect = sideEffect.Invoke(newState)
        test <@ stateAfterSideEffect.Mode = GameMode.WaitingToReplay @>

        test <@ !spokenStateRef = "Replaying the solution. Say 'Next' to replay the next step." @>

    [<Test>]
    let ``Can tell you what state you are in``() = 
        let (solutionFoundState, spokenStateRef) = StateTestUtilities.solutionFoundState ()
        let waitingToReplayState = WaitingToReplay.StartReplaying(solutionFoundState).NewState

        let expectedSpokenState = "Right now I am ready to replay the solution. You can Say 'Next' to replay the next step."
        GameState.SayStatus(waitingToReplayState) 
            |> StateTestUtilities.testSingleSideEffectSpeaks expectedSpokenState spokenStateRef waitingToReplayState
