using HarmonyLib;
using Il2CppLE.UI.Minimap;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Minimap
{
    internal class Minimap_FogOfWar
    {
        public const float OracleRadius = 600f;
        public static float ZoneDefaultRadius = 150f;
        private static Il2CppLE.UI.Minimap.Minimap _currentMinimap;

        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed())
                {
                    return Save_Manager.instance.data.Scenes.Minimap.Enable_RemoveFogOfWar;
                }
                else { return false; }
            }
            else { return false; }
        }

        public static void RevealAllNow()
        {
            try
            {
                if (_currentMinimap.IsNullOrDestroyed())
                {
                    _currentMinimap = Object.FindObjectOfType<Il2CppLE.UI.Minimap.Minimap>();
                }
                if (!_currentMinimap.IsNullOrDestroyed())
                {
                    if (CanRun())
                    {
                        _currentMinimap.RevealRadius = OracleRadius;
                    }
                    else
                    {
                        _currentMinimap.RevealRadius = ZoneDefaultRadius > 0f ? ZoneDefaultRadius : 150f;
                    }
                }
            }
            catch { }
        }

        [HarmonyPatch(typeof(Il2CppLE.UI.Minimap.Minimap), "Awake")]
        public class Minimap_Awake
        {
            static void Postfix(Il2CppLE.UI.Minimap.Minimap __instance)
            {
                if (__instance.IsNullOrDestroyed()) return;
                _currentMinimap = __instance;
                float defRadius = __instance.RevealRadius;
                if (defRadius > 0f && defRadius < OracleRadius)
                {
                    ZoneDefaultRadius = defRadius;
                }
                if (CanRun())
                {
                    __instance.RevealRadius = OracleRadius;
                }
            }
        }

        [HarmonyPatch(typeof(Il2CppLE.UI.Minimap.Minimap), "Update")]
        public class Minimap_Update
        {
            static void Postfix(Il2CppLE.UI.Minimap.Minimap __instance)
            {
                if (__instance.IsNullOrDestroyed()) return;
                _currentMinimap = __instance;
                if (CanRun())
                {
                    if (__instance.RevealRadius != OracleRadius)
                    {
                        __instance.RevealRadius = OracleRadius;
                    }
                }
                else
                {
                    if (__instance.RevealRadius == OracleRadius)
                    {
                        __instance.RevealRadius = ZoneDefaultRadius > 0f ? ZoneDefaultRadius : 150f;
                    }
                }
            }
        }
    }
}
