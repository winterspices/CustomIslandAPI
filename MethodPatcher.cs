using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomIslandAPI
{
    internal class MethodPatcher
    {
        [HarmonyPatch(typeof(IslandHorizon))]
        public class Patch_IslandHorizon
        {
            [HarmonyPrefix]
            [HarmonyPatch("RegisterIsland")]
            public static bool RegisterIslandPatch(IslandHorizon __instance)
            {
                if (Refs.islands == null)
                {
                    Refs.islands = new Transform[100];
                }
                Refs.islands[__instance.islandIndex] = __instance.transform;

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("LoadIslandScene")]
            public static bool LoadIslandScenePatch(IslandHorizon __instance, ref float ___loadUnloadCoolown, ref bool ___sceneLoaded)
            {
                if (IslandManager.sceneIndexes.ContainsKey(__instance.islandIndex))
                {
                    ___loadUnloadCoolown = 12f;
                    Debug.Log(string.Concat(new object[]
                    {
                        "Loading scene ",
                        __instance.islandIndex,
                        ". sceneLoaded is ",
                        ___sceneLoaded.ToString()
                    }));
                    ___sceneLoaded = true;
                    SceneManager.LoadSceneAsync(IslandManager.sceneIndexes[__instance.islandIndex], LoadSceneMode.Additive);
                    GameState.loadingScenes++;

                    __instance.StartCoroutine(RegisterLoadingFinished(IslandManager.sceneIndexes[__instance.islandIndex]));

                    return false;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("UnloadIslandScene")]
            public static bool UnloadIslandScenePatch(IslandHorizon __instance, ref float ___loadUnloadCoolown, ref bool ___unloading)
            {
                if (IslandManager.sceneIndexes.ContainsKey(__instance.islandIndex))
                {
                    ___loadUnloadCoolown = 12f;
                    Debug.Log("Unloading scene " + __instance.islandIndex);
                    if (___unloading)
                    {
                        return false;
                    }
                    ___unloading = true;
                    __instance.StartCoroutine(DoUnloadScene(IslandManager.sceneIndexes[__instance.islandIndex], __instance));

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(IslandDistanceTracker))]
        public class Patch_IslandDistanceTracker
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateDistance")]
            public static bool UpdateDistancePatch(IslandDistanceTracker __instance, ref IEnumerator __result, ref bool ___updating)
            {
                __result = InjectorCoroutine(__instance, ___updating);
                return false;
            }

            private static IEnumerator InjectorCoroutine(IslandDistanceTracker __instance, bool ___updating)
            {
                List<IslandHorizon> islands = new List<IslandHorizon>(__instance.islands);

                float closestDistance = 100000000f;

                foreach (IslandHorizon islandHorizon in islands)
                {
                    float num = Vector3.Distance(islandHorizon.GetPosition(), Refs.observerMirror.transform.position);
                    if (num < closestDistance)
                    {
                        closestDistance = num;
                    }
                    yield return new WaitForEndOfFrame();
                }

                List<IslandHorizon>.Enumerator enumerator = default(List<IslandHorizon>.Enumerator);

                GameState.distanceToLand = closestDistance;

                FieldInfo field = AccessTools.Field(typeof(IslandDistanceTracker), "updating");
                field.SetValue(__instance, false);

                yield break;
                yield break;
            }
        }

        [HarmonyPatch(typeof(IslandMarket))]
        public class Patch_IslandMarket
        {
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static bool AwakePatch(IslandMarket __instance, ref Port ___port, ref IslandMarketWarehouseArea ___warehouseArea, ref float ___econTimer)
            {
                ___port = __instance.GetComponent<Port>();
                ___warehouseArea = __instance.GetComponent<IslandMarketWarehouseArea>();
                __instance.currentSupply = (__instance.production.Clone() as float[]);
                __instance.currentPlayerGoods = new int[__instance.production.Length];
                ___econTimer = 1f;
                __instance.knownPrices = new PriceReport[100];
                __instance.knownPrices[__instance.GetPortIndex()] = new PriceReport();

                return false;
            }
        }

        [HarmonyPatch(typeof(Port))]
        public class Patch_Port
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            public static bool StartPatch(Port __instance, ref Vector3 ___initialPos)
            {
                ___initialPos = __instance.transform.localPosition;
                if (Port.ports == null)
                {
                    Port.ports = new Port[100];
                }
                Port.ports[__instance.portIndex] = __instance;

                return false;
            }
        }

        [HarmonyPatch(typeof(TraderBoat))]
        public class Patch_TraderBoat
        {
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static bool AwakePatch(TraderBoat __instance)
            {
                __instance.carriedGoods = new int[__instance.goodsCapacity];
                __instance.carriedPriceReports = new PriceReport[100];

                return false;
            }
        }

        [HarmonyPatch(typeof(CargoCarrier))]
        public class Patch_CargoCarrier
        {
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static bool AwakePatch(CargoCarrier __instance)
            {
                if (__instance.portIndex == 0 && __instance.transportPriceMult == 0f && __instance.storagePriceMult == 0f)
                {
                    // custom island
                    return false;
                }

                if (CargoCarrier.carriers == null)
                {
                    CargoCarrier.carriers = new CargoCarrier[100];
                }
                CargoCarrier.carriers[__instance.portIndex] = __instance;
                __instance.cargo = new List<ShipItem>();
                Sun.OnNewDay += __instance.RegisterDayPassed;

                return false;
            }
        }

        public static IEnumerator RegisterLoadingFinished(string path)
        {
            while (!SceneManager.GetSceneByPath(path).isLoaded)
            {
                yield return new WaitForEndOfFrame();
            }
            GameState.loadingScenes--;

            // change all shaders
            Scene scene = SceneManager.GetSceneByPath(path);

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                IslandManager.InitialiseShaders(root.transform);
            }

            yield break;
        }

        public static IEnumerator DoUnloadScene(string path, IslandHorizon instance)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(path);
            while (unload != null && !unload.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();

            FieldInfo field = AccessTools.Field(typeof(IslandHorizon), "sceneLoaded");
            field.SetValue(instance, false);
            FieldInfo field2 = AccessTools.Field(typeof(IslandHorizon), "unloading");
            field2.SetValue(instance, false);

            yield break;
        }
    }
}
