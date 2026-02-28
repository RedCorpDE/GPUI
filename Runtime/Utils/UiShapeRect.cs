using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// Lightweight vector 2D rectangle shape for UGUI.
    /// - Solid rect (optionally with rounded corners)
    /// - Optional outline (ring geometry) with Inside/Center/Outside positioning
    /// - Optional soft shadow (offset + size + softness)
    ///
    /// Drop this onto a GameObject in place of an Image.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UiShapeRect : MaskableGraphic
    {
        public enum OutlinePosition
        {
            Inside,
            Center,
            Outside
        }

        [Header("Rectangle")]
        [Tooltip("Fill color of the shape (independent from outline and shadow).")]
        [SerializeField]
        [TabGroup("Settings", "Shape", SdfIconType.BoundingBoxCircles)]
        private Color fillColor = Color.white;

        [Tooltip("Corner radius in pixels: X = TL, Y = TR, Z = BR, W = BL.")]
        [SerializeField]
        [TabGroup("Settings", "Shape", SdfIconType.BoundingBoxCircles)]
        private Vector4 cornerRadius = Vector4.zero;

        [Tooltip("Number of segments used to approximate each rounded corner.")]
        [Range(1, 24)]
        [SerializeField]
        [TabGroup("Settings", "Shape", SdfIconType.BoundingBoxCircles)]
        private float cornerSegments = 8;

        [TabGroup("Settings", "Outline", SdfIconType.BorderOuter)]
        [SerializeField]
        private bool useOutline = false;

        [TabGroup("Settings", "Outline", SdfIconType.BorderOuter)]
        [Tooltip("Outline thickness in pixels.")]
        [Min(0f)]
        [SerializeField]
        private float outlineThickness = 2f;

        [TabGroup("Settings", "Outline", SdfIconType.BorderOuter)]
        [Tooltip("Where the outline is placed relative to the rect.")]
        [SerializeField]
        private OutlinePosition outlinePosition = OutlinePosition.Center;

        [TabGroup("Settings", "Outline", SdfIconType.BorderOuter)]
        [SerializeField]
        private Color outlineColor = Color.black;

        [TabGroup("Settings", "Shadow", SdfIconType.Subtract)]
        [SerializeField]
        private bool useShadow = false;

        [TabGroup("Settings", "Shadow")]
        [Tooltip("Base shadow offset in local pixels.")]
        [SerializeField]
        private Vector2 shadowOffset = new Vector2(2f, -2f);

        [TabGroup("Settings", "Shadow")]
        [Tooltip("Base expansion of the shadow in pixels (like a big solid drop shadow).")]
        [Min(0f)]
        [SerializeField]
        private float shadowSize = 4f;

        [TabGroup("Settings", "Shadow")]
        [Tooltip("Extra soft fade distance in pixels. 0 = hard edge, higher = softer shadow.")]
        [Min(0f)]
        [SerializeField]
        private float shadowSoftness = 8f;

        [TabGroup("Settings", "Shadow")]
        [Tooltip("How many soft steps to draw. More = smoother but more vertices.")]
        [Range(1, 8)]
        [SerializeField]
        private int shadowSteps = 4;

        [TabGroup("Settings", "Shadow")]
        [SerializeField]
        private Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

        // Reusable buffers (avoid allocations in OnPopulateMesh)
        private readonly List<Vector2> _tmpBorderA = new List<Vector2>(128);
        private readonly List<Vector2> _tmpBorderB = new List<Vector2>(128);

        #region Public API

        public Color FillColor
        {
            get => fillColor;
            set { fillColor = value; SetVerticesDirty(); }
        }

        public Vector4 CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = value; SetVerticesDirty(); }
        }

        public bool UseOutline
        {
            get => useOutline;
            set { useOutline = value; SetVerticesDirty(); }
        }

        public float OutlineThickness
        {
            get => outlineThickness;
            set { outlineThickness = Mathf.Max(0f, value); SetVerticesDirty(); }
        }

        public OutlinePosition OutlinePlacement
        {
            get => outlinePosition;
            set { outlinePosition = value; SetVerticesDirty(); }
        }

        public Color OutlineColor
        {
            get => outlineColor;
            set { outlineColor = value; SetVerticesDirty(); }
        }

        public bool UseShadow
        {
            get => useShadow;
            set { useShadow = value; SetVerticesDirty(); }
        }

        public Vector2 ShadowOffset
        {
            get => shadowOffset;
            set { shadowOffset = value; SetVerticesDirty(); }
        }

        public float ShadowSize
        {
            get => shadowSize;
            set { shadowSize = Mathf.Max(0f, value); SetVerticesDirty(); }
        }

        public float ShadowSoftness
        {
            get => shadowSoftness;
            set { shadowSoftness = Mathf.Max(0f, value); SetVerticesDirty(); }
        }

        public int ShadowSteps
        {
            get => shadowSteps;
            set { shadowSteps = Mathf.Clamp(value, 1, 8); SetVerticesDirty(); }
        }

        public Color ShadowCol
        {
            get => shadowColor;
            set { shadowColor = value; SetVerticesDirty(); }
        }

        public float ShapeRoundness
        {
            get => cornerSegments;
            set { cornerSegments = value; SetVerticesDirty(); }
        }

        #endregion

        #region Unity Overrides

        public override Texture mainTexture
        {
            get
            {
                if (material != null && material.mainTexture != null)
                    return material.mainTexture;

                return s_WhiteTexture;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            cornerSegments = Mathf.Max(1, cornerSegments);
            outlineThickness = Mathf.Max(0f, outlineThickness);
            shadowSize = Mathf.Max(0f, shadowSize);
            shadowSoftness = Mathf.Max(0f, shadowSoftness);
            shadowSteps = Mathf.Clamp(shadowSteps, 1, 8);

            // Ensure the built-in Graphic color does not tint our geometry.
            if (color != Color.white)
                color = Color.white;

            SetVerticesDirty();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            // Also enforce white at runtime in case someone changes it via script.
            if (color != Color.white)
                color = Color.white;

            Rect rect = GetPixelAdjustedRect();
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            Vector4 radius = GetClampedRadius(rect, cornerRadius);
            int segments = Mathf.Max(1, Mathf.RoundToInt(cornerSegments));

            // SOFT SHADOW (drawn first so it appears behind everything)
            if (useShadow && shadowColor.a > 0f)
            {
                int steps = Mathf.Max(1, shadowSteps);

                for (int i = 0; i < steps; i++)
                {
                    // t=0 is inner-most (solid), t=1 is outer-most (faded)
                    float t = (steps == 1) ? 0f : (float)i / (steps - 1);
                    float size = shadowSize + t * shadowSoftness;

                    float alphaFactor = 1f - t;
                    Color col = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * alphaFactor);

                    Rect shadowRect = InflateRect(rect, size);
                    shadowRect.position += (Vector2)shadowOffset;

                    // Keep "radius=0" corners sharp; only expand radii that are > 0
                    Vector4 shadowRadius = OffsetRadius(radius, size);
                    shadowRadius = GetClampedRadius(shadowRect, shadowRadius);

                    AddFilledRoundedRect(vh, shadowRect, shadowRadius, col, segments);
                }
            }

            // FILL (uses fillColor only)
            AddFilledRoundedRect(vh, rect, radius, fillColor, segments);

            // OUTLINE (uses outlineColor only)
            if (useOutline && outlineThickness > 0f && outlineColor.a > 0f)
            {
                AddRoundedOutline(vh, rect, radius, outlineThickness, outlinePosition, outlineColor, segments);
            }
        }

        #endregion

        #region Geometry Helpers

        private static Vector4 GetClampedRadius(Rect rect, Vector4 r)
        {
            float maxRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (maxRadius < 0f) maxRadius = 0f;

            r.x = Mathf.Clamp(r.x, 0f, maxRadius);
            r.y = Mathf.Clamp(r.y, 0f, maxRadius);
            r.z = Mathf.Clamp(r.z, 0f, maxRadius);
            r.w = Mathf.Clamp(r.w, 0f, maxRadius);
            return r;
        }

        /// <summary>
        /// Offsets radii by amount, but keeps 0-radius corners sharp (0 stays 0).
        /// Also clamps to >= 0.
        /// </summary>
        private static Vector4 OffsetRadius(Vector4 baseRadius, float amount)
        {
            float OffsetOne(float r)
            {
                if (r <= 0f) return 0f;
                return Mathf.Max(0f, r + amount);
            }

            return new Vector4(
                OffsetOne(baseRadius.x),
                OffsetOne(baseRadius.y),
                OffsetOne(baseRadius.z),
                OffsetOne(baseRadius.w)
            );
        }

        private void AddFilledRoundedRect(VertexHelper vh, Rect rect, Vector4 radius, Color col, int segmentsPerCorner)
        {
            Color32 c32 = col;
            Vector2 center = rect.center;

            GenerateRoundedRectBorderFixedCount(rect, radius, segmentsPerCorner, _tmpBorderA);

            int startIndex = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            // center
            v.position = center;
            vh.AddVert(v);

            // border verts
            for (int i = 0; i < _tmpBorderA.Count; i++)
            {
                v.position = _tmpBorderA[i];
                vh.AddVert(v);
            }

            int borderCount = _tmpBorderA.Count;
            for (int i = 0; i < borderCount; i++)
            {
                int i0 = startIndex;
                int i1 = startIndex + 1 + i;
                int i2 = startIndex + 1 + ((i + 1) % borderCount);
                vh.AddTriangle(i0, i1, i2);
            }
        }

        private void AddRoundedOutline(
            VertexHelper vh,
            Rect rect,
            Vector4 radius,
            float thickness,
            OutlinePosition position,
            Color col,
            int segmentsPerCorner)
        {
            if (thickness <= 0f)
                return;

            Color32 c32 = col;

            float outerOffset;
            float innerOffset;

            switch (position)
            {
                case OutlinePosition.Inside:
                    outerOffset = 0f;
                    innerOffset = thickness;
                    break;
                case OutlinePosition.Outside:
                    outerOffset = thickness;
                    innerOffset = 0f;
                    break;
                default: // Center
                    outerOffset = thickness * 0.5f;
                    innerOffset = thickness * 0.5f;
                    break;
            }

            Rect outerRect = InflateRect(rect, outerOffset);
            Rect innerRect = InflateRect(rect, -innerOffset);

            if (outerRect.width <= 0f || outerRect.height <= 0f)
                return;

            // If the "hole" collapses (outline too thick for the available space),
            // fall back to a filled outer shape in outlineColor.
            if (innerRect.width <= 0f || innerRect.height <= 0f)
            {
                Vector4 outerRadiusFill = GetClampedRadius(outerRect, OffsetRadius(radius, outerOffset));
                AddFilledRoundedRect(vh, outerRect, outerRadiusFill, col, segmentsPerCorner);
                return;
            }

            // Radius handling:
            // - Outside/Center: outer radius expands for rounded corners
            // - Inside/Center: inner radius shrinks for rounded corners
            // - 0-radius corners stay 0 (sharp) regardless of offsets
            Vector4 outerRadius = OffsetRadius(radius, outerOffset);
            Vector4 innerRadius = OffsetRadius(radius, -innerOffset);

            outerRadius = GetClampedRadius(outerRect, outerRadius);
            innerRadius = GetClampedRadius(innerRect, innerRadius);

            GenerateRoundedRectBorderFixedCount(outerRect, outerRadius, segmentsPerCorner, _tmpBorderA);
            GenerateRoundedRectBorderFixedCount(innerRect, innerRadius, segmentsPerCorner, _tmpBorderB);

            // FixedCount guarantees this (unless rect invalid, which we handled).
            int count = _tmpBorderA.Count;
            if (count != _tmpBorderB.Count || count < 3)
                return;

            int startIndex = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            // outer
            for (int i = 0; i < count; i++)
            {
                v.position = _tmpBorderA[i];
                vh.AddVert(v);
            }

            // inner
            for (int i = 0; i < count; i++)
            {
                v.position = _tmpBorderB[i];
                vh.AddVert(v);
            }

            // stitch as a strip of quads (2 tris each)
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;

                int outer0 = startIndex + i;
                int outer1 = startIndex + next;
                int inner0 = startIndex + count + i;
                int inner1 = startIndex + count + next;

                vh.AddTriangle(outer0, outer1, inner0);
                vh.AddTriangle(outer1, inner1, inner0);
            }
        }

        /// <summary>
        /// Generates a rounded-rect border with a FIXED vertex count:
        /// total points = 4 * (segmentsPerCorner + 1).
        ///
        /// This is critical for outlines, because inner/outer radii can hit 0 independently.
        /// If a radius is 0, we still emit the same number of points for that corner (duplicates),
        /// so inner and outer lists always match and the ring can be stitched.
        /// </summary>
        private static void GenerateRoundedRectBorderFixedCount(
            Rect rect,
            Vector4 radius,
            int segmentsPerCorner,
            List<Vector2> output)
        {
            output.Clear();

            if (rect.width <= 0f || rect.height <= 0f)
                return;

            segmentsPerCorner = Mathf.Max(1, segmentsPerCorner);

            float left = rect.xMin;
            float right = rect.xMax;
            float bottom = rect.yMin;
            float top = rect.yMax;

            float tl = radius.x;
            float tr = radius.y;
            float br = radius.z;
            float bl = radius.w;

            Vector2 tlCenter = new Vector2(left + tl, top - tl);
            Vector2 trCenter = new Vector2(right - tr, top - tr);
            Vector2 brCenter = new Vector2(right - br, bottom + br);
            Vector2 blCenter = new Vector2(left + bl, bottom + bl);

            // Adds (segmentsPerCorner + 1) points, even if rad == 0 (duplicates).
            static void AddArcFixed(List<Vector2> dst, Vector2 center, float rad, float startDeg, float endDeg, int segs)
            {
                // If rad is 0, center is already the corner point (because tlCenter etc. collapse to corners).
                // We still emit fixed count points for matching indices between borders.
                float step = (endDeg - startDeg) / segs;

                for (int i = 0; i <= segs; i++)
                {
                    float angle = (startDeg + step * i) * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    dst.Add(center + dir * rad);
                }
            }

            // TL: 180 -> 90
            AddArcFixed(output, tlCenter, tl, 180f, 90f, segmentsPerCorner);
            // TR: 90 -> 0
            AddArcFixed(output, trCenter, tr, 90f, 0f, segmentsPerCorner);
            // BR: 0 -> -90
            AddArcFixed(output, brCenter, br, 0f, -90f, segmentsPerCorner);
            // BL: -90 -> -180
            AddArcFixed(output, blCenter, bl, -90f, -180f, segmentsPerCorner);
        }

        private static Rect InflateRect(Rect rect, float amount)
        {
            rect.xMin -= amount;
            rect.xMax += amount;
            rect.yMin -= amount;
            rect.yMax += amount;
            return rect;
        }

        #endregion
    }
}