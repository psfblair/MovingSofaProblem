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

    void Start()
    {
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;

        boundingBox = GameObject.FindObjectOfType<BoundingBox>();
        textToSpeechManager = GameObject.FindObjectOfType<TextToSpeechManager>();
        Action<string> StatusSpeaker = status => { textToSpeechManager.SpeakText(status); };

        var stateTransition = StateTransitions.Start(StatusSpeaker);
        HandleStateTransition(stateTransition);
    }

    void Update()
    {
        switch (currentGameState.Mode)
        {
            case GameMode.Starting:
                var startingStateTransition = StateTransitions.StartMeasuring(currentGameState, Camera.main.transform, MeasureCreator(measurePrefab));
                HandleStateTransition(startingStateTransition);
                break;
            case GameMode.Following:
                var followingStateTransition = StateTransitions.KeepFollowing(currentGameState, Camera.main.transform, MeasureMover);
                HandleStateTransition(followingStateTransition);
                break;
            case GameMode.Replaying:
                // TODO Replay saved path with segments next, next, next
                // TODO: If the current breadcrumb position <> end breadcrumb position, translate an increment toward it.
                // TODO: If the current bradcrumb rotation <> end breadcrumb rotation, rotate an increment toward it.
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
                                                             , MeasureMover);
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
        var stateTransition = StateTransitions.RouteInitialized(currentGameState
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

#region FUNCTIONS WITH SIDE EFFECTS

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

    private Func<GameObject, Func<GameObject, PositionAndRotation, GameObject>> MeasureCreator = 
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

    private Func<GameObject, Transform, PositionAndRotation> MeasureMover = (measure, cameraTransform) =>
    {
        var measureExtents = measure.GetComponent<Collider>().bounds.extents;
        var newPositionAndRotation = SpatialCalculations.PositionInFrontOf(measureExtents, cameraTransform);
        measure.transform.position = newPositionAndRotation.Position;
        measure.transform.rotation = newPositionAndRotation.Rotation;
        return newPositionAndRotation;
    };

    private Func<BoundingBox,Action> BoundingBoxDisabler = box => () => box.Target = null;

    private Action SpatialMappingObserverStarter = () => SpatialMappingManager.Instance.StartObserver();

    private Action SpatialMappingObserverStopper = () => SpatialMappingManager.Instance.StopObserver();

    private Action RoomPlanesCreator = () =>
    {
        // Generate planes based on the spatial map.
        var surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    };

    private Action WallVertexRemover = () =>
    {
        RemoveSurfaceVertices removeVerts = RemoveSurfaceVertices.Instance;
        if (removeVerts != null && removeVerts.enabled)
        {
            removeVerts.RemoveSurfaceVerticesWithinBounds(SurfaceMeshesToPlanes.Instance.ActivePlanes);
        }
    };

    private Func<Material,Action> WallSurfaceCreator = 
        wallPlaneMaterial => () => SpatialMappingManager.Instance.SetSurfaceMaterial(wallPlaneMaterial);

#endregion
}
