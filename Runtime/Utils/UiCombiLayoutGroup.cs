using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// LayoutGroup that can arrange children either horizontally or vertically,
    /// with optional wrapping (flow layout style).
    ///
    /// - Orientation.Horizontal: children laid out in rows, wrapping to new rows.
    /// - Orientation.Vertical:   children laid out in columns, wrapping to new columns.
    ///
    /// It uses the standard HorizontalOrVerticalLayoutGroup fields:
    ///  - padding
    ///  - childAlignment
    ///  - spacing (along main axis)
    ///  - childControlWidth / childControlHeight
    ///
    /// New:
    ///  - orientation (Horizontal / Vertical)
    ///  - wrap (on/off)
    ///  - lineSpacing (spacing between rows/columns)
    /// </summary>
    [AddComponentMenu("Layout/Combi Layout Group (Wrap)")]
    public class UiCombiLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        public enum LayoutOrientation
        {
            Horizontal,
            Vertical
        }

        [SerializeField]
        private LayoutOrientation m_Orientation = LayoutOrientation.Horizontal;

        [SerializeField]
        private bool m_Wrap = true;

        [Tooltip("Spacing between rows (when Horizontal) or columns (when Vertical).")]
        [SerializeField]
        private float m_LineSpacing = 0f;

        private struct Row
        {
            public int startIndex;
            public int endIndex; // exclusive
            public float width;
            public float height;
        }

        private struct Column
        {
            public int startIndex;
            public int endIndex; // exclusive
            public float width;
            public float height;
        }

        // Reused lists to avoid GC
        private readonly List<Row> m_Rows = new List<Row>(8);
        private readonly List<Column> m_Columns = new List<Column>(8);

        #region Public API

        public LayoutOrientation Orientation
        {
            get => m_Orientation;
            set
            {
                if (m_Orientation == value)
                    return;

                m_Orientation = value;
                SetDirty();
            }
        }

        /// <summary>
        /// If true, children wrap onto new lines/columns when they exceed available space.
        /// If false, they behave like a normal Horizontal/VerticalLayoutGroup (single row/column).
        /// </summary>
        public bool Wrap
        {
            get => m_Wrap;
            set
            {
                if (m_Wrap == value)
                    return;

                m_Wrap = value;
                SetDirty();
            }
        }

        /// <summary>
        /// Spacing between rows (horizontal mode) or columns (vertical mode).
        /// </summary>
        public float LineSpacing
        {
            get => m_LineSpacing;
            set
            {
                if (Mathf.Approximately(m_LineSpacing, value))
                    return;

                m_LineSpacing = value;
                SetDirty();
            }
        }

        #endregion

        protected UiCombiLayoutGroup() { }

        #region LayoutGroup overrides

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal(); // builds rectChildren, etc.

            if (m_Orientation == LayoutOrientation.Horizontal)
            {
                float innerWidth = rectTransform.rect.width - padding.horizontal;
                if (innerWidth < 0f) innerWidth = 0f;

                BuildRows(innerWidth, out float contentWidth, out _);
                float totalWidth = contentWidth + padding.horizontal;
                SetLayoutInputForAxis(totalWidth, totalWidth, -1, 0);
            }
            else
            {
                float innerHeight = rectTransform.rect.height - padding.vertical;
                if (innerHeight < 0f) innerHeight = 0f;

                BuildColumns(innerHeight, out float contentWidth, out _);
                float totalWidth = contentWidth + padding.horizontal;
                SetLayoutInputForAxis(totalWidth, totalWidth, -1, 0);
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            if (m_Orientation == LayoutOrientation.Horizontal)
            {
                float innerWidth = rectTransform.rect.width - padding.horizontal;
                if (innerWidth < 0f) innerWidth = 0f;

                BuildRows(innerWidth, out _, out float contentHeight);
                float totalHeight = contentHeight + padding.vertical;
                SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
            }
            else
            {
                float innerHeight = rectTransform.rect.height - padding.vertical;
                if (innerHeight < 0f) innerHeight = 0f;

                BuildColumns(innerHeight, out _, out float contentHeight);
                float totalHeight = contentHeight + padding.vertical;
                SetLayoutInputForAxis(totalHeight, totalHeight, -1, 1);
            }
        }

        public override void SetLayoutHorizontal()
        {
            if (m_Orientation == LayoutOrientation.Horizontal)
                LayoutHorizontal_Horizontal();
            else
                LayoutHorizontal_Vertical();
        }

        public override void SetLayoutVertical()
        {
            if (m_Orientation == LayoutOrientation.Horizontal)
                LayoutVertical_Horizontal();
            else
                LayoutVertical_Vertical();
        }

        #endregion

        #region Core layout helpers

        // Get child size as used by this layout (consistent for metrics + actual layout)
        private void GetChildSize(RectTransform child, out float width, out float height)
        {
            float minWidth = LayoutUtility.GetMinWidth(child);
            float prefWidth = LayoutUtility.GetPreferredWidth(child);
            float minHeight = LayoutUtility.GetMinHeight(child);
            float prefHeight = LayoutUtility.GetPreferredHeight(child);

            width = childControlWidth ? Mathf.Max(minWidth, prefWidth) : child.rect.size.x;
            height = childControlHeight ? Mathf.Max(minHeight, prefHeight) : child.rect.size.y;
        }

        #endregion

        #region Horizontal orientation (rows)

        private void BuildRows(float innerWidth, out float contentWidth, out float contentHeight)
        {
            m_Rows.Clear();

            int count = rectChildren.Count;
            if (count == 0)
            {
                contentWidth = 0f;
                contentHeight = 0f;
                return;
            }

            bool useWrap = m_Wrap && innerWidth > 0f && !float.IsInfinity(innerWidth);

            int rowStart = 0;
            float rowWidth = 0f;
            float rowHeight = 0f;
            bool firstInRow = true;

            float maxRowWidth = 0f;
            float totalHeight = 0f;

            for (int i = 0; i < count; i++)
            {
                RectTransform child = rectChildren[i];
                GetChildSize(child, out float w, out float h);

                bool needWrap = false;
                if (useWrap && !firstInRow)
                {
                    float testWidth = rowWidth + spacing + w;
                    if (testWidth > innerWidth + 0.001f)
                        needWrap = true;
                }

                if (needWrap)
                {
                    // finalize current row
                    Row row = new Row
                    {
                        startIndex = rowStart,
                        endIndex = i,
                        width = rowWidth,
                        height = rowHeight
                    };
                    m_Rows.Add(row);

                    maxRowWidth = Mathf.Max(maxRowWidth, rowWidth);
                    totalHeight += rowHeight;

                    rowStart = i;
                    rowWidth = 0f;
                    rowHeight = 0f;
                    firstInRow = true;
                }

                if (!firstInRow)
                    rowWidth += spacing;

                rowWidth += w;
                rowHeight = Mathf.Max(rowHeight, h);
                firstInRow = false;
            }

            // last row
            if (!firstInRow)
            {
                Row row = new Row
                {
                    startIndex = rowStart,
                    endIndex = count,
                    width = rowWidth,
                    height = rowHeight
                };
                m_Rows.Add(row);

                maxRowWidth = Mathf.Max(maxRowWidth, rowWidth);
                totalHeight += rowHeight;
            }

            if (m_Rows.Count > 1)
                totalHeight += m_LineSpacing * (m_Rows.Count - 1);

            contentWidth = maxRowWidth;
            contentHeight = totalHeight;
        }

        private void LayoutHorizontal_Horizontal()
        {
            float innerWidth = rectTransform.rect.width - padding.horizontal;
            if (innerWidth < 0f) innerWidth = 0f;

            BuildRows(innerWidth, out _, out _);

            int rowCount = m_Rows.Count;
            for (int r = 0; r < rowCount; r++)
            {
                Row row = m_Rows[r];
                float rowWidth = row.width;

                // Horizontal alignment per row
                float rowStartX = GetStartOffset(0, rowWidth);
                float x = rowStartX;

                for (int i = row.startIndex; i < row.endIndex; i++)
                {
                    RectTransform child = rectChildren[i];
                    GetChildSize(child, out float w, out float h);

                    float finalWidth = childControlWidth ? w : child.rect.size.x;
                    SetChildAlongAxis(child, 0, x, finalWidth);

                    x += finalWidth + spacing;
                }
            }
        }

        private void LayoutVertical_Horizontal()
        {
            float innerWidth = rectTransform.rect.width - padding.horizontal;
            if (innerWidth < 0f) innerWidth = 0f;

            BuildRows(innerWidth, out _, out float contentHeight);

            int rowCount = m_Rows.Count;
            float startY = GetStartOffset(1, contentHeight);
            float y = startY;

            for (int r = 0; r < rowCount; r++)
            {
                Row row = m_Rows[r];
                float rowHeight = row.height;

                for (int i = row.startIndex; i < row.endIndex; i++)
                {
                    RectTransform child = rectChildren[i];
                    GetChildSize(child, out float w, out float h);

                    float finalHeight = childControlHeight ? rowHeight : h;
                    SetChildAlongAxis(child, 1, y, finalHeight);
                }

                y += rowHeight + m_LineSpacing;
            }
        }

        #endregion

        #region Vertical orientation (columns)

        private void BuildColumns(float innerHeight, out float contentWidth, out float contentHeight)
        {
            m_Columns.Clear();

            int count = rectChildren.Count;
            if (count == 0)
            {
                contentWidth = 0f;
                contentHeight = 0f;
                return;
            }

            bool useWrap = m_Wrap && innerHeight > 0f && !float.IsInfinity(innerHeight);

            int colStart = 0;
            float colWidth = 0f;
            float colHeight = 0f;
            bool firstInCol = true;

            float totalWidth = 0f;
            float maxColumnHeight = 0f;

            for (int i = 0; i < count; i++)
            {
                RectTransform child = rectChildren[i];
                GetChildSize(child, out float w, out float h);

                bool needWrap = false;
                if (useWrap && !firstInCol)
                {
                    float testHeight = colHeight + spacing + h;
                    if (testHeight > innerHeight + 0.001f)
                        needWrap = true;
                }

                if (needWrap)
                {
                    Column col = new Column
                    {
                        startIndex = colStart,
                        endIndex = i,
                        width = colWidth,
                        height = colHeight
                    };
                    m_Columns.Add(col);

                    totalWidth += colWidth;
                    maxColumnHeight = Mathf.Max(maxColumnHeight, colHeight);

                    colStart = i;
                    colWidth = 0f;
                    colHeight = 0f;
                    firstInCol = true;
                }

                if (!firstInCol)
                    colHeight += spacing;

                colHeight += h;
                colWidth = Mathf.Max(colWidth, w);
                firstInCol = false;
            }

            if (!firstInCol)
            {
                Column col = new Column
                {
                    startIndex = colStart,
                    endIndex = count,
                    width = colWidth,
                    height = colHeight
                };
                m_Columns.Add(col);

                totalWidth += colWidth;
                maxColumnHeight = Mathf.Max(maxColumnHeight, colHeight);
            }

            if (m_Columns.Count > 1)
                totalWidth += m_LineSpacing * (m_Columns.Count - 1);

            contentWidth = totalWidth;
            contentHeight = maxColumnHeight;
        }

        private void LayoutHorizontal_Vertical()
        {
            float innerHeight = rectTransform.rect.height - padding.vertical;
            if (innerHeight < 0f) innerHeight = 0f;

            BuildColumns(innerHeight, out float contentWidth, out _);

            int colCount = m_Columns.Count;
            float startX = GetStartOffset(0, contentWidth);
            float x = startX;

            for (int c = 0; c < colCount; c++)
            {
                Column col = m_Columns[c];
                float colWidth = col.width;

                for (int i = col.startIndex; i < col.endIndex; i++)
                {
                    RectTransform child = rectChildren[i];
                    GetChildSize(child, out float w, out float h);

                    float finalWidth = childControlWidth ? colWidth : w;
                    SetChildAlongAxis(child, 0, x, finalWidth);
                }

                x += colWidth + m_LineSpacing;
            }
        }

        private void LayoutVertical_Vertical()
        {
            float innerHeight = rectTransform.rect.height - padding.vertical;
            if (innerHeight < 0f) innerHeight = 0f;

            BuildColumns(innerHeight, out _, out float contentHeight);

            int colCount = m_Columns.Count;
            float startY = GetStartOffset(1, contentHeight);

            for (int c = 0; c < colCount; c++)
            {
                Column col = m_Columns[c];

                float colHeight = col.height;
                float y = startY;

                for (int i = col.startIndex; i < col.endIndex; i++)
                {
                    RectTransform child = rectChildren[i];
                    GetChildSize(child, out float w, out float h);

                    float finalHeight = childControlHeight ? h : h; // same for now, but hook if you want column-based height
                    SetChildAlongAxis(child, 1, y, finalHeight);

                    y += h + spacing;
                }
            }
        }

        #endregion

        #region Boilerplate

        protected override void OnDisable()
        {
            base.OnDisable();
            SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetDirty();
        }
#endif

        protected override void OnTransformChildrenChanged()
        {
            base.OnTransformChildrenChanged();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        #endregion
    }
}
