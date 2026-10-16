using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_Drop_DuplicateItem
    {
        public static bool CanRun()
        {
            if (!Save_Manager.instance.IsNullOrDestroyed() && !Save_Manager.instance.data.IsNullOrDestroyed())
            {
                return Save_Manager.instance.data.Items.Drop.Enable_DuplicateItem;
            }
            return false;
        }

        [HarmonyPatch(typeof(ItemContainersManager), "DropItem", new System.Type[] { typeof(Actor), typeof(ItemData), typeof(int) })]
        public class ItemContainersManager_DropItem
        {
            [HarmonyPrefix]
            static void Prefix(ItemContainersManager __instance, Actor actor, ItemData data, ref int amount)
            {
                if (CanRun())
                {
                    if (amount < 2)
                    {
                        amount = 2;
                    }
                    else
                    {
                        amount *= 2;
                    }
                }
            }
        }
    }
}
