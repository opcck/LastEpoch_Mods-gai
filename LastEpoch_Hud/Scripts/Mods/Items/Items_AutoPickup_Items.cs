using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_AutoPickup_Items
    {
        [HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
        public class GroundItemManager_dropItemForPlayer
        {
            [HarmonyPrefix]
            static bool Prefix(ref GroundItemManager __instance, ref Actor __0, ref ItemData __1, ref UnityEngine.Vector3 __2, bool __3)
            {
                bool result = true;
                if (Scenes.IsGameScene())
                {
                    //Fix Load items with more than 4 affixs
                    if ((__1.rarity == 5) || (__1.rarity == 6))
                    {
                        __1.rarity = 4;
                        __1.RefreshIDAndValues();
                    }
                    //AutoPickup / AutoSell                    
                    ItemDataUnpacked item = __1.TryCast<ItemDataUnpacked>();
                    if ((!Save_Manager.instance.IsNullOrDestroyed()) && (!item.IsNullOrDestroyed()))
                    {
                        if (((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Keys) && (Item.isKey(__1.itemType))) ||
                            ((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_WovenEchoes) && (__1.itemType == 107)) ||
                            ((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Materials) && (ItemList.isCraftingItem(__1.itemType))))
                        {
                            bool pickup = ItemContainersManager.Instance.attemptToPickupItem(__1, __0.position());
                            if (pickup) { result = false; }
                        }
                        else if ((__1.itemType < 34) &&
                            (!Refs_Manager.filter_manager.IsNullOrDestroyed()) &&
                            ((Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_FromFilter) ||
                            (Save_Manager.instance.data.Items.Pickup.Enable_AutoSell_FromFilter) ||
                            (Save_Manager.instance.data.Items.Pickup.Enable_AutoShatter_FromFilter)))
                        {
                            if (!Refs_Manager.filter_manager.Filter.IsNullOrDestroyed())
                            {
                                bool FilterShow = false;
                                foreach (Rule rule in Refs_Manager.filter_manager.Filter.rules)
                                {
                                    if ((rule.isEnabled) && (rule.Match(item, Refs_Manager.player_actor.stats.level))) //&&
                                        //(((rule.levelDependent) && (rule.LevelInBounds(__0.stats.level))) ||
                                        //(!rule.levelDependent)))
                                    {
                                        if (rule.type == Rule.RuleOutcome.SHOW) //|| (rule.type == Rule.RuleOutcome.HIGHLIGHT_DEPRECATED))
                                        {
                                            FilterShow = true;
                                            break;
                                        }
                                    }
                                }
                                if ((FilterShow) && (Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_FromFilter))
                                {
                                    bool pickup = ItemContainersManager.Instance.attemptToPickupItem(__1, __0.position());
                                    if (pickup) { result = false; }
                                }
                                else if ((!FilterShow) && (Save_Manager.instance.data.Items.Pickup.Enable_AutoShatter_FromFilter) && (TryAutoShatter(item)))
                                {
                                    result = false;
                                }
                                else if ((!FilterShow) && (Save_Manager.instance.data.Items.Pickup.Enable_AutoSell_FromFilter))
                                {
                                    __0.goldTracker.modifyGold(item.VendorSaleValue);
                                    result = false;
                                }
                            }
                        }
                    }
                }

                return result;
            }

            static bool TryAutoShatter(ItemDataUnpacked item)
            {
                try
                {
                    var pickup = Save_Manager.instance.data.Items.Pickup;
                    int chance = pickup.AutoShatter_Chance;
                    if (chance <= 0) { return false; }
                    if ((chance < 100) && (UnityEngine.Random.Range(0, 100) >= chance)) { return false; }

                    ItemContainersManager manager = ItemContainersManager.Instance;
                    if (manager.IsNullOrDestroyed()) { return false; }
                    if (pickup.Enable_AutoShatter_UseRune && !HasRune(manager)) { return false; }
                    if (!GrantShards(manager, item, pickup.AutoShatter_AffixChance, pickup.AutoShatter_QuantityChance)) { return false; }
                    if (pickup.Enable_AutoShatter_UseRune) { ConsumeRune(manager); }
                    return true;
                }
                catch (System.Exception ex)
                {
                    Main.logger_instance?.Error("Auto shatter: " + ex.Message);
                    return false;
                }
            }

            static bool HasRune(ItemContainersManager manager)
            {
                if (manager.materials.IsNullOrDestroyed() || manager.materials.shattering.IsNullOrDestroyed()) { return false; }
                return manager.materials.shattering.GetQuantity() > 0;
            }

            static void ConsumeRune(ItemContainersManager manager)
            {
                SingleSubTypeContainer container = manager.materials.shattering;
                ItemContainerEntry entry = null;
                if ((container.IsNullOrDestroyed()) || (!container.TryGetContent(out entry)) || (entry.IsNullOrDestroyed())) { return; }
                container.TryRemoveItem(entry, 1, Context.SILENT);
            }

            static bool GrantShards(ItemContainersManager manager, ItemDataUnpacked item, int affixChance, int quantityChance)
            {
                if (manager.shardStorage == null) { return false; }
                if (affixChance <= 0) { return true; }
                Il2CppSystem.Collections.Generic.List<ItemAffix> affixes = item.affixes;
                if (affixes.IsNullOrDestroyed()) { return true; }
                for (int i = 0; i < affixes.Count; i++)
                {
                    ItemAffix affix = affixes[i];
                    if ((affix.IsNullOrDestroyed()) || (affix.affixId == 0)) { continue; }
                    if ((affixChance < 100) && (UnityEngine.Random.Range(0, 100) >= affixChance)) { continue; }
                    int quantity = 1;
                    if ((quantityChance > 0) && ((quantityChance >= 100) || (UnityEngine.Random.Range(0, 100) < quantityChance))) { quantity = 2; }
                    manager.shardStorage.AddShard(affix.affixId, quantity, true);
                }
                return true;
            }
        }
    }
}