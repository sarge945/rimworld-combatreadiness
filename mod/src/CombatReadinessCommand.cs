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
        public CombatReadinessCommand()
        {
            defaultDesc = "Get ready for Combat";
            defaultLabel = "Combat Readiness";
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
                Mod.ModDebug($"Pawn {pawn.Name} Starting Job");
            
                //Draft colonist
                if (!pawn.drafter.Drafted)
                    pawn.drafter.Drafted = true;
                
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
                    if (Event.current.shift)
                        yield return new FloatMenuOption($"{outfit.label} (Global)", () => CombatReadinessJobDriver.CombatOutfit = outfit);
                    else
                        yield return new FloatMenuOption(outfit.label, () => CombatReadinessJobDriver.CombatOutfit = outfit);
                }
            }
        }
    }
    
    public class CombatReadinessGizmo : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new CombatReadinessCommand();
        }
    }
}