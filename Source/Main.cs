using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;

namespace PollutionTweaks {
    [StaticConstructorOnStartup]
    public class Main : Mod {
        public static Main Instance { get; private set; }

        static Main() {
            var harmony = new Harmony(Strings.ID);
            harmony.PatchAll();
        }

        public Main(ModContentPack content) : base(content) {
            Instance = this;
            GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect rect) 
            => GetSettings<Settings>().DoWindowContents(rect);

        public override string SettingsCategory() 
            => Strings.Name;
    }
}
