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
