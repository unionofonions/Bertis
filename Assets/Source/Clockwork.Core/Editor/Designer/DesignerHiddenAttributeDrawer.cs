using UnityEditor;
using UnityEngine.UIElements;

namespace Clockwork.Designer.Editor
{
    [CustomPropertyDrawer(typeof(DesignerHiddenAttribute))]
    public sealed class DesignerHiddenAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) => new();
    }
}