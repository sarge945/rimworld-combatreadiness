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
        
        //Reserve an outfit
        void GetOutfitted()
        {
            if (pawn.thinker == null)
                return;
            
            pawn.drafter.Drafted = true;
            
            Mod.ModDebug($"Job - {pawn.Name} Outfitting...");
            pawn.mindState?.Notify_OutfitChanged();
            var mainTreeThinkNode = pawn.thinker.TryGetMainTreeThinkNode<JobGiver_OptimizeApparel>();

            if (mainTreeThinkNode == null)
                return;
        
            Mod.ModDebug($"Job - Main think node found. Looking for Wear job...");
            ThinkResult thinkResult = mainTreeThinkNode.TryIssueJobPackage(pawn, new JobIssueParams());
            Job job = thinkResult.Job;

            //If we have a valid job, reserve the object and re-queue the same job
            if (thinkResult != ThinkResult.NoJob)
            {

                if (job?.def == JobDefOf.Wear)
                {
                    Mod.ModDebug($"Job - Found Wear job...");
                    pawn.Reserve(job.targetA, job);
                    pawn.jobs.jobQueue.EnqueueFirst(new Job(JobDef, TargetA));
                    pawn.jobs.jobQueue.EnqueueFirst(job);
                }
            }
            else
            {
                pawn.jobs.jobQueue.EnqueueFirst(new Job(JobDefOf.Goto,TargetA));
                Mod.ModDebug($"Job - No wear job found... Nothing to do or no valid wearables in a stockpile zone?");
            }
        }

        //Draft and go to final location
        void GoToJobLocation()
        {
            Mod.ModDebug($"Job - {pawn.Name} going to final location...");
            pawn.drafter.Drafted = true;
            Mod.ModDebug("1");
            IntVec3 destination = RCellFinder.BestOrderedGotoDestNear(this.TargetA.Cell, pawn);
            Mod.ModDebug("2");
            var newJob = new Job(JobDefOf.Goto,destination);
            Mod.ModDebug("3");
            if (pawn.Map.exitMapGrid.IsExitCell(destination))
                newJob.exitMapOnArrival = true;
            Mod.ModDebug("4");
            pawn.jobs.StartJob(newJob, JobCondition.Succeeded);
            //pawn.jobs.jobQueue.EnqueueFirst(newJob);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //yield return new Toil {initAction = GoToJobLocation};
            yield return new Toil {initAction = GetOutfitted};
        }
    }
}