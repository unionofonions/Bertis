using System;
using UnityEngine;
using Clockwork.Bertis.Components;
using Clockwork.Pooling;
using Clockwork.Simulation;

#nullable enable

namespace Clockwork.Bertis.Gameplay
{
    public interface IFirearmOperator
    {
        float SpreadFactor { get; }
    }

    public enum FirearmFireMode
    {
        SemiAutomatic,
        FullyAutomatic
    }

    public class Firearm : MonoBehaviour
    {
        [SerializeField]
        private FirearmFireMode _fireMode;

        [SerializeField]
        private int _fireRate;

        private float _fireTimestamp;

        private bool _isTriggerPulled;

        [SerializeField]
        private int _magazineCapacity;

        [SerializeField]
        private int _roundsInMagazine;

        [SerializeField]
        private float _projectileLinearSpeed;

        [SerializeField]
        private float _projectileMaxRange;

        [SerializeField]
        [PrefabReference]
        private Projectile? _projectilePrefab;

        [SerializeField]
        private Transform? _muzzleTransform;

        [SerializeField]
        [PrefabReference]
        private Casing? _casingPrefab;

        [SerializeField]
        private Transform? _ejectorTransform;

        [SerializeField]
        private float _baseRecoil;

        [SerializeField]
        private float _fireRecoilGain;

        [SerializeField]
        private float _recoilRecovery;

        [SerializeField]
        private SoundDescriptor? _singleFireSound;

        [SerializeField]
        private SoundDescriptor? _dryFireSound;

        [SerializeField]
        private SoundDescriptor? _foleySound;

        [SerializeField]
        private AudioClip? _loopSound;

        [SerializeField]
        private AudioClip? _tailSound;

        [SerializeField]
        private AudioSource? _loopSource;

        [SerializeField]
        private AudioLowPassFilter? _loopLowPassFilter;

        [SerializeField]
        private AudioSource? _tailSource;

        private bool _isAutoSoundStarted;

        [SerializeField]
        private ParticleSystem? _muzzleFlash;

        [SerializeField]
        private Light? _muzzleLight;

        [SerializeField]
        private float _muzzleLightDuration;

        [SerializeField]
        private CameraShakeDescriptor? _fireCameraShake;

        [SerializeField]
        private PlayerCamera? _playerCamera;

        private Entity? _owner;

        public event Action<Firearm>? FiredRound;

        public event Action<Firearm>? ReloadStarted;

        public event Action<Firearm>? Reloaded;

        public float ProjectileMaxRange => _projectileMaxRange;

        public int MagazineCapacity
        {
            get => _magazineCapacity;
            set
            {
                _magazineCapacity = Math.Max(0, value);
                Reloaded?.Invoke(this);
            }
        }

        public int RoundsInMagazine => _roundsInMagazine;

        public bool IsMagazineEmpty => _roundsInMagazine <= 0;

        public bool IsMagazineFull => _roundsInMagazine >= _magazineCapacity;

        public float BaseRecoil => _baseRecoil;

        public float FireRecoilGain => _fireRecoilGain;

        public float RecoilRecovery => _recoilRecovery;

        public Entity? Owner { get => _owner; set => _owner = value; }

        private double FireInterval => 60d / _fireRate;

        public void PullTrigger()
        {
            if (_isTriggerPulled)
            {
                return;
            }

            _isTriggerPulled = true;
            _loopSource!.Stop();
            TryFireRound();
        }

        public void ReleaseTrigger()
        {
            _isTriggerPulled = false;

            if (_fireMode == FirearmFireMode.FullyAutomatic && _isAutoSoundStarted)
            {
                _isAutoSoundStarted = false;
                double offset = FireInterval - _loopSource!.time % FireInterval;
                double time = AudioSettings.dspTime + offset;
                _loopSource!.SetScheduledEndTime(time);
                _loopSource!.loop = false;
                _tailSource!.PlayScheduled(time);
            }
        }

        public void StartReload()
        {
            ReleaseTrigger();
            ReloadStarted?.Invoke(this);
        }

