namespace MovingSofaProblemTests.Path

open System.Collections.Generic
open NUnit.Framework
open Swensen.Unquote
open Functional.Option

open MovingSofaProblem.Path
open Domain

module SpatialCalculationsTests =
    let originVector = Vector(0.0f, 0.0f, 0.0f)
    let forwardOneZ = Vector(0.0f, 0.0f, 1.0f)
    let unitX = Vector(1.0f, 0.0f, 0.0f)

    let noRotation = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let ninetyDegreesAboutZ = Rotation(0.0f, 0.0f, 1.0f, 90.0f)
    
    [<Test>]
    let ``SpatialCalculations: Calculates position and orientation relative to one unit in front of the camera``() =
        let cameraLoc = CameraLocation(originVector, noRotation, forwardOneZ)

        let expected = PositionAndRotation(Vector(1.0f, 0.0f, 1.0f), Rotation(0.0f, 0.0f, 0.0f, 1.0f))
        let result = 
            SpatialCalculations.OrientationRelativeToOneUnitForward(cameraLoc, unitX)
        
        test <@ result = expected @>
        
    [<Test>]
    let ``SpatialCalculations: Calculates angle in XZ plane between two segments``() =
        let segment1 = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(unitX, noRotation, 1.5f))
        let segment2 = PathSegment(Breadcrumb(unitX, noRotation, 1.5f), 
                                   Breadcrumb(forwardOneZ, noRotation, 1.5f))
        let result = SpatialCalculations.AngleInXZPlaneBetween(segment1, segment2)

        test <@ result = 90.0f @>

    [<Test>]
    let ``SpatialCalculations: Calculates angle in YZ plane between two segments``() =
        let segment1 = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(unitX, noRotation, 1.5f))
        let segment2 = PathSegment(Breadcrumb(unitX, noRotation, 1.5f), 
                                   Breadcrumb(forwardOneZ, noRotation, 1.5f))
        let result = SpatialCalculations.AngleInXZPlaneBetween(segment1, segment2)

        test <@ result = 0.0f @>

    [<Test>]
    let ``SpatialCalculations: Gives no interpolation if translation and rotation are both complate``() =
        let currentTime = 1.0f
        let startTime = 0.0f
        let translationSpeed = 1.0f
        let rotationSpeed = 1.0f
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(unitX, noRotation, 1.5f))
        let result = 
            SpatialCalculations.MaybeNewInterpolatedPosition(
                currentTime, startTime, translationSpeed, rotationSpeed, segment
            )

        test <@ result = Option<PositionAndRotation>.None @>

    [<Test>]
    let ``SpatialCalculations: Interpolates if translation is not complete``() =
        let currentTime = 0.5f
        let startTime = 0.0f
        let translationSpeed = 1.0f
        let rotationSpeed = 1.0f
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(unitX, noRotation, 1.5f))
        let result = 
            SpatialCalculations.MaybeNewInterpolatedPosition(
                currentTime, startTime, translationSpeed, rotationSpeed, segment
            )
        let expected = PositionAndRotation(Vector(0.5f, 0.0f, 0.0f), noRotation)
        test <@ result = Option<PositionAndRotation>.Some(expected) @>

    [<Test>]
    let ``SpatialCalculations: Interpolates if rotation is not complete``() =
        let currentTime = 0.5f
        let startTime = 0.0f
        let translationSpeed = 1.0f
        let rotationSpeed = 1.0f
        let segment = PathSegment(Breadcrumb(originVector, noRotation, 1.5f), 
                                   Breadcrumb(originVector, ninetyDegreesAboutZ, 1.5f))
        let result = 
            SpatialCalculations.MaybeNewInterpolatedPosition(
                currentTime, startTime, translationSpeed, rotationSpeed, segment
            )
        let expected = PositionAndRotation(originVector, Rotation(0.0f, 0.0f, 0.0f, 45.0f))
        test <@ result = Option<PositionAndRotation>.Some(expected) @>

    
