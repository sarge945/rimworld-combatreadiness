using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CombatReadiness
{
    public abstract class CombatReadinessCommandBase : Command
    {
        private readonly CombatReadinessComp thingComp;

        public abstract string ICON_DEFAULT { get; }
        public abstract string ICON_CUSTOM { get; }
        
        public void UpdateIcon(string newIcon) => icon = ContentFinder<Texture2D>.Get(newIcon);
        
        protected abstract JobDef JobDef { get; }

        protected CombatReadinessCommandBase(CombatReadinessComp thingComp) => this.thingComp = thingComp;

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
            //Create a target if we don't have defensive positions, otherwise just work instantly
            SetTarget();
            Mod.ModDebug("Clicked Gizmo");
        }

        //Create a targeter and pass it to our execute function
        protected abstract void SetTarget();
        protected abstract void StopTargeting();
        
        protected void Execute(LocalTargetInfo targetInfo)
        {
            foreach (var pawn in Find.Selector.SelectedPawns.Where(p => p.IsColonistPlayerControlled && !p.drafter.Drafted))
            {
                Mod.ModDebug($"Pawn {pawn.Name} Targeted - Assigning Job");
            
                //Draft Pawn
                if (!pawn.drafter.Drafted)
                    pawn.drafter.Drafted = true;

                //Add a new job to get ready for combat
                pawn.jobs.TryTakeOrderedJob(new Job(JobDef, targetInfo), JobTag.DraftedOrder);
            }
            StopTargeting();
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

    public class CombatReadinessCommand : CombatReadinessCommandBase
    {
        public override string ICON_DEFAULT => "CombatReadiness1";
        public override string ICON_CUSTOM => "CombatReadiness2";
        protected override JobDef JobDef => DefDatabase<JobDef>.GetNamed("CombatReadinessJob");
        public CombatReadinessCommand(CombatReadinessComp thingComp) : base(thingComp)
        {
            defaultLabel = ("Sarge945.CombatReadinessGizmoText").Translate();
            defaultDesc = ("Sarge945.CombatReadinessGizmoDesc").Translate();
        }

        //Create a targeter and pass it to our execute function
        protected override void SetTarget()
        {
            var targetParams = new TargetingParameters {canTargetLocations = true};
            Find.Targeter.BeginTargeting(targetParams, Execute); 
        }

        protected override void StopTargeting()
        {
            Find.Targeter.StopTargeting();
        }

    }
    public class CombatReadinessCommand2 : CombatReadinessCommandBase
    {
        public override string ICON_DEFAULT => "CombatReadinessDP1";
        public override string ICON_CUSTOM => "CombatReadinessDP2";
        protected override JobDef JobDef => DefDatabase<JobDef>.GetNamed("CombatReadinessJobDefensive");
        public CombatReadinessCommand2(CombatReadinessComp thingComp) : base(thingComp)
        {
            defaultLabel = ("Sarge945.CombatReadinessGizmoText2").Translate();
            defaultDesc = ("Sarge945.CombatReadinessGizmoDesc2").Translate();
        }

        //Don't Create a targeter, just execute straight away
        protected override void SetTarget() => Execute(LocalTargetInfo.Invalid);

        protected override void StopTargeting() { }

    }
}