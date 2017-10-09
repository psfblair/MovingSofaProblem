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
        public Vector3 carryPositionRelativeToCamera; // E.g. (0,-0.2,0)
        public float replayingTranslationSpeed; // E.g. 0.7f units/sec
        public float replayingRotationSpeed; // E.g. 20.0f degrees/sec

        private GameObject measure;
		private BoundingBox boundingBox;
        private TextToSpeech textToSpeech;

        private GameState currentGameState;

        void Start()
        {
            boundingBox = FindObjectOfType<BoundingBox>();
            textToSpeech = FindObjectOfType<TextToSpeech>();

			var stateTransition = Starting.Start(
                Camera.main.transform,
                carryPositionRelativeToCamera,
                replayingTranslationSpeed,
                replayingRotationSpeed,
                StatusSpeaker(textToSpeech)
            );
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
                                                , MeasureCreator(measurePrefab, measure));
                    HandleStateTransition(startingStateTransition);
                    break;
                case GameMode.Following:
                    var followingStateTransition =
                        Following.KeepFollowing(currentGameState, Camera.main.transform, MeasureMover(measure));
                    HandleStateTransition(followingStateTransition);
                    break;
                case GameMode.Replaying:
                    var keepReplayingStateTransition =
                        Replaying.KeepReplaying(currentGameState, Time.time, MeasureMover(measure));
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
                                                          , MeasureCreator(measurePrefab, measure));
            HandleStateTransition(stateTransition);
        }

        public void StartFollowing()
        {
            boundingBox = FindObjectOfType<BoundingBox>();
            var startingMeasurePositionAndRotation =
                new PositionAndRotation(
                    measure.transform.position,
                    measure.transform.rotation
                );
            var stateTransition = Following.StartFollowing(currentGameState
                                                          , startingMeasurePositionAndRotation
                                                          , Camera.main.transform
                                                          , BoundingBoxDisabler(boundingBox)
                                                          , SpatialMappingObserverStarter
                                                          , MeasureMover(measure));
            HandleStateTransition(stateTransition);
        }

        public void StopFollowing()
        {
            var stateTransition = StoppedFollowing.StopFollowing(currentGameState
                                                                , Camera.main.transform
                                                                , MeasureReleaser(this, measure)
                                                                , SpatialMappingObserverStopper);
            HandleStateTransition(stateTransition);
        }

        public void BreakFall_MeasureHasLanded(object source, EventArgs args)
        {
            var stateTransition = PathSimplified.SimplifyPath(
                currentGameState,
                new PositionAndRotation(measure.transform.position, measure.transform.rotation),
                pathHolder => pathHolder // TODO Use a real solution finder
            );
            HandleStateTransition(stateTransition);
        }

        public void StartReplaying()
        {
            var stateTransition = WaitingToReplay.StartReplaying(currentGameState, MeasureMover(measure));
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

        private static Action<string> StatusSpeaker(TextToSpeech textToSpeech)
        {
            return status => textToSpeech.StartSpeaking(status);
        }

        private static Func<GameObject, GameObject, Action<PositionAndRotation>> MeasureCreator =
            (prefab, currentMeasure) =>
                newPositionAndRotation =>
                    {
                        if (currentMeasure != null)
                        {
                            Destroy(currentMeasure);
                        }
                        currentMeasure = Instantiate(prefab, newPositionAndRotation.Position, newPositionAndRotation.Rotation);
                    };

        private static Func<GameObject, Action<PositionAndRotation>> MeasureMover =
            measure =>
                newPositionAndRotation =>
                    {
                        measure.transform.position = newPositionAndRotation.Position;
                        measure.transform.rotation = newPositionAndRotation.Rotation;
                    };

        private static Func<BoundingBox, Action> BoundingBoxDisabler = box => () => box.Target = null;

        private static Func<MovingSofaGameController, GameObject, Action> MeasureReleaser = 
            (controller, currentMeasure) =>
                () =>
                    {
                        BreakFall breakFall = currentMeasure.AddComponent<BreakFall>();
                        breakFall.MeasureHasLanded += controller.BreakFall_MeasureHasLanded;
                        Rigidbody measureRigidBody = currentMeasure.AddComponent<Rigidbody>();
                        measureRigidBody.mass = 20;
                        measureRigidBody.useGravity = true;
                    };

        private static Action SpatialMappingObserverStarter = () => SpatialMappingManager.Instance.StartObserver();

        private static Action SpatialMappingObserverStopper = () => SpatialMappingManager.Instance.StopObserver();
#endregion
    }
}