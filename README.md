# RefundRotationSalts

RefundRotationSalts is a mod made with BepInEx for use with the Potion Craft game. It will remove moon salt from the recipe of a potion if you add sun salt *immediately* afterwards (and vice versa). Cheatmode (activated by modifying the config) will add both sun and moon salt to your inventory, equal to the amount removed.

## Installation

### BepInEx

- Download the latest BepInEx 5 release from [here](http:/https://github.com/BepInEx/BepInEx/releases/ "here") Note: You need the `BepInEx_64` version for use with Potion Craft
- You can read the installation for BepInEx [here](http:/https://docs.bepinex.dev/articles/user_guide/installation/index.html/ "here")
- Once installed run once to generate the config files and folders

### RefundRotationSalts installation

1) Download the latest version from the releases page
2) Unzip the folder
3) Copy the folder into `/Potion Craft/BepInEx/plugins/`
4) Run the game

## Cheatmode
With cheatmode enabled, the game will add both sun and moon salt to your inventory, equal to the amount removed. It is disabled by default. It can be enabled/disabled by doing the following:
1) Close PotionCraft.
2) Open `/Potion Craft/BepInEx/configs/RefundRotationStals.cfg`.
3) Under `[General]`, change the value assigned to `Refund salt`
    - To enable cheatmode, set the value to `true` (eg `Refund salt = true`)
    - To disable cheatmode, set the value to `false` (eg `Refund salt = false`)
4) Save the file and restart PotionCraft
