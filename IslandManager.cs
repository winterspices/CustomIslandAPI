using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CustomIslandAPI
{
    public static class IslandManager
    {
        public static Dictionary<int, string> sceneIndexes = new Dictionary<int, string>();

        public static void InitialiseIsland(GameObject island, int index, PortRegion region, Currency currency)
        {
            IslandMarket market = island.GetComponentInChildren<IslandMarket>(true);

            AddToTraderBoats(region, market);
            InitialiseShaders(island.transform);
            AddCargoCarrier(index, currency);
        }

        public static void AddToTraderBoats(PortRegion region, IslandMarket market)
        {
            String reg = "";

            switch (region)
            {
                case PortRegion.alankh:
                    reg = "A";
                    break;
                case PortRegion.emerald:
                    reg = "E";
                    break;
                case PortRegion.medi:
                    reg = "M";
                    break;
            }

            // find all trader boats
            for (int i = 1; i < 7; i++)
            {
                // expand their destination to include your island
                TraderBoat tb = GameObject.Find($"TraderBoat({reg}) ({i})").GetComponent<TraderBoat>();
                IslandMarket[] destinations = tb.destinations;

                Array.Resize(ref destinations, destinations.Length + 1);
                destinations[destinations.Length - 1] = market;
                tb.destinations = destinations;
            }
        }

        public static void InitialiseShaders(Transform parent)
        {
            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null && material.shader != null)
                    {
                        Shader shader = Shader.Find(material.shader.name);
                        material.shader = shader;
                    }
                }
            }
        }

        public static void AddCargoCarrier(int index, Currency currency)
        {
            GameObject obj = GameObject.Find("_shifting world/OVRPlayerController (observer)/head (cam)/OVRCameraRig/TrackingSpace/LeftHandAnchor/LeftControllerAnchor/touch grabber (left)/Go Pointer/cargo carriers");
            CargoCarrier component = obj.AddComponent<CargoCarrier>();
            component.portIndex = index;
            component.currency = currency;
            component.transportPriceMult = 2f;
            component.storagePriceMult = 1f;

            MethodInfo awake = AccessTools.Method(typeof(CargoCarrier), "Awake");
            awake.Invoke(component, null);
        }
    }
}
