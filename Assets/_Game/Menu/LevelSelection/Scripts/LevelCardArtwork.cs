using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.LevelSelection
{
    public enum LevelTheme
    {
        Beach,
        City,
        Desert,
        Forest
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LevelCardArtwork : MaskableGraphic
    {
        [SerializeField] private LevelTheme theme;
        [SerializeField, Range(0f, 1f)] private float focus;

        public void Configure(LevelTheme levelTheme)
        {
            theme = levelTheme;
            raycastTarget = false;
            SetVerticesDirty();
        }

        public void SetFocus(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(focus, value)) return;
            focus = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();

            switch (theme)
            {
                case LevelTheme.Beach:
                    DrawBeach(vh, rect);
                    break;
                case LevelTheme.City:
                    DrawCity(vh, rect);
                    break;
                case LevelTheme.Desert:
                    DrawDesert(vh, rect);
                    break;
                case LevelTheme.Forest:
                    DrawForest(vh, rect);
                    break;
            }
        }

        private void DrawBeach(VertexHelper vh, Rect rect)
        {
            AddGradient(vh, rect, Tint(new Color(0.20f, 0.72f, 0.96f)), Tint(new Color(0.03f, 0.30f, 0.62f)));
            AddCircle(vh, Point(rect, 0.77f, 0.78f), rect.width * 0.10f, Tint(new Color(1f, 0.88f, 0.48f)), 20);
            AddQuad(vh, new Rect(rect.xMin, rect.yMin, rect.width, rect.height * 0.47f), Tint(new Color(0.02f, 0.44f, 0.67f)));
            AddTriangle(vh, Point(rect, 0f, 0.44f), Point(rect, 0.46f, 0.61f), Point(rect, 0.82f, 0.44f), Tint(new Color(0.05f, 0.34f, 0.25f)));
            AddTriangle(vh, Point(rect, 0.18f, 0.43f), Point(rect, 0.62f, 0.57f), Point(rect, 1f, 0.42f), Tint(new Color(0.12f, 0.48f, 0.30f)));
            AddTriangle(vh, Point(rect, 0f, 0f), Point(rect, 0.34f, 0.36f), Point(rect, 0.52f, 0f), Tint(new Color(0.93f, 0.78f, 0.48f)));
            AddLine(vh, Point(rect, 0.06f, 0.27f), Point(rect, 0.48f, 0.08f), rect.width * 0.018f, Tint(new Color(0.88f, 0.98f, 1f)));
        }

        private void DrawCity(VertexHelper vh, Rect rect)
        {
            AddGradient(vh, rect, Tint(new Color(0.21f, 0.66f, 0.92f)), Tint(new Color(0.03f, 0.14f, 0.30f)));
            float[] heights = { 0.44f, 0.62f, 0.52f, 0.78f, 0.58f, 0.70f, 0.48f };
            for (int i = 0; i < heights.Length; i++)
            {
                float width = rect.width / heights.Length;
                var building = new Rect(rect.xMin + i * width + 3f, rect.yMin, width - 6f, rect.height * heights[i]);
                AddGradient(vh, building, Tint(new Color(0.09f, 0.31f, 0.50f)), Tint(new Color(0.015f, 0.08f, 0.17f)));
                for (int row = 1; row < 6; row++)
                {
                    for (int column = 0; column < 2; column++)
                    {
                        float x = building.xMin + building.width * (0.28f + column * 0.44f);
                        float y = building.yMin + building.height * (0.12f + row * 0.12f);
                        AddQuad(vh, new Rect(x, y, building.width * 0.12f, building.height * 0.035f), Tint(new Color(0.52f, 0.88f, 1f, 0.72f)));
                    }
                }
            }
        }

        private void DrawDesert(VertexHelper vh, Rect rect)
        {
            AddGradient(vh, rect, Tint(new Color(1f, 0.76f, 0.30f)), Tint(new Color(0.66f, 0.20f, 0.06f)));
            AddCircle(vh, Point(rect, 0.72f, 0.76f), rect.width * 0.12f, Tint(new Color(1f, 0.92f, 0.62f)), 20);
            AddTriangle(vh, Point(rect, 0.03f, 0.38f), Point(rect, 0.25f, 0.69f), Point(rect, 0.48f, 0.38f), Tint(new Color(0.56f, 0.20f, 0.08f)));
            AddQuad(vh, new Rect(rect.xMin + rect.width * 0.15f, rect.yMin + rect.height * 0.37f, rect.width * 0.20f, rect.height * 0.12f), Tint(new Color(0.58f, 0.22f, 0.08f)));
            AddTriangle(vh, Point(rect, 0f, 0f), Point(rect, 0.35f, 0.38f), Point(rect, 0.72f, 0f), Tint(new Color(0.93f, 0.48f, 0.10f)));
            AddTriangle(vh, Point(rect, 0.35f, 0f), Point(rect, 0.74f, 0.32f), Point(rect, 1f, 0f), Tint(new Color(0.72f, 0.28f, 0.06f)));
        }

        private void DrawForest(VertexHelper vh, Rect rect)
        {
            AddGradient(vh, rect, Tint(new Color(0.36f, 0.72f, 0.92f)), Tint(new Color(0.02f, 0.16f, 0.20f)));
            AddTriangle(vh, Point(rect, 0f, 0.34f), Point(rect, 0.28f, 0.82f), Point(rect, 0.58f, 0.34f), Tint(new Color(0.32f, 0.48f, 0.56f)));
            AddTriangle(vh, Point(rect, 0.30f, 0.32f), Point(rect, 0.66f, 0.90f), Point(rect, 1f, 0.32f), Tint(new Color(0.46f, 0.62f, 0.70f)));
            AddTriangle(vh, Point(rect, 0.49f, 0.62f), Point(rect, 0.66f, 0.90f), Point(rect, 0.82f, 0.62f), Tint(new Color(0.91f, 0.97f, 1f)));
            AddQuad(vh, new Rect(rect.xMin, rect.yMin, rect.width, rect.height * 0.31f), Tint(new Color(0.02f, 0.22f, 0.14f)));
            for (int i = 0; i < 7; i++)
            {
                float x = 0.06f + i * 0.15f;
                float height = 0.22f + (i % 3) * 0.055f;
                AddTriangle(vh, Point(rect, x - 0.09f, 0.05f), Point(rect, x, height), Point(rect, x + 0.09f, 0.05f), Tint(new Color(0.02f, 0.34f + i * 0.012f, 0.20f)));
            }
        }

        private Color Tint(Color source)
        {
            return Color.Lerp(source * 0.82f, source * 1.12f, focus) * color;
        }

        private static Vector2 Point(Rect rect, float x, float y) => new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, x), Mathf.Lerp(rect.yMin, rect.yMax, y));

        private static void AddGradient(VertexHelper vh, Rect rect, Color top, Color bottom)
        {
            int start = vh.currentVertCount;
            vh.AddVert(new Vector2(rect.xMin, rect.yMin), bottom, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMin, rect.yMax), top, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMax, rect.yMax), top, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMax, rect.yMin), bottom, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void AddQuad(VertexHelper vh, Rect rect, Color tint) => AddGradient(vh, rect, tint, tint);

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color tint)
        {
            int start = vh.currentVertCount;
            vh.AddVert(a, tint, Vector2.zero);
            vh.AddVert(b, tint, Vector2.zero);
            vh.AddVert(c, tint, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        private static void AddCircle(VertexHelper vh, Vector2 center, float radius, Color tint, int segments)
        {
            int start = vh.currentVertCount;
            vh.AddVert(center, tint, Vector2.zero);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, tint, Vector2.zero);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(start, start + i + 1, start + i + 2);
        }

        private static void AddLine(VertexHelper vh, Vector2 from, Vector2 to, float width, Color tint)
        {
            Vector2 normal = new Vector2(-(to.y - from.y), to.x - from.x).normalized * width * 0.5f;
            int start = vh.currentVertCount;
            vh.AddVert(from - normal, tint, Vector2.zero);
            vh.AddVert(from + normal, tint, Vector2.zero);
            vh.AddVert(to + normal, tint, Vector2.zero);
            vh.AddVert(to - normal, tint, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
