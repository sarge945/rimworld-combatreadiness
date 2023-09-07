using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CombatReadiness
{
    public class CombatReadinessCommand : Command
    {
        private readonly CombatReadinessComp thingComp;

        private const string ICON = "CombatReadiness1";

        private void UpdateIcon(string newIcon) => icon = ContentFinder<Texture2D>.Get(newIcon);

        public CombatReadinessCommand()
        {
            hotKey = KeyBindingDef.Named("R");
            UpdateIcon(ICON);
            defaultDesc = "Sarge945.CombatReadinessGizmoDesc".Translate();
            defaultLabel = "Sarge945.CombatReadinessGizmoText".Translate();
        }

        public override bool Visible => Find.Selector.SelectedPawns.Any(p => !p.drafter.Drafted);

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
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
                yield return new FloatMenuOption("(Use Global Setting)", () => thingComp.CombatOutfit = null);
                foreach (var outfit in Current.Game.outfitDatabase.AllOutfits) 
                {
                    yield return new FloatMenuOption(outfit.label, () => thingComp.CombatOutfit = outfit);
                }
            }
        }
    }
    
    public class CombatReadinessComp : ThingComp
    {
        private string combatOutfit = "";
        private string previousOutfit = "";
        public Outfit CombatOutfit
        {
            get
            {
                string outfit = combatOutfit;
                if (string.IsNullOrEmpty(outfit))
                    outfit = LoadedModManager.GetMod<Mod>().GetSettings<CombatReadinessSettings>().DefaultCombatOutfitName;
                
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
            Mod.ModDebug($"Saving Data for pawn: {this.parent.Label} (combatOutfit: {CombatOutfit})");
            base.PostExposeData();
            Scribe_Values.Look(ref combatOutfit,"combatOutfit","");
            Scribe_Values.Look(ref previousOutfit,"previousOutfit","");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new CombatReadinessCommand();
        }
    }
}