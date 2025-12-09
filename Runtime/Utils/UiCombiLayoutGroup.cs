using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A layout group that can behave like a Horizontal or VerticalLayoutGroup
/// using Unity's built-in HorizontalOrVerticalLayoutGroup logic.
/// 
/// Supports:
/// - Orientation (Horizontal / Vertical)
/// - Spacing
/// - Padding
/// - Child control width / height
/// - Child force expand width / height
/// - Child scale width / height
/// - Reverse arrangement
/// - Child alignment
/// </summary>
[AddComponentMenu("Layout/Combi Layout Group")]
public class UiCombiLayoutGroup : HorizontalOrVerticalLayoutGroup
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    [SerializeField]
    private Orientation m_Orientation = Orientation.Horizontal;

    public Orientation orientation
    {
        get => m_Orientation;
        set
        {
            if (m_Orientation == value) return;
            m_Orientation = value;
            SetDirty();
        }
    }

    private bool IsVertical => m_Orientation == Orientation.Vertical;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        // axis 0 = horizontal; isVertical tells Unity which mode to use
        CalcAlongAxis(0, IsVertical);
    }

    public override void CalculateLayoutInputVertical()
    {
        // axis 1 = vertical; isVertical tells Unity which mode to use
        CalcAlongAxis(1, IsVertical);
    }

    public override void SetLayoutHorizontal()
    {
        SetChildrenAlongAxis(0, IsVertical);
    }

    public override void SetLayoutVertical()
    {
        SetChildrenAlongAxis(1, IsVertical);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty();
    }
#endif
}