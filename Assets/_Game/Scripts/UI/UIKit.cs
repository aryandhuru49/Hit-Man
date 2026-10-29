using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ExitClause.UI
{
    /// <summary>
    /// Tiny helper library that builds uGUI elements from code, so every screen works without
    /// prefabs or imported fonts. Replace with designed prefabs later if you like.
    /// </summary>
    public static class UIKit
    {
        static Font s_font;
        public static Font Font => s_font != null ? s_font : (s_font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public static readonly Color PanelColor = new Color(0.05f, 0.06f, 0.08f, 0.88f);
        public static readonly Color ButtonColor = new Color(0.16f, 0.18f, 0.22f, 0.95f);
        public static readonly Color ButtonHover = new Color(0.75f, 0.55f, 0.2f, 1f);
        public static readonly Color Accent = new Color(0.95f, 0.72f, 0.3f);
        public static readonly Color TextColor = new Color(0.92f, 0.92f, 0.9f);
        public static readonly Color Dim = new Color(0.6f, 0.6f, 0.6f);

        public static Canvas CreateCanvas(string name, int sortOrder, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Anchors in 0..1 screen fractions, plus pixel offsets.</summary>
        public static RectTransform Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        /// <summary>Fixed-size element anchored at a point.</summary>
        public static RectTransform PlaceFixed(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Image FullScreen(Transform parent, string name, Color color)
        {
            var img = Panel(parent, name, color);
            Place(img.rectTransform, Vector2.zero, Vector2.one);
            return img;
        }

        public static Text Label(Transform parent, string text, int size = 22, TextAnchor align = TextAnchor.MiddleLeft, Color? color = null, FontStyle style = FontStyle.Normal)
        {
            var rt = Rect(parent, "Label");
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color ?? TextColor;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.7f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        public static Button Button(Transform parent, string label, Action onClick, float height = 52f, int fontSize = 24)
        {
            var img = Panel(parent, "Button_" + label, ButtonColor);
            var le = img.gameObject.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = height;
            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.6f, 1.3f, 0.8f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.selectedColor = new Color(1.4f, 1.2f, 0.8f);
            colors.colorMultiplier = 1.5f;
            btn.colors = colors;
            var text = Label(img.transform, label, fontSize, TextAnchor.MiddleCenter);
            Place(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            if (onClick != null) btn.onClick.AddListener(new UnityAction(onClick));
            return btn;
        }

        public static void SetButtonLabel(Button b, string label) => b.GetComponentInChildren<Text>().text = label;

        public static RectTransform VerticalLayout(Transform parent, string name, float spacing = 10f, int padding = 16, bool fitHeight = false)
        {
            var rt = Rect(parent, name);
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            if (fitHeight)
            {
                var f = rt.gameObject.AddComponent<ContentSizeFitter>();
                f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            return rt;
        }

        public static RectTransform HorizontalLayout(Transform parent, string name, float spacing = 10f, float height = 52f)
        {
            var rt = Rect(parent, name);
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childControlHeight = true;
            h.childControlWidth = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = height;
            return rt;
        }

        /// <summary>Text that sizes itself inside a layout group.</summary>
        public static Text LayoutLabel(Transform parent, string text, int size = 22, TextAnchor align = TextAnchor.MiddleLeft, Color? color = null, FontStyle style = FontStyle.Normal)
        {
            var t = Label(parent, text, size, align, color, style);
            var f = t.gameObject.AddComponent<ContentSizeFitter>();
            f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return t;
        }

        public static ScrollRect ScrollList(Transform parent, string name, out RectTransform content)
        {
            var bg = Panel(parent, name, new Color(0, 0, 0, 0.25f));
            var scroll = bg.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.scrollSensitivity = 30f;
            var viewport = Panel(bg.transform, "Viewport", new Color(1, 1, 1, 0.01f));
            Place(viewport.rectTransform, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            content = VerticalLayout(viewport.transform, "Content", 8f, 10, true);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            scroll.viewport = viewport.rectTransform;
            scroll.content = content;
            return scroll;
        }

        public static Slider Slider(Transform parent, string label, float min, float max, float value, Action<float> onChange)
        {
            var row = HorizontalLayout(parent, "Row_" + label, 12f, 44f);
            LayoutLabel(row, label, 22);
            var bg = Panel(row, "Slider", new Color(0.2f, 0.2f, 0.22f));
            var slider = bg.gameObject.AddComponent<Slider>();
            var fillArea = Rect(bg.transform, "FillArea");
            Place(fillArea, Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
            var fill = Panel(fillArea, "Fill", Accent);
            Place(fill.rectTransform, Vector2.zero, Vector2.one);
            var handleArea = Rect(bg.transform, "HandleArea");
            Place(handleArea, Vector2.zero, Vector2.one);
            var handle = Panel(handleArea, "Handle", Color.white);
            handle.rectTransform.sizeDelta = new Vector2(14, 0);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(v => onChange(v));
            return slider;
        }

        public static Toggle Toggle(Transform parent, string label, bool value, Action<bool> onChange)
        {
            var row = HorizontalLayout(parent, "Row_" + label, 12f, 44f);
            LayoutLabel(row, label, 22);
            var box = Panel(row, "Box", new Color(0.2f, 0.2f, 0.22f));
            var le = box.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = 44; le.flexibleWidth = 0;
            var check = Panel(box.transform, "Check", Accent);
            Place(check.rectTransform, Vector2.zero, Vector2.one, new Vector2(8, 8), new Vector2(-8, -8));
            var toggle = box.gameObject.AddComponent<Toggle>();
            toggle.graphic = check;
            toggle.targetGraphic = box;
            toggle.isOn = value;
            toggle.onValueChanged.AddListener(v => onChange(v));
            return toggle;
        }

        public static InputField InputField(Transform parent, string placeholder, InputField.ContentType type)
        {
            var bg = Panel(parent, "Input", new Color(0.12f, 0.12f, 0.14f));
            var le = bg.gameObject.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = 48;
            var text = Label(bg.transform, "", 24);
            Place(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(12, 0), new Vector2(-12, 0));
            text.raycastTarget = false;
            var ph = Label(bg.transform, placeholder, 24, TextAnchor.MiddleLeft, Dim, FontStyle.Italic);
            Place(ph.rectTransform, Vector2.zero, Vector2.one, new Vector2(12, 0), new Vector2(-12, 0));
            var input = bg.gameObject.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = ph;
            input.contentType = type;
            return input;
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        /// <summary>Centered dialog window used by most screens.</summary>
        public static RectTransform Window(Transform parent, string title, Vector2 size, out RectTransform body)
        {
            var win = Panel(parent, "Window_" + title, PanelColor);
            PlaceFixed(win.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            var header = Label(win.transform, title.ToUpperInvariant(), 34, TextAnchor.MiddleLeft, Accent, FontStyle.Bold);
            Place(header.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(28, -70), new Vector2(-28, -10));
            var line = Panel(win.transform, "Rule", Accent);
            Place(line.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(28, -74), new Vector2(-28, -72));
            body = Rect(win.transform, "Body");
            Place(body, Vector2.zero, Vector2.one, new Vector2(24, 20), new Vector2(-24, -86));
            return win.rectTransform;
        }
    }
}
