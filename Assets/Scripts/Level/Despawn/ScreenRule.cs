using System;
using UnityEngine;

/// <summary>Ось, по которой сравниваются координаты экранов.</summary>
public enum ScreenAxis { X, Y }

/// <summary>Левая часть правила: чья координата сравнивается.</summary>
public enum ScreenSubject { Object, Camera }

/// <summary>Правая часть правила: с чем сравнивается.</summary>
public enum ScreenOperand { Object, Camera, Constant }

/// <summary>
/// Операция сравнения. Пусть d = левое − правое, n = tolerance (допустимая погрешность):
/// <list type="bullet">
/// <item>Greater        — d &gt; n</item>
/// <item>Less           — d &lt; −n</item>
/// <item>Equal          — |d| ≤ n</item>
/// <item>GreaterOrEqual — d ≥ −n</item>
/// <item>LessOrEqual    — d ≤ n</item>
/// <item>NotEqual       — |d| &gt; n</item>
/// </list>
/// При n = 0 это обычные &gt;, &lt;, ==, &gt;=, &lt;=, !=.
/// </summary>
public enum ScreenComparison { Greater, Less, Equal, GreaterOrEqual, LessOrEqual, NotEqual }

/// <summary>
/// Одно правило: [X|Y] [Объекта|Камеры] [Больше|Меньше|Равно|…] [Объекта|Камеры|Константы] ± погрешность.
/// Пример: «X объекта Больше X камеры ± 1» → objectScreen.x − cameraScreen.x &gt; 1.
/// </summary>
[Serializable]
public struct ScreenRule
{
    public ScreenAxis axis;
    public ScreenSubject left;
    public ScreenComparison comparison;
    public ScreenOperand right;

    [Tooltip("Используется, только если правая часть — Constant.")]
    public int constant;

    [Min(0)]
    [Tooltip("Допустимая погрешность n (см. описание ScreenComparison).")]
    public int tolerance;

    private int Component(Vector2Int screen) => axis == ScreenAxis.X ? screen.x : screen.y;

    /// <summary>Выполняется ли правило для данной пары экранов.</summary>
    public bool Evaluate(Vector2Int objectScreen, Vector2Int cameraScreen)
    {
        int a = Component(left == ScreenSubject.Object ? objectScreen : cameraScreen);

        int b;
        switch (right)
        {
            case ScreenOperand.Object: b = Component(objectScreen); break;
            case ScreenOperand.Camera: b = Component(cameraScreen); break;
            default:                   b = constant;                break;
        }

        int d = a - b;
        int n = Mathf.Max(0, tolerance);

        switch (comparison)
        {
            case ScreenComparison.Greater:        return d > n;
            case ScreenComparison.Less:           return d < -n;
            case ScreenComparison.Equal:          return Mathf.Abs(d) <= n;
            case ScreenComparison.GreaterOrEqual: return d >= -n;
            case ScreenComparison.LessOrEqual:    return d <= n;
            case ScreenComparison.NotEqual:       return Mathf.Abs(d) > n;
            default:                              return false;
        }
    }

    /// <summary>Правило «координата объекта ? координата камеры ± n».</summary>
    public static ScreenRule ObjectVsCamera(ScreenAxis axis, ScreenComparison comparison, int tolerance = 0)
    {
        return new ScreenRule
        {
            axis = axis,
            left = ScreenSubject.Object,
            comparison = comparison,
            right = ScreenOperand.Camera,
            tolerance = tolerance
        };
    }

    /// <summary>Правило «координата объекта/камеры ? константа ± n».</summary>
    public static ScreenRule VsConstant(ScreenAxis axis, ScreenSubject left, ScreenComparison comparison,
                                        int constant, int tolerance = 0)
    {
        return new ScreenRule
        {
            axis = axis,
            left = left,
            comparison = comparison,
            right = ScreenOperand.Constant,
            constant = constant,
            tolerance = tolerance
        };
    }

