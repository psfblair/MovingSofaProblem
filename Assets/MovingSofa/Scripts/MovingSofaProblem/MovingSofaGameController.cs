using System;
using System.Collections.Generic;
using UnityEngine;

using HUX.Interaction;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;

using MovingSofaProblem.State;
using MovingSofaProblem.Path;

namespace MovingSofaProblem
{
    public class MovingSofaGameController : MonoBehaviour
    {
        public GameObject measurePrefab;
        private BoundingBox boundingBox;
        private TextToSpeechManager textToSpeechManager;

        private GameState currentGameState;

        void Start()
        {
            boundingBox = GameObject.FindObjectOfType<BoundingBox>();
            textToSpeechManager = GameObject.FindObjectOfType<TextToSpeechManager>();

            var stateTransition = Starting.Start(StatusSpeaker(textToSpeechManager));
            HandleStateTransition(stateTransition);
        }

        void Update()
        {
            switch (currentGameState.Mode)
            {
                case GameMode.Starting:
                    var startingStateTransition =
                        Measuring.StartMeasuring(currentGameState
                                                , Camera.main.transform
                                                , MeasureCreator(measurePrefab));
                    HandleStateTransition(startingStateTransition);
                    break;
                case GameMode.Following:
                    var followingStateTransition =
                        Following.KeepFollowing(currentGameState, Camera.main.transform, MeasureCarrier);
                    HandleStateTransition(followingStateTransition);
                    break;
                case GameMode.Replaying:
                    var keepReplayingStateTransition =
                        Replaying.KeepReplaying(currentGameState, Time.time, MeasureMover);
                    HandleStateTransition(keepReplayingStateTransition);
                    break;
            }
        }

        // TODO Use box to select spatial mesh instead of as an approximation
        // TODO Allow use of other boxes to select and remove meshes of objects in the room we want to ignore.
        public void StartMeasuring()
        {
            var stateTransition = Measuring.StartMeasuring(currentGameState
                                                          , Camera.main.transform
                                                          , MeasureCreator(measurePrefab));
            HandleStateTransition(stateTransition);
        }

        public void StartFollowing()
        {
            boundingBox = GameObject.FindObjectOfType<BoundingBox>();
            var stateTransition = Following.StartFollowing(currentGameState
                                                          , Camera.main.transform
                                                          , BoundingBoxDisabler(boundingBox)
                                                          , SpatialMappingObserverStarter
                                                          , MeasureCarrier);
            HandleStateTransition(stateTransition);
        }

        public void StopFollowing()
        {
            var stateTransition = StoppedFollowing.StopFollowing(currentGameState
                                                                , Camera.main.transform
                                                                , MeasureReleaser(this)
                                                                , SpatialMappingObserverStopper);
            HandleStateTransition(stateTransition);
        }

        public void BreakFall_MeasureHasLanded(object source, EventArgs args)
        {
            var stateTransition = PathSimplified.SimplifyPath(currentGameState);
            HandleStateTransition(stateTransition);
        }

        public void StartReplaying()
        {
            var stateTransition = WaitingToReplay.StartReplaying(currentGameState);
            HandleStateTransition(stateTransition);
        }

        public void PlayNextSegment()
        {
            var stateTransition = Replaying.PlayNextSegment(currentGameState, Time.time);
            HandleStateTransition(stateTransition);
        }

        public void ReplayCurrentSegment()
        {
            var stateTransition = Replaying.ReplayCurrentSegment(currentGameState, Time.time);
            HandleStateTransition(stateTransition);
        }

        public void SayStatus()
        {
            var sideEffects = GameState.SayStatus(currentGameState);
            HandleSideEffects(sideEffects);
        }

        #region SIDE EFFECTS

        private void HandleStateTransition(StateTransition stateTransition)
        {
            currentGameState = stateTransition.NewState;
            HandleSideEffects(stateTransition.SideEffects);
        }

        private void HandleSideEffects(List<Func<GameState, GameState>> sideEffects)
        {
            // TODO Add coroutine here
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

        private static Func<GameObject, PositionAndRotation, PositionAndRotation> MeasureMover =
            (measure, newPositionAndRotation) =>
                {
                    measure.transform.position = newPositionAndRotation.Position;
                    measure.transform.rotation = newPositionAndRotation.Rotation;
                    return newPositionAndRotation;
                };

        private static Func<Vector3, Func<GameObject, Transform, PositionAndRotation>> MeasureCarrier =
            positionRelativeToForward =>
                (measure, cameraTransform) =>
                    {
                        var newPositionAndRotation = 
                            SpatialCalculations.OrientationRelativeToOneUnitForward(cameraTransform, positionRelativeToForward);
                        return MeasureMover(measure, newPositionAndRotation);
                    };

        private static Func<BoundingBox, Action> BoundingBoxDisabler = box => () => box.Target = null;

        private static Func<MovingSofaGameController, Action<GameObject>> MeasureReleaser = controller => 
            measure =>
                {
                    BreakFall breakFall = measure.AddComponent<BreakFall>();
                    breakFall.MeasureHasLanded += controller.BreakFall_MeasureHasLanded;
                    Rigidbody measureRigidBody = measure.AddComponent<Rigidbody>();
                    measureRigidBody.mass = 20;
                    measureRigidBody.useGravity = true;
                };

        private static Action SpatialMappingObserverStarter = () => SpatialMappingManager.Instance.StartObserver();

        private static Action SpatialMappingObserverStopper = () => SpatialMappingManager.Instance.StopObserver();
        #endregion
    }
}