# Custom Island API

This is a project intended for use by mod authors who wish to add custom islands without having to go through the faff of patching the base game themselves. If you are not a mod author, do not download this unless a mod requires it in its dependencies.

Please note that this API is not complete and will have bugs and/or updates. If you have any requests do not hesitate to reach out.

## Instructions for use

Reference the .dll in your visual studio project, you will have access to the `IslandManager` class. This can handle both the loading of the scenery, as well as the terrain. Upon loading the island prefab, use `InitialiseIsland()` to create an entry. This takes four arguments:

- `GameObject island` : the island prefab loaded using `Object.Instantiate()`
- `int index` : the index of your island and port
- `PortRegion region` : the region of the world that your port is based in
- `Currency currency` : the currency of your port

Example:
```
island = UnityEngine.Object.Instantiate<GameObject>(islandAsset, __instance.transform);
IslandManager.InitialiseIsland(island, 67, PortRegion.emerald, Currency.emerald);
```

To load the scenery you need only reference the path to the Unity scene in your AssetBundle. We can add to the `IslandManager`'s `sceneIndexes` list like so:
`IslandManager.sceneIndexes.Add(index, path)`

Example:
```
IslandManager.sceneIndexes.Add(67, "Assets/Better Ports/Scenes/island 67 Bottleneck.unity");
```

Finally, you will need to patch the class `Port` from your mod. At this version, I have not yet implemented a way to handle adding destinations to the `IslandManager`, so you must patch it yourself.

We will patch the `Start()` method from the `Port` class, referencing `ref Port[] ___destinationPorts` in the arguments. Example:

```
[HarmonyPrefix]
[HarmonyPatch("Start")]
public static bool StartPatch(Port __instance, ref Port[] ___destinationPorts)
{
    if (__instance.name == "port 67 Bottleneck")
    {
        Port[] ports = GameObject.FindObjectsByType<Port>().ToList();

        ___destinationPorts[0] = ports.FirstOrDefault(p => p.name == "port E 9 (Dragon cliffs)");
        ___destinationPorts[1] = ports.FirstOrDefault(p => p.name == "port E 13 Sage Hills");
        ___destinationPorts[2] = ports.FirstOrDefault(p => p.name == "port E 14 Serpent Isle");
        ___destinationPorts[3] = ports.FirstOrDefault(p => p.name == "port E 12 New Port");
        ___destinationPorts[4] = ports.FirstOrDefault(p => p.name == "port E 10 sanctuary");
        ___destinationPorts[5] = ports.FirstOrDefault(p => p.name == "port E 11 crab beach");
        ___destinationPorts[6] = ports.FirstOrDefault(p => p.name == "port E 29 (jungle)");
        ___destinationPorts[7] = ports.FirstOrDefault(p => p.name == "port L 22 Lagoon Bay");
        ___destinationPorts[8] = ports.FirstOrDefault(p => p.name == "port A0 (Gold Rock)");
    }

    return true;
}
```

Couple of things to note:

- Ensure the spelling of all strings is exact
- Do not miss the `ref` in the arguments
- In the Unity editor, if you set the `destinationPorts` of your port script to be length 8 then you must have 8 destinations. No more, no less. If I only wanted 2 destinations, I would use something like this:

```
___destinationPorts[0] = ports.FirstOrDefault(p => p.name == "port E 9 (Dragon cliffs)");
___destinationPorts[1] = ports.FirstOrDefault(p => p.name == "port E 13 Sage Hills");
```
