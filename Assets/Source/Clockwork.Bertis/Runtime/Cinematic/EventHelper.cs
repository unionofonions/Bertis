using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Scripting;

namespace Clockwork.Bertis.Cinematic
{
    public class EventHelper : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _sceneObjects;

        [SerializeField]
        private Behaviour[] _sceneBehaviours;

        [SerializeField]
        private GameObject[] _cinematicObjects;

        [SerializeField]
        private Behaviour[] _cinematicBehaviours;

        [SerializeField]
        private Animator _playerAnimator;

        [SerializeField]
        private RuntimeAnimatorController _playerRtController;

        [Preserve]
        public void ActivateSceneObjects()
            => ToggleObjects(_sceneObjects, true);

        [Preserve]
        public void DeactivateSceneObjects()
            => ToggleObjects(_sceneObjects, false);

        [Preserve]
        public void EnableSceneBehaviours()
            => ToggleBehaviours(_sceneBehaviours, true);

        [Preserve]
        public void DisableSceneBehaviours()
            => ToggleBehaviours(_sceneBehaviours, false);

        [Preserve]
        public void ActivateCinematicObjects()
            => ToggleObjects(_sceneObjects, true);

        [Preserve]
        public void DeactivateCinematicObjects()
            => ToggleObjects(_sceneObjects, false);

        [Preserve]
        public void EnableCinematicBehaviours()
            => ToggleBehaviours(_sceneBehaviours, true);

        [Preserve]
        public void DisableCinematicBehaviours()
            => ToggleBehaviours(_sceneBehaviours, false);

        [Preserve]
        public void StopPlayerAnimTrack()
        {
            var director = GetComponent<PlayableDirector>();
            foreach (var output in director.playableAsset.outputs)
            {
                if (output.streamName == "Player Track")
                {
                    director.SetGenericBinding(output.sourceObject, null);
                    break;
                }
            }
            _playerAnimator.runtimeAnimatorController = _playerRtController;
        }

        private void ToggleObjects(GameObject[] objects, bool value)
        {
            foreach (GameObject gameObject in objects)
            {
                gameObject.SetActive(value);
            }
        }

        private void ToggleBehaviours(Behaviour[] behaviours, bool value)
        {
            foreach (Behaviour behaviour in behaviours)
            {
                behaviour.enabled = value;
            }
        }
    }
}