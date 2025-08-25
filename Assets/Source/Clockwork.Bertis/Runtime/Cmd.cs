using System;
using UnityEngine;
using Clockwork.Bertis.Gameplay;
using Clockwork.Collections;

namespace Clockwork.Bertis.Internal
{
    internal sealed class Cmd : MonoBehaviour
    {
        [SerializeField]
        private Entity[] _entities;

        [SerializeField]
        private PlayerPuppet _playerPuppet;

        private bool _attackToggle = true;

        private bool _followToggle = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                foreach (Entity entity in _entities)
                {
                    entity.Revive();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (Entity entity in _entities)
                {
                    if (entity is MeleePuppet meleePuppet && entity.gameObject.activeInHierarchy)
                    {
                        if (_attackToggle)
                        {
                            meleePuppet.BeginChase(_playerPuppet.transform);
                        }
                        else
                        {
                            meleePuppet.StopChase();
                        }
                    }
                }
                _attackToggle = !_attackToggle;
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                foreach (Entity entity in _entities)
                {
                    if (entity is EscortPuppet escortPuppet && entity.gameObject.activeInHierarchy)
                    {
                        if (_followToggle)
                        {
                            escortPuppet.BeginFollow(_playerPuppet.transform);
                        }
                        else
                        {
                            escortPuppet.StopFollow();
                        }
                    }
                }
                _followToggle = !_followToggle;
            }
        }
    }
}