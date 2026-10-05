using HarmonyLib;
using Il2Cpp;
using Il2CppLE.MicrotransactionSystem;
using Il2CppLE.Networking.Cosmetics;
using Il2CppLE.Services.Cosmetics;
using Il2CppLE.UI.MTXStore;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics
{
    [RegisterTypeInIl2Cpp]
    public class Cosmetics_Offline : MonoBehaviour
    {
        public Cosmetics_Offline(System.IntPtr ptr) : base(ptr) { }
        public static Cosmetics_Offline instance { get; private set; }

        static Il2CppSystem.Collections.Generic.List<string> cachedIds;

        void Awake()
        {
            instance = this;
        }

        static Il2CppSystem.Collections.Generic.List<string> GetAllIds()
        {
            if (!cachedIds.IsNullOrDestroyed() && cachedIds.Count > 0)
            {
                return cachedIds;
            }

            var seen = new System.Collections.Generic.HashSet<string>();
            try
            {
                foreach (CosmeticResources resources in Resources.FindObjectsOfTypeAll<CosmeticResources>())
                {
                    if (resources.IsNullOrDestroyed())
                    {
                        continue;
                    }

                    AddIds(resources.ItemCosmetics, seen);
                    AddIds(resources.AbilityCosmetics, seen);
                    AddIds(resources.BackSlotCosmetics, seen);
                    AddIds(resources.PortalCosmetics, seen);
                    AddIds(resources.PetCosmetics, seen);
                    AddIds(resources.PetEffectCosmetics, seen);
                    AddIds(resources.PortraitCosmetics, seen);
                    AddIds(resources.CharacterTitleCosmetics, seen);
                    AddIds(resources.FootprintCosmetics, seen);
                    AddIds(resources.CharacterFXCosmetics, seen);
                    AddIds(resources.StashCosmetics, seen);

                    if (!resources.CosmeticLookupPairs.IsNullOrDestroyed())
                    {
                        foreach (var pair in resources.CosmeticLookupPairs)
                        {
                            if (!string.IsNullOrEmpty(pair.Key))
                            {
                                seen.Add(pair.Key);
                            }
                        }
                    }
                    if (!resources.AbilityLookupPairs.IsNullOrDestroyed())
                    {
                        foreach (var pair in resources.AbilityLookupPairs)
                        {
                            if (!string.IsNullOrEmpty(pair.Key))
                            {
                                seen.Add(pair.Key);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Warning("Cosmetics catalog read failed: " + ex.Message);
            }

            try
            {
                foreach (Cosmetic cosmetic in Resources.FindObjectsOfTypeAll<Cosmetic>())
                {
                    if (!cosmetic.IsNullOrDestroyed() && !string.IsNullOrEmpty(cosmetic.BackendID))
                    {
                        seen.Add(cosmetic.BackendID);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Warning("Cosmetics object scan failed: " + ex.Message);
            }

            if (seen.Count == 0)
            {
                return null;
            }

            cachedIds = new Il2CppSystem.Collections.Generic.List<string>();
            foreach (string id in seen)
            {
                cachedIds.Add(id);
            }

            Main.logger_instance?.Msg("Cosmetics unlock list ready: " + cachedIds.Count);
            return cachedIds;
        }

        static void AddIds<T>(Il2CppSystem.Collections.Generic.List<T> list, System.Collections.Generic.HashSet<string> seen) where T : Cosmetic
        {
            if (list.IsNullOrDestroyed())
            {
                return;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var cosmetic = list[i];
                if (!cosmetic.IsNullOrDestroyed() && !string.IsNullOrEmpty(cosmetic.BackendID))
                {
                    seen.Add(cosmetic.BackendID);
                }
            }
        }

        static void ReplaceOwnedList(ref Il2CppSystem.Collections.Generic.List<string> result)
        {
            var all = GetAllIds();
            if (!all.IsNullOrDestroyed() && all.Count > 0)
            {
                result = all;
            }
        }

        [HarmonyPatch(typeof(CosmeticsManager), nameof(CosmeticsManager.GetOwnedCosmetics))]
        public class CosmeticsManager_GetOwnedCosmetics
        {
            [HarmonyPostfix]
            static void Postfix(ref CosmeticsManager __instance, ref Il2CppCysharp.Threading.Tasks.UniTask<Il2CppSystem.Collections.Generic.List<string>> __result)
            {
                if (!Refs_Manager.player_actor.IsNullOrDestroyed())
                {
                    __instance.player = Refs_Manager.player_actor.gameObject;
                }

                var all = GetAllIds();
                if (all.IsNullOrDestroyed() || all.Count == 0)
                {
                    return;
                }

                __result = new Il2CppCysharp.Threading.Tasks.UniTask<Il2CppSystem.Collections.Generic.List<string>>(all);
            }
        }

        [HarmonyPatch(typeof(UserInventory), nameof(UserInventory.GetOwnedCosmeticIds))]
        public class UserInventory_GetOwnedCosmeticIds
        {
            [HarmonyPostfix]
            static void Postfix(ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                ReplaceOwnedList(ref __result);
            }
        }

        [HarmonyPatch(typeof(UserInventory), nameof(UserInventory.HasActiveItemId))]
        public class UserInventory_HasActiveItemId
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result)
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(UserInventory), nameof(UserInventory.HasActiveItemRawId))]
        public class UserInventory_HasActiveItemRawId
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result)
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(OfflineUserInventory), nameof(OfflineUserInventory.LoadOfflineOwnedCosmeticIds))]
        public class OfflineUserInventory_LoadOfflineOwnedCosmeticIds
        {
            [HarmonyPostfix]
            static void Postfix(ref Il2CppSystem.Collections.Generic.List<string> __result)
            {
                ReplaceOwnedList(ref __result);
            }
        }

        [HarmonyPatch(typeof(PlayerCosmeticInventory), nameof(PlayerCosmeticInventory.IsOwnedItem))]
        public class PlayerCosmeticInventory_IsOwnedItem
        {
            [HarmonyPostfix]
            static void Postfix(ref bool __result)
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(MTXStoreController), nameof(MTXStoreController.IsSupporterPackLive))]
        public class MTXStoreController_IsSupporterPackLive
        {
            [HarmonyPrefix]
            static bool Prefix(string backendId, ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(MTXStoreController), nameof(MTXStoreController.GetLiveSupporterPacks))]
        public class MTXStoreController_GetLiveSupporterPacks
        {
            [HarmonyPrefix]
            static bool Prefix(ref Il2CppSystem.Collections.Generic.List<CatalogItem> __result)
            {
                __result = new Il2CppSystem.Collections.Generic.List<CatalogItem>();
                return false;
            }
        }

        [HarmonyPatch(typeof(CosmeticSelectionFlyoutPanelUI), nameof(CosmeticSelectionFlyoutPanelUI.PopulateFlyoutWindowAsync))]
        public class CosmeticSelectionFlyoutPanelUI_PopulateFlyoutWindowAsync
        {
            [HarmonyFinalizer]
            static System.Exception Finalizer(System.Exception __exception, CosmeticSelectionFlyoutPanelUI __instance)
            {
                if (__exception != null)
                {
                    Main.logger_instance?.Warning("PopulateFlyoutWindowAsync exception suppressed: " + __exception.Message);
                    if (!__instance.IsNullOrDestroyed() && !__instance._loadingOverlay.IsNullOrDestroyed())
                    {
                        __instance._loadingOverlay.SetActive(false);
                    }
                }
                return null;
            }
        }
    }
}
