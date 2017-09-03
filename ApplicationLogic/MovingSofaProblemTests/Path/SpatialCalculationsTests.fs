namespace MovingSofaProblemTests.Path

open NUnit.Framework
open Swensen.Unquote

open MovingSofaProblem.Path
open Domain

module SpatialCalculationsTests =
    let originVector = Vector(0.0f, 0.0f, 0.0f)
    let forwardOneZFromOrigin = Vector(0.0f, 0.0f, 1.0f)
    let unitX = Vector(1.0f, 0.0f, 0.0f)
    let forwardOneZFromUnitX = Vector(1.0f, 0.0f, 1.0f)

    let noRotation = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let ninetyDegreesAboutZ = Rotation(0.0f, 0.0f, 90.0f, 0.0f)


    [<Test>]
    let ``SpatialCalculations: Calculates position and orientation relative to one unit in front of the camera``() =
        let cameraLoc = Situation(Vector(2.0f, 3.0f, 4.0f), Rotation(90.0f, 179.0f, 270.0f, 90.0f), forwardOneZFromOrigin)

        // This calculation is expected to leave rotation at zero except on the y axis.
        // For ease of testing, the Dummy euler angles just takes the x, y, z of the rotation
        let expected = PositionAndRotation(Vector(3.0f, 3.0f, 5.0f), Rotation(0.0f, -1.0f, 0.0f, 0.0f))
        
        test <@ SpatialCalculations.ReorientRelativeToOneUnitForwardFrom(cameraLoc, unitX) = expected @>

    [<Test>]
    let ``SpatialCalculations: Calculates angle in XZ plane between two segments that have no Y component``() =
        let segment1 = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(unitX, noRotation, 1.5f))
        let segment2 = PathSegment(Breadcrumb(unitX, noRotation, 1.5f), 
                                   Breadcrumb(forwardOneZFromUnitX, noRotation, 1.5f))

        test <@ SpatialCalculations.AngleInXZPlaneBetween(segment1, segment2) = 90.0f @>

    [<Test>]
    let ``SpatialCalculations: Calculates angle in YZ plane between two segments that have no Y component``() =
        let segment1 = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(unitX, noRotation, 1.5f))
                                   // This segment has zero Y and Z magnitude
        let segment2 = PathSegment(Breadcrumb(unitX, noRotation, 1.5f), 
                                   Breadcrumb(forwardOneZFromUnitX, noRotation, 1.5f))
                                   // Arc cos of 0 is 90 degrees
        test <@ SpatialCalculations.AngleInYZPlaneBetween(segment1, segment2) = 90.0f @>

    [<Test>]
    let ``SpatialCalculations: Calculates angle in XZ plane between two arbitrary segments``() =
        let segment1 = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(Vector(2.0f, 1.0f, 3.0f), noRotation, 1.5f))
                                   // X displacement 2, Z displacement 3 -> angle from origin is atan(3/2) = 56.31deg
        let segment2 = PathSegment(Breadcrumb(Vector(3.0f, 5.0f, 6.0f), noRotation, 1.5f), 
                                   Breadcrumb(Vector(9.0f, 1.0f, 7.0f), noRotation, 1.5f))
                                   // X displacement 6, Z displacement 1 -> angle from origin is atan(1/6) = 9.46deg
                                   // Difference is 46.84 degrees
        test <@ SpatialCalculations.AngleInXZPlaneBetween(segment1, segment2) = 46.847610f @>

    [<Test>]
    let ``SpatialCalculations: Calculates angle in YZ plane between two arbitrary segments``() =
        let segment1 = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(Vector(2.0f, 1.0f, 3.0f), noRotation, 1.5f))
                                   // Y displacement 1, Z displacement 3 -> angle from origin atan(1/3) = 18.43deg
        let segment2 = PathSegment(Breadcrumb(Vector(3.0f, 5.0f, 6.0f), noRotation, 1.5f), 
                                   Breadcrumb(Vector(9.0f, 1.0f, 7.0f), noRotation, 1.5f))
                                   // Y displacement -4, Z displacement 1 -> angle from origin atan(-4) = = -75.96deg
                                   // Difference is 85.60deg
        test <@ SpatialCalculations.AngleInYZPlaneBetween(segment1, segment2) = 94.398704f @>

    [<Test>]
    let ``SpatialCalculations: Indicates if translation and rotation are both complate``() =
        let segment = PathSegment(Breadcrumb(originVector, ninetyDegreesAboutZ, 1.5f), 
                                  Breadcrumb(unitX, ninetyDegreesAboutZ, 1.5f))

        test <@ SpatialCalculations.IsMovementComplete(segment, unitX, ninetyDegreesAboutZ) = true @>

    [<Test>]
    let ``SpatialCalculations: Indicates if translation is not complete``() =
        let segment = PathSegment(Breadcrumb(originVector, ninetyDegreesAboutZ, 1.5f), 
                                  Breadcrumb(unitX, ninetyDegreesAboutZ, 1.5f))

        test <@ SpatialCalculations.IsMovementComplete(segment, Vector(0.5f, 0.0f, 0.0f), ninetyDegreesAboutZ) = false @>

    [<Test>]
    let ``SpatialCalculations: Indicates if rotation is not complete``() =
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                  Breadcrumb(unitX, ninetyDegreesAboutZ, 1.5f))

        test <@ SpatialCalculations.IsMovementComplete(segment, unitX, Rotation(0.0f, 0.0f, 0.0f, 45.0f)) = false @>

    [<Test>]
    let ``SpatialCalculations: Calculates new position if there is no rotation``() =
        let currentTime = 0.5f
        let startTime = 0.0f
        let translationSpeed = 1.0f
        let rotationSpeed = 90.0f // degrees per time
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                  Breadcrumb(unitX, noRotation, 1.5f))
        
        test <@ SpatialCalculations.InterpolatedPositionAndRotation(currentTime, startTime, translationSpeed, rotationSpeed, segment) = 
                    PositionAndRotation(Vector(0.5f, 0.0f, 0.0f), noRotation) @>

    [<Test>]
    let ``SpatialCalculations: Calculates new position with complete rotation``() =
        let currentTime = 0.5f
        let startTime = 0.0f
        let translationSpeed = 1.0f
        let rotationSpeed = 180.0f // degrees per time
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                  Breadcrumb(unitX, ninetyDegreesAboutZ, 1.5f))
        
        test <@ SpatialCalculations.InterpolatedPositionAndRotation(currentTime, startTime, translationSpeed, rotationSpeed, segment) = 
                    PositionAndRotation(Vector(0.5f, 0.0f, 0.0f), ninetyDegreesAboutZ) @>

    [<Test>]
    let ``SpatialCalculations: Calculates new rotation if there is no translation``() =
        let currentTime = 0.5f
        let startTime = 0.0f
        let translationSpeed = 1.0f
        let rotationSpeed = 90.0f // degrees per time
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                  Breadcrumb(originVector, ninetyDegreesAboutZ, 1.5f))
        // Dummy Rotation angle calculation gives angle to rotate as 90 degrees with these inputs
        // Dummy Rotation Lerp just multiplies x,y,z,w all by the specified proportion.
        test <@ SpatialCalculations.InterpolatedPositionAndRotation(currentTime, startTime, translationSpeed, rotationSpeed, segment) = 
                    PositionAndRotation(originVector, Rotation(0.0f, 0.0f, 45.0f, 0.0f)) @>


    [<Test>]
    let ``SpatialCalculations: Calculates new rotation with complete translation``() =
        let currentTime = 0.5f
        let startTime = 0.0f
        let translationSpeed = 2.0f
        let rotationSpeed = 90.0f // degrees per time
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                  Breadcrumb(unitX, ninetyDegreesAboutZ, 1.5f))

        // Dummy Rotation angle calculation gives angle to rotate as 90 degrees with these inputs
        // Dummy Rotation Lerp just multiplies x,y,z,w all by the specified proportion.
        test <@ SpatialCalculations.InterpolatedPositionAndRotation(currentTime, startTime, translationSpeed, rotationSpeed, segment) = 
                    PositionAndRotation(unitX, Rotation(0.0f, 0.0f, 45.0f, 0.0f)) @>
    
