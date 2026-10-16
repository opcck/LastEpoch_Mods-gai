using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Character
{
    public class Character_UnlockBlockedIdols
    {
        public static bool CanRun()
        {
            if (!Save_Manager.instance.IsNullOrDestroyed() && !Save_Manager.instance.data.IsNullOrDestroyed())
            {
                return Save_Manager.instance.data.Character.Cheats.Enable_UnlockBlockedIdols;
            }
            return false;
        }

        public static void Update()
        {
            try
            {
                if (!Refs_Manager.item_containers_manager.IsNullOrDestroyed())
                {
                    if (!Refs_Manager.item_containers_manager.idols.IsNullOrDestroyed())
                    {
                        Refs_Manager.item_containers_manager.idols.RefreshUnlockIndex();
                        Refs_Manager.item_containers_manager.idols.OnIdolInventoryChanged();
                    }
                }
            }
            catch { }
        }

        [HarmonyPatch(typeof(IdolsItemContainer), "CheckSlotUnlocked", new System.Type[] { typeof(int), typeof(int) })]
        public class IdolsItemContainer_CheckSlotUnlocked
        {
            [HarmonyPrefix]
            static bool Prefix(int x, int y, ref bool __result)
            {
                if (CanRun())
                {
                    if (x >= 0 && x < 5 && y >= 0 && y < 5)
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(IdolsItemContainer), "GetSlotState")]
        public class IdolsItemContainer_GetSlotState
        {
            [HarmonyPostfix]
            static void Postfix(int x, int y, ref bool isRefracted, ref IdolsItemContainer.IdolSlotState __result)
            {
                if (CanRun())
                {
                    if (__result == IdolsItemContainer.IdolSlotState.Blocked)
                    {
                        __result = IdolsItemContainer.IdolSlotState.Unlocked;
                    }
                }
            }
        }
    }
}
