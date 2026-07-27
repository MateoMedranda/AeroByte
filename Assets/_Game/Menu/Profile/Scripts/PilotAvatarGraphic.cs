using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.Profile
{
    [ExecuteAlways, RequireComponent(typeof(CanvasRenderer))]
    public sealed class PilotAvatarGraphic : MaskableGraphic
    {
        [SerializeField, Range(0, 5)] private int avatarId;

        public int AvatarId
        {
            get => avatarId;
            set
            {
                avatarId = Mathf.Clamp(value, 0, 5);
                SetVerticesDirty();
            }
        }

        public static string GetAvatarName(int id)
        {
            return Mathf.Clamp(id, 0, 5) switch
            {
                0 => "CÓNDOR",
                1 => "ÁGUILA",
                2 => "ANDINO",
                3 => "NOCTURNO",
                4 => "RESCATE",
                _ => "INSTRUCTOR"
            };
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float size = Mathf.Min(rect.width, rect.height);
            Vector2 center = rect.center;
            GetPalette(avatarId, out Color primary, out Color secondary, out Color visor);

            AddCircle(vh, center, size * 0.48f, new Color(0.015f, 0.07f, 0.11f, 1f), 32);
            AddRing(vh, center, size * 0.46f, size * 0.42f, new Color(primary.r, primary.g, primary.b, 0.75f), 32);

            Vector2 shouldersCenter = center + new Vector2(0f, -size * 0.34f);
            AddTrapezoid(vh, shouldersCenter, size * 0.64f, size * 0.40f, size * 0.24f, secondary);

            Vector2 headCenter = center + new Vector2(0f, size * 0.08f);
            AddCircle(vh, headCenter, size * 0.29f, primary, 28);
            AddArc(vh, headCenter + new Vector2(0f, size * 0.03f), size * 0.31f, 15f, 165f, size * 0.055f, secondary, 16);

            Rect visorRect = new Rect(headCenter.x - size * 0.23f, headCenter.y - size * 0.03f, size * 0.46f, size * 0.17f);
            AddRoundedBar(vh, visorRect, visor);
            AddRoundedBar(vh, new Rect(headCenter.x - size * 0.10f, headCenter.y - size * 0.20f, size * 0.20f, size * 0.13f), new Color(0.035f, 0.09f, 0.12f, 1f));
            AddLine(vh, headCenter + new Vector2(-size * 0.10f, -size * 0.13f), headCenter + new Vector2(-size * 0.24f, -size * 0.26f), size * 0.025f, new Color(0.45f, 0.62f, 0.70f, 1f));
            AddLine(vh, headCenter + new Vector2(size * 0.10f, -size * 0.13f), headCenter + new Vector2(size * 0.24f, -size * 0.26f), size * 0.025f, new Color(0.45f, 0.62f, 0.70f, 1f));

            AddCircle(vh, center + new Vector2(size * 0.31f, size * 0.30f), size * 0.045f, new Color(0.40f, 0.95f, 1f, 1f), 12);
        }

        private static void GetPalette(int id, out Color primary, out Color secondary, out Color visor)
        {
            switch (Mathf.Clamp(id, 0, 5))
            {
                case 1:
                    primary = new Color(0.95f, 0.52f, 0.12f, 1f);
                    secondary = new Color(0.28f, 0.12f, 0.05f, 1f);
                    visor = new Color(0.10f, 0.28f, 0.34f, 1f);
                    break;
                case 2:
                    primary = new Color(0.08f, 0.68f, 0.60f, 1f);
                    secondary = new Color(0.02f, 0.20f, 0.22f, 1f);
                    visor = new Color(0.05f, 0.18f, 0.24f, 1f);
                    break;
                case 3:
                    primary = new Color(0.42f, 0.30f, 0.78f, 1f);
                    secondary = new Color(0.09f, 0.06f, 0.20f, 1f);
                    visor = new Color(0.30f, 0.15f, 0.42f, 1f);
                    break;
                case 4:
                    primary = new Color(0.90f, 0.18f, 0.20f, 1f);
                    secondary = new Color(0.26f, 0.04f, 0.06f, 1f);
                    visor = new Color(0.12f, 0.24f, 0.30f, 1f);
                    break;
                case 5:
                    primary = new Color(0.72f, 0.78f, 0.82f, 1f);
                    secondary = new Color(0.16f, 0.20f, 0.24f, 1f);
                    visor = new Color(0.06f, 0.18f, 0.25f, 1f);
                    break;
                default:
                    primary = new Color(0.08f, 0.50f, 0.88f, 1f);
                    secondary = new Color(0.02f, 0.16f, 0.28f, 1f);
                    visor = new Color(0.05f, 0.24f, 0.34f, 1f);
                    break;
            }
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

        private static void AddRing(VertexHelper vh, Vector2 center, float outer, float inner, Color tint, int segments)
        {
            int start = vh.currentVertCount;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vh.AddVert(center + direction * outer, tint, Vector2.zero);
                vh.AddVert(center + direction * inner, tint, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
            {
                int index = start + i * 2;
                vh.AddTriangle(index, index + 2, index + 3);
                vh.AddTriangle(index, index + 3, index + 1);
            }
        }

        private static void AddTrapezoid(VertexHelper vh, Vector2 center, float bottomWidth, float topWidth, float height, Color tint)
        {
            int index = vh.currentVertCount;
            vh.AddVert(center + new Vector2(-bottomWidth * 0.5f, -height * 0.5f), tint, Vector2.zero);
            vh.AddVert(center + new Vector2(-topWidth * 0.5f, height * 0.5f), tint, Vector2.zero);
            vh.AddVert(center + new Vector2(topWidth * 0.5f, height * 0.5f), tint, Vector2.zero);
            vh.AddVert(center + new Vector2(bottomWidth * 0.5f, -height * 0.5f), tint, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static void AddRoundedBar(VertexHelper vh, Rect rect, Color tint)
        {
            float radius = rect.height * 0.5f;
            AddCircle(vh, new Vector2(rect.xMin + radius, rect.center.y), radius, tint, 12);
            AddCircle(vh, new Vector2(rect.xMax - radius, rect.center.y), radius, tint, 12);
            AddQuad(vh, new Vector2(rect.xMin + radius, rect.yMin), new Vector2(rect.xMin + radius, rect.yMax), new Vector2(rect.xMax - radius, rect.yMax), new Vector2(rect.xMax - radius, rect.yMin), tint);
        }

        private static void AddArc(VertexHelper vh, Vector2 center, float radius, float startDegrees, float endDegrees, float width, Color tint, int segments)
        {
            Vector2 previous = center + Direction(startDegrees) * radius;
            for (int i = 1; i <= segments; i++)
            {
                Vector2 current = center + Direction(Mathf.Lerp(startDegrees, endDegrees, i / (float)segments)) * radius;
                AddLine(vh, previous, current, width, tint);
                previous = current;
            }
        }

        private static void AddLine(VertexHelper vh, Vector2 from, Vector2 to, float width, Color tint)
        {
            Vector2 normal = new Vector2(-(to.y - from.y), to.x - from.x).normalized * width * 0.5f;
            AddQuad(vh, from - normal, from + normal, to + normal, to - normal, tint);
        }

        private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            int index = vh.currentVertCount;
            vh.AddVert(a, tint, Vector2.zero);
            vh.AddVert(b, tint, Vector2.zero);
            vh.AddVert(c, tint, Vector2.zero);
            vh.AddVert(d, tint, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static Vector2 Direction(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
