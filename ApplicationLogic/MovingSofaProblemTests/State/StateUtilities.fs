namespace MovingSofaProblemTests.State

open MovingSofaProblem.State
open Domain

module StateUtilities =

    let origin = Vector(0.0f, 0.0f, 0.0f)
    let zeroRotation = Rotation(0.0f, 0.0f, 0.0f, 0.0f)
    let forwardZ = Vector(0.0f, 0.0f, 1.0f)

    let initialState = 
        let whatSheSays = ref ""
        let startingTransition = Starting.Start(fun text -> whatSheSays := text)
        (whatSheSays, startingTransition.NewState)

    let cameraAtOrigin =
        let originPosition = origin
        let zeroRotation = zeroRotation
        Situation(originPosition, zeroRotation, forwardZ)

    let measureAt position rotation forward =
        let situation = Situation(position, rotation, forward)
        Measure(situation)
