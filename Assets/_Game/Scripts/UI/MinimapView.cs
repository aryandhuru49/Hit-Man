using System.Collections.Generic;
using ExitClause.AI;
using ExitClause.CameraSystem;
using ExitClause.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Vice City style circular radar. When a scene loads it renders the level once from above into a
    /// texture (one camera render, no per-frame cost), then pans and rotates that image with the camera
    /// and overlays blips for enemies and the active objective.
    /// </summary>
    public class MinimapView : MonoBehaviour
    {
        public float worldRadius = 70f;
        const float Size = 230f;

        RectTransform m_frame, m_rotator, m_playerArrow;
        RawImage m_map;
        RenderTexture m_rt;
        Vector2 m_worldMin;
        float m_worldSize = 1f;
        readonly List<Image> m_blips = new List<Image>();
        Image m_objectiveBlip;
        bool m_captured;

        public void Build(RectTransform hud)
        {
            var mask = UIKit.Panel(hud, "Radar", new Color(0.1f, 0.12f, 0.1f, 0.85f));
            mask.sprite = CircleSprite();
            UIKit.PlaceFixed(mask.rectTransform, Vector2.zero, Vector2.zero, new Vector2(40 + Size / 2, 40 + Size / 2), new Vector2(Size, Size));
            mask.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            m_frame = mask.rectTransform;

            m_rotator = UIKit.Rect(mask.transform, "Rotator");
            UIKit.PlaceFixed(m_rotator, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size, Size));
            var mapGo = UIKit.Rect(m_rotator, "Map");
            UIKit.PlaceFixed(mapGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Size, Size) * 1.5f);
            m_map = mapGo.gameObject.AddComponent<RawImage>();
            m_map.color = new Color(0.8f, 0.9f, 0.8f, 0.9f);
            m_map.enabled = false;

            m_objectiveBlip = UIKit.Panel(m_rotator, "ObjectiveBlip", new Color(1f, 0.85f, 0.2f));
            m_objectiveBlip.rectTransform.sizeDelta = new Vector2(14, 14);

            m_playerArrow = UIKit.Panel(mask.transform, "Player", Color.white).rectTransform;
            UIKit.PlaceFixed(m_playerArrow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10, 16));

            var ring = UIKit.Panel(hud, "RadarRing", new Color(0, 0, 0, 0));
            UIKit.PlaceFixed(ring.rectTransform, Vector2.zero, Vector2.zero, new Vector2(40 + Size / 2, 40 + Size / 2), new Vector2(Size + 6, Size + 6));
            ring.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            SceneManager.sceneLoaded += (_, __) => m_captured = false;
        }

        public void SetVisible(bool v)
        {
            if (m_frame.gameObject.activeSelf != v) m_frame.gameObject.SetActive(v);
        }

        void LateUpdate()
        {
            var p = PlayerCharacter.Instance;
            var rig = GameCameraRig.Instance;
            if (p == null || rig == null) return;
            if (!m_captured) Capture();

            float camYaw = rig.MainCamera.transform.eulerAngles.y;
            m_rotator.localRotation = Quaternion.Euler(0, 0, camYaw);
            float playerYaw = p.CurrentVehicle != null ? p.CurrentVehicle.transform.eulerAngles.y : p.transform.eulerAngles.y;
            m_playerArrow.localRotation = Quaternion.Euler(0, 0, camYaw - playerYaw);

            Vector3 pp = p.transform.position;
            if (m_map.enabled)
            {
                float r = worldRadius * 1.5f;
                float u = (pp.x - m_worldMin.x) / m_worldSize;
                float v = (pp.z - m_worldMin.y) / m_worldSize;
                float w = 2f * r / m_worldSize;
                m_map.uvRect = new Rect(u - w / 2f, v - w / 2f, w, w);
            }

            int used = 0;
            foreach (var e in EnemyAI.All)
            {
                if (e == null || e.IsDead) continue;
                Vector3 d = e.transform.position - pp;
                if (d.magnitude > worldRadius * 1.4f) continue;
                if (used >= m_blips.Count)
                {
                    var b = UIKit.Panel(m_rotator, "Blip", Color.red);
                    b.rectTransform.sizeDelta = new Vector2(9, 9);
                    m_blips.Add(b);
                }
                var blip = m_blips[used++];
                blip.gameObject.SetActive(true);
                blip.rectTransform.anchoredPosition = Clamp(new Vector2(d.x, d.z) / worldRadius * (Size / 2f));
                blip.color = e.State == EnemyState.Combat ? Color.red : e.Perception.Suspicion > 0.3f ? Color.yellow : new Color(0.85f, 0.35f, 0.35f);
            }
            for (int i = used; i < m_blips.Count; i++) m_blips[i].gameObject.SetActive(false);

            var marker = HUDView.ActiveMarker();
            m_objectiveBlip.gameObject.SetActive(marker != null);
            if (marker != null)
            {
                Vector3 d = marker.transform.position - pp;
                m_objectiveBlip.rectTransform.anchoredPosition = Clamp(new Vector2(d.x, d.z) / worldRadius * (Size / 2f));
            }
        }

        static Vector2 Clamp(Vector2 v)
        {
            float max = Size / 2f - 8f;
            return v.magnitude > max ? v.normalized * max : v;
        }

        void Capture()
        {
            m_captured = true;
            var bounds = new Bounds();
            bool any = false;
            foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
            {
                if (!r.gameObject.isStatic) continue;
                if (!any) { bounds = r.bounds; any = true; } else bounds.Encapsulate(r.bounds);
            }
            if (!any) { m_map.enabled = false; return; }
            float size = Mathf.Min(Mathf.Max(bounds.size.x, bounds.size.z), 1200f);
            m_worldSize = size;
            m_worldMin = new Vector2(bounds.center.x - size / 2f, bounds.center.z - size / 2f);

            if (m_rt == null) m_rt = new RenderTexture(1024, 1024, 24) { name = "RadarCapture" };
            var go = new GameObject("RadarCaptureCam");
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = size / 2f;
            cam.transform.SetPositionAndRotation(new Vector3(bounds.center.x, bounds.max.y + 50f, bounds.center.z), Quaternion.Euler(90, 0, 0));
            cam.nearClipPlane = 1f;
            cam.farClipPlane = bounds.size.y + 200f;
            cam.cullingMask = 1 << 0;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.15f, 0.18f, 0.2f);
            cam.targetTexture = m_rt;
            cam.enabled = false;
            try { cam.Render(); m_map.texture = m_rt; m_map.enabled = true; }
            catch (System.Exception ex) { Debug.LogWarning("[Minimap] Capture failed: " + ex.Message); m_map.enabled = false; }
            cam.targetTexture = null;
            Destroy(go);
        }

        static Sprite s_circle;
        public static Sprite CircleSprite()
        {
            if (s_circle != null) return s_circle;
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1f - d) * 30f)));
                }
            tex.Apply();
            s_circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
            return s_circle;
        }
    }
}
