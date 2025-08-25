using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UImGui;
using Clockwork.Bertis.Components;

namespace Clockwork.Scripting
{
    public sealed class Console : MonoBehaviour
    {
        [SerializeField]
        private Behaviour[] _behaviours;

        [SerializeField]
        private CameraShakeComponent _cameraShakeComponent;

        [SerializeField]
        private InputActionReference _toggleConsoleActionReference;

        private LiveConsole _liveConsole;

        [SerializeField]
        private CommandDefinitions _commandDefinitions;

        [NonSerialized]
        private bool _isVisible;

        [NonSerialized]
        private Logger _logger;

        [NonSerialized]
        private CommandConsole _commandConsole;

        private void Awake()
        {
            if (_toggleConsoleActionReference != null)
            {
                _toggleConsoleActionReference.action.performed += OnToggleConsoleActionPerformed;
            }

            _logger = new Logger();
        }

        private void OnEnable()
        {
            _commandConsole = new CommandConsole();
            _liveConsole = new LiveConsole(_cameraShakeComponent);

            if (_commandDefinitions != null)
            {
                _commandDefinitions.Register(_commandConsole);
            }

            UImGuiUtility.Layout -= OnLayout;
            UImGuiUtility.Layout += OnLayout;
            Debug.Logged -= OnLogged;
            Debug.Logged += OnLogged;

            _logger.Log(LogLevel.Information, "Console initialized.");
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
            Debug.Logged -= OnLogged;
        }

        private void OnDestroy()
        {
            if (_toggleConsoleActionReference != null)
            {
                _toggleConsoleActionReference.action.performed -= OnToggleConsoleActionPerformed;
            }
        }

        private void OnLayout(UImGui.UImGui obj)
        {
            _logger.Update();
            _liveConsole.Update();
            _commandConsole.Update();
        }

        private void OnLogged(Clockwork.LogLevel logLevel, object message, object context)
            => _logger?.Log((LogLevel)logLevel, message, context);

        private void OnToggleConsoleActionPerformed(InputAction.CallbackContext context)
        {
            _isVisible = !_isVisible;

            if (Keyboard.current.leftShiftKey.isPressed)
            {
                gameObject.SetActive(_isVisible);
            }

            if (_isVisible)
            {
                Time.timeScale = 0f;

                _commandConsole.ToggleFocus(true);
            }
            else
            {
                if (Time.timeScale == 0f)
                {
                    Time.timeScale = 1f;
                }
            }

            ToggleBehaviours(!_isVisible);
        }

        private void ToggleBehaviours(bool enable)
        {
            foreach (Behaviour behaviour in _behaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = enable;
                }
            }
        }
    }
}