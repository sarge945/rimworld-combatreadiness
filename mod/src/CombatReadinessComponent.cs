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
        
        public CombatReadinessCommand(CombatReadinessComp thingComp)
        {
            defaultDesc = "Get ready for Combat";
            defaultLabel = "Combat Readiness";
            this.thingComp = thingComp;
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
                foreach (var outfit in Current.Game.outfitDatabase.AllOutfits) 
                {
                    //if (Event.current.shift)
                        //yield return new FloatMenuOption($"{outfit.label} (Global)", () => CombatReadinessJobDriver.CombatOutfit = outfit);
                    //else
                        yield return new FloatMenuOption(outfit.label, () => thingComp.CombatOutfit = outfit);
                }
            }
        }
    }
    
    public class CombatReadinessComp : ThingComp
    {
        private string combatOutfit = "";
        private string previousOutfit = "";
        public Outfit CombatOutfit {
            get
            {
                var combatOutfitObj = Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == combatOutfit);
                return combatOutfitObj ?? Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == "Soldier");
            }
            set => combatOutfit = value.label; }

        public Outfit PreviousOutfit
        {
            get
            {
                var prevOutfitObject = Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == previousOutfit);
                return prevOutfitObject ?? Current.Game.outfitDatabase.AllOutfits.FirstOrDefault(x => x.label == "Worker");
            }
            set => previousOutfit = value.label;
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
            yield return new CombatReadinessCommand(this);
        }
    }
}