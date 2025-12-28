/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace TSEspionage
{
    /// <summary>
    /// BepInEx plugin entry point for TSEspionage.
    /// This replaces the old Doorstop-based injection.
    /// </summary>
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource Log;
        private static Harmony _harmony;
        private static bool _initialized = false;

        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo($"Loading {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}...");

            // Register custom MonoBehaviour types with Il2Cpp
            RegisterCustomTypes();

            // Initialize the mod systems
            InitializeMod();

            Log.LogInfo($"{PluginInfo.PLUGIN_NAME} loaded successfully!");
        }

        /// <summary>
        /// Register custom MonoBehaviour classes with Il2Cpp's ClassInjector.
        /// This is required for any custom Unity components we add to GameObjects.
        /// </summary>
        private void RegisterCustomTypes()
        {
            Log.LogInfo("Registering custom IL2CPP types...");

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<CardCountManager>();
                Log.LogDebug("  - CardCountManager registered");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to register CardCountManager: {ex}");
            }

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<CardTabBehaviour>();
                Log.LogDebug("  - CardTabBehaviour registered");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to register CardTabBehaviour: {ex}");
            }

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<RegionControlBar>();
                Log.LogDebug("  - RegionControlBar registered");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to register RegionControlBar: {ex}");
            }

            Log.LogInfo("Custom types registered.");
        }

        /// <summary>
        /// Initialize the mod's core systems and apply Harmony patches.
        /// </summary>
        private void InitializeMod()
        {
            if (_initialized)
            {
                Log.LogWarning("Mod already initialized, skipping...");
                return;
            }

            Log.LogInfo("Initializing mod systems...");

            var gameLogWriter = new GameLogWriter("");
            var gameEventHandler = new GameEventHandler(gameLogWriter);

            // Initialize the patch classes with their dependencies
            GameLogPatches.Init(gameLogWriter, new BepInExUnityLogger(Log));
            LoadLevelSplashScreenPatches.Init();
            TwilightStrugglePatches.Init(gameEventHandler);
            UI_SettingsMenuPatches.Init();

            // Apply Harmony patches
            Log.LogInfo("Applying Harmony patches...");
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();

            _initialized = true;
            Log.LogInfo("Mod initialization complete!");
        }
    }

    /// <summary>
    /// Plugin metadata constants.
    /// </summary>
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "com.twilight-struggle.TSEspionage";
        public const string PLUGIN_NAME = "TSEspionage";
        public const string PLUGIN_VERSION = "0.2.0";
    }

    /// <summary>
    /// A wrapper around BepInEx's ManualLogSource to provide Unity's ILogger interface.
    /// This allows existing code that expects Unity's logger to work with BepInEx logging.
    /// </summary>
    public class BepInExUnityLogger : UnityEngine.ILogger
    {
        private readonly ManualLogSource _log;

        public BepInExUnityLogger(ManualLogSource log)
        {
            _log = log;
        }

        public UnityEngine.ILogHandler logHandler { get; set; }
        public bool logEnabled { get; set; } = true;
        public UnityEngine.LogType filterLogType { get; set; } = UnityEngine.LogType.Log;

        public bool IsLogTypeAllowed(UnityEngine.LogType logType) => logEnabled;

        public void Log(UnityEngine.LogType logType, object message)
        {
            if (!logEnabled) return;
            
            switch (logType)
            {
                case UnityEngine.LogType.Error:
                case UnityEngine.LogType.Exception:
                case UnityEngine.LogType.Assert:
                    _log.LogError(message?.ToString());
                    break;
                case UnityEngine.LogType.Warning:
                    _log.LogWarning(message?.ToString());
                    break;
                default:
                    _log.LogInfo(message?.ToString());
                    break;
            }
        }

        public void Log(UnityEngine.LogType logType, object message, UnityEngine.Object context)
        {
            Log(logType, message);
        }

        public void Log(UnityEngine.LogType logType, string tag, object message)
        {
            Log(logType, $"[{tag}] {message}");
        }

        public void Log(UnityEngine.LogType logType, string tag, object message, UnityEngine.Object context)
        {
            Log(logType, tag, message);
        }

        public void Log(object message)
        {
            Log(UnityEngine.LogType.Log, message);
        }

        public void LogError(string tag, object message)
        {
            Log(UnityEngine.LogType.Error, tag, message);
        }

        public void LogException(System.Exception exception)
        {
            _log.LogError(exception.ToString());
        }

        public void LogException(System.Exception exception, UnityEngine.Object context)
        {
            LogException(exception);
        }

        public void LogFormat(UnityEngine.LogType logType, string format, params object[] args)
        {
            Log(logType, string.Format(format, args));
        }

        public void LogFormat(UnityEngine.LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            LogFormat(logType, format, args);
        }

        public void LogWarning(string tag, object message)
        {
            Log(UnityEngine.LogType.Warning, tag, message);
        }
    }
}
