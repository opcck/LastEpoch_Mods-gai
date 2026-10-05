using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Skills
{
    public class Skills_Level
    {
        public static bool CanRun()
        {
            return Ready() && (LevelOn() || MultiplierOn());
        }

        static bool Ready()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.IsNullOrDestroyed()))
            {
                if ((!Save_Manager.instance.data.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.specialisedSkillTrees.IsNullOrDestroyed()))
                {
                    return true;
                }
            }
            return false;
        }

        static bool LevelOn()
        {
            return Save_Manager.instance.data.Skills.Enable_SkillLevel;
        }

        static bool MultiplierOn()
        {
            return Save_Manager.instance.data.Skills.Enable_SkillLevelMultiplier;
        }

        public static byte ChosenLevel()
        {
            int level = (int)Save_Manager.instance.data.Skills.SkillLevel;
            if (level < 0) { level = 0; }
            if (level > byte.MaxValue) { level = byte.MaxValue; }
            return (byte)level;
        }

        static int Multiplier()
        {
            return SettingRow.Clamp(Save_Manager.instance.data.Skills.SkillLevelMultiplier);
        }

        static bool writing;
        static readonly Dictionary<string, byte> real_additional = new Dictionary<string, byte>();

        public static void Sync()
        {
            Reapply();
            RefreshOpenTree();
        }

        public static void ApplyAll()
        {
            if (!LevelOn() && !MultiplierOn()) { return; }
            Sync();
        }

        static void Reapply()
        {
            if (!Ready()) { return; }
            try
            {
                writing = true;
                if (LevelOn()) { ApplyLevels(); }
                else { RestoreLevels(); }
                if (MultiplierOn()) { ApplyPointBonus(); }
                else { RestorePoints(); }
                writing = false;
            }
            catch { writing = false; }
        }

        static void ApplyLevels()
        {
            byte level = ChosenLevel();
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                if (data == null) { continue; }
                data.level = level;
            }
        }

        static void RestoreLevels()
        {
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                if (data == null) { continue; }
                data.level = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
            }
        }

        static int PointLevel(LocalTreeData.SkillTreeData data)
        {
            if (LevelOn()) { return ChosenLevel(); }
            int level = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
            if (level < 0) { level = 0; }
            if (level > byte.MaxValue) { level = byte.MaxValue; }
            return level;
        }

        static string Key(LocalTreeData.SkillTreeData data)
        {
            if (data == null || data.ability.IsNullOrDestroyed()) { return null; }
            return data.slot + ":" + data.ability.abilityName;
        }

        static byte BonusAdditional(int level, int real)
        {
            long additional = (((long)level + real) * Multiplier()) - level;
            if (additional < 0) { additional = 0; }
            if (additional > byte.MaxValue) { additional = byte.MaxValue; }
            return (byte)additional;
        }

        static void ApplyPointBonus()
        {
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                string key = Key(data);
                if (key == null) { continue; }
                if (!real_additional.ContainsKey(key)) { real_additional[key] = data.additionalMaxPointsFromStats; }
                data.additionalMaxPointsFromStats = BonusAdditional(PointLevel(data), real_additional[key]);
            }
        }

        static void RestorePoints()
        {
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                string key = Key(data);
                if (key == null || !real_additional.ContainsKey(key)) { continue; }
                data.additionalMaxPointsFromStats = real_additional[key];
            }
            real_additional.Clear();
        }

        static void OnAdditionalPointsUpdated()
        {
            if (writing || !Ready()) { return; }
            real_additional.Clear();
            if (!MultiplierOn()) { return; }
            try
            {
                writing = true;
                ApplyPointBonus();
                writing = false;
            }
            catch { writing = false; }
        }

        static void RefreshOpenTree()
        {
            try
            {
                foreach (SkillsPanelManager panel in Object.FindObjectsOfType<SkillsPanelManager>())
                {
                    if (!panel.IsNullOrDestroyed()) { panel.updateVisuals(false); }
                }
            }
            catch { }
        }

        [HarmonyPatch(typeof(SkillsPanelManager), "OnOpenSkillTree")]
        public class SkillsPanelManager_OnOpenSkillTree
        {
            [HarmonyPostfix]
            static void Postfix(SkillsPanelManager __instance, SkillTree __0)
            {
                try
                {
                    if (__0.IsNullOrDestroyed()) { return; }
                    Sync();
                    if (!__instance.IsNullOrDestroyed()) { __instance.updateVisuals(false); }
                }
                catch { Main.logger_instance?.Msg("SkillsPanelManager.OnOpenSkillTree() ERROR"); }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), "getAbilityLevel")]
        public class LocalTreeData_getAbilityLevel
        {
            [HarmonyPostfix]
            static void Postfix(LocalTreeData __instance, Ability __0, ref byte __result)
            {
                if (writing || !CanRun() || __0.IsNullOrDestroyed() || __instance.specialisedSkillTrees.IsNullOrDestroyed()) { return; }
                foreach (LocalTreeData.SkillTreeData data in __instance.specialisedSkillTrees)
                {
                    if (data == null || data.ability.IsNullOrDestroyed()) { continue; }
                    if (data.ability.abilityName != __0.abilityName) { continue; }
                    if (LevelOn())
                    {
                        byte level = ChosenLevel();
                        writing = true;
                        data.level = level;
                        writing = false;
                        __result = level;
                    }
                    else
                    {
                        byte real = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
                        if (data.level != real)
                        {
                            writing = true;
                            data.level = real;
                            writing = false;
                        }
                        __result = real;
                        if (MultiplierOn())
                        {
                            writing = true;
                            ApplyPointBonus();
                            writing = false;
                        }
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.levelUpAbility))]
        public class LocalTreeData_levelUpAbility
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!CanRun()) { return; }
                Reapply();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.ApplyAbilityXp))]
        public class LocalTreeData_ApplyAbilityXp
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!CanRun()) { return; }
                Reapply();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.LevelSkillsToMinLevel))]
        public class LocalTreeData_LevelSkillsToMinLevel
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!CanRun()) { return; }
                Reapply();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.setAdditionalMaxPointsFromStatsOnServerOrInSingleplayer))]
        public class LocalTreeData_setAdditionalMaxPointsFromStats
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                OnAdditionalPointsUpdated();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.receiveAdditionalMaxPointsFromStatsOnClient))]
        public class LocalTreeData_receiveAdditionalMaxPointsFromStatsOnClient
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                OnAdditionalPointsUpdated();
            }
        }
    }
}
