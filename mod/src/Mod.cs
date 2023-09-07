using Verse;

namespace CombatReadiness
{
    [StaticConstructorOnStartup]
    public class Mod
    {
        static Mod()
        {
            ModDebug("Mod Initialised!",true);
        }

        /// <summary>
        /// Simple logging function
        /// </summary>
        public static void ModDebug(string text, bool always = false)
        {
            Log.Message($"[Combat Readiness] {text}");
        }
    }
}