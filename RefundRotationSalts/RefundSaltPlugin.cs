using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using PotionCraft.ManagersSystem;
using PotionCraft.ManagersSystem.Potion;
using PotionCraft.ObjectBased.Potion;
using PotionCraft.ScriptableObjects.Potion;
using PotionCraft.ScriptableObjects.Salts;
using System.Collections.Generic;

namespace RefundRotationSalts
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class RefundSaltPlugin : BaseUnityPlugin
    {
        private static ManualLogSource Log;

        private void Awake()
        {
            // Plugin startup logic
            Log = Logger;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(typeof(RefundSaltPlugin));

            refund = Config.Bind("General", "Refund salt", false, "If true, adds sun salt and moon salt (equal to the amount removed) to the player inventory.");
        }

        public static bool Refund
        {
            get { return refund.Value; }
            set
            {
                if (refund.Value != value)
                {
                    refund.Value = value;
                }
            }
        }
        private static ConfigEntry<bool> refund;

        private const string SaltSunName = "Sun Salt";
        private const string SaltMoonName = "Moon Salt";

        [HarmonyPrefix, HarmonyPatch(typeof(PotionManager.RecipeMarksSubManager), "AddSaltMark")]
        public static bool AddSaltMark_Prefix(List<SerializedRecipeMark> recipeMarksList, Salt salt, ref int amount)
        {
            // Log.LogDebug($"{amount} {salt.name} added");
            // only intercept sun|moon salt
            switch (salt)
            {
                case SaltMoon:
                case SaltSun:
                {
                    // Log.LogDebug($"Intercepting {salt.name} addition");
                    break;
                }
                default:
                {
                    // Log.LogDebug($"Ignoring {salt.name} addition");
                    return true;
                }
            }

            // if any of these fail, we have nothing to do
            if ((recipeMarksList == null) || (recipeMarksList.Count == 0) || (amount <= 0))
            {
                return true;
            }

            // if the last mark is salt and the same type of salt as this one, get the second last mark
            SerializedRecipeMark mark = recipeMarksList[^1];
            if (mark.type == SerializedRecipeMark.Type.Salt && mark.stringValue.Equals(salt.name))
            {
                
                // Log.LogDebug("Last mark is same as this mark, getting mark before that");
                mark = recipeMarksList[^2];
            }

            // if the mark being altered isn't a salt mark, let the game continue as normal
            if (mark.type != SerializedRecipeMark.Type.Salt)
            {
                
                // Log.LogDebug("RecipeMark is not salt");
                return true;
            }

            // we only want to make changes if this mark is sunsalt AND the previous mark is moonsalt (or vice versa)
            switch (mark.stringValue)
            {
                case SaltSunName:
                {
                    if (salt.name != SaltMoonName)
                    {
                        return true;
                    }
                    // Log.LogDebug("Added moonsalt, last salt was sunsalt");
                    break;
                }
                case SaltMoonName:
                {
                    if (salt.name != SaltSunName)
                    {
                        return true;
                    }
                    // Log.LogDebug("Added sunsalt, last salt was moonsalt");
                    break;
                }
                default:
                {
                    return true;
                }
            }

            bool addNewMark = true;
            int refund;
            int lastAmount = (int)mark.floatValue;
            if (amount >= lastAmount)
            {
                // if this mark adds equal or more salt than the previous mark,
                // remove the previous mark and adjust the amount added by this mark.
                // if the amounts are equal, prevent the creation of a new mark
                if (amount == lastAmount)
                {
                    addNewMark = false;
                }

                amount -= lastAmount;
                refund = lastAmount;
                recipeMarksList.Remove(mark);
            }
            else
            {
                // if this mark adds less salt than the previous mark,
                // remove salt from the previous mark and prevent the creation of a new mark.
                mark.floatValue -= amount;
                refund = amount;
                addNewMark = false;
            }

            // the game added the salt of this mark to usedComponents before calling this function,
            // so remove both salts from usedComponents.
            Salt markSalt = Salt.GetByName(mark.stringValue);
            UpdateUsedComponents(markSalt, refund);
            UpdateUsedComponents(salt, refund);

            // if refund is enabled, return the removed salt to the player inventory
            if (Refund)
            {
                Managers.Player.inventory.AddItem(markSalt, refund);
                Managers.Player.inventory.AddItem(salt, refund);
            }

            return addNewMark;
        }
        
        public static void UpdateUsedComponents(Salt salt, int amountToRemove)
        {
            var usedComponents = Managers.Potion.usedComponents;
            var saltInRecipe = PotionUsedComponent.GetComponent(usedComponents, salt);
            if (saltInRecipe == null)
            {
                return;
            }

            if (saltInRecipe.amount > amountToRemove)
            {
                saltInRecipe.amount -= amountToRemove;
            }
            else
            {
                int num = PotionUsedComponent.usedComponentsIndexes[salt];
                usedComponents.RemoveAt(num);
                PotionUsedComponent.usedComponentsIndexes.Remove(salt);
            }
        }
    }
}
