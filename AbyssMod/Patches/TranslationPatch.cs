using HarmonyLib;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using Project.Novel;

namespace AbyssMod.Patches;

/// <summary>
/// 剧情翻译补丁：标题、人名、对话文本的翻译注入。
/// </summary>
[HarmonyPatch]
public static class TranslationPatch
{
    private static NovelController _novelController;
    private static string NovelId
    {
        get => _novelController?._common?.ScriptId ?? string.Empty;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NovelController), nameof(NovelController.InitNovel))]
    public static void InitNovelController(NovelController __instance)
    {
        _novelController = __instance;
    }

    public static bool TryGetCurrentNovel(
        out System.Collections.Generic.Dictionary<string, string> translation
    )
    {
        translation = null;
        return Config.Translation.Value
            && Plugin.Trans.Novels.TryGetValue(NovelId, out translation);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NovelPathUtility), nameof(NovelPathUtility.GetNovelScenarioDirectory))]
    public static void SetupTranslation(string novelId)
    {
        if (!Config.Translation.Value)
            return;

        Plugin.Log.LogInfo($"NovelId: {novelId}");

        Plugin.Trans.GetNovelTranslationAsync(novelId).Wait();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NovelScriptInfoUtility), nameof(NovelScriptInfoUtility.GetScriptInfo))]
    public static void SetTitleAndDescription(ValueTuple<string, string> __result)
    {
        if (TryGetCurrentNovel(out var _))
        {
            string title = __result.Item1;
            if (
                !string.IsNullOrEmpty(title)
                && Plugin.Trans.Titles.TryGetValue(title, out string text)
            )
                __result.Item1 = text;

            string description = __result.Item2;
            if (
                !string.IsNullOrEmpty(description)
                && Plugin.Trans.Descriptions.TryGetValue(description, out string desc)
            )
                __result.Item2 = desc;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NovelTitle), nameof(NovelTitle.SetTitle))]
    public static void SetTitle(ref string title)
    {
        if (TryGetCurrentNovel(out var _))
        {
            if (
                !string.IsNullOrEmpty(title)
                && Plugin.Trans.Titles.TryGetValue(title, out string text)
            )
                title = text;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NovelViewMessageWindow), nameof(NovelViewMessageWindow.SetName))]
    public static void SetName(ref string name)
    {
        if (TryGetCurrentNovel(out var _))
        {
            if (
                !string.IsNullOrEmpty(name) && Plugin.Trans.Names.TryGetValue(name, out string text)
            )
                name = text;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NovelText), nameof(NovelText.Parse))]
    public static void SetText(List<Letter> letters, ref string message)
    {
        if (TryGetCurrentNovel(out var translation))
        {
            if (!string.IsNullOrEmpty(message) && translation.TryGetValue(message, out string text))
                message = text;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NovelLogPopup), nameof(NovelLogPopup.SetData))]
    public static void SetLog(ref List<NovelLogData> dataList)
    {
        if (TryGetCurrentNovel(out var translation))
        {
            List<NovelLogData> list = new();
            foreach (var data in dataList)
            {
                var logData = new NovelLogData(
                    data.ScriptId,
                    data.AssetId,
                    data.Name,
                    data.Message,
                    data.LogId,
                    data.Voice,
                    data.Ct
                );

                if (
                    !string.IsNullOrEmpty(data.Name)
                    && Plugin.Trans.Names.TryGetValue(data.Name, out string name)
                )
                    logData.Name = name;

                if (
                    !string.IsNullOrEmpty(data.Message)
                    && translation.TryGetValue(data.Message, out string text)
                )
                    logData.Message = text;

                list.Add(logData);
            }
            dataList = list;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NovelModelDotBalloon), nameof(NovelModelDotBalloon.StartBalloonMessage))]
    public static void SetBalloon(CommandDotMessageData messageData)
    {
        if (TryGetCurrentNovel(out var translation))
        {
            string message = messageData.Message;
            if (!string.IsNullOrEmpty(message) && translation.TryGetValue(message, out string text))
                messageData.Message = text;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(NovelMessageTextComponent),
        nameof(NovelMessageTextComponent.SetMessageText)
    )]
    public static void SetTextCenter(NovelModelCommon common, CommandMessageTextData data)
    {
        if (TryGetCurrentNovel(out var translation))
        {
            string message = data.Message;
            if (!string.IsNullOrEmpty(message) && translation.TryGetValue(message, out string text))
                data.Message = text;
        }
    }
}
