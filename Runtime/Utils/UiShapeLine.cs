using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GPUI
{
    /// <summary>
    /// Lightweight vector 2D line shape for UGUI.
    /// - Thick polyline (open or closed)
    /// - Optional rounded corners at joints (without moving the points)
    /// - Optional soft shadow (offset + size + softness)
    ///
    /// Drop this onto a GameObject in place of an Image, and
    /// edit the points list in local RectTransform space.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UiShapeLine : MaskableGraphic
    {
        [Header("Line")]
        [TabGroup("Settings", "Shape", SdfIconType.Activity)]
        [Tooltip("Local-space points of the line (in RectTransform space).")]
        [SerializeField]
        [ListDrawerSettings(DraggableItems = true, Expanded = true)]
        private List<Vector2> points = new()
        {
            new Vector2(-50f, 0f),
            new Vector2( 50f, 0f)
        };

        [TabGroup("Settings", "Shape", SdfIconType.Activity)]
        [Tooltip("Thickness of the line in pixels.")]
        [Min(0.1f)]
        [SerializeField]
        private float thickness = 4f;

        [TabGroup("Settings", "Shape", SdfIconType.Activity)]
        [Tooltip("If true, connects last point back to first.")]
        [SerializeField]
        private bool closed = false;

        [TabGroup("Settings", "Shape", SdfIconType.BoundingBoxCircles)]
        [Tooltip("Round the line corners at joints (visual only, does not move the points).")]
        [SerializeField]
        private bool useRoundedCorners = true;

        [TabGroup("Settings", "Shape", SdfIconType.BoundingBoxCircles)]
        [Tooltip("Segments used to approximate each rounded corner.")]
        [Range(2, 24)]
        [SerializeField]
        private int cornerSegments = 8;

        [TabGroup("Settings", "Shadow", SdfIconType.Subtract)]
        [SerializeField]
        private bool useShadow = false;

        [TabGroup("Settings", "Shadow")]
        [Tooltip("Base shadow offset in local pixels.")]
        [SerializeField]
        private Vector2 shadowOffset = new Vector2(2f, -2f);

        [TabGroup("Settings", "Shadow")]
        [Tooltip("Base expansion added to the line thickness for the shadow (like a big solid drop shadow).")]
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

        public List<Vector2> Points
        {
            get => points;
            set
            {
                points = value;
                SetVerticesDirty();
            }
        }

        public float Thickness
        {
            get => thickness;
            set
            {
                thickness = Mathf.Max(0.1f, value);
                SetVerticesDirty();
            }
        }

        public bool Closed
        {
            get => closed;
            set
            {
                closed = value;
                SetVerticesDirty();
            }
        }

        public bool UseRoundedCorners
        {
            get => useRoundedCorners;
            set
            {
                useRoundedCorners = value;
                SetVerticesDirty();
            }
        }

        public int CornerSegments
        {
            get => cornerSegments;
            set
            {
                cornerSegments = Mathf.Clamp(value, 2, 24);
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
            thickness      = Mathf.Max(0.1f, thickness);
            cornerSegments = Mathf.Clamp(cornerSegments, 2, 24);
            shadowSize     = Mathf.Max(0f, shadowSize);
            shadowSoftness = Mathf.Max(0f, shadowSoftness);
            shadowSteps    = Mathf.Clamp(shadowSteps, 1, 8);
            SetVerticesDirty();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (points == null || points.Count < 2 || thickness <= 0f)
                return;

            // SHADOW FIRST
            if (useShadow && shadowColor.a > 0f)
            {
                int steps = Mathf.Max(1, shadowSteps);

                for (int i = 0; i < steps; i++)
                {
                    float t = (steps == 1) ? 0f : (float)i / (steps - 1);
                    float extra = shadowSize + t * shadowSoftness;

                    float alphaFactor = 1f - t;
                    Color col = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * alphaFactor);

                    float width = thickness + extra * 2f;

                    AddPolylineWithRoundedCorners(vh, points, width, col, shadowOffset, closed, useRoundedCorners, cornerSegments);
                }
            }

            // MAIN LINE
            AddPolylineWithRoundedCorners(vh, points, thickness, color, Vector2.zero, closed, useRoundedCorners, cornerSegments);
        }

        #endregion

        #region Geometry Helpers

        private static void AddPolylineWithRoundedCorners(
            VertexHelper vh,
            List<Vector2> pts,
            float width,
            Color col,
            Vector2 offset,
            bool closed,
            bool roundedCorners,
            int cornerSegments)
        {
            int count = pts.Count;
            if (count < 2 || width <= 0f)
                return;

            Color32 c32 = col;
            float halfWidth = width * 0.5f;

            int vertStart = vh.currentVertCount;
            int segmentCount = closed ? count : count - 1;

            // Precompute directions & normals for each point
            Vector2[] tangents = new Vector2[count];
            Vector2[] normals = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                Vector2 pPrev, pNext;

                if (i == 0)
                {
                    pPrev = closed ? pts[count - 1] : pts[0];
                    pNext = pts[1];
                }
                else if (i == count - 1)
                {
                    pPrev = pts[count - 2];
                    pNext = closed ? pts[0] : pts[count - 1];
                }
                else
                {
                    pPrev = pts[i - 1];
                    pNext = pts[i + 1];
                }

                Vector2 dir = (pNext - pPrev);
                if (dir.sqrMagnitude < 0.0001f)
                    dir = Vector2.right;

                dir.Normalize();
                tangents[i] = dir;

                Vector2 n = new Vector2(-dir.y, dir.x);
                normals[i] = n;
            }

            // Create two verts per point for the ribbon (butt joins)
            UIVertex v = UIVertex.simpleVert;
            v.color = c32;

            for (int i = 0; i < count; i++)
            {
                Vector2 p = pts[i] + offset;
                Vector2 n = normals[i] * halfWidth;

                // left
                v.position = p - n;
                vh.AddVert(v);

                // right
                v.position = p + n;
                vh.AddVert(v);
            }

            // Triangles between segments (simple ribbon)
            for (int i = 0; i < segmentCount; i++)
            {
                int i0 = i;
                int i1 = (i + 1) % count;

                int v0 = vertStart + i0 * 2;
                int v1 = vertStart + i0 * 2 + 1;
                int v2 = vertStart + i1 * 2;
                int v3 = vertStart + i1 * 2 + 1;

                // quad: v0-v2-v1, v2-v3-v1
                vh.AddTriangle(v0, v2, v1);
                vh.AddTriangle(v2, v3, v1);
            }

            // Optional corner rounding (visual only – does not move path points)
            if (!roundedCorners || cornerSegments < 2)
                return;

            int startIndex = closed ? 0 : 1;
            int endIndex   = closed ? count : count - 1;

            for (int i = startIndex; i < endIndex - 1; i++)
            {
                int prevIndex = (i - 1 + count) % count;
                int currIndex = i;
                int nextIndex = (i + 1) % count;

                // For open polyline, skip rounding at ends
                if (!closed && (currIndex == 0 || currIndex == count - 1))
                    continue;

                Vector2 p0 = pts[prevIndex];
                Vector2 p1 = pts[currIndex];
                Vector2 p2 = pts[nextIndex];

                Vector2 v0 = (p1 - p0);
                Vector2 v1 = (p2 - p1);
                float len0 = v0.magnitude;
                float len1 = v1.magnitude;

                if (len0 < 1e-4f || len1 < 1e-4f)
                    continue;

                Vector2 d0 = v0 / len0; // incoming
                Vector2 d1 = v1 / len1; // outgoing

                // If almost straight, no need to round
                float dot = Vector2.Dot(d0, d1);
                if (dot > 0.999f)
                    continue;

                // Turn direction
                float cross = d0.x * d1.y - d0.y * d1.x;
                if (Mathf.Abs(cross) < 1e-4f)
                    continue; // nearly colinear

                bool leftTurn = cross > 0f;

                // Outer normals for each segment
                Vector2 leftNormal0 = new Vector2(-d0.y, d0.x);
                Vector2 leftNormal1 = new Vector2(-d1.y, d1.x);

                Vector2 nOut0 = leftTurn ? leftNormal0 : -leftNormal0;
                Vector2 nOut1 = leftTurn ? leftNormal1 : -leftNormal1;

                // Outer edge directions at this joint
                float ang0 = Mathf.Atan2(nOut0.y, nOut0.x);
                float ang1 = Mathf.Atan2(nOut1.y, nOut1.x);

                // Adjust ang1 so we go the correct way around the outside of the corner
                if (leftTurn)
                {
                    // CCW
                    if (ang1 < ang0)
                        ang1 += 2f * Mathf.PI;
                }
                else
                {
                    // CW
                    if (ang1 > ang0)
                        ang1 -= 2f * Mathf.PI;
                }

                float totalAngle = ang1 - ang0;
                float step = totalAngle / cornerSegments;

                // We build a fan rooted at the joint center, overlapping the band,
                // but visually rounding the outer corner.
                Vector2 center = p1 + offset;
                int centerVertIndex = vh.currentVertCount;

                // Center vertex (at joint center)
                UIVertex cv = UIVertex.simpleVert;
                cv.color = c32;
                cv.position = center;
                vh.AddVert(cv);

                // Arc vertices on the outer edge
                int firstArcIndex = vh.currentVertCount;
                UIVertex av = UIVertex.simpleVert;
                av.color = c32;

                for (int s = 0; s <= cornerSegments; s++)
                {
                    float a = ang0 + step * s;
                    Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    av.position = center + dir * halfWidth;
                    vh.AddVert(av);
                }

                // Fan triangles
                for (int s = 0; s < cornerSegments; s++)
                {
                    int i0v = centerVertIndex;
                    int i1v = firstArcIndex + s;
                    int i2v = firstArcIndex + s + 1;
                    vh.AddTriangle(i0v, i1v, i2v);
                }
            }
        }

        #endregion
    }
}
