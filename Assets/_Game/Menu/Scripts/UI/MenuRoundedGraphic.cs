using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.UI
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuRoundedGraphic : MaskableGraphic
    {
        [SerializeField] private Color topColor = Color.white;
        [SerializeField] private Color bottomColor = Color.white;
        [SerializeField, Min(0f)] private float cornerRadius = 18f;
        [SerializeField, Min(0f)] private float borderWidth;
        [SerializeField] private Color borderColor = Color.clear;

        private const int CornerSegments = 8;

        public void SetStyle(Color top, Color bottom, float radius, Color border, float width)
        {
            topColor = top;
            bottomColor = bottom;
            cornerRadius = radius;
            borderColor = border;
            borderWidth = width;
            SetVerticesDirty();
        }

        public void SetFill(Color top, Color bottom)
        {
            topColor = top;
            bottomColor = bottom;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            var rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(cornerRadius, Mathf.Min(rect.width, rect.height) * 0.5f);
            var outer = BuildPerimeter(rect, radius);
            AddFill(vertexHelper, outer, rect);

            if (borderWidth > 0.01f && borderColor.a > 0.001f)
            {
                float width = Mathf.Min(borderWidth, Mathf.Min(rect.width, rect.height) * 0.45f);
                var innerRect = new Rect(rect.x + width, rect.y + width, rect.width - width * 2f, rect.height - width * 2f);
                var inner = BuildPerimeter(innerRect, Mathf.Max(0f, radius - width));
                AddBorder(vertexHelper, outer, inner);
            }
        }

        private void AddFill(VertexHelper vertexHelper, List<Vector2> perimeter, Rect rect)
        {
            int centerIndex = vertexHelper.currentVertCount;
            var centerColor = Color.Lerp(bottomColor, topColor, 0.5f) * color;
            vertexHelper.AddVert(rect.center, centerColor, new Vector2(0.5f, 0.5f));

            for (int i = 0; i < perimeter.Count; i++)
            {
                Vector2 point = perimeter[i];
                float gradient = Mathf.InverseLerp(rect.yMin, rect.yMax, point.y);
                vertexHelper.AddVert(point, Color.Lerp(bottomColor, topColor, gradient) * color, Vector2.zero);
            }

            for (int i = 0; i < perimeter.Count; i++)
            {
                vertexHelper.AddTriangle(centerIndex, centerIndex + 1 + i, centerIndex + 1 + ((i + 1) % perimeter.Count));
            }
        }

        private void AddBorder(VertexHelper vertexHelper, List<Vector2> outer, List<Vector2> inner)
        {
            int start = vertexHelper.currentVertCount;
            Color tint = borderColor * color;
            for (int i = 0; i < outer.Count; i++)
            {
                vertexHelper.AddVert(outer[i], tint, Vector2.zero);
                vertexHelper.AddVert(inner[i], tint, Vector2.zero);
            }

            for (int i = 0; i < outer.Count; i++)
            {
                int next = (i + 1) % outer.Count;
                int outerCurrent = start + i * 2;
                int innerCurrent = outerCurrent + 1;
                int outerNext = start + next * 2;
                int innerNext = outerNext + 1;
                vertexHelper.AddTriangle(outerCurrent, outerNext, innerNext);
                vertexHelper.AddTriangle(outerCurrent, innerNext, innerCurrent);
            }
        }

        private static List<Vector2> BuildPerimeter(Rect rect, float radius)
        {
            var points = new List<Vector2>(CornerSegments * 4);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMax - radius), radius, 0f, 90f);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMax - radius), radius, 90f, 180f);
            AddCorner(points, new Vector2(rect.xMin + radius, rect.yMin + radius), radius, 180f, 270f);
            AddCorner(points, new Vector2(rect.xMax - radius, rect.yMin + radius), radius, 270f, 360f);
            return points;
        }

        private static void AddCorner(List<Vector2> points, Vector2 center, float radius, float startAngle, float endAngle)
        {
            for (int i = 0; i < CornerSegments; i++)
            {
                float angle = Mathf.Lerp(startAngle, endAngle, i / (float)(CornerSegments - 1)) * Mathf.Deg2Rad;
                points.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }
    }
}
