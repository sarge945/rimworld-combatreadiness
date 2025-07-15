using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CombatReadiness
{
    public class CombatReadinessComp : ThingComp
    {
        private string combatOutfit = string.Empty;
        private string previousOutfit = string.Empty;
        private bool showBothGizmos;

        private string defaultOutfit;

        public string DefaultOutfit => defaultOutfit;

        public bool HasDefensivePositions;

        public CombatReadinessComp()
        {
            HasDefensivePositions = Mod.DefensivePositionsInstalled();
        }

        public bool HasOutfitSet => !string.IsNullOrEmpty(combatOutfit);
        
        public ApparelPolicy CombatOutfit
        {
            get
            {
                var outfit = combatOutfit;
                if (string.IsNullOrEmpty(outfit))
                    outfit = defaultOutfit;
                
                return Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == outfit);
            }
            set => combatOutfit = value?.label;
        }

        public ApparelPolicy PreviousOutfit
        {
            get => Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == previousOutfit);
            set => previousOutfit = value?.label;
        }

        public override void PostExposeData()
        {
            //Mod.ModDebug($"Saving Data for pawn: {this.parent.Label} (combatOutfit: {CombatOutfit})");
            Scribe_Values.Look(ref combatOutfit, "combatOutfit", "");
            Scribe_Values.Look(ref previousOutfit, "previousOutfit", "");
            base.PostExposeData();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Mod.ModDebug("Updating Gizmos");
            defaultOutfit = LoadedModManager.GetMod<Mod>()?.GetSettings<CombatReadinessSettings>()?.DefaultCombatOutfitName;
            showBothGizmos = LoadedModManager.GetMod<Mod>()?.GetSettings<CombatReadinessSettings>()?.ShowBothGizmos == true;
            
            //First, add the Defensive Positions version of the gizmo
            if (HasDefensivePositions)
            {
                var command = new CombatReadinessCommand2(this);
                command.UpdateIcon(string.IsNullOrEmpty(combatOutfit)
                    ? command.ICON_DEFAULT
                    : command.ICON_CUSTOM);
                yield return command;
            }

            //Otherwise, add the other one
            if (!HasDefensivePositions || showBothGizmos)
            {
                var command = new CombatReadinessCommand(this);
                command.UpdateIcon(string.IsNullOrEmpty(combatOutfit)
                    ? command.ICON_DEFAULT
                    : command.ICON_CUSTOM);
                yield return command;
            }

        }
    }
}