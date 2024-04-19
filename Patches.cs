using HarmonyLib;
using UnityEngine;

namespace CircletDemister;

[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
static class ZNetSceneAwakePatch
{
    static void Postfix(ZNetScene __instance)
    {
        CircletDemisterPlugin.DemisterParticleFf = __instance.GetPrefab("Demister").GetComponentInChildren<ParticleSystemForceField>();
        CircletDemisterPlugin.DemisterParticleFf.endRange = 4f;
    }
}

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
static class PlayerEquipItemPatch
{
    static void Postfix(ref Humanoid __instance, ref bool __result, ItemDrop.ItemData? item,
        bool triggerEquipEffects = true)
    {
        if (Player.m_localPlayer == null || !__instance.IsPlayer() || Player.m_localPlayer.m_isLoading) return;
        if (item?.m_dropPrefab.name != "HelmetDverger") return;
        // If the player doesn't already have the particle force field, add it
        if (CircletDemisterPlugin.PlayerParticleFf == null)
        {
            CircletDemisterPlugin.PlayerParticleFf = Object.Instantiate(CircletDemisterPlugin.DemisterParticleFf, __instance.GetHeadPoint(), Quaternion.identity, __instance.transform);
        }
    }
}

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
static class PlayerUnequipItemPatch
{
    static void Postfix(ref Humanoid __instance, ItemDrop.ItemData? item,
        bool triggerEquipEffects = true)
    {
        if (!__instance.IsPlayer()) return;
        if (item?.m_dropPrefab.name != "HelmetDverger" || Player.m_localPlayer == null || !__instance.IsPlayer() || Player.m_localPlayer != __instance) return;
        // Remove component from player of ParticleSystemForceField and Demister
        if (CircletDemisterPlugin.PlayerParticleFf == null) return;
        Demister.m_instances.Remove(CircletDemisterPlugin.PlayerParticleFf.GetComponent<Demister>());
        Object.Destroy(CircletDemisterPlugin.PlayerParticleFf.gameObject);
        CircletDemisterPlugin.PlayerParticleFf = null;
    }
}