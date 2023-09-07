using Verse;

namespace CombatReadiness
{
    public class Mod : Verse.Mod
    {
        /// <summary>
        /// Simple logging function
        /// </summary>
        public static void ModDebug(string text, bool always = false)
        {
            Log.Message($"[Combat Readiness] {text}");
        }

        public Mod(ModContentPack content) : base(content)
        {
            ModDebug("Mod Initialised!",true);
        }
    }
}