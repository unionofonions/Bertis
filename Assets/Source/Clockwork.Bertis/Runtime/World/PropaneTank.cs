using System.Threading;
using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Pooling;
using Clockwork.Simulation;

#nullable enable

namespace Clockwork.Bertis.World
{
    public class PropaneTank : Entity
    {
        [SerializeField]
        [PrefabReference]
        private ParticleDescriptor? _pressureGasParticle;

        [SerializeField]
        private SoundDescriptor? _pressureGasSound;

        [SerializeField]
        private ExplosionDefinition _explosionDefinition = null!;

        private CancellationTokenSource? _cts;

        private CancellationToken CreateCancellationToken() => (_cts ??= new()).Token;

        protected override void OnTookNonFatalDamage(in ImpactContext context)
        {
            base.OnTookNonFatalDamage(context);

            if (PrefabRegistry.Rent(_pressureGasParticle, out ParticleDescriptor? pressureGas))
            {
                pressureGas.Play(
                    context.ContactPoint,
                    Quaternion.LookRotation(-context.ContactDirection),
                    CreateCancellationToken());
            }

            SoundOutputModel.Play(_pressureGasSound, context.ContactPoint, CreateCancellationToken());
        }

        protected override void OnTookFatalDamage(in ImpactContext context)
        {
            base.OnTookFatalDamage(context);

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            _explosionDefinition.Explode(this, context.ContactPoint);
        }

        protected void OnDrawGizmos()
        {
            Color temp = Gizmos.color;
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, _explosionDefinition.ImpactRadius);
            Gizmos.color = temp;
        }
    }
}