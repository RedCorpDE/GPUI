using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// Lightweight vector 2D rectangle shape for UGUI.
    /// - Solid rect (optionally with rounded corners)
    /// - Optional outline (ring geometry)
    /// - Optional soft shadow (offset + size + softness)
    ///
    /// Drop this onto a GameObject in place of an Image.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UiShapeRect : MaskableGraphic
    {
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

        #region Public API

        public Color FillColor
        {
            get => fillColor;
            set
            {
                fillColor = value;
                SetVerticesDirty();
            }
        }

        public Vector4 CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = value;
                SetVerticesDirty();
            }
        }

        public bool UseOutline
        {
            get => useOutline;
            set
            {
                useOutline = value;
                SetVerticesDirty();
            }
        }

        public float OutlineThickness
        {
            get => outlineThickness;
            set
            {
                outlineThickness = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public Color OutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                SetVerticesDirty();
            }
        }

        public bool UseShadow
        {
            get => useShadow;
            set
            {
                useShadow = value;
                SetVerticesDirty();
            }
        }

        public Vector2 ShadowOffset
        {
            get => shadowOffset;
            set
            {
                shadowOffset = value;
                SetVerticesDirty();
            }
        }

        public float ShadowSize
        {
            get => shadowSize;
            set
            {
                shadowSize = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public float ShadowSoftness
        {
            get => shadowSoftness;
            set
            {
                shadowSoftness = Mathf.Max(0f, value);
                SetVerticesDirty();
            }
        }

        public int ShadowSteps
        {
            get => shadowSteps;
            set
            {
                shadowSteps = Mathf.Clamp(value, 1, 8);
                SetVerticesDirty();
            }
        }

        public Color ShadowCol
        {
            get => shadowColor;
            set
            {
                shadowColor = value;
                SetVerticesDirty();
            }
        }

        public float ShapeRoundness
        {
            get => cornerSegments;
            set
            {
                cornerSegments = value;
                SetVerticesDirty();
            }
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
            cornerSegments   = Mathf.Max(1, cornerSegments);
            outlineThickness = Mathf.Max(0f, outlineThickness);
            shadowSize       = Mathf.Max(0f, shadowSize);
            shadowSoftness   = Mathf.Max(0f, shadowSoftness);
            shadowSteps      = Mathf.Clamp(shadowSteps, 1, 8);

            // Ensure the built-in Graphic color does not tint our geometry.
            // All actual colors come from fillColor / outlineColor / shadowColor.
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
            Vector4 radius = GetClampedRadius(rect, cornerRadius);
            int segments = Mathf.Max(1, Mathf.RoundToInt(cornerSegments));

            // SOFT SHADOW (drawn first so it appears behind everything)
            if (useShadow && shadowColor.a > 0f)
            {
                int steps = Mathf.Max(1, shadowSteps);

                for (int i = 0; i < steps; i++)
                {
                    // t=0 is inner-most (solid), t=1 is outer-most (faded)
                    float t    = (steps == 1) ? 0f : (float)i / (steps - 1);
                    float size = shadowSize + t * shadowSoftness;

                    // Fade alpha outwards
                    float alphaFactor = 1f - t;
                    Color col = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * alphaFactor);

                    Rect shadowRect = InflateRect(rect, size);
                    shadowRect.position += (Vector2)shadowOffset;

                    Vector4 shadowRadius = radius + Vector4.one * size;
                    shadowRadius = GetClampedRadius(shadowRect, shadowRadius);

                    AddFilledRoundedRect(vh, shadowRect, shadowRadius, col, segments);
                }
            }

            // FILL (uses fillColor only)
            AddFilledRoundedRect(vh, rect, radius, fillColor, segments);

            // OUTLINE (uses outlineColor only)
            if (useOutline && outlineThickness > 0f)
            {
                AddRoundedOutline(vh, rect, radius, outlineThickness, outlineColor, segments);
            }
        }

        #endregion

        #region Geometry Helpers

        private static Vector4 GetClampedRadius(Rect rect, Vector4 r)
        {
            float maxRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
            r.x = Mathf.Clamp(r.x, 0f, maxRadius);
            r.y = Mathf.Clamp(r.y, 0f, maxRadius);
            r.z = Mathf.Clamp(r.z, 0f, maxRadius);
            r.w = Mathf.Clamp(r.w, 0f, maxRadius);
            return r;
        }

        private void AddFilledRoundedRect(VertexHelper vh, Rect rect, Vector4 radius, Color col, int segmentsPerCorner)
        {
            Color32 c32 = col;
            var center = rect.center;

            List<Vector2> border = new List<Vector2>(segmentsPerCorner * 4);
            GenerateRoundedRectBorder(rect, radius, segmentsPerCorner, border);

            int startIndex = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            // center
            v.position = center;
            vh.AddVert(v);

            // border verts
            for (int i = 0; i < border.Count; i++)
            {
                v.position = border[i];
                vh.AddVert(v);
            }

            int borderCount = border.Count;
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
            Color col,
            int segmentsPerCorner)
        {
            Color32 c32 = col;

            Rect outerRect = InflateRect(rect, thickness * 0.5f);
            Rect innerRect = InflateRect(rect, -thickness * 0.5f);

            Vector4 outerRadius = radius + Vector4.one * (thickness * 0.5f);
            Vector4 innerRadius = radius - Vector4.one * (thickness * 0.5f);
            innerRadius.x = Mathf.Max(0f, innerRadius.x);
            innerRadius.y = Mathf.Max(0f, innerRadius.y);
            innerRadius.z = Mathf.Max(0f, innerRadius.z);
            innerRadius.w = Mathf.Max(0f, innerRadius.w);

            outerRadius = GetClampedRadius(outerRect, outerRadius);
            innerRadius = GetClampedRadius(innerRect, innerRadius);

            List<Vector2> outerBorder = new List<Vector2>(segmentsPerCorner * 4);
            List<Vector2> innerBorder = new List<Vector2>(segmentsPerCorner * 4);
            GenerateRoundedRectBorder(outerRect, outerRadius, segmentsPerCorner, outerBorder);
            GenerateRoundedRectBorder(innerRect, innerRadius, segmentsPerCorner, innerBorder);

            if (outerBorder.Count != innerBorder.Count)
                return;

            int startIndex = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            for (int i = 0; i < outerBorder.Count; i++)
            {
                v.position = outerBorder[i];
                vh.AddVert(v);
            }

            for (int i = 0; i < innerBorder.Count; i++)
            {
                v.position = innerBorder[i];
                vh.AddVert(v);
            }

            int count = outerBorder.Count;
            for (int i = 0; i < count; i++)
            {
                int outer0 = startIndex + i;
                int outer1 = startIndex + ((i + 1) % count);
                int inner0 = startIndex + count + i;
                int inner1 = startIndex + count + ((i + 1) % count);

                vh.AddTriangle(outer0, outer1, inner0);
                vh.AddTriangle(outer1, inner1, inner0);
            }
        }

        private static void GenerateRoundedRectBorder(
            Rect rect,
            Vector4 radius,
            int segmentsPerCorner,
            List<Vector2> output)
        {
            output.Clear();

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

            void AddArc(Vector2 center, float rad, float startDeg, float endDeg)
            {
                if (rad <= 0f)
                {
                    float radStart = startDeg * Mathf.Deg2Rad;
                    output.Add(center + new Vector2(Mathf.Cos(radStart), Mathf.Sin(radStart)) * rad);
                    return;
                }

                float step = (endDeg - startDeg) / segmentsPerCorner;
                for (int i = 0; i <= segmentsPerCorner; i++)
                {
                    float angle = (startDeg + step * i) * Mathf.Deg2Rad;
                    Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rad;
                    output.Add(p);
                }
            }

            // TL: 180 -> 90
            AddArc(tlCenter, tl, 180f, 90f);
            // TR: 90 -> 0
            AddArc(trCenter, tr, 90f, 0f);
            // BR: 0 -> -90
            AddArc(brCenter, br, 0f, -90f);
            // BL: -90 -> -180
            AddArc(blCenter, bl, -90f, -180f);
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
