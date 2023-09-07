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
            foreach (var pawn in Find.Selector.SelectedPawns.Where(p => p.IsColonistPlayerControlled))
            {
                Mod.ModDebug($"Pawn {pawn.Name} Starting Job");
                pawn.jobs.TryTakeOrderedJob(new Job(CombatReadinessJobDriver.JobDef, targetInfo), JobTag.DraftedOrder);
            }
            Find.Targeter.StopTargeting();
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