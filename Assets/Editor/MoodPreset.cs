using UnityEngine;

namespace ExitClause.EditorTools
{
    /// <summary>Lighting and post-processing recipe for one location's look.</summary>
    public class MoodPreset
    {
        public string name;
        public bool physicalSky = true;
        public Color gradientTop, gradientMiddle, gradientBottom;
        public Vector3 sunEuler = new Vector3(50, 30, 0);
        public float sunLux = 100000f;
        public float sunTemperature = 6500f;
        public float fogDistance = 600f;
        public float fogHeight = 60f;
        public Color fogTint = Color.white;
        public bool volumetricFog;
        // Auto-exposure limits in EV100. Night scenes play-tested well at about 1.5 to 8: at -3 a mostly black frame drove
        // exposure so high that lamp-lit walls burned out to white, and at 4 the streets were too dark to read.
        public float exposureMin = 10f, exposureMax = 15f, exposureCompensation;
        public float bloom = 0.2f;
        public float vignette = 0.25f;
        public float saturation;
        public float contrast = 10f;
        public Color colorFilter = Color.white;
        public bool rain;

        public static MoodPreset PragueNight => new MoodPreset
        {
            name = "PragueNight", physicalSky = false,
            gradientTop = new Color(0.02f, 0.025f, 0.05f), gradientMiddle = new Color(0.06f, 0.07f, 0.1f), gradientBottom = new Color(0.08f, 0.07f, 0.06f),
            sunEuler = new Vector3(35, -40, 0), sunLux = 2f, sunTemperature = 9000,
            fogDistance = 70, fogHeight = 25, fogTint = new Color(0.6f, 0.65f, 0.75f), volumetricFog = true,
            exposureMin = 1.5f, exposureMax = 8,
            bloom = 0.35f, vignette = 0.35f, saturation = -15, contrast = 18, colorFilter = new Color(0.85f, 0.92f, 1f), rain = true
        };

        public static MoodPreset RivieraGolden => new MoodPreset
        {
            name = "RivieraGolden", sunEuler = new Vector3(9, -65, 0), sunLux = 20000, sunTemperature = 3600,
            fogDistance = 900, fogHeight = 80, fogTint = new Color(1f, 0.85f, 0.7f),
            exposureMin = 8, exposureMax = 13, bloom = 0.3f, vignette = 0.2f, saturation = 12, contrast = 12, colorFilter = new Color(1f, 0.95f, 0.88f)
        };

        public static MoodPreset RivieraDay => new MoodPreset
        {
            name = "RivieraDay", sunEuler = new Vector3(48, -40, 0), sunLux = 90000, sunTemperature = 5600,
            fogDistance = 1200, fogHeight = 100, exposureMin = 11, exposureMax = 15, saturation = 10, contrast = 8
        };

        public static MoodPreset RivieraNight => new MoodPreset
        {
            name = "RivieraNight", physicalSky = false,
            gradientTop = new Color(0.01f, 0.015f, 0.04f), gradientMiddle = new Color(0.04f, 0.05f, 0.09f), gradientBottom = new Color(0.08f, 0.06f, 0.05f),
            sunEuler = new Vector3(40, 120, 0), sunLux = 2f, sunTemperature = 8000, fogDistance = 200, fogHeight = 40,
            exposureMin = 1.5f, exposureMax = 8, bloom = 0.35f, vignette = 0.3f, contrast = 15
        };

        public static MoodPreset AlpsCold => new MoodPreset
        {
            name = "AlpsCold", sunEuler = new Vector3(28, 150, 0), sunLux = 110000, sunTemperature = 7600,
            fogDistance = 450, fogHeight = 120, fogTint = new Color(0.8f, 0.9f, 1f),
            exposureMin = 12, exposureMax = 16, exposureCompensation = -0.5f, saturation = -20, contrast = 14, colorFilter = new Color(0.9f, 0.95f, 1f)
        };

        public static MoodPreset MarseilleDay => new MoodPreset
        {
            name = "MarseilleDay", sunEuler = new Vector3(55, -20, 0), sunLux = 100000, sunTemperature = 5200,
            fogDistance = 700, fogHeight = 60, fogTint = new Color(1f, 0.95f, 0.85f),
            exposureMin = 11, exposureMax = 15, saturation = 5, contrast = 12
        };

        public static MoodPreset BerlinNeon => new MoodPreset
        {
            name = "BerlinNeon", physicalSky = false,
            gradientTop = new Color(0.02f, 0.01f, 0.03f), gradientMiddle = new Color(0.06f, 0.03f, 0.08f), gradientBottom = new Color(0.05f, 0.05f, 0.06f),
            sunEuler = new Vector3(40, 60, 0), sunLux = 1.5f, sunTemperature = 10000,
            fogDistance = 90, fogHeight = 30, fogTint = new Color(0.8f, 0.7f, 1f), volumetricFog = true,
            exposureMin = 1.5f, exposureMax = 8, bloom = 0.5f, vignette = 0.3f, saturation = 10, contrast = 22, colorFilter = new Color(0.95f, 0.9f, 1f)
        };

        public static MoodPreset ZurichMarble => new MoodPreset
        {
            name = "ZurichMarble", sunEuler = new Vector3(14, 200, 0), sunLux = 25000, sunTemperature = 4300,
            fogDistance = 800, fogHeight = 80, exposureMin = 5, exposureMax = 13, bloom = 0.25f, vignette = 0.2f, saturation = -5, contrast = 15
        };

        public static MoodPreset SafehouseEvening => new MoodPreset
        {
            name = "SafehouseEvening", physicalSky = false,
            gradientTop = new Color(0.05f, 0.06f, 0.12f), gradientMiddle = new Color(0.3f, 0.18f, 0.12f), gradientBottom = new Color(0.1f, 0.08f, 0.07f),
            sunEuler = new Vector3(5, -80, 0), sunLux = 800, sunTemperature = 3000, fogDistance = 300, fogHeight = 50,
            exposureMin = 0, exposureMax = 8, bloom = 0.3f, vignette = 0.35f, contrast = 14, colorFilter = new Color(1f, 0.92f, 0.85f)
        };
    }
}
