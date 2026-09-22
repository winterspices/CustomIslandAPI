using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CustomIslandAPI
{
    [BepInPlugin("com.winter.customislandapi", "Custom Island API", "1.0.2")]
    public class IslandPatcher : BaseUnityPlugin
    {
        public const string pluginGuid = "com.winter.customislandapi";
        public const string pluginName = "Custom Island API";
        public const string pluginVersion = "1.0.2";

        private void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.winter.customislandapi");
        }
    }
}
