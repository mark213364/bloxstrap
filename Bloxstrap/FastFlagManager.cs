using Bloxstrap.Enums.FlagPresets;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        private Dictionary<string, object> OriginalProp = new();

        public override string ClassName => nameof(FastFlagManager);
        public override string LOG_IDENT_CLASS => ClassName;
        public override string FileName => "ClientAppSettings.json";
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings", FileName);

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            // === Rendering ===
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },

            // === Rendering mode (Vulkan / D3D11 / OpenGL) ===
            { "Rendering.Mode.Vulkan", "FFlagDebugGraphicsPreferVulkan" },
            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.OpenGL", "FFlagDebugGraphicsPreferOpenGL" },
            { "Rendering.Mode.DisableD3D11", "FFlagDebugGraphicsDisableDirect3D11" },

            // === Texture quality ===
            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },

            // === Performance — LOD ===
            { "Performance.CSGLevelOfDetail", "DFIntCSGLevelOfDetailSwitchingDistance" },
            { "Performance.CSGLevelOfDetailL12", "DFIntCSGLevelOfDetailSwitchingDistanceL12" },
            { "Performance.CSGLevelOfDetailL23", "DFIntCSGLevelOfDetailSwitchingDistanceL23" },
            { "Performance.CSGLevelOfDetailL34", "DFIntCSGLevelOfDetailSwitchingDistanceL34" },

            // === Performance — graphics override ===
            { "Performance.GraphicsQuality", "DFIntDebugFRMQualityLevelOverride" },

            // === Performance — grass ===
            { "Performance.GrassMaxDistance", "FIntFRMMaxGrassDistance" },
            { "Performance.GrassMinDistance", "FIntFRMMinGrassDistance" },
            { "Performance.GrassMotionReduced", "FIntGrassMovementReducedMotionFactor" },

            // === Performance — visual simplification ===
            { "Performance.PauseVoxelizer", "DFFlagDebugPauseVoxelizer" },
            { "Performance.GraySky", "FFlagDebugSkyGray" },

            // === Performance — FPS cap ===
            { "Performance.FpsCap", "DFIntTaskSchedulerTargetFps" },
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        public static IReadOnlyDictionary<GraphicsQuality, string?> GraphicsQualityLevels => new Dictionary<GraphicsQuality, string?>
        {
            { GraphicsQuality.Default, null },
            { GraphicsQuality.Level1, "1" },
            { GraphicsQuality.Level5, "5" },
            { GraphicsQuality.Level10, "10" },
            { GraphicsQuality.Level15, "15" },
            { GraphicsQuality.Level21, "21" },
        };

        public static IReadOnlyDictionary<RenderingMode, string?> RenderingModes => new Dictionary<RenderingMode, string?>
        {
            { RenderingMode.Default, null },
            { RenderingMode.Vulkan, "Vulkan" },
            { RenderingMode.D3D11, "D3D11" },
            { RenderingMode.OpenGL, "OpenGL" },
        };

        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");
                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (key == Prop[key].ToString())
                        return;
                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }
                Prop[key] = value.ToString()!;
            }
        }

        public string? GetValue(string key)
        {
            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();
            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
            {
                if (pair.Key.StartsWith($"{prefix}.{target}"))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name)
        {
            if (!PresetFlags.ContainsKey(name))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }
            return GetValue(PresetFlags[name]);
        }

        public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
        {
            foreach (var pair in mapping)
            {
                if (pair.Value == "None")
                    continue;
                if (GetPreset($"{prefix}.{pair.Value}") == value)
                    return pair.Key;
            }
            return mapping.First().Key;
        }

        public void ApplyPerformancePreset(PerformancePreset preset)
        {
            switch (preset)
            {
                case PerformancePreset.Default:
                    SetPreset("Performance", null);
                    break;
                case PerformancePreset.Balanced:
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistance", "300");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL12", "200");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL23", "300");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL34", "400");
                    SetValue("FIntFRMMaxGrassDistance", "200");
                    SetValue("DFIntTaskSchedulerTargetFps", "144");
                    SetValue("FFlagDebugSkyGray", "False");
                    break;
                case PerformancePreset.Performance:
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistance", "150");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL12", "100");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL23", "150");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL34", "200");
                    SetValue("FIntFRMMaxGrassDistance", "50");
                    SetValue("DFIntTaskSchedulerTargetFps", "240");
                    SetValue("FFlagDebugSkyGray", "True");
                    SetValue("DFFlagDebugPauseVoxelizer", "True");
                    break;
                case PerformancePreset.Potato:
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistance", "0");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL12", "0");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL23", "0");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL34", "0");
                    SetValue("FIntFRMMaxGrassDistance", "0");
                    SetValue("FIntFRMMinGrassDistance", "0");
                    SetValue("DFIntDebugFRMQualityLevelOverride", "1");
                    SetValue("FFlagDebugSkyGray", "True");
                    SetValue("DFFlagDebugPauseVoxelizer", "True");
                    SetValue("FIntGrassMovementReducedMotionFactor", "True");
                    break;
            }
            App.Logger.WriteLine("FastFlagManager::ApplyPerformancePreset", $"Applied preset: {preset}");
        }

        public override void Save()
        {
            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;
            base.Save();
            OriginalProp = new(Prop);
        }

        public override bool Load(bool alertFailure = true)
        {
            bool result = base.Load(alertFailure);
            OriginalProp = new(Prop);
            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");
            return result;
        }
    }
}
