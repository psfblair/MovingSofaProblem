namespace MovingSofaProblemTests.Path

open System.Collections.Generic
open NUnit.Framework
open Swensen.Unquote
open Functional.Option

open MovingSofaProblem.Path
open Domain

module PathStepTests =
    let vector1 = Vector(1.0f, 1.0f, 1.0f)
    let vector2 = Vector(2.0f, 2.0f, 2.0f)
    let vector3 = Vector(3.0f, 3.0f, 3.0f)
    let rotation1 = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let rotation2 = Rotation(0.5f, 0.5f, 0.5f, 0.5f)
    let rotation3 = Rotation(1.0f, 1.0f, 1.0f, 1.0f)
    let cameraHeight1 = 1.0f
    let cameraHeight2 = 2.0f
    let cameraHeight3 = 3.0f
    let breadcrumb1 = Breadcrumb(vector1, rotation1, cameraHeight1)
    let breadcrumb2 = Breadcrumb(vector2, rotation2, cameraHeight2)
    let breadcrumb3 = Breadcrumb(vector3, rotation3, cameraHeight3)
    let breadcrumbList = new LinkedList<Breadcrumb>()
    breadcrumbList.AddLast(breadcrumb1) |> ignore
    breadcrumbList.AddLast(breadcrumb2) |> ignore
    breadcrumbList.AddLast(breadcrumb3) |> ignore
    let first = breadcrumbList.First
    let second = first.Next
    let third = second.Next

    [<Test>]
    let ``PathStep: Defines a start and end``() = 
        let step = PathStep(first, second)  
        test <@ step.StartNode = first @>
        test <@ step.EndNode = second @>
        
    [<Test>]
    let ``PathStep: Defines a path segment``() = 
        let step = PathStep(first, second)  
        test <@ step.PathSegment = PathSegment(breadcrumb1, breadcrumb2) @>
        
    [<Test>]
    let ``PathStep: Is connected to the next step``() = 
        let step = PathStep(first, second)  
        test <@ PathStep.NextStep(step).Value = PathStep(second, third) @>
  
    [<Test>]
    let ``PathStep: Returns none if there is no next step``() = 
        let step = PathStep(second, third)  
        test <@ PathStep.NextStep(step) = Option<PathStep>.None @>
  
    [<Test>]
    let ``PathStep: Is connected to the previous step``() = 
        let step = PathStep(second, third)   
        test <@ PathStep.PreviousStep(step).Value = PathStep(first, second) @>

    [<Test>]
    let ``PathStep: Returns none if there is no previous step``() = 
        let step = PathStep(first, second)   
        test <@ PathStep.PreviousStep(step) = Option<PathStep>.None @>
