using System;
using System.Collections.Generic;
using UnityEngine;

using HUX.Interaction;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using MovingSofaProblem;

public class MovingSofaGameController : MonoBehaviour
{
    public GameObject measurePrefab;
    public Material wallPlane;

    private BoundingBox boundingBox;
    private TextToSpeechManager textToSpeechManager;

    private GameState currentGameState;

    private static float replayingTranslationSpeed = 1.0f; // units/sec
    private static float replayingRotationSpeed = 1.0f; // degrees/sec


    void Start()
    {
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;

        boundingBox = GameObject.FindObjectOfType<BoundingBox>();
        textToSpeechManager = GameObject.FindObjectOfType<TextToSpeechManager>();

        var stateTransition = StateTransitions.Start(StatusSpeaker(textToSpeechManager));
        HandleStateTransition(stateTransition);
    }

    void Update()
    {
        switch (currentGameState.Mode)
        {
            case GameMode.Starting:
                var startingStateTransition = 
                    StateTransitions.StartMeasuring(currentGameState
                                                   , Camera.main.transform
                                                   , MeasureCreator(measurePrefab));
                HandleStateTransition(startingStateTransition);
                break;
            case GameMode.Following:
                var followingStateTransition = 
                    StateTransitions.KeepFollowing(currentGameState, Camera.main.transform, MeasureCarrier);
                HandleStateTransition(followingStateTransition);
                break;
            case GameMode.Replaying:
                var keepReplayingStateTransition =
                    StateTransitions.KeepReplaying(currentGameState
                                                  , Time.time
                                                  , replayingTranslationSpeed
                                                  , replayingRotationSpeed
                                                  , MeasureMover);
                HandleStateTransition(keepReplayingStateTransition);
                break;
        }
    }

    // TODO Use box to select spatial mesh instead of as an approximation
    // TODO Allow use of other boxes to select and remove meshes of objects in the room we want to ignore.
    public void StartMeasuring()
    {
        var stateTransition = StateTransitions.StartMeasuring( currentGameState
                                                             , Camera.main.transform
                                                             , MeasureCreator(measurePrefab));
        HandleStateTransition(stateTransition);
    }

    public void StartFollowing()
    {
        var stateTransition = StateTransitions.StartFollowing( currentGameState
                                                             , Camera.main.transform
                                                             , BoundingBoxDisabler(boundingBox)
                                                             , SpatialMappingObserverStarter
                                                             , MeasureCarrier);
        HandleStateTransition(stateTransition);
    }

    public void StopFollowing()
    {
        var stateTransition = StateTransitions.StopFollowing( currentGameState
                                                            , SpatialMappingObserverStopper
                                                            , RoomPlanesCreator);
        HandleStateTransition(stateTransition);
    }

    private void SurfaceMeshesToPlanes_MakePlanesComplete(object source, EventArgs args)
    {
        var stateTransition = StateTransitions.InitializeRoute(currentGameState
                                                               , WallVertexRemover 
                                                               , WallSurfaceCreator(wallPlane));
        HandleStateTransition(stateTransition);
    }

    public void StartReplaying()
    {
        var stateTransition = StateTransitions.StartReplaying(currentGameState);
        HandleStateTransition(stateTransition);
    }

    public void SayStatus(string intro)
    {
        var sideEffects = StateTransitions.SayStatus(currentGameState, intro);
        HandleSideEffects(sideEffects);
    }

#region SIDE EFFECTS

    private void HandleStateTransition(StateTransition stateTransition)
    {
        currentGameState = stateTransition.NewState;
        HandleSideEffects(stateTransition.SideEffects);
    }

    private void HandleSideEffects(List<Func<GameState,GameState>> sideEffects)
    {
        foreach (var sideEffect in sideEffects)
        {
            currentGameState = sideEffect(currentGameState);
        }
    }

    private static Action<string> StatusSpeaker(TextToSpeechManager textToSpeechManager) 
    {
        return status => textToSpeechManager.SpeakText(status);
    }

    private static Func<GameObject, Func<GameObject, PositionAndRotation, GameObject>> MeasureCreator = 
        measurePrefab => 
            (measure, newPositionAndRotation) =>
                {
                    if (measure != null)
                    {
                        Destroy(measure);
                    }
                    measure = Instantiate(measurePrefab, newPositionAndRotation.Position, newPositionAndRotation.Rotation);
                    return measure;
                };

    private static Func<GameObject, PositionAndRotation, PositionAndRotation> MeasureMover = (measure, newPositionAndRotation) =>
    {
        measure.transform.position = newPositionAndRotation.Position;
        measure.transform.rotation = newPositionAndRotation.Rotation;
        return newPositionAndRotation;
    };

    private static Func<GameObject, Transform, PositionAndRotation> MeasureCarrier = (measure, cameraTransform) =>
    {
        var measureExtents = measure.GetComponent<Collider>().bounds.extents;
        var newPositionAndRotation = SpatialCalculations.PositionInFrontOf(measureExtents, cameraTransform);
        return MeasureMover(measure, newPositionAndRotation);
    };

    private static Func<BoundingBox,Action> BoundingBoxDisabler = box => () => box.Target = null;

    private static Action SpatialMappingObserverStarter = () => SpatialMappingManager.Instance.StartObserver();

    private static Action SpatialMappingObserverStopper = () => SpatialMappingManager.Instance.StopObserver();

    private static Action RoomPlanesCreator = () =>
    {
        // Generate planes based on the spatial map.
        var surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    };

    private static Action WallVertexRemover = () =>
    {
        RemoveSurfaceVertices removeVerts = RemoveSurfaceVertices.Instance;
        if (removeVerts != null && removeVerts.enabled)
        {
            removeVerts.RemoveSurfaceVerticesWithinBounds(SurfaceMeshesToPlanes.Instance.ActivePlanes);
        }
    };

    private static Func<Material,Action> WallSurfaceCreator = 
        wallPlaneMaterial => () => SpatialMappingManager.Instance.SetSurfaceMaterial(wallPlaneMaterial);

#endregion
}
