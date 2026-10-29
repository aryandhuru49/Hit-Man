using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace ExitClause.EditorTools
{
    /// <summary>
    /// Applies a MoodPreset to the open scene: sun/moon directional light, a global HDRP Volume
    /// (sky, fog, auto exposure, tonemapping, bloom, vignette, colour grading) and optional rain.
    /// Volume profiles are saved under Assets/_Game/Settings/Volumes so you can tweak them in the Inspector.
    /// </summary>
    public static class LightingBuilder
    {
        public static void Apply(MoodPreset mood, Camera mainCamera)
        {
            var root = new GameObject("Lighting");

            var sunGo = new GameObject(mood.sunLux < 10 ? "Moon" : "Sun");
            sunGo.transform.SetParent(root.transform);
            sunGo.transform.rotation = Quaternion.Euler(mood.sunEuler);
            sunGo.AddHDLight(LightType.Directional);
            var sun = sunGo.GetComponent<Light>();
            sun.lightUnit = LightUnit.Lux;
            sun.intensity = mood.sunLux;
            sun.useColorTemperature = true;
            sun.colorTemperature = mood.sunTemperature;
            sun.shadows = LightShadows.Soft;

            var volGo = new GameObject("Global Volume");
            volGo.transform.SetParent(root.transform);
            var vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = BuildProfile(mood);

            if (mood.rain && mainCamera != null) AddRain(mainCamera.transform);
        }

        static VolumeProfile BuildProfile(MoodPreset mood)
        {
            Directory.CreateDirectory("Assets/_Game/Settings/Volumes");
            string path = $"Assets/_Game/Settings/Volumes/{mood.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            var env = Add<VisualEnvironment>(profile);
            env.skyType.Override(mood.physicalSky ? (int)SkyType.PhysicallyBased : (int)SkyType.Gradient);
            env.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            if (mood.physicalSky)
            {
                Add<PhysicallyBasedSky>(profile);
            }
            else
            {
                var g = Add<GradientSky>(profile);
                g.top.Override(mood.gradientTop);
                g.middle.Override(mood.gradientMiddle);
                g.bottom.Override(mood.gradientBottom);
            }

            var fog = Add<Fog>(profile);
            fog.enabled.Override(true);
            fog.meanFreePath.Override(mood.fogDistance);
            fog.maximumHeight.Override(mood.fogHeight);
            fog.tint.Override(mood.fogTint);
            fog.enableVolumetricFog.Override(mood.volumetricFog);

            var exp = Add<Exposure>(profile);
            exp.mode.Override(ExposureMode.Automatic);
            exp.limitMin.Override(mood.exposureMin);
            exp.limitMax.Override(mood.exposureMax);
            exp.compensation.Override(mood.exposureCompensation);

            var tone = Add<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.ACES);

            var bloom = Add<Bloom>(profile);
            bloom.intensity.Override(mood.bloom);

            var vig = Add<Vignette>(profile);
            vig.intensity.Override(mood.vignette);

            var ca = Add<ColorAdjustments>(profile);
            ca.saturation.Override(mood.saturation);
            ca.contrast.Override(mood.contrast);
            ca.colorFilter.Override(mood.colorFilter);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var c = profile.Add<T>(true);
            c.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(c, profile);
            return c;
        }

        static void AddRain(Transform camera)
        {
            var go = new GameObject("Rain");
            go.transform.SetParent(camera, false);
            go.transform.localPosition = new Vector3(0, 12, 6);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 18f;
            main.startSize3D = true;
            main.startSizeX = 0.02f;
            main.startSizeY = 0.5f;
            main.startSizeZ = 0.02f;
            main.maxParticles = 4000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 2500;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            // Rotated 90 on X so drops emit downward: local Z becomes world -Y, so the thin axis must be Z
            // (a 40 m tall box used to spawn drops below roofs, inside rooms).
            shape.scale = new Vector3(40, 40, 1);
            shape.rotation = new Vector3(90, 0, 0);
            // Drops die when they hit a roof or awning, so it never rains indoors.
            var col = ps.collision;
            col.enabled = true;
            col.type = ParticleSystemCollisionType.World;
            col.mode = ParticleSystemCollisionMode.Collision3D;
            col.quality = ParticleSystemCollisionQuality.Medium;
            col.collidesWith = (1 << ExitClause.Systems.Layers.Default) | (1 << ExitClause.Systems.Layers.Props) | (1 << ExitClause.Systems.Layers.Vehicle);
            col.lifetimeLoss = 1f;
            col.radiusScale = 0.5f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.04f;
            r.lengthScale = 1f;
            r.sharedMaterial = MaterialLibrary.Get("Rain");
            r.shadowCastingMode = ShadowCastingMode.Off;
            go.AddComponent<ExitClause.CameraSystem.RainShelter>();
        }

        /// <summary>Street lamp, neon tube or chandelier light. Shadows off to stay cheap.</summary>
        public static Light PointLight(Transform parent, Vector3 pos, Color color, float lumens, float range)
        {
            var go = new GameObject("PointLight");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.AddHDLight(LightType.Point);
            var l = go.GetComponent<Light>();
            l.lightUnit = LightUnit.Lumen;
            l.intensity = lumens;
            l.color = color;
            l.range = range;
            l.shadows = LightShadows.None;
            return l;
        }
    }
}
