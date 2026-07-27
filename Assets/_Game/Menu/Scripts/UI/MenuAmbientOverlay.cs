using UnityEngine;
using UnityEngine.UI;

namespace AeroByte.Menu.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuAmbientOverlay : MaskableGraphic
    {
        private readonly Vector2[] _points =
        {
            new Vector2(0.53f, 0.20f), new Vector2(0.61f, 0.27f), new Vector2(0.70f, 0.18f),
            new Vector2(0.77f, 0.31f), new Vector2(0.84f, 0.23f), new Vector2(0.90f, 0.37f),
            new Vector2(0.57f, 0.43f), new Vector2(0.66f, 0.52f), new Vector2(0.75f, 0.46f),
            new Vector2(0.86f, 0.55f), new Vector2(0.94f, 0.48f), new Vector2(0.49f, 0.67f)
        };

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        private void Update()
        {
            if (Application.isPlaying) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float time = Application.isPlaying ? Time.unscaledTime : 0f;
            for (int i = 0; i < _points.Length; i++)
            {
                Vector2 normalized = _points[i];
                float drift = Mathf.Sin(time * (0.25f + i * 0.013f) + i * 1.7f);
                Vector2 center = new Vector2(rect.xMin + normalized.x * rect.width, rect.yMin + normalized.y * rect.height + drift * 9f);
                float size = 1.4f + (i % 3) * 0.7f;
                float alpha = 0.10f + (Mathf.Sin(time * 0.8f + i) * 0.5f + 0.5f) * 0.22f;
                AddDiamond(vh, center, size, new Color(color.r, color.g, color.b, alpha * color.a));
            }
        }

        private static void AddDiamond(VertexHelper vh, Vector2 center, float size, Color tint)
        {
            int index = vh.currentVertCount;
            vh.AddVert(center + Vector2.up * size, tint, Vector2.zero);
            vh.AddVert(center + Vector2.right * size, tint, Vector2.zero);
            vh.AddVert(center + Vector2.down * size, tint, Vector2.zero);
            vh.AddVert(center + Vector2.left * size, tint, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
