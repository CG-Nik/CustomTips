using Alta.Networking.Servers;
using Alta.UserAuthentication;
using HarmonyLib;
using MelonLoader;
using System.Reflection;

[assembly: MelonInfo(typeof(CustomTips.Core), "CustomTips", "1.0.0", "CGNik", null)]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace CustomTips
{
    [HarmonyPatch(typeof(GeneralLoadingObjectManager), "InitializeTips")]
    public class InitializeTips
    {
        private static bool Prefix(GeneralLoadingObjectManager __instance)
        {
            String[] oldTips = (String[])typeof(GeneralLoadingObjectManager).GetField("tips", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
            List<string> newTips = oldTips.ToList();
            if (Core.melonPrefEntry_Overwrite.Value)
            {
                newTips = Core.melonPrefEntry_Tips.Value;
            } else
            {
                newTips.AddAll(Core.melonPrefEntry_Tips.Value);
            }
            if (Core.melonPrefEntry_Shuffle.Value)
            {
                newTips.Shuffle();
            }
            typeof(GeneralLoadingObjectManager).GetField("tips", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, newTips.ToArray());
            typeof(GeneralLoadingObjectManager).GetField("tipDurationSeconds", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, Core.melonPrefEntry_TipDurationInSeconds.Value);
            if (newTips.Count != 0)
            {
                CancellationTokenSource tipCancellationTokenSource = new CancellationTokenSource();
                typeof(GeneralLoadingObjectManager).GetField("tipCancellationTokenSource", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, tipCancellationTokenSource);
                typeof(GeneralLoadingObjectManager).GetMethod("UpdateTipText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new Object[] { tipCancellationTokenSource.Token });
            }
            return false;
        }
    }

    public class Core : MelonMod
    {
        private MelonPreferences_Category melonPrefCategory;
        internal static MelonPreferences_Entry<List<string>> melonPrefEntry_Tips;
        internal static MelonPreferences_Entry<bool> melonPrefEntry_Shuffle;
        internal static MelonPreferences_Entry<bool> melonPrefEntry_Overwrite;
        internal static MelonPreferences_Entry<float> melonPrefEntry_TipDurationInSeconds;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");

            melonPrefCategory = MelonPreferences.CreateCategory("CustomTips");
            melonPrefEntry_Tips = melonPrefCategory.CreateEntry<List<string>>("Tips", new List<string>());
            melonPrefEntry_Shuffle = melonPrefCategory.CreateEntry<bool>("Shuffle", true);
            melonPrefEntry_Overwrite = melonPrefCategory.CreateEntry<bool>("Overwrite", false);
            melonPrefEntry_TipDurationInSeconds = melonPrefCategory.CreateEntry<float>("TipDurationInSeconds", 5f);
            MelonPreferences.Save();
        }
    }
}