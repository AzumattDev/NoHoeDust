using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace NoHoeDust
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NoHoeDustPlugin : BaseUnityPlugin
    {
        internal const string ModName = "NoHoeDust";
        internal const string ModVersion = "1.0.5";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource NoHoeDustLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        private void OnDestroy()
        {
            Config.Save();
        }
    }

    #region Just In Case

    [HarmonyPatch(typeof(TerrainModifier), nameof(TerrainModifier.OnPlaced))]
    [HarmonyPriority(Priority.VeryHigh)]
    static class TerrainModifierAwakePatch
    {
        static void Prefix(TerrainModifier __instance)
        {
            // Make the __instance.m_onPlacedEffect an empty EffectList to prevent the hoe dust from spawning
            __instance.m_onPlacedEffect = new EffectList();
        }

        static void Postfix(TerrainModifier __instance)
        {
            // Make the __instance.m_onPlacedEffect an empty EffectList to prevent the hoe dust from spawning
            __instance.m_onPlacedEffect = new EffectList();
        }
    }

    [HarmonyPatch(typeof(TerrainOp), nameof(TerrainOp.OnPlaced))]
    [HarmonyPriority(Priority.VeryHigh)]
    static class TerrainOpAwakePatch
    {
        static void Prefix(TerrainOp __instance)
        {
            // Make the __instance.m_onPlacedEffect an empty EffectList to prevent the hoe dust from spawning
            __instance.m_onPlacedEffect = new EffectList();
        }

        static void Postfix(TerrainOp __instance)
        {
            // Make the __instance.m_onPlacedEffect an empty EffectList to prevent the hoe dust from spawning
            __instance.m_onPlacedEffect = new EffectList();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    static class ZNetSceneAwakePatch
    {
        [HarmonyPriority(Priority.Last)]
        static void Postfix(ZNetScene __instance)
        {
            NoHoeDustPlugin.NoHoeDustLogger.LogDebug("ZNetScene Awake Postfix, turning off build dust");
            foreach (GameObject instanceMPrefab in __instance.m_prefabs.Where(instanceMPrefab =>
                         instanceMPrefab.GetComponent<Piece>()))
            {
                if (!instanceMPrefab.name.Contains("road") && !instanceMPrefab.name.Contains("raise") &&
                    !instanceMPrefab.name.Contains("paved")) continue;
                Piece? pieceComponent = instanceMPrefab.GetComponent<Piece>();
                pieceComponent.m_placeEffect.m_effectPrefabs = pieceComponent.m_placeEffect.m_effectPrefabs
                    .Where(effect => !effect.m_prefab.name.Contains("vfx")).ToArray();
                NoHoeDustPlugin.NoHoeDustLogger.LogDebug("Removed build dust from " + instanceMPrefab.name +
                                                         " Current list of effect prefabs: " + string.Join("\n",
                                                             pieceComponent.m_placeEffect.m_effectPrefabs.Select(
                                                                 effect => effect.m_prefab.name)));
            }
        }
    }

    #endregion

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    static class PiecePlacePiecePatch
    {
        [HarmonyPriority(Priority.VeryHigh)]
        static void Prefix(Player __instance, ref Piece piece)
        {
            // cache the piece.gameObject.name
            string pieceName = piece.gameObject.name;
            if (!pieceName.Contains("road") && !pieceName.Contains("raise") && !pieceName.Contains("path_") && 
                !pieceName.Contains("paved")) return;
            NoHoeDustPlugin.NoHoeDustLogger.LogDebug("Preventing hoe dust from spawning " + pieceName);
            piece.m_placeEffect.m_effectPrefabs = piece.m_placeEffect.m_effectPrefabs
                .Where(effect => !effect.m_prefab.name.Contains("vfx")).ToArray();
            NoHoeDustPlugin.NoHoeDustLogger.LogDebug("Removed build dust from " + pieceName +
                                                     " Current list of effect prefabs: " + string.Join("\n",
                                                         piece.m_placeEffect.m_effectPrefabs.Select(
                                                             effect => effect.m_prefab.name)));
        }
    }
}