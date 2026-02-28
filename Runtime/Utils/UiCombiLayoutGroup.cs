using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// FlexLayoutGroup: a LayoutGroup with simple flexbox-like behavior:
    /// - Row or Column direction
    /// - Optional wrapping into multiple lines (rows/columns)
    /// - Uses LayoutElement (via LayoutUtility) for min/preferred/flexible sizing
    /// - Uses built-in childAlignment for alignment (no custom "align-items" system)
    ///
    /// ContentSizeFitter notes:
    /// - With wrapping enabled, the "wrapped axis" preferred size depends on the current size of the main axis.
    ///   Example: Row+Wrap => preferred height depends on current width.
    ///   This is typical for wrap layouts in Unity; it's why you usually constrain width and fit height.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("GPUI/UiCombiLayoutGroup")]
    public sealed class UiCombiLayoutGroup : LayoutGroup
    {
        public enum FlexDirection
        {
            Row = 0,    // main axis = X
            Column = 1, // main axis = Y
        }

        [SerializeField] private FlexDirection _direction = FlexDirection.Row;
        [SerializeField] private bool _wrap = true;

        [Header("Spacing")]
        [SerializeField] private float _spacing = 0f;      // between items in the same line (main axis)
        [SerializeField] private float _lineSpacing = 0f;  // between lines (cross axis)

        [Header("Child Size Controls (like Unity's built-in LayoutGroups)")]
        [SerializeField] private bool _controlChildWidth = true;
        [SerializeField] private bool _controlChildHeight = true;
        [SerializeField] private bool _forceExpandWidth = false;
        [SerializeField] private bool _forceExpandHeight = false;

        // Cached layout input (for ContentSizeFitter / parent layouts)
        private readonly float[] _totalMin = new float[2];
        private readonly float[] _totalPreferred = new float[2];
        private readonly float[] _totalFlexible = new float[2];

        private sealed class Line
        {
            public readonly List<RectTransform> Children = new List<RectTransform>(8);
        }

        private readonly List<Line> _lines = new List<Line>(16);
        private readonly List<float> _sizeBuffer = new List<float>(32);

        public FlexDirection Direction
        {
            get => _direction;
            set { if (_direction != value) { _direction = value; SetDirty(); } }
        }

        public bool Wrap
        {
            get => _wrap;
            set { if (_wrap != value) { _wrap = value; SetDirty(); } }
        }

        public float Spacing
        {
            get => _spacing;
            set { if (!Mathf.Approximately(_spacing, value)) { _spacing = value; SetDirty(); } }
        }

        public float LineSpacing
        {
            get => _lineSpacing;
            set { if (!Mathf.Approximately(_lineSpacing, value)) { _lineSpacing = value; SetDirty(); } }
        }

        public bool ControlChildWidth
        {
            get => _controlChildWidth;
            set { if (_controlChildWidth != value) { _controlChildWidth = value; SetDirty(); } }
        }

        public bool ControlChildHeight
        {
            get => _controlChildHeight;
            set { if (_controlChildHeight != value) { _controlChildHeight = value; SetDirty(); } }
        }

        public bool ForceExpandWidth
        {
            get => _forceExpandWidth;
            set { if (_forceExpandWidth != value) { _forceExpandWidth = value; SetDirty(); } }
        }

        public bool ForceExpandHeight
        {
            get => _forceExpandHeight;
            set { if (_forceExpandHeight != value) { _forceExpandHeight = value; SetDirty(); } }
        }

        private int MainAxis => _direction == FlexDirection.Row ? 0 : 1;
        private int CrossAxis => 1 - MainAxis;

        private bool ControlSizeOnAxis(int axis) => axis == 0 ? _controlChildWidth : _controlChildHeight;
        private bool ForceExpandOnAxis(int axis) => axis == 0 ? _forceExpandWidth : _forceExpandHeight;

        private float InnerSize(int axis)
        {
            float size = rectTransform.rect.size[axis];
            float pad = axis == 0 ? padding.horizontal : padding.vertical;
            return Mathf.Max(0f, size - pad);
        }

        private static float RectSize(RectTransform rt, int axis) => rt.rect.size[axis];

        private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
            out float min, out float preferred, out float flexible)
        {
            if (controlSize)
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }
            else
            {
                float s = RectSize(child, axis);
                min = s;
                preferred = s;
                flexible = 0f;
            }

            if (childForceExpand)
                flexible = Mathf.Max(flexible, 1f);

            // Ensure preferred is at least min (LayoutUtility should already satisfy this, but be defensive)
            preferred = Mathf.Max(preferred, min);
        }

        /// <summary>
        /// Build wrap lines (rows or columns) using each child's preferred size on the main axis as the "basis" for wrapping.
        /// This mirrors the common "flex-basis determines wrapping" approach.
        /// </summary>
        private void BuildLines(float availableMain)
        {
            _lines.Clear();

            // If wrapping is off (or no space known), treat as one line.
            bool doWrap = _wrap && availableMain > 0f;

            Line current = new Line();
            _lines.Add(current);

            int mainAxis = MainAxis;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];

                float basisMain;
                if (ControlSizeOnAxis(mainAxis))
                    basisMain = LayoutUtility.GetPreferredSize(child, mainAxis);
                else
                    basisMain = RectSize(child, mainAxis);

                // Start a new line if wrapping and the item doesn't fit
                if (doWrap && current.Children.Count > 0)
                {
                    float currentUsed = 0f;
                    for (int k = 0; k < current.Children.Count; k++)
                    {
                        RectTransform c = current.Children[k];
                        float w = ControlSizeOnAxis(mainAxis) ? LayoutUtility.GetPreferredSize(c, mainAxis) : RectSize(c, mainAxis);
                        currentUsed += w;
                        if (k > 0) currentUsed += _spacing;
                    }

                    float candidate = currentUsed + _spacing + basisMain;
                    if (candidate > availableMain)
                    {
                        current = new Line();
                        _lines.Add(current);
                    }
                }

                current.Children.Add(child);
            }
        }

        private void CalculateNonWrappedSizes(int mainAxis, int crossAxis)
        {
            float mainMin = 0f, mainPreferred = 0f, mainFlexible = 0f;
            float crossMin = 0f, crossPreferred = 0f, crossFlexible = 0f;

            int count = rectChildren.Count;
            for (int i = 0; i < count; i++)
            {
                RectTransform child = rectChildren[i];

                GetChildSizes(child, mainAxis, ControlSizeOnAxis(mainAxis), ForceExpandOnAxis(mainAxis),
                    out float cMinMain, out float cPrefMain, out float cFlexMain);

                GetChildSizes(child, crossAxis, ControlSizeOnAxis(crossAxis), ForceExpandOnAxis(crossAxis),
                    out float cMinCross, out float cPrefCross, out float cFlexCross);

                mainMin += cMinMain;
                mainPreferred += cPrefMain;
                mainFlexible += cFlexMain;

                crossMin = Mathf.Max(crossMin, cMinCross);
                crossPreferred = Mathf.Max(crossPreferred, cPrefCross);
                crossFlexible = Mathf.Max(crossFlexible, cFlexCross);

                if (i > 0)
                {
                    mainMin += _spacing;
                    mainPreferred += _spacing;
                }
            }

            _totalMin[mainAxis] = mainMin + (mainAxis == 0 ? padding.horizontal : padding.vertical);
            _totalPreferred[mainAxis] = mainPreferred + (mainAxis == 0 ? padding.horizontal : padding.vertical);
            _totalFlexible[mainAxis] = mainFlexible;

            _totalMin[crossAxis] = crossMin + (crossAxis == 0 ? padding.horizontal : padding.vertical);
            _totalPreferred[crossAxis] = crossPreferred + (crossAxis == 0 ? padding.horizontal : padding.vertical);
            _totalFlexible[crossAxis] = crossFlexible;
        }

        private void CalculateWrappedSizes(int mainAxis, int crossAxis, float availableMain)
        {
            // Build lines based on preferred size basis
            BuildLines(availableMain);

            // We calculate:
            // - Main axis size = max line main size
            // - Cross axis size = sum of line cross sizes + lineSpacing
            float mainMin = 0f, mainPreferred = 0f;
            float crossMin = 0f, crossPreferred = 0f;

            // MIN pass (use min sizes for per-line sizes)
            {
                float maxLineMain = 0f;
                float sumLineCross = 0f;

                for (int li = 0; li < _lines.Count; li++)
                {
                    Line line = _lines[li];

                    float lineMain = 0f;
                    float lineCross = 0f;

                    for (int i = 0; i < line.Children.Count; i++)
                    {
                        RectTransform child = line.Children[i];

                        GetChildSizes(child, mainAxis, ControlSizeOnAxis(mainAxis), ForceExpandOnAxis(mainAxis),
                            out float cMinMain, out _, out _);

                        GetChildSizes(child, crossAxis, ControlSizeOnAxis(crossAxis), ForceExpandOnAxis(crossAxis),
                            out float cMinCross, out _, out _);

                        lineMain += cMinMain;
                        if (i > 0) lineMain += _spacing;

                        lineCross = Mathf.Max(lineCross, cMinCross);
                    }

                    maxLineMain = Mathf.Max(maxLineMain, lineMain);
                    sumLineCross += lineCross;
                    if (li > 0) sumLineCross += _lineSpacing;
                }

                mainMin = maxLineMain;
                crossMin = sumLineCross;
            }

            // PREFERRED pass (use preferred sizes for per-line sizes)
            {
                float maxLineMain = 0f;
                float sumLineCross = 0f;

                for (int li = 0; li < _lines.Count; li++)
                {
                    Line line = _lines[li];

                    float lineMain = 0f;
                    float lineCross = 0f;

                    for (int i = 0; i < line.Children.Count; i++)
                    {
                        RectTransform child = line.Children[i];

                        GetChildSizes(child, mainAxis, ControlSizeOnAxis(mainAxis), ForceExpandOnAxis(mainAxis),
                            out _, out float cPrefMain, out _);

                        GetChildSizes(child, crossAxis, ControlSizeOnAxis(crossAxis), ForceExpandOnAxis(crossAxis),
                            out _, out float cPrefCross, out _);

                        lineMain += cPrefMain;
                        if (i > 0) lineMain += _spacing;

                        lineCross = Mathf.Max(lineCross, cPrefCross);
                    }

                    maxLineMain = Mathf.Max(maxLineMain, lineMain);
                    sumLineCross += lineCross;
                    if (li > 0) sumLineCross += _lineSpacing;
                }

                mainPreferred = maxLineMain;
                crossPreferred = sumLineCross;
            }

            _totalMin[mainAxis] = mainMin + (mainAxis == 0 ? padding.horizontal : padding.vertical);
            _totalPreferred[mainAxis] = mainPreferred + (mainAxis == 0 ? padding.horizontal : padding.vertical);
            _totalFlexible[mainAxis] = 0f; // wrapping makes flexible container size ambiguous; keep 0

            _totalMin[crossAxis] = crossMin + (crossAxis == 0 ? padding.horizontal : padding.vertical);
            _totalPreferred[crossAxis] = crossPreferred + (crossAxis == 0 ? padding.horizontal : padding.vertical);
            _totalFlexible[crossAxis] = 0f;
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            int mainAxis = MainAxis;
            int crossAxis = CrossAxis;

            // Default to non-wrapped calculations
            if (!_wrap)
            {
                CalculateNonWrappedSizes(mainAxis, crossAxis);
            }
            else
            {
                float availableMain = InnerSize(mainAxis);
                // If available is 0 (e.g., during some edit-time passes), treat as non-wrapped for stability.
                if (availableMain <= 0f)
                    CalculateNonWrappedSizes(mainAxis, crossAxis);
                else
                    CalculateWrappedSizes(mainAxis, crossAxis, availableMain);
            }

            // Set horizontal axis (0) now; vertical axis (1) will be set in CalculateLayoutInputVertical
            SetLayoutInputForAxis(_totalMin[0], _totalPreferred[0], _totalFlexible[0], 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            SetLayoutInputForAxis(_totalMin[1], _totalPreferred[1], _totalFlexible[1], 1);
        }

        public override void SetLayoutHorizontal()
        {
            if (_direction == FlexDirection.Row)
            {
                // Row: wrapping depends on width, so we can place X here.
                LayoutRow_SetX();
            }
            else
            {
                // Column: widths matter for many child types (e.g., text), so set child widths here.
                LayoutColumn_SetWidthsOnly();
            }
        }

        public override void SetLayoutVertical()
        {
            if (_direction == FlexDirection.Row)
            {
                LayoutRow_SetY();
            }
            else
            {
                // Column: wrapping depends on height; finalize both axes here after widths are set.
                LayoutColumn_Final();
            }
        }

        private void LayoutRow_SetX()
        {
            int mainAxis = 0;
            float availableMain = InnerSize(mainAxis);

            BuildLines(availableMain);

            for (int li = 0; li < _lines.Count; li++)
            {
                Line line = _lines[li];
                int n = line.Children.Count;
                if (n == 0) continue;

                // Compute sizes along X for this line using Unity-like min->preferred interpolation + flexible expansion.
                _sizeBuffer.Clear();
                EnsureSizeBuffer(n);

                float totalMin = 0f, totalPref = 0f, totalFlex = 0f;
                for (int i = 0; i < n; i++)
                {
                    RectTransform child = line.Children[i];
                    GetChildSizes(child, mainAxis, _controlChildWidth, _forceExpandWidth,
                        out float cMin, out float cPref, out float cFlex);

                    totalMin += cMin;
                    totalPref += cPref;
                    totalFlex += cFlex;
                }

                float spacingTotal = _spacing * (n - 1);
                float minLine = totalMin + spacingTotal;
                float prefLine = totalPref + spacingTotal;

                // Determine child sizes
                float used = 0f;
                if (availableMain > prefLine && totalFlex > 0f)
                {
                    float extra = availableMain - prefLine;
                    for (int i = 0; i < n; i++)
                    {
                        RectTransform child = line.Children[i];
                        GetChildSizes(child, mainAxis, _controlChildWidth, _forceExpandWidth,
                            out _, out float cPref, out float cFlex);

                        float size = cPref + extra * (cFlex / totalFlex);
                        _sizeBuffer[i] = size;
                        used += size;
                    }
                }
                else
                {
                    float t = 0f;
                    if (prefLine > minLine)
                        t = Mathf.Clamp01((availableMain - minLine) / (prefLine - minLine));

                    for (int i = 0; i < n; i++)
                    {
                        RectTransform child = line.Children[i];
                        GetChildSizes(child, mainAxis, _controlChildWidth, _forceExpandWidth,
                            out float cMin, out float cPref, out _);

                        float size = Mathf.Lerp(cMin, cPref, t);
                        _sizeBuffer[i] = size;
                        used += size;
                    }
                }

                float lineUsed = used + spacingTotal;
                float start = GetStartOffset(mainAxis, lineUsed);

                float pos = start;
                for (int i = 0; i < n; i++)
                {
                    RectTransform child = line.Children[i];
                    float size = _sizeBuffer[i];
                    SetChildAlongAxis(child, mainAxis, pos, size);
                    pos += size + _spacing;
                }
            }
        }

        private void LayoutRow_SetY()
        {
            int crossAxis = 1;
            float availableMain = InnerSize(0); // wrap determined by width
            BuildLines(availableMain);

            // Compute each line's height (max child height)
            _sizeBuffer.Clear();
            EnsureSizeBuffer(_lines.Count);

            float totalHeight = 0f;
            for (int li = 0; li < _lines.Count; li++)
            {
                Line line = _lines[li];
                float lineHeight = 0f;

                for (int i = 0; i < line.Children.Count; i++)
                {
                    RectTransform child = line.Children[i];
                    GetChildSizes(child, crossAxis, _controlChildHeight, _forceExpandHeight,
                        out _, out float cPref, out _);

                    lineHeight = Mathf.Max(lineHeight, cPref);
                }

                _sizeBuffer[li] = lineHeight;
                totalHeight += lineHeight;
                if (li > 0) totalHeight += _lineSpacing;
            }

            float startY = GetStartOffset(crossAxis, totalHeight);
            float alignInLine = GetAlignmentOnAxis(crossAxis);

            float y = startY;
            for (int li = 0; li < _lines.Count; li++)
            {
                Line line = _lines[li];
                float lineHeight = _sizeBuffer[li];

                for (int i = 0; i < line.Children.Count; i++)
                {
                    RectTransform child = line.Children[i];

                    GetChildSizes(child, crossAxis, _controlChildHeight, _forceExpandHeight,
                        out _, out float cPref, out _);

                    float childHeight = _controlChildHeight ? cPref : RectSize(child, crossAxis);

                    // Optional "stretch within line" using ForceExpandHeight
                    if (_controlChildHeight && _forceExpandHeight)
                        childHeight = lineHeight;

                    float offset = (lineHeight - childHeight) * alignInLine;
                    SetChildAlongAxis(child, crossAxis, y + offset, childHeight);
                }

                y += lineHeight + _lineSpacing;
            }
        }

        private void LayoutColumn_SetWidthsOnly()
        {
            // For Column direction, we set widths first so children (e.g., Text) can resolve preferred heights properly.
            int axis = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];

                GetChildSizes(child, axis, _controlChildWidth, _forceExpandWidth,
                    out float cMin, out float cPref, out _);

                float w = _controlChildWidth ? cPref : RectSize(child, axis);
                w = Mathf.Max(w, cMin);

                // Temporary position; will be corrected in LayoutColumn_Final()
                SetChildAlongAxis(child, axis, padding.left, w);
            }
        }

        private void LayoutColumn_Final()
        {
            int mainAxis = 1;
            int crossAxis = 0;

            float availableMain = InnerSize(mainAxis);
            BuildLines(availableMain); // lines = columns in Column direction

            // First compute each column width (max child width) and total width
            _sizeBuffer.Clear();
            EnsureSizeBuffer(_lines.Count);

            float totalWidth = 0f;
            for (int li = 0; li < _lines.Count; li++)
            {
                Line col = _lines[li];
                float colWidth = 0f;

                for (int i = 0; i < col.Children.Count; i++)
                {
                    RectTransform child = col.Children[i];
                    GetChildSizes(child, crossAxis, _controlChildWidth, _forceExpandWidth,
                        out _, out float cPref, out _);

                    float childW = _controlChildWidth ? cPref : RectSize(child, crossAxis);
                    colWidth = Mathf.Max(colWidth, childW);
                }

                _sizeBuffer[li] = colWidth;
                totalWidth += colWidth;
                if (li > 0) totalWidth += _lineSpacing;
            }

            float startX = GetStartOffset(crossAxis, totalWidth);
            float alignInColumn = GetAlignmentOnAxis(crossAxis);

            float x = startX;
            for (int li = 0; li < _lines.Count; li++)
            {
                Line col = _lines[li];
                float colWidth = _sizeBuffer[li];

                int n = col.Children.Count;
                if (n == 0)
                {
                    x += colWidth + _lineSpacing;
                    continue;
                }

                // Compute sizes along Y for this column (main axis) similarly to Unity's vertical layout distribution.
                List<float> heights = _sizeBuffer; // reuse buffer, but we need a separate segment; use a local temp list instead
                // We'll use _sizeBuffer2 pattern via a local list to keep code simple:
                List<float> heightBuffer = new List<float>(n);

                float totalMin = 0f, totalPref = 0f, totalFlex = 0f;
                for (int i = 0; i < n; i++)
                {
                    RectTransform child = col.Children[i];
                    GetChildSizes(child, mainAxis, _controlChildHeight, _forceExpandHeight,
                        out float cMin, out float cPref, out float cFlex);

                    totalMin += cMin;
                    totalPref += cPref;
                    totalFlex += cFlex;
                }

                float spacingTotal = _spacing * (n - 1);
                float minCol = totalMin + spacingTotal;
                float prefCol = totalPref + spacingTotal;

                // Determine child heights
                if (availableMain > prefCol && totalFlex > 0f)
                {
                    float extra = availableMain - prefCol;
                    for (int i = 0; i < n; i++)
                    {
                        RectTransform child = col.Children[i];
                        GetChildSizes(child, mainAxis, _controlChildHeight, _forceExpandHeight,
                            out _, out float cPref, out float cFlex);

                        float h = cPref + extra * (cFlex / totalFlex);
                        heightBuffer.Add(h);
                    }
                }
                else
                {
                    float t = 0f;
                    if (prefCol > minCol)
                        t = Mathf.Clamp01((availableMain - minCol) / (prefCol - minCol));

                    for (int i = 0; i < n; i++)
                    {
                        RectTransform child = col.Children[i];
                        GetChildSizes(child, mainAxis, _controlChildHeight, _forceExpandHeight,
                            out float cMin, out float cPref, out _);

                        float h = Mathf.Lerp(cMin, cPref, t);
                        heightBuffer.Add(h);
                    }
                }

                float usedHeights = 0f;
                for (int i = 0; i < heightBuffer.Count; i++) usedHeights += heightBuffer[i];
                float colUsed = usedHeights + spacingTotal;

                float startY = GetStartOffset(mainAxis, colUsed);
                float y = startY;

                for (int i = 0; i < n; i++)
                {
                    RectTransform child = col.Children[i];
                    float childH = heightBuffer[i];

                    // Width: within column, align using childAlignment (left/center/right).
                    GetChildSizes(child, crossAxis, _controlChildWidth, _forceExpandWidth,
                        out _, out float cPrefW, out _);

                    float childW = _controlChildWidth ? cPrefW : RectSize(child, crossAxis);

                    // Optional "stretch within column" using ForceExpandWidth
                    if (_controlChildWidth && _forceExpandWidth)
                        childW = colWidth;

                    float offsetX = (colWidth - childW) * alignInColumn;

                    SetChildAlongAxis(child, crossAxis, x + offsetX, childW);
                    SetChildAlongAxis(child, mainAxis, y, childH);

                    y += childH + _spacing;
                }

                x += colWidth + _lineSpacing;
            }
        }

        private void EnsureSizeBuffer(int needed)
        {
            while (_sizeBuffer.Count < needed)
                _sizeBuffer.Add(0f);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            // Wrapping depends on available space; ensure we rebuild when dimensions change.
            SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif
    }
}