    private static string Symbol(ScreenComparison c)
    {
        switch (c)
        {
            case ScreenComparison.Greater:        return ">";
            case ScreenComparison.Less:           return "<";
            case ScreenComparison.Equal:          return "==";
            case ScreenComparison.GreaterOrEqual: return ">=";
            case ScreenComparison.LessOrEqual:    return "<=";
            default:                              return "!=";
        }
    }

    public override string ToString()
    {
        string rhs = right == ScreenOperand.Constant ? constant.ToString() : axis + "(" + right + ")";
        string tol = tolerance > 0 ? " ±" + tolerance : "";
        return axis + "(" + left + ") " + Symbol(comparison) + " " + rhs + tol;
    }
}

/// <summary>
/// Базовые пресеты. Во всех пресетах правила объединяются по «ИЛИ»:
/// достаточно выполнения любого правила, чтобы объект был деспавнен.
/// «Range» — сколько экранов вокруг камеры остаются загруженными
/// (Range = 1 → камера + 1 экран в каждую сторону).
/// </summary>
public enum ScreenDespawnPreset
{
    /// <summary>Использовать собственные правила из инспектора.</summary>
    Custom,

    /// <summary>Жив только на экране камеры (Range игнорируется).</summary>
    SameScreenOnly,

    /// <summary>Жив внутри прямоугольника вокруг камеры: |Δx| ≤ Range.x и |Δy| ≤ Range.y.</summary>
    OutsideBox,

    /// <summary>Учитывается только горизонталь: |Δx| ≤ Range.x.</summary>
    OutsideHorizontal,

    /// <summary>Учитывается только вертикаль: |Δy| ≤ Range.y.</summary>
    OutsideVertical,

    /// <summary>Деспавн, когда объект ушёл левее камеры больше чем на Range.x (удобно для скролла вправо).</summary>
    LeftOfCamera,

    /// <summary>Деспавн, когда объект ушёл правее камеры больше чем на Range.x.</summary>
    RightOfCamera,

    /// <summary>Деспавн, когда объект выше камеры больше чем на Range.y (считается, что Y растёт вверх).</summary>
    AboveCamera,

    /// <summary>Деспавн, когда объект ниже камеры больше чем на Range.y (упал вниз).</summary>
    BelowCamera
}

public static class ScreenDespawnPresets
{
    /// <summary>Строит набор правил (объединять по «ИЛИ») для пресета.</summary>
    public static ScreenRule[] Build(ScreenDespawnPreset preset, Vector2Int range)
    {
        int rx = Mathf.Max(0, range.x);
        int ry = Mathf.Max(0, range.y);

        switch (preset)
        {
            case ScreenDespawnPreset.SameScreenOnly:
                return new[]
                {
                    ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.NotEqual),
                    ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.NotEqual)
                };

            case ScreenDespawnPreset.OutsideBox:
                return new[]
                {
                    ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.Greater, rx),
                    ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.Less, rx),
                    ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.Greater, ry),
                    ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.Less, ry)
                };

            case ScreenDespawnPreset.OutsideHorizontal:
                return new[]
                {
                    ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.Greater, rx),
                    ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.Less, rx)
                };

            case ScreenDespawnPreset.OutsideVertical:
                return new[]
                {
                    ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.Greater, ry),
                    ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.Less, ry)
                };

            case ScreenDespawnPreset.LeftOfCamera:
                return new[] { ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.Less, rx) };

            case ScreenDespawnPreset.RightOfCamera:
                return new[] { ScreenRule.ObjectVsCamera(ScreenAxis.X, ScreenComparison.Greater, rx) };

            case ScreenDespawnPreset.AboveCamera:
                return new[] { ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.Greater, ry) };

            case ScreenDespawnPreset.BelowCamera:
                return new[] { ScreenRule.ObjectVsCamera(ScreenAxis.Y, ScreenComparison.Less, ry) };

            default: // Custom
                return new ScreenRule[0];
        }
    }
}