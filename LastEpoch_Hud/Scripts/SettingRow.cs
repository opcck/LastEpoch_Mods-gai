using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts
{
    public static class SettingRow
    {
        public static void AddMultiplier(GameObject content, string source_panel, string panel_name, string toggle_from, string toggle_to, string slider_from, string slider_to, string label)
        {
            GameObject source = Functions.GetChild(content, source_panel, false);
            if (source.IsNullOrDestroyed() || source.transform.parent.IsNullOrDestroyed()) { return; }
            Transform parent = source.transform.parent;
            GameObject row = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).name == panel_name) { row = parent.GetChild(i).gameObject; break; }
            }
            if (row.IsNullOrDestroyed())
            {
                row = Object.Instantiate(source, parent);
                row.name = panel_name;
            }
            row.transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);
            Rename(row.transform, toggle_from, toggle_to);
            Rename(row.transform, slider_from, slider_to);
            Transform label_transform = Find(row.transform, "Label");
            if (label_transform != null)
            {
                Text label_text = label_transform.GetComponent<Text>();
                if (label_text.IsNullOrDestroyed()) { label_text = label_transform.GetComponentInChildren<Text>(true); }
                if (!label_text.IsNullOrDestroyed()) { label_text.text = label; }
            }
            RectTransform parent_rect = parent as RectTransform;
            if (!parent_rect.IsNullOrDestroyed()) { LayoutRebuilder.ForceRebuildLayoutImmediate(parent_rect); }
        }

        public static int Clamp(float value)
        {
            int multiplier = (int)value;
            if (multiplier < 1) { return 2; }
            if (multiplier > 10) { return 10; }
            return multiplier;
        }

        public static void PrepareSlider(Slider slider, float value)
        {
            if (slider.IsNullOrDestroyed()) { return; }
            slider.wholeNumbers = true;
            slider.minValue = 1f;
            slider.maxValue = 10f;
            slider.value = Clamp(value);
        }

        static void Rename(Transform root, string from, string to)
        {
            if (root.name == from) { root.name = to; }
            for (int i = 0; i < root.childCount; i++) { Rename(root.GetChild(i), from, to); }
        }

        static Transform Find(Transform root, string name)
        {
            if (root.name == name) { return root; }
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = Find(root.GetChild(i), name);
                if (found != null) { return found; }
            }
            return null;
        }
    }
}
