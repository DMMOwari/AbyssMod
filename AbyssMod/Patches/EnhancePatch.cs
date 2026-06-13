using System.Linq;
using HarmonyLib;
using Project.Notice;
using Project.Novel;

namespace AbyssMod.Patches;

/// <summary>
/// 游戏通用增强补丁：帧率修改 + 跳过大招动画。
/// </summary>
[HarmonyPatch]
public static class EnhancePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(NovelLive2DObject), nameof(NovelLive2DObject.Initialize))]
    public static void DisableMosaic(NovelLive2DObject __instance)
    {
        if (Config.DynamicMosaic.Value)
            return;

        __instance
            .GetDrawables()
            ?.Where(d => d.name.StartsWith("Mosaic_") || d.name.StartsWith("MosaicInsted_"))
            .ToList()
            .ForEach(d => d.gameObject.SetActive(false));
    }

    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(SoundCautionPopupController),
        nameof(SoundCautionPopupController.SetupPopupEvent)
    )]
    public static bool DisableSoundCaution(SoundCautionPopupController __instance)
    {
        if (!Config.SoundCaution.Value)
        {
            __instance._onClickOk.Invoke();
            return false;
        }

        return true;
    }
}
