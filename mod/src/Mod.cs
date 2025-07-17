using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace CombatReadiness
{
    public class CombatReadinessSettings : ModSettings
    {
        public string DefaultCombatOutfitName = "Soldier";
        /// <summary>
        /// Gizmo Mode.
        /// If enabled, will show both gizmos when running Defensive Positions
        /// Otherwise, only show the Defensive Positions version
        /// </summary>
        public bool ShowBothGizmos = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DefaultCombatOutfitName, "DefaultCombatOutfitName");
            Scribe_Values.Look(ref ShowBothGizmos, "ShowBothGizmos");
            base.ExposeData();
        }
    }
    
    public class Mod : Verse.Mod
    {
        private CombatReadinessSettings settings;
        
        /// <summary>
        /// Simple logging function
        /// </summary>
        public static void ModDebug(string text)
        {
            #if DEBUG
            Log.Message($"[Combat Readiness] {text}");
            #endif
        }

        /// <summary>
        /// Returns whether or not a pawn has the defensive positions gizmo currently.
        /// Does some horrible evil hacky madness!
        /// This is to add Defensive Positions support, without
        /// actually having it as a hard requirement.
        /// </summary>
        public static Gizmo GetDefensivePositionsGizmo(Pawn p)
        {
            return p?.GetGizmos().FirstOrDefault(g => g.GetType().Name is "Gizmo_DefensivePositionButton" or "Gizmo_QuadButtonPanel");
        }
        
        /// <summary>
        /// Returns whether or not a pawn has the defensive positions gizmo currently.
        /// Does some horrible evil hacky madness!
        /// This is to add Defensive Positions support, without
        /// actually having it as a hard requirement.
        /// </summary>
        public static bool DefensivePositionsInstalled()
        {
            ModDebug("Checking for Defensive Positions");
            return LoadedModManager.RunningMods.FirstOrDefault(x => x.Name.Contains("Defensive Positions")) != null;
        }

        public Mod(ModContentPack content) : base(content)
        {
            ModDebug("Mod Initialised!");
            settings = GetSettings<CombatReadinessSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var standard = new Listing_Standard();
            standard.Begin(inRect);
            standard.Label("Sarge945.CombatReadinessDefaultLabel".Translate());
            settings.DefaultCombatOutfitName = standard.TextEntry(settings.DefaultCombatOutfitName);
            if (DefensivePositionsInstalled())
                standard.CheckboxLabeled("Sarge945.CombatReadinessShowGizmosLabel".Translate(),ref settings.ShowBothGizmos);
            standard.End();
            
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Sarge945.CombatReadinessModName".Translate();
        }
    }
}