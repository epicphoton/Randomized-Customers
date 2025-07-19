# Randomized Customers and Better Customer Visuals
A mod for TCG Card Shop Simulator.
This randomizes every customer as they are spawned/respawned, picking from the existing Character Customizer options. It also tries to swap the assets that only use LOD1 at most, to LOD0, for higher quality visuals. This is not yet working for all character clothing and hair.

## Installation 
Install BepInEx to TCG, and optionally Configuration Manager.
Unzip and move the mod folder/dll into the BepInEx plugins folder.

## Config

- Enable Mod: Enable or disable the entire mod. Requires a reload to revert changes to customers.
- Enable High Quality Clothing: Swaps customer clothing to higher quality assets.
- Normal Clothes Only: Ensures that customers only wear appropriate clothing. For now, customers may appear in socks or barefoot, even though this is enabled. Disabling this allows customers to spawn in their underwear. These files are in the game. I did not add them. The only thing this setting actually changes is the range from which random clothing is selected. Disable at your own discretion. 
- Enable High Quality Customers: Swaps the customer body models to the higher quality. Affects face and bare skin.
- Enable High Quality Hair: Swaps customer hair to higher quality. (Does not change facial hair at the moment.) Hair has the largest number of assets that don't work and stay lower quality at the moment.
- Randomize Customers: The big one. Enables randomization. Customers are random every time they walk into the outside street.

## Technical Info
The game uses an older version of this asset: [Character Customizer from Jordbugg on the Unity AssetStore](https://assetstore.unity.com/packages/tools/game-toolkits/character-customizer-241861]) The [demo of the older version](https://drive.google.com/file/d/1v3ljpyXLxykG5w8OmUR1j9zp8k2ypGxv/view)﻿ can be found via the Wayback Machine, though I had to dig into the HTML to find the link. I used dotpeek on the game files and that demo to decompile them and get a sense for what was going on and how the base character objects were changed in TCG. If you can figure out more than I was able to, let me know in a comment and I'll take a look. There's probably a way to fix the prefabs before the scene gets initialized so everything gets instantiated normally, but I haven't been able to figure it out yet.

For those familiar with Level of Detail models (LODs), many assets only ever render using the LOD1 mesh at most, despite LOD0 being included in the game files. The prefabs for character models were changed to disable these LOD0 meshes. The assets use Unity's SkinnedMeshRenderer, which is very picky, so between bone, material, and mesh differences, I couldn't get some assets swapped properly where they'd render and animate correctly. The proper fix for this would be to correct the prefabs to not disable the LOD0 assets, and if required for lower end PCs, add a customer quality setting.

## Developing
Probably just clone this repo and make sure to set up the dev environment: [BepInEx Documentation](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/1_setup.html)
