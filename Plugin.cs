using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CircletDemister
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class CircletDemisterPlugin : BaseUnityPlugin
    {
        internal const string ModName = "CircletDemister";
        internal const string ModVersion = "1.0.2";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource CircletDemisterLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        internal static ParticleSystemForceField? DemisterParticleFf = null!;
        internal static ParticleSystemForceField? PlayerParticleFf = null!;

        public void Awake()
        {
            DemisterMaxRange = Config.Bind("1 - General", "End Range", 3.5f, new ConfigDescription("The end range of the demister effect on the circlet. Capped to be weaker than the Mistlands Demister Orb (Wisp) that has a range of 6", new AcceptableValueRange<float>(0f, 5f)));
            DemisterMaxRange.SettingChanged += (_, _) => UpdateDemisterRange(null!, null!);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
            try
            {
                if (DemisterParticleFf != null)
                    Destroy(DemisterParticleFf);
                if (PlayerParticleFf != null)
                    Destroy(PlayerParticleFf);
            }
            catch
            {
                // eh, we tried.
            }
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                CircletDemisterLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                CircletDemisterLogger.LogError($"There was an issue loading your {ConfigFileName}");
                CircletDemisterLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        private void UpdateDemisterRange(object sender, EventArgs e)
        {
            if (PlayerParticleFf != null)
                PlayerParticleFf.endRange = DemisterMaxRange.Value;
        }

        private static ConfigEntry<float> DemisterMaxRange { get; set; } = null!;
    }
}