using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_LockWovenEnchanter
    {
        public static bool CanRun()
        {
            if (!Save_Manager.instance.IsNullOrDestroyed() && !Save_Manager.instance.data.IsNullOrDestroyed())
            {
                return Save_Manager.instance.data.Items.Drop.Enable_LockWovenEnchanter;
            }
            return false;
        }

        [HarmonyPatch(typeof(ZoneRestrictedCraftingManager), "OnUse", new System.Type[] { typeof(Actor), typeof(ZoneRestrictedCraftingManager.CraftingType) })]
        public class ZoneRestrictedCraftingManager_OnUse_CraftingType
        {
            [HarmonyPrefix]
            static bool Prefix(ZoneRestrictedCraftingManager __instance, Actor actorIfOnServer, ZoneRestrictedCraftingManager.CraftingType craftingType)
            {
                if (CanRun() && craftingType == ZoneRestrictedCraftingManager.CraftingType.IdolEnchantment)
                {
                    __instance.localTimesUsed = 0;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneRestrictedCraftingManager), "OnUse", new System.Type[] { typeof(Actor) })]
        public class ZoneRestrictedCraftingManager_OnUse_Actor
        {
            [HarmonyPrefix]
            static bool Prefix(ZoneRestrictedCraftingManager __instance, Actor actorIfOnServer)
            {
                if (CanRun() && __instance.craftingType == ZoneRestrictedCraftingManager.CraftingType.IdolEnchantment)
                {
                    __instance.localTimesUsed = 0;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ZoneRestrictedCraftingManager), "CanCraft", new System.Type[] { typeof(ZoneRestrictedCraftingManager.CraftingType), typeof(Actor) })]
        public class ZoneRestrictedCraftingManager_CanCraft
        {
            [HarmonyPostfix]
            static void Postfix(ZoneRestrictedCraftingManager.CraftingType craftingType, Actor actorIfOnServer, ref bool __result)
            {
                if (CanRun() && craftingType == ZoneRestrictedCraftingManager.CraftingType.IdolEnchantment)
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(ZoneRestrictedCraftingManager), "UsesRemaining", new System.Type[] { typeof(Actor) })]
        public class ZoneRestrictedCraftingManager_UsesRemaining
        {
            [HarmonyPostfix]
            static void Postfix(ZoneRestrictedCraftingManager __instance, Actor actorIfOnServer, ref int __result)
            {
                if (CanRun() && __instance.craftingType == ZoneRestrictedCraftingManager.CraftingType.IdolEnchantment)
                {
                    if (__result < 1) __result = 1;
                }
            }
        }

        [HarmonyPatch(typeof(IdolEnchantmentCraftingUI), "get_CanCraft")]
        public class IdolEnchantmentCraftingUI_CanCraft
        {
            [HarmonyPostfix]
            static void Postfix(IdolEnchantmentCraftingUI __instance, ref bool __result)
            {
                if (CanRun() && !__instance.GetItemToCraft().IsNullOrDestroyed())
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(IdolEnchantmentCraftingUI), "get_CraftIsUnlocked")]
        public class IdolEnchantmentCraftingUI_CraftIsUnlocked
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result)
            {
                if (CanRun())
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(CraftingUsesRemainingUI), "UpdateUsesRemaining")]
        public class CraftingUsesRemainingUI_UpdateUsesRemaining
        {
            [HarmonyPostfix]
            static void Postfix(CraftingUsesRemainingUI __instance)
            {
                if (CanRun() && __instance._craftingType == ZoneRestrictedCraftingManager.CraftingType.IdolEnchantment)
                {
                    if (!__instance._text.IsNullOrDestroyed())
                    {
                        __instance._text.text = "(锁定不减)";
                        __instance._text.color = __instance._hasUsesRemainingColor;
                    }
                    if (!__instance._glow.IsNullOrDestroyed())
                    {
                        __instance._glow.color = __instance._hasUsesRemainingColor;
                    }
                }
            }
        }
    }
}
