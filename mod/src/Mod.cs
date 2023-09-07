using UnityEngine;
using Verse;

namespace CombatReadiness
{
    public class CombatReadinessSettings : ModSettings
    {
        public string DefaultCombatOutfitName = "Soldier";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DefaultCombatOutfitName, "DefaultCombatOutfitName");
            base.ExposeData();
        }
    }
    
    public class Mod : Verse.Mod
    {
        private CombatReadinessSettings settings;
        
        /// <summary>
        /// Simple logging function
        /// </summary>
        public static void ModDebug(string text, bool always = false)
        {
            Log.Message($"[Combat Readiness] {text}");
        }

        public Mod(ModContentPack content) : base(content)
        {
            ModDebug("Mod Initialised!",true);
            settings = GetSettings<CombatReadinessSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var standard = new Listing_Standard();
            standard.Begin(inRect);
            standard.Label("Sarge945.CombatReadinessDefaultLabel".Translate());
            settings.DefaultCombatOutfitName = standard.TextEntry(settings.DefaultCombatOutfitName);
            standard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Sarge945.CombatReadinessModName".Translate();
        }
    }
}