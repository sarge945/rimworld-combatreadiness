using UnityEngine;
using Verse;

namespace CombatReadiness
{
    public class CombatReadinessJobDefensiveDriver : CombatReadinessJobDriver
    {
        protected override JobDef JobDef => DefDatabase<JobDef>.GetNamed("CombatReadinessJobDefensive");
        protected override void OrderToFinalLocation()
        {
            var g = Mod.GetDefensivePositionsGizmo(pawn);
            if (g == null)
                return;

            Mod.ModDebug("Found Defensive Positions Gizmo - Sending to position");
            g.ProcessInput(Event.current);
        }
    }
}