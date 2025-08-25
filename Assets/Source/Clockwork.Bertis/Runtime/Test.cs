using UnityEngine;
using Clockwork.Bertis.Components;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;
using Clockwork.Bertis.World;

namespace Clockwork.Bertis
{
    public class Test : MonoBehaviour
    {
        [SerializeField]
        private DestructibleComponent _destructibleComponent;

        [SerializeField]
        private SoundDescriptor _tickingSound;

        [SerializeField]
        private float _delay;

        [SerializeField]
        private Transform _position;

        [SerializeField]
        private C4 _c4;

        [SerializeField]
        private AirStrike _airStrike;

        [SerializeField]
        private Transform _start;

        [SerializeField]
        private Transform _end;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _c4.Detonate();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _c4.Rebuild();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                _airStrike.BeginStrike(_start.position, _end.position);
            }
        }

        private async Awaitable DetonateAsync()
        {
            SoundOutputModel.Play(_tickingSound);
            await Awaitable.WaitForSecondsAsync(_delay);
            _destructibleComponent.Explode();
        }

        [SerializeField]
        private Transform[] _sourceTransforms;

        [SerializeField]
        private Transform[] _targetTransforms;

        [ContextMenu("Sync")]
        private void Sync()
        {
            if (_sourceTransforms.Length != _targetTransforms.Length)
            {
                Debug.LogWarning("Sizes don't match.");
                return;
            }

            for (int i = 0; i < _sourceTransforms.Length; i++)
            {
                var source = _sourceTransforms[i];
                var target = _targetTransforms[i];

                if (source.name != target.name)
                {
                    Debug.LogError($"Names don't match: {source.name}, {target.name}");
                    return;
                }

                target.localPosition = source.localPosition;
                target.localRotation = source.localRotation;
                target.localScale = source.localScale;
            }
        }
    }
}