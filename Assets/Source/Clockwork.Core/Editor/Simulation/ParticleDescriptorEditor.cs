using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clockwork.Simulation.Editor
{
    [CustomEditor(typeof(ParticleDescriptor), editorForChildClasses: true)]
    public class ParticleDescriptorEditor : UnityEditor.Editor
    {
        private SerializedProperty _emitDurationProperty;
        private SerializedProperty _playDurationProperty;
        private SerializedObject _particleSystemObject;
        private ParticleSystem _particleSystem;

        protected void OnEnable()
        {
            _emitDurationProperty = serializedObject.FindProperty("_emitDuration");
            _playDurationProperty = serializedObject.FindProperty("_playDuration");
            _particleSystem = ((Component)target).GetComponent<ParticleSystem>();
            _particleSystemObject = new SerializedObject(_particleSystem);
        }

        protected void OnDisable()
        {
            _particleSystemObject.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();
            var scriptField = new PropertyField(serializedObject.FindProperty("m_Script"))
            {
                enabledSelf = false
            };

            rootElement.TrackSerializedObjectValue(_particleSystemObject, UpdateProperties);

            rootElement.Add(scriptField);
            return rootElement;
        }

        private void UpdateProperties(SerializedObject _ = null)
        {
            var mainModule = _particleSystem.main;
            _emitDurationProperty.floatValue = mainModule.duration;
            _playDurationProperty.floatValue = mainModule.duration + mainModule.startLifetime.constantMax;
            serializedObject.ApplyModifiedProperties();
        }
    }
}