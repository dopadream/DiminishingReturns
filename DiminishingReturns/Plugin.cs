﻿using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JackEhttack.patch;
using JackEhttack.service;
using UnityEngine;

namespace JackEhttack;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; set; }

    public ManualLogSource Log => Instance.Logger;

    public static MoonTracker Service;
    public AssetBundle MainAssetBundle;
    public static new DRConfig Config { get; private set; }

    public Plugin()
    {
        Instance = this;
    }
    
    private static void NetcodePatcher()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    private void Awake()
    {
        Service = new MoonTracker();
        Config = new DRConfig(base.Config);
        
        var dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
        var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "DiminishingReturns.assets");
        MainAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

        NetcodePatcher();
        
        Log.LogInfo($"Applying patches...");
        Harmony.CreateAndPatchAll(typeof(ScrapModifierPatches));
        Harmony.CreateAndPatchAll(typeof(TerminalPatches));
        Harmony.CreateAndPatchAll(typeof(NetworkObjectManager));
        Log.LogInfo($"Applied all patches!");
        
    }

}
