using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// Circular sector / donut-slice shape for UGUI.
    /// - Sector defined by start angle + arc angle (0-360)
    /// - Inner radius 0 => full pie slice, >0 => donut
    /// - Optional outline ring
    /// - Optional soft shadow
    ///
    /// Drop this onto a GameObject in place of an Image.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UiShapeSector : MaskableGraphic
    {
        [Header("Sector")]
        [TabGroup("Settings", "Shape", SdfIconType.PieChart)]
        [Tooltip("Fill color of the sector (independent from outline and shadow).")]
        [SerializeField]
        private Color fillColor = Color.white;

        [TabGroup("Settings", "Shape", SdfIconType.PieChart)]
        [Tooltip("Start angle of the sector in degrees (0 = +X axis, CCW).")]
        [SerializeField]
        [PropertyRange(0f, 360f)]
        private float startAngle = 0f;

        [TabGroup("Settings", "Shape", SdfIconType.PieChart)]
        [Tooltip("Arc angle in degrees (0-360). Useful for timers/value displays.")]
        [SerializeField]
        [PropertyRange(0f, 360f)]
        private float arcAngle = 360f;

        [TabGroup("Settings", "Shape", SdfIconType.PieChart)]
        [Tooltip("Outer radius as a fraction of half the shortest rect side (0-1).")]
        [SerializeField]
        [Range(0f, 1f)]
        private float outerRadius = 1f;

        [TabGroup("Settings", "Shape", SdfIconType.PieChart)]
        [Tooltip("Inner radius as a fraction of the outer radius (0 = full pie, 0.8 = ring).")]
        [SerializeField]
        [Range(0f, 1f)]
        private float innerRadius = 0f;

        [TabGroup("Settings", "Shape", SdfIconType.PieChart)]
        [Tooltip("Segments used to approximate the arc.")]
        [SerializeField]
        [Range(3, 128)]
        private int segments = 32;

        [TabGroup("Settings", "Outline", SdfIconType.BorderOuter)]
        [SerializeField]
        private bool useOutline = false;

        [TabGroup("Settings", "Outline", SdfIconType.BorderOuter)]
        [Tooltip("Outline thickness in pixels around the sector.")]
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
            set { fillColor = value; SetVerticesDirty(); }
        }

        public float StartAngle
        {
            get => startAngle;
            set { startAngle = Mathf.Repeat(value, 360f); SetVerticesDirty(); }
        }

        public float ArcAngle
        {
            get => arcAngle;
            set { arcAngle = Mathf.Clamp(value, 0f, 360f); SetVerticesDirty(); }
        }

        public float OuterRadius
        {
            get => outerRadius;
            set { outerRadius = Mathf.Clamp01(value); SetVerticesDirty(); }
        }

        public float InnerRadius
        {
            get => innerRadius;
            set { innerRadius = Mathf.Clamp01(value); SetVerticesDirty(); }
        }

        public int Segments
        {
            get => segments;
            set { segments = Mathf.Clamp(value, 3, 128); SetVerticesDirty(); }
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
            segments        = Mathf.Clamp(segments, 3, 128);
            outlineThickness = Mathf.Max(0f, outlineThickness);
            shadowSize      = Mathf.Max(0f, shadowSize);
            shadowSoftness  = Mathf.Max(0f, shadowSoftness);
            shadowSteps     = Mathf.Clamp(shadowSteps, 1, 8);
            outerRadius     = Mathf.Clamp01(outerRadius);
            innerRadius     = Mathf.Clamp01(innerRadius);
            arcAngle        = Mathf.Clamp(arcAngle, 0f, 360f);

            // Ensure the built-in Graphic color does not tint our geometry.
            if (color != Color.white)
                color = Color.white;

            SetVerticesDirty();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (arcAngle <= 0f || outerRadius <= 0f || segments < 3)
                return;

            if (color != Color.white)
                color = Color.white;

            Rect rect = GetPixelAdjustedRect();
            Vector2 center = rect.center;
            float halfMin = Mathf.Min(rect.width, rect.height) * 0.5f;

            float outerR = halfMin * outerRadius;
            float innerR = Mathf.Clamp01(innerRadius) * outerR;

            int segCount = Mathf.Max(3, segments);

            // SHADOW (behind)
            if (useShadow && shadowColor.a > 0f)
            {
                int steps = Mathf.Max(1, shadowSteps);

                for (int i = 0; i < steps; i++)
                {
                    float t = (steps == 1) ? 0f : (float)i / (steps - 1);
                    float extra = shadowSize + t * shadowSoftness;

                    float alphaFactor = 1f - t;
                    Color col = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * alphaFactor);

                    float outerShadowR = outerR + extra;
                    float innerShadowR = Mathf.Max(0f, innerR - extra * 0.5f);

                    Vector2 shadowCenter = center + shadowOffset;

                    AddFilledSector(vh, shadowCenter, innerShadowR, outerShadowR, startAngle, arcAngle, col, segCount);
                }
            }

            // FILL
            AddFilledSector(vh, center, innerR, outerR, startAngle, arcAngle, fillColor, segCount);

            // OUTLINE
            if (useOutline && outlineThickness > 0f)
            {
                float outerOutlineR = outerR + outlineThickness * 0.5f;
                float innerOutlineR = Mathf.Max(0f, innerR - outlineThickness * 0.5f);

                AddSectorOutline(vh, center, innerOutlineR, outerOutlineR, startAngle, arcAngle, outlineColor, segCount);
            }
        }

        #endregion

        #region Geometry Helpers

        private static void AddFilledSector(
            VertexHelper vh,
            Vector2 center,
            float innerR,
            float outerR,
            float startDeg,
            float arcDeg,
            Color col,
            int segments)
        {
            if (arcDeg <= 0f || outerR <= 0f)
                return;

            Color32 c32 = col;

            float endDeg = startDeg + arcDeg;
            float startRad = startDeg * Mathf.Deg2Rad;
            float endRad   = endDeg   * Mathf.Deg2Rad;

            int segCount = Mathf.Max(1, segments);
            float step = (endRad - startRad) / segCount;

            int startIndex = vh.currentVertCount;
            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            bool hasInner = innerR > 0f;

            if (!hasInner)
            {
                // Fan from center
                v.position = center;
                vh.AddVert(v);

                for (int i = 0; i <= segCount; i++)
                {
                    float ang = startRad + step * i;
                    Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    v.position = center + dir * outerR;
                    vh.AddVert(v);
                }

                for (int i = 0; i < segCount; i++)
                {
                    int i0 = startIndex;          // center
                    int i1 = startIndex + 1 + i;
                    int i2 = startIndex + 1 + i + 1;
                    vh.AddTriangle(i0, i1, i2);
                }
            }
            else
            {
                // Donut sector band between inner and outer radius
                int outerStart = vh.currentVertCount;

                // Outer arc
                for (int i = 0; i <= segCount; i++)
                {
                    float ang = startRad + step * i;
                    Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    v.position = center + dir * outerR;
                    vh.AddVert(v);
                }

                int innerStart = vh.currentVertCount;

                // Inner arc
                for (int i = 0; i <= segCount; i++)
                {
                    float ang = startRad + step * i;
                    Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    v.position = center + dir * innerR;
                    vh.AddVert(v);
                }

                for (int i = 0; i < segCount; i++)
                {
                    int o0 = outerStart + i;
                    int o1 = outerStart + i + 1;
                    int i0 = innerStart + i;
                    int i1 = innerStart + i + 1;

                    vh.AddTriangle(o0, o1, i0);
                    vh.AddTriangle(o1, i1, i0);
                }
            }
        }

        private static void AddSectorOutline(
            VertexHelper vh,
            Vector2 center,
            float innerR,
            float outerR,
            float startDeg,
            float arcDeg,
            Color col,
            int segments)
        {
            if (arcDeg <= 0f || outerR <= 0f)
                return;

            Color32 c32 = col;

            float endDeg = startDeg + arcDeg;
            float startRad = startDeg * Mathf.Deg2Rad;
            float endRad   = endDeg   * Mathf.Deg2Rad;

            int segCount = Mathf.Max(1, segments);
            float step = (endRad - startRad) / segCount;

            float inner = Mathf.Max(0f, innerR);
            float outer = Mathf.Max(inner, outerR);

            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            int outerStart = vh.currentVertCount;
            for (int i = 0; i <= segCount; i++)
            {
                float ang = startRad + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                v.position = center + dir * outer;
                vh.AddVert(v);
            }

            int innerStart = vh.currentVertCount;
            for (int i = 0; i <= segCount; i++)
            {
                float ang = startRad + step * i;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                v.position = center + dir * inner;
                vh.AddVert(v);
            }

            for (int i = 0; i < segCount; i++)
            {
                int o0 = outerStart + i;
                int o1 = outerStart + i + 1;
                int i0 = innerStart + i;
                int i1 = innerStart + i + 1;

                vh.AddTriangle(o0, o1, i0);
                vh.AddTriangle(o1, i1, i0);
            }
        }

        #endregion
    }
}
