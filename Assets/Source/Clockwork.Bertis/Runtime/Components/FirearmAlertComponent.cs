using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Simulation;

namespace Clockwork.Bertis.Components
{
    [RequireComponent(typeof(Firearm))]
    public sealed class FirearmAlertComponent : MonoBehaviour
    {
        [SerializeField]
        private SoundDescriptor _lowAmmoSound;
        [SerializeField]
        private SoundDescriptor _criticalAmmoSound;

        [SerializeField]
        private int _lowAmmoThreshold;
        [SerializeField]
        private int _criticalAmmoThreshold;

        private Firearm _firearm;

        private void OnEnable()
        {
            _firearm = GetComponent<Firearm>();
            _firearm.FiredRound += OnFiredRound;
        }

        private void OnDisable()
        {
            _firearm.FiredRound -= OnFiredRound;
        }

        private void OnValidate()
        {
            _lowAmmoThreshold = Math.Max(0, _lowAmmoThreshold);
            _criticalAmmoThreshold = Math.Clamp(_criticalAmmoThreshold, 0, _lowAmmoThreshold);
        }

        private void OnFiredRound(Firearm firearm)
        {
            if (firearm.RoundsInMagazine <= _lowAmmoThreshold)
            {
                SoundDescriptor sound = _lowAmmoSound;
                if (firearm.RoundsInMagazine < _criticalAmmoThreshold)
                {
                    sound = _criticalAmmoSound;
                }
                SoundOutputModel.Play(sound);
            }
        }
    }
}