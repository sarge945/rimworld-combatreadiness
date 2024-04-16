using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CombatReadiness
{
    public class CombatReadinessJobDriver : JobDriver
    {
        public static JobDef JobDef => DefDatabase<JobDef>.GetNamed("CombatReadinessJob");

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //throw new System.NotImplementedException();
            return true;
        }
        
        void GetOutfitted()
        {
            if (pawn?.thinker == null || pawn?.Map == null)
                return;

            var combatReadinessComponent = pawn.GetComp<CombatReadinessComp>();
            if (combatReadinessComponent == null)
                return;
            
            Mod.ModDebug($"Job - {pawn.Name} Outfitting...");

            //Set up outfit control
            if (pawn.outfits.CurrentApparelPolicy != combatReadinessComponent.CombatOutfit)
            {
                combatReadinessComponent.PreviousOutfit = pawn.outfits.CurrentApparelPolicy;
                pawn.outfits.CurrentApparelPolicy = combatReadinessComponent.CombatOutfit;
            }

            var mainTreeThinkNode = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>();
            if (mainTreeThinkNode == null)
                return;
        
            Mod.ModDebug($"Job - Main think node found. Looking for Wear job...");
            ThinkResult thinkResult = mainTreeThinkNode.TryIssueJobPackage(pawn, new JobIssueParams());
            Job job = thinkResult.Job;

            //If we have a valid job, reserve the object and re-queue the same job
            if (thinkResult != ThinkResult.NoJob)
            {
                if (job?.def == JobDefOf.Wear || job?.def == JobDefOf.RemoveApparel)
                {
                    Mod.ModDebug($"Job - Found Wear job...");
                    pawn.Reserve(job.targetA, job);
                    FleckMaker.Static(job.targetA.Cell, pawn.Map, FleckDefOf.FeedbackEquip);
                    pawn.jobs.jobQueue.EnqueueFirst(new Job(JobDef, TargetA));
                    pawn.jobs.jobQueue.EnqueueFirst(job);
                    //pawn.jobs.TryTakeOrderedJob(new Job(JobDef, TargetA), JobTag.DraftedOrder, true);
                    //pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder, true);
                    return;
                }
            }
            else
            {
                Mod.ModDebug($"Job - No wear job found... Nothing to do or no valid wearables in a stockpile zone?");
            }
            
            pawn.jobs.jobQueue.EnqueueLast(new Job(JobDefOf.Goto,TargetA));
            FleckMaker.Static(TargetA.Cell, pawn.Map, FleckDefOf.FeedbackGoto);
            //pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Goto, TargetA), JobTag.DraftedOrder, true);
            Mod.ModDebug("Job - Moving to destination");
        }

        private void OnJobComplete()
        {
            var combatReadinessComponent = pawn.GetComp<CombatReadinessComp>();
            if (combatReadinessComponent == null)
                return;
            
            Mod.ModDebug("Job - Finished Job");
            pawn.outfits.CurrentApparelPolicy = combatReadinessComponent.PreviousOutfit;
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var T = new Toil {initAction = GetOutfitted};
            T.AddFinishAction(OnJobComplete);
            yield return T;
        }
    }
}