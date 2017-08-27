namespace MovingSofaProblemTests.Path

open NUnit.Framework
open Swensen.Unquote

open MovingSofaProblem.Path
open Domain

module PathSegmentTests =
    let vector1 = Vector(1.0f, 1.0f, 1.0f)
    let vector2 = Vector(2.0f, 3.0f, 4.0f)
    let noRotation = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let ninetyDegreesAboutZ = Rotation(0.0f, 0.0f, 1.0f, 0.25f)
    let cameraHeight1 = 1.0f
    let cameraHeight2 = 2.0f
    let breadcrumb1 = Breadcrumb(vector1, noRotation, cameraHeight1)
    let breadcrumb2 = Breadcrumb(vector2, ninetyDegreesAboutZ, cameraHeight2)

    [<Test>]
    let ``PathSegment: Defines a start and end``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.Start = breadcrumb1 @>
        test <@ segment.End = breadcrumb2 @>

    [<Test>]
    let ``PathSegment: Calculates X displacement``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.XDisplacement = 1.0f @>

    [<Test>]
    let ``PathSegment: Calculates Y displacement``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.YDisplacement = 2.0f @>

    [<Test>]
    let ``PathSegment: Calculates Z displacement``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.ZDisplacement = 3.0f @>

    [<Test>]
    let ``PathSegment: Calculates length``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)
        test <@ segment.TranslationDistance = sqrt 14.0f @>
    
    // Dummy domain assumes that the euler angles are the same as the x, y, z of the rotation
    // times 360 degrees.
    [<Test>]
    let ``PathSegment: Calculates X axis rotation in terms of proportion of a circle``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.XAxisRotationChange = 0.0f @>

    [<Test>]
    let ``PathSegment: Calculates Y axis rotation in terms of proportion of a circle``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.YAxisRotationChange = 0.0f @>

    [<Test>]
    let ``PathSegment: Calculates Z axis rotation in terms of proportion of a circle``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)  
        test <@ segment.ZAxisRotationChange = 1.0f @>

    // Dummy domain gives canned values for specific rotations
    [<Test>]
    let ``PathSegment: Calculates rotation angle``() = 
        let segment = PathSegment(breadcrumb1, breadcrumb2)
        test <@ segment.RotationAngle = 90.0f @>
