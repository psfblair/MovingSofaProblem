namespace MovingSofaProblemTests.Path

open NUnit.Framework
open Swensen.Unquote
open MovingSofaProblem.Path
open Domain

module BreadcrumbTests =
    let vector1 = Vector(1.0f, 1.0f, 1.0f)
    let vector2 = Vector(2.0f, 2.0f, 2.0f)
    let rotation1 = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let rotation2 = Rotation(1.0f, 1.0f, 1.0f, 1.0f)
    let cameraHeight1 = 1.0f
    let cameraHeight2 = 2.0f

    [<Test>]
    let ``Breadcrumb: Breadcrumbs with equal values are equal``() = 
        let breadcrumb1 = new Breadcrumb(vector1, rotation1, cameraHeight1);
        let breadcrumb2 = new Breadcrumb(vector1, rotation1, cameraHeight1);

        test <@ breadcrumb1 = breadcrumb2 @>

    [<Test>]
    let ``Breadcrumb: Breadcrumbs with unequal positions are not equal``() = 
        let breadcrumb1 = new Breadcrumb(vector1, rotation1, cameraHeight1);
        let breadcrumb2 = new Breadcrumb(vector2, rotation1, cameraHeight1);

        test <@ breadcrumb1 <> breadcrumb2 @>

    [<Test>]
    let ``Breadcrumb: Breadcrumbs with unequal rotations are not equal``() = 
        let breadcrumb1 = new Breadcrumb(vector1, rotation1, cameraHeight1);
        let breadcrumb2 = new Breadcrumb(vector1, rotation2, cameraHeight1);

        test <@ breadcrumb1 <> breadcrumb2 @>

    [<Test>]
    let ``Breadcrumb: Breadcrumbs with unequal camera heights are not equal``() = 
        let breadcrumb1 = new Breadcrumb(vector1, rotation1, cameraHeight1);
        let breadcrumb2 = new Breadcrumb(vector1, rotation1, cameraHeight2);

        test <@ breadcrumb1 <> breadcrumb2 @>