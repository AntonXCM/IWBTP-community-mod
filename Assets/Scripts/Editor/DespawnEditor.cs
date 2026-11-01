using UnityEditor;
using UnityEngine;

/// <summary>
/// Рисует правило в одну строку, как предложение:
/// [X] [Object] [Greater] [Camera] ±[1]   или   [Y] [Camera] [Less] [Constant] [3] ±[0]
/// Положите файл в любую папку с именем Editor.
/// </summary>
[CustomPropertyDrawer(typeof(ScreenRule))]
public class ScreenRuleDrawer : PropertyDrawer
{
    private const float Gap = 3f;
    private const float ToleranceWidth = 50f;
    private const float ConstantWidth = 42f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty axis = property.FindPropertyRelative("axis");
        SerializedProperty left = property.FindPropertyRelative("left");
        SerializedProperty comparison = property.FindPropertyRelative("comparison");
        SerializedProperty right = property.FindPropertyRelative("right");
        SerializedProperty constant = property.FindPropertyRelative("constant");
        SerializedProperty tolerance = property.FindPropertyRelative("tolerance");

        bool isConstant = right.enumValueIndex == (int)ScreenOperand.Constant;

        int oldIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Фиксированная ширина у погрешности и (если нужна) константы, остальное делим по весам.
        int gaps = isConstant ? 5 : 4;
        float fixedWidth = ToleranceWidth + (isConstant ? ConstantWidth : 0f) + Gap * gaps;
        float flexible = Mathf.Max(0f, position.width - fixedWidth);
        const float totalWeight = 1f + 2f + 2.4f + 2f; // axis, left, comparison, right

        float x = position.x;

        EditorGUI.PropertyField(Take(ref x, position, flexible * 1f / totalWeight), axis, GUIContent.none);
        EditorGUI.PropertyField(Take(ref x, position, flexible * 2f / totalWeight), left, GUIContent.none);
        EditorGUI.PropertyField(Take(ref x, position, flexible * 2.4f / totalWeight), comparison, GUIContent.none);
        EditorGUI.PropertyField(Take(ref x, position, flexible * 2f / totalWeight), right, GUIContent.none);

        if (isConstant)
            EditorGUI.PropertyField(Take(ref x, position, ConstantWidth), constant, GUIContent.none);

        float previousLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 14f;
        EditorGUI.PropertyField(
            Take(ref x, position, ToleranceWidth),
            tolerance,
            new GUIContent("±", "Допустимая погрешность n"));
        EditorGUIUtility.labelWidth = previousLabelWidth;

        EditorGUI.indentLevel = oldIndent;
        EditorGUI.EndProperty();
    }

    private static Rect Take(ref float x, Rect position, float width)
    {
        Rect rect = new Rect(x, position.y, width, position.height);
        x += width + Gap;
        return rect;
    }
}