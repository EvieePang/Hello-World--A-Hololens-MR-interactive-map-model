using UnityEngine;

public class AutoCountryColorizer : MonoBehaviour
{
    void Start()
    {
        ColorizeAllCountries();
    }

    void ColorizeAllCountries()
    {
        foreach (Transform country in transform)
        {
            Renderer renderer = country.GetComponent<Renderer>();
            if (renderer == null) continue;

            // 创建材质实例
            Material matInstance = renderer.material;

            // 获取国家颜色
            Color color = GenerateColorFromName(country.name);

            // 设置颜色
            matInstance.color = color;
        }

        Debug.Log("🌈 Auto coloring completed (Pastel + Noise).");
    }

    // ============================================================
    // ★ 美观配色算法（Pastel Hue + Noise）
    // ============================================================
    Color GenerateColorFromName(string name)
    {
        int hash = Mathf.Abs(name.GetHashCode());

        // 基础 hue 均匀分布
        float hue = (hash % 360) / 360f;

        // 增加轻微噪声，避免相邻国家同色
        float noise = ((hash / 7) % 100) / 500f; // 0 ~ 0.2
        hue = Mathf.Repeat(hue + noise, 1f);

        // Pastel（粉彩）风格的饱和度与亮度
        float saturation = 0.35f + (((hash / 13) % 10) / 100f); // 0.35 ~ 0.45
        float value = 0.92f; // 明亮但柔和

        return Color.HSVToRGB(hue, saturation, value);
    }
}
