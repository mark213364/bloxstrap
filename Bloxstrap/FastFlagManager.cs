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

        /// <summary>
        /// Все FastFlags, которые сейчас в белом списке Roblox.
        /// Флаги вне белого списка клиент молча игнорирует.
        /// </summary>
        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            // === Рендеринг — Fullscreen / DPI / MSAA ===
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },

            // === Качество текстур ===
            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },

            // === Производительность — детализация геометрии (LOD) ===
            // Снижение дистанции переключения LOD даёт заметный прирост FPS
            { "Performance.CSGLevelOfDetail", "DFIntCSGLevelOfDetailSwitchingDistance" },
            { "Performance.CSGLevelOfDetailL12", "DFIntCSGLevelOfDetailSwitchingDistanceL12" },
            { "Performance.CSGLevelOfDetailL23", "DFIntCSGLevelOfDetailSwitchingDistanceL23" },
            { "Performance.CSGLevelOfDetailL34", "DFIntCSGLevelOfDetailSwitchingDistanceL34" },

            // === Производительность — переопределение качества графики ===
            // Диапазон 0–21, можно выйти за стандартные рамки 1–10
            { "Performance.GraphicsQuality", "DFIntDebugFRMQualityLevelOverride" },

            // === Производительность — трава (отключение травы сильно поднимает FPS) ===
            { "Performance.GrassMaxDistance", "FIntFRMMaxGrassDistance" },
            { "Performance.GrassMinDistance", "FIntFRMMinGrassDistance" },
            { "Performance.GrassMotionReduced", "FIntGrassMovementReducedMotionFactor" },

            // === Производительность — упрощение визуала ===
            { "Performance.PauseVoxelizer", "DFFlagDebugPauseVoxelizer" },
            { "Performance.GraySky", "FFlagDebugSkyGray" },

            // === Производительность — контроль FPS ===
            // Целевой FPS движка. 0 = без лимита (но может быть ограничен другими механизмами)
            { "Performance.FpsCap", "DFIntTaskSchedulerTargetFps" },
        };

        // === Соответствия для Enum (нужны для UI) ===

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

        /// <summary>
        /// Уровни качества графики (0–21, чем ниже — тем выше FPS).
        /// </summary>
        public static IReadOnlyDictionary<GraphicsQuality, string?> GraphicsQualityLevels => new Dictionary<GraphicsQuality, string?>
        {
            { GraphicsQuality.Default, null },
            { GraphicsQuality.Level1, "1" },
            { GraphicsQuality.Level5, "5" },
            { GraphicsQuality.Level10, "10" },
            { GraphicsQuality.Level15, "15" },
            { GraphicsQuality.Level21, "21" },
        };

        // все fflag хранятся как строки
        // чтобы удалить флаг — передай null в качестве значения
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

        // возвращает null, если fflag не существует
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

        /// <summary>
        /// Применяет один из готовых пресетов производительности.
        /// Это основной метод оптимизации.
        /// </summary>
        public void ApplyPerformancePreset(PerformancePreset preset)
        {
            switch (preset)
            {
                case PerformancePreset.Default:
                    // Сбрасываем всё, что связано с производительностью
                    SetPreset("Performance", null);
                    break;

                case PerformancePreset.Balanced:
                    // Сбалансированный режим: умеренное снижение LOD, базовая визуальная часть сохраняется
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistance", "300");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL12", "200");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL23", "300");
                    SetValue("DFIntCSGLevelOfDetailSwitchingDistanceL34", "400");
                    SetValue("FIntFRMMaxGrassDistance", "200");
                    SetValue("DFIntTaskSchedulerTargetFps", "144");
                    SetValue("FFlagDebugSkyGray", "False");
                    break;

                case PerformancePreset.Performance:
                    // Режим производительности: заметное снижение детализации, подходит для средних ПК
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
                    // Режим «картошка»: минимальная графика, максимум FPS
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
            // перед сохранением приводим все значения флагов к строкам
            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // клонируем словарь
            OriginalProp = new(Prop);
        }

        public override bool Load(bool alertFailure = true)
        {
            bool result = base.Load(alertFailure);

            // клонируем словарь
            OriginalProp = new(Prop);

            // Оригинальная логика Bloxstrap: принудительно ManualFullscreen = False
            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");

            return result;
        }
    }
}
