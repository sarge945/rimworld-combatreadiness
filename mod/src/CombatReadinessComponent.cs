using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatReadiness
{
    public class CombatReadinessCommand : Command
    {
        private readonly CombatReadinessComp thingComp;

        public const string ICON_DEFAULT = "CombatReadiness1";
        public const string ICON_CUSTOM = "CombatReadiness2";

        public void UpdateIcon(string newIcon) => icon = ContentFinder<Texture2D>.Get(newIcon);

        public CombatReadinessCommand(CombatReadinessComp thingComp)
        {
            defaultDesc = "Sarge945.CombatReadinessGizmoDesc".Translate();
            defaultLabel = "Sarge945.CombatReadinessGizmoText".Translate();
            this.thingComp = thingComp;
        }

        public override bool Visible
        {
            get
            {
                var selectedPawns = Find.Selector?.SelectedPawns;
                return selectedPawns != null && selectedPawns.Any(p => p.drafter is {ShowDraftGizmo: true, Drafted: false} && p.IsColonistPlayerControlled && !p.Downed && !p.Deathresting);
            }
        }
        
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            SoundDefOf.Click.PlayOneShotOnCamera();
            SetTarget();
            Mod.ModDebug("Clicked Gizmo");
        }

        //Create a targeter and pass it to our execute function
        void SetTarget()
        {
            var targetParams = new TargetingParameters {canTargetLocations = true};
            Find.Targeter.BeginTargeting(targetParams, Execute); 
        }
        
        void Execute(LocalTargetInfo targetInfo)
        {
            foreach (var pawn in Find.Selector.SelectedPawns.Where(p => p.IsColonistPlayerControlled && !p.drafter.Drafted))
            {
                Mod.ModDebug($"Pawn {pawn.Name} Targeted - Assigning Job");
            
                //Draft Pawn
                if (!pawn.drafter.Drafted)
                    pawn.drafter.Drafted = true;

                //Add a new job to get ready for combat
                pawn.jobs.TryTakeOrderedJob(new Job(CombatReadinessJobDriver.JobDef, targetInfo), JobTag.DraftedOrder);
            }
            Find.Targeter.StopTargeting();
        }
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                if (thingComp.HasOutfitSet)
                    yield return new FloatMenuOption($"Use Global Setting {thingComp.DefaultOutfit}", () => thingComp.CombatOutfit = null);
                foreach (var outfit in Current.Game.outfitDatabase.AllOutfits)
                {
                    string suffix = thingComp.HasOutfitSet && outfit == thingComp.CombatOutfit ? " (Selected)" : string.Empty;
                    yield return new FloatMenuOption(outfit.label + suffix, () => thingComp.CombatOutfit = outfit);
                }
            }
        }
    }
    
    public class CombatReadinessComp : ThingComp
    {
        private string combatOutfit = string.Empty;
        private string previousOutfit = string.Empty;

        private readonly string defaultOutfit;

        public string DefaultOutfit => defaultOutfit;

        public CombatReadinessComp()
        {
            defaultOutfit = LoadedModManager.GetMod<Mod>()?.GetSettings<CombatReadinessSettings>()?.DefaultCombatOutfitName;
        }

        public bool HasOutfitSet => !string.IsNullOrEmpty(combatOutfit);
        
        public Outfit CombatOutfit
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

        public Outfit PreviousOutfit
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
            var command = new CombatReadinessCommand(this);
            {
                command.UpdateIcon(string.IsNullOrEmpty(combatOutfit)
                    ? CombatReadinessCommand.ICON_DEFAULT
                    : CombatReadinessCommand.ICON_CUSTOM);
            }

            yield return command;
        }
    }
}