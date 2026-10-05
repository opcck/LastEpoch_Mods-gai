using HarmonyLib;
using UnityEngine;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_Drop_LegendaryPotencial
    {
        public static bool CanRun()
        {
            if ((Hud_Manager.IsPauseOpen()) && (Hud_Manager.Content.OdlForceDrop.enable)) { return false; }
            else if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed())
                {
                    return Save_Manager.instance.data.Items.Drop.Enable_LegendaryPotencial;
                }
                else { return false; }
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(ItemDataUnpacked), nameof(ItemDataUnpacked.RollLegendaryPotential))]
        public class RollLegendaryPotential
        {
            [HarmonyPrefix]
            static bool Prefix(ref int __result)
            {
                if (CanRun())
                {
                    int roll = 0;
                    if (Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min == Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max)
                    {
                        roll = (int)Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max;
                    }
                    else
                    {
                        roll = (int)Random.Range((int)Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min, (int)Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max + 1);
                    }
                    __result = roll;
                    return false;
                }
                return true;
            }
        }
    }
}
