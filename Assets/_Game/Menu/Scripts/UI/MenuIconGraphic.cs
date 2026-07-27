using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.UI
{
    public enum MenuIconType
    {
        Play,
        Settings,
        Credits,
        Exit,
        Sound,
        Controls,
        University,
        Tools,
        Project,
        Calendar,
        Back,
        Mute
    }

    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuIconGraphic : MaskableGraphic
    {
        [SerializeField] private MenuIconType iconType;
        [SerializeField, Range(1f, 8f)] private float strokeWidth = 2.5f;

        public MenuIconType IconType
        {
            get => iconType;
            set
            {
                iconType = value;
                SetVerticesDirty();
            }
        }

        public void Configure(MenuIconType type, Color tint, float stroke = 2.5f)
        {
            iconType = type;
            color = tint;
            strokeWidth = stroke;
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float scale = Mathf.Min(rect.width, rect.height);
            Vector2 center = rect.center;
            float half = scale * 0.5f;
            float stroke = strokeWidth * Mathf.Max(0.75f, scale / 32f);

            switch (iconType)
            {
                case MenuIconType.Play:
                    AddTriangle(vh, center + new Vector2(-half * 0.28f, half * 0.55f), center + new Vector2(-half * 0.28f, -half * 0.55f), center + new Vector2(half * 0.58f, 0f));
                    break;
                case MenuIconType.Settings:
                    AddRing(vh, center, half * 0.42f, half * 0.23f, 20);
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * Mathf.PI / 4f;
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                        AddLine(vh, center + direction * half * 0.43f, center + direction * half * 0.68f, stroke * 1.35f);
                    }
                    break;
                case MenuIconType.Credits:
                    AddCircle(vh, center + new Vector2(-half * 0.28f, half * 0.25f), half * 0.19f, 14);
                    AddCircle(vh, center + new Vector2(half * 0.30f, half * 0.17f), half * 0.16f, 14);
                    AddArc(vh, center + new Vector2(-half * 0.27f, -half * 0.34f), half * 0.45f, 25f, 155f, stroke, 12);
                    AddArc(vh, center + new Vector2(half * 0.28f, -half * 0.30f), half * 0.34f, 25f, 155f, stroke, 10);
                    break;
                case MenuIconType.Exit:
                    AddArc(vh, center, half * 0.58f, -48f, 228f, stroke, 22);
                    AddLine(vh, center + new Vector2(0f, half * 0.72f), center + new Vector2(0f, half * 0.05f), stroke * 1.25f);
                    break;
                case MenuIconType.Sound:
                case MenuIconType.Mute:
                    AddSpeaker(vh, center, half, stroke);
                    if (iconType == MenuIconType.Mute)
                    {
                        AddLine(vh, center + new Vector2(half * 0.22f, half * 0.28f), center + new Vector2(half * 0.68f, -half * 0.28f), stroke);
                        AddLine(vh, center + new Vector2(half * 0.22f, -half * 0.28f), center + new Vector2(half * 0.68f, half * 0.28f), stroke);
                    }
                    else
                    {
                        AddArc(vh, center + new Vector2(half * 0.02f, 0f), half * 0.48f, -52f, 52f, stroke, 8);
                        AddArc(vh, center + new Vector2(half * 0.02f, 0f), half * 0.72f, -48f, 48f, stroke, 9);
                    }
                    break;
                case MenuIconType.Controls:
                    AddRectOutline(vh, new Rect(center.x - half * 0.70f, center.y - half * 0.45f, half * 1.4f, half * 0.9f), stroke);
                    for (int x = -2; x <= 2; x++) AddCircle(vh, center + new Vector2(x * half * 0.22f, half * 0.17f), stroke * 0.55f, 8);
                    AddLine(vh, center + new Vector2(-half * 0.44f, -half * 0.17f), center + new Vector2(half * 0.44f, -half * 0.17f), stroke);
                    break;
                case MenuIconType.University:
                    AddTriangleOutline(vh, center + new Vector2(0f, half * 0.68f), center + new Vector2(-half * 0.72f, half * 0.18f), center + new Vector2(half * 0.72f, half * 0.18f), stroke);
                    for (int i = -2; i <= 2; i += 2) AddLine(vh, center + new Vector2(i * half * 0.24f, half * 0.12f), center + new Vector2(i * half * 0.24f, -half * 0.52f), stroke);
                    AddLine(vh, center + new Vector2(-half * 0.72f, -half * 0.57f), center + new Vector2(half * 0.72f, -half * 0.57f), stroke * 1.3f);
                    break;
                case MenuIconType.Tools:
                    AddLine(vh, center + new Vector2(-half * 0.50f, -half * 0.55f), center + new Vector2(half * 0.48f, half * 0.53f), stroke * 1.5f);
                    AddLine(vh, center + new Vector2(half * 0.50f, -half * 0.55f), center + new Vector2(-half * 0.48f, half * 0.53f), stroke * 1.5f);
                    AddCircle(vh, center + new Vector2(-half * 0.52f, -half * 0.57f), stroke * 1.2f, 10);
                    break;
                case MenuIconType.Project:
                    AddRectOutline(vh, new Rect(center.x - half * 0.68f, center.y - half * 0.48f, half * 1.36f, half * 0.92f), stroke);
                    AddLine(vh, center + new Vector2(-half * 0.68f, half * 0.45f), center + new Vector2(-half * 0.20f, half * 0.45f), stroke);
                    AddLine(vh, center + new Vector2(-half * 0.20f, half * 0.45f), center + new Vector2(-half * 0.04f, half * 0.62f), stroke);
                    AddLine(vh, center + new Vector2(-half * 0.04f, half * 0.62f), center + new Vector2(half * 0.28f, half * 0.62f), stroke);
                    break;
                case MenuIconType.Calendar:
                    AddRectOutline(vh, new Rect(center.x - half * 0.62f, center.y - half * 0.58f, half * 1.24f, half * 1.1f), stroke);
                    AddLine(vh, center + new Vector2(-half * 0.62f, half * 0.20f), center + new Vector2(half * 0.62f, half * 0.20f), stroke);
                    AddLine(vh, center + new Vector2(-half * 0.30f, half * 0.68f), center + new Vector2(-half * 0.30f, half * 0.38f), stroke);
                    AddLine(vh, center + new Vector2(half * 0.30f, half * 0.68f), center + new Vector2(half * 0.30f, half * 0.38f), stroke);
                    break;
                case MenuIconType.Back:
                    AddLine(vh, center + new Vector2(half * 0.38f, half * 0.58f), center + new Vector2(-half * 0.30f, 0f), stroke * 1.4f);
                    AddLine(vh, center + new Vector2(-half * 0.30f, 0f), center + new Vector2(half * 0.38f, -half * 0.58f), stroke * 1.4f);
                    break;
            }
        }

        private void AddSpeaker(VertexHelper vh, Vector2 center, float half, float stroke)
        {
            AddRect(vh, new Rect(center.x - half * 0.62f, center.y - half * 0.20f, half * 0.34f, half * 0.40f));
            AddTriangle(vh, center + new Vector2(-half * 0.31f, half * 0.22f), center + new Vector2(-half * 0.31f, -half * 0.22f), center + new Vector2(half * 0.10f, -half * 0.52f));
            AddTriangle(vh, center + new Vector2(-half * 0.31f, half * 0.22f), center + new Vector2(half * 0.10f, half * 0.52f), center + new Vector2(half * 0.10f, -half * 0.52f));
        }

        private void AddRect(VertexHelper vh, Rect rect)
        {
            AddQuad(vh, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin));
        }

        private void AddRectOutline(VertexHelper vh, Rect rect, float stroke)
        {
            AddLine(vh, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax), stroke);
            AddLine(vh, new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMax, rect.yMax), stroke);
            AddLine(vh, new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), stroke);
            AddLine(vh, new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMin, rect.yMin), stroke);
        }

        private void AddTriangleOutline(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, float stroke)
        {
            AddLine(vh, a, b, stroke);
            AddLine(vh, b, c, stroke);
            AddLine(vh, c, a, stroke);
        }

        private void AddLine(VertexHelper vh, Vector2 from, Vector2 to, float width)
        {
            Vector2 normal = new Vector2(-(to.y - from.y), to.x - from.x).normalized * width * 0.5f;
            AddQuad(vh, from - normal, from + normal, to + normal, to - normal);
        }

        private void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c)
        {
            int index = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
        }

        private void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            int index = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddVert(d, color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private void AddCircle(VertexHelper vh, Vector2 center, float radius, int segments)
        {
            int centerIndex = vh.currentVertCount;
            vh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, color, Vector2.zero);
            }
            for (int i = 0; i < segments; i++) vh.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + i + 2);
        }

        private void AddRing(VertexHelper vh, Vector2 center, float outerRadius, float innerRadius, int segments)
        {
            int start = vh.currentVertCount;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(center + direction * outerRadius, color, Vector2.zero);
                vh.AddVert(center + direction * innerRadius, color, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
            {
                int index = start + i * 2;
                vh.AddTriangle(index, index + 2, index + 3);
                vh.AddTriangle(index, index + 3, index + 1);
            }
        }

        private void AddArc(VertexHelper vh, Vector2 center, float radius, float startDegrees, float endDegrees, float width, int segments)
        {
            Vector2 previous = center + Direction(startDegrees) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Lerp(startDegrees, endDegrees, i / (float)segments);
                Vector2 current = center + Direction(angle) * radius;
                AddLine(vh, previous, current, width);
                previous = current;
            }
        }

        private static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
