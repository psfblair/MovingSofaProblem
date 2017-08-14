namespace MovingSofaProblemTests.Path

open NUnit.Framework
open Swensen.Unquote
open Functional.Option
open MovingSofaProblem.Path
open Domain

module PathHolderTests =
    let vector1 = Vector(1.0f, 1.0f, 1.0f)
    let vector2 = Vector(2.0f, 2.0f, 2.0f)
    let vector3 = Vector(3.0f, 3.0f, 3.0f)
    let rotation1 = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let rotation2 = Rotation(0.5f, 0.5f, 0.5f, 0.5f)
    let rotation3 = Rotation(1.0f, 1.0f, 1.0f, 1.0f)
    let cameraHeight1 = 1.0f
    let cameraHeight2 = 2.0f
    let cameraHeight3 = 3.0f

    [<Test>]
    let ``PathHolder: Empty PathHolder has no segments``() = 
        let pathHolder = PathHolder()
        test <@ not <| PathHolder.HasSegments(pathHolder) @>

    [<Test>]
    let ``PathHolder: FirstStep of an empty PathHolder is None``() = 
        let pathHolder = PathHolder()
        test <@ PathHolder.FirstStep(pathHolder) = Option<PathStep>.None @>

    [<Test>]
    let ``PathHolder: Final camera height of an empty PathHolder is None``() = 
        let pathHolder = PathHolder()
        test <@ PathHolder.FinalCameraY(pathHolder) = Option<float32>.None @>
    
    [<Test>]
    let ``PathHolder: A PathHolder adds a breadcrumb to an empty PathHolder``() = 
        let pathHolder = PathHolder()
        pathHolder.Add(vector1, rotation1, cameraHeight1)

        test <@ PathHolder.FirstStep(pathHolder) = Option<PathStep>.None @>

    [<Test>]
    let ``PathHolder: Adds a breadcrumb to a PathHolder that already has one in it``() = 
        let pathHolder = PathHolder()
        pathHolder.Add(vector1, rotation1, cameraHeight1)
        pathHolder.Add(vector2, rotation2, cameraHeight2)

        let firstStep = PathHolder.FirstStep(pathHolder).Value

        test <@ firstStep.PathSegment.Start = Breadcrumb(vector1, rotation1, cameraHeight1) @>
        test <@ firstStep.PathSegment.End = Breadcrumb(vector2, rotation2, cameraHeight2) @>
        test <@ PathStep.NextStep(firstStep) = Option<PathStep>.None @>

    [<Test>]
    let ``PathHolder: Has segments with two breadcrumbs``() = 
        let pathHolder = PathHolder()
        pathHolder.Add(vector1, rotation1, cameraHeight1)
        pathHolder.Add(vector2, rotation2, cameraHeight2)

        test <@ PathHolder.HasSegments(pathHolder) @>
 
    [<Test>]
    let ``PathHolder: Has two path segments with three breadcrumbs``() = 
        let pathHolder = PathHolder()
        pathHolder.Add(vector1, rotation1, cameraHeight1)
        pathHolder.Add(vector2, rotation2, cameraHeight2)
        pathHolder.Add(vector3, rotation3, cameraHeight3)

        let firstStep = PathHolder.FirstStep(pathHolder).Value

        test <@ firstStep.PathSegment.Start = Breadcrumb(vector1, rotation1, cameraHeight1) @>
        test <@ firstStep.PathSegment.End = Breadcrumb(vector2, rotation2, cameraHeight2) @>

        let secondStep = PathStep.NextStep(firstStep).Value

        test <@ secondStep.PathSegment.Start = Breadcrumb(vector2, rotation2, cameraHeight2) @>
        test <@ secondStep.PathSegment.End = Breadcrumb(vector3, rotation3, cameraHeight3) @>

        test <@ PathStep.NextStep(secondStep) = Option<PathStep>.None @>
        
    [<Test>]
    let ``PathHolder: Has segments if it has multiple path segments``() = 
        let pathHolder = PathHolder()
        pathHolder.Add(vector1, rotation1, cameraHeight1)
        pathHolder.Add(vector2, rotation2, cameraHeight2)
        pathHolder.Add(vector3, rotation3, cameraHeight3)

        test <@ PathHolder.HasSegments(pathHolder) @>
 
    [<Test>]
    let ``PathHolder: Final camera Y is camera Y of last breadcrumb``() = 
        let pathHolder = PathHolder()
        pathHolder.Add(vector1, rotation1, cameraHeight1)
        pathHolder.Add(vector2, rotation2, cameraHeight2)
        pathHolder.Add(vector3, rotation3, cameraHeight3)

        test <@ PathHolder.FinalCameraY(pathHolder).Value = cameraHeight3 @>
 
    [<Test>]
    let ``PathHolder: Simplifies a path``() = 
        true =! false