        public void Reload()
        {
            if (_roundsInMagazine < _magazineCapacity)
            {
                _roundsInMagazine = _magazineCapacity;
                Reloaded?.Invoke(this);
            }
        }

        public void CycleFireMode()
        {
            _fireMode = _fireMode switch
            {
                FirearmFireMode.SemiAutomatic => FirearmFireMode.FullyAutomatic,
                FirearmFireMode.FullyAutomatic or _ => FirearmFireMode.SemiAutomatic
            };
        }

        public void PlayFoleySound(int index)
            => SoundOutputModel.PlayAtIndex(_foleySound, index);

        private void FireRound()
        {
            if (_muzzleTransform != null &&
                PrefabRegistry.Rent(_projectilePrefab, out Projectile? projectile))
            {
                _muzzleTransform.GetPositionAndRotation(
                    out Vector3 muzzlePosition, out Quaternion muzzleRotation);

                if (Owner is IFirearmOperator firearmOperator)
                {
                    muzzleRotation = Deviate(muzzleRotation, firearmOperator.SpreadFactor);
                }

                var context = new ProjectileShootContext(
                    _owner,
                    muzzlePosition,
                    muzzleRotation,
                    _projectileLinearSpeed,
                    _projectileMaxRange);

                projectile.Shoot(context);
                _roundsInMagazine--;
                PlayFireEffects();
                FiredRound?.Invoke(this);
            }
        }

        private void DryFire()
        {
            ReleaseTrigger();
            SoundOutputModel.Play(_dryFireSound);
        }

        private void PlayFireEffects()
        {
            if (_ejectorTransform != null && PrefabRegistry.Rent(_casingPrefab, out Casing? casing))
            {
                _ejectorTransform.GetPositionAndRotation(
                    out Vector3 ejectorPosition, out Quaternion ejectorRotation);

                casing.Eject(ejectorPosition, ejectorRotation);
            }
            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }
            if (_muzzleLight != null)
            {
                _ = ToggleMuzzleLightAsync();
            }
            if (_playerCamera != null)
            {
                _playerCamera.StartShake(_fireCameraShake);
            }
            if (_fireMode == FirearmFireMode.SemiAutomatic)
            {
                SoundOutputModel.Play(_singleFireSound);
            }
            else if (_fireMode == FirearmFireMode.FullyAutomatic && !_isAutoSoundStarted)
            {
                _isAutoSoundStarted = true;
                _loopSource!.loop = true;
                _loopSource!.Play();
            }
        }

        private async Awaitable ToggleMuzzleLightAsync()
        {
            _muzzleLight!.enabled = true;
            await Awaitable.WaitForSecondsAsync(_muzzleLightDuration);
            _muzzleLight!.enabled = false;
        }

        private void Awake()
        {
            TimeDilationSystem.TimeScaleChanged += OnTimeScaleChanged;
        }

        private void OnDestroy()
        {
            TimeDilationSystem.TimeScaleChanged -= OnTimeScaleChanged;
        }

        protected void Update()
        {
            if (_isTriggerPulled && _fireMode is FirearmFireMode.FullyAutomatic)
            {
                TryFireRound();
            }
        }

        private void TryFireRound()
        {
            if ((Time.time - _fireTimestamp) >= (float)FireInterval)
            {
                _fireTimestamp = Time.time;
                if (!IsMagazineEmpty)
                {
                    FireRound();
                }
                else
                {
                    DryFire();
                }
            }
        }

        [SerializeField]
        private float _pitchInfluence;

        private void OnTimeScaleChanged(float value)
        {
            float pitch = Math.Lerp(1f, value, _pitchInfluence);
            _loopSource!.pitch = pitch;
            //_tailSource!.pitch = pitch;
            _loopLowPassFilter!.enabled = value != 1f;
            _loopLowPassFilter!.cutoffFrequency = Math.Lerp(22000f, 8000f, 1f - value);
        }

        private static Quaternion Deviate(Quaternion rotation, float deviation)
        {
            if (deviation > 0f)
            {
                Vector2 angle = Random.Shared.NextVector2(new Vector2(deviation, deviation));
                return rotation * Math.FromEuler(angle);
            }
            return rotation;
        }
    }
}