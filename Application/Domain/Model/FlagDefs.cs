namespace TESVSnip.Domain.Model
{
    using System;

    internal static class FlagDefs
    {
        public static readonly string[] RecFlags1 =
            {
                "ESM file", null, null, null, null, "Deleted", "Constant/HiddenFromLocalMap/BorderRegion/HasTreeLOD", "Localized/IsPerch/AddOnLODObject/TurnOffFire/TreatSpellsAsPowers", "MustUpdateAnims/Inaccessible/DoesntLightWater", "HiddenFromLocalMap/StartsDead/MotionBlurCastsShadows", "PersistentReference/QuestItem/DisplaysInMainMenu", "Initially disabled"
                , "Ignored", null, null, "Visible when distant", "RandomAnimationStart/NeverFades/IsfullLOD", "Dangerous/OffLimits(Interior cell)/DoesntLightLandscape/HighDetailLOD/CanHoldNPC", "Compressed", "CantWait/HasCurrents", "IgnoreObjectInteraction"
                , null, null, "IsMarker", null, "Obstacle/NoAIAcquire", "NavMeshFilter", "NavMeshBoundingBox", "MustExitToTalk/ShowInWorldMap", "ChildCanUse/DontHavokSettle", "NavMeshGround NoRespawn", "MultiBound"
            };

        public static string GetRecFlags1Desc(uint flags)
        {
            string desc = string.Empty;
            bool b = false;
            long brr; //brr = bit rotation result

            for (int i = 0; i < 32; i++)
            {
                brr = (int) (1 << i);
                if (brr < 0) //brr < 0 when 1 << 31 = overflow 
                    brr = Convert.ToInt64(0x80000000); //Header Flags : (REFR) MultiBound 0x80000000; 
                if ((flags & brr) > 0)
                {
                    if (b) desc += ", ";

                    b = true;
                    desc += RecFlags1[i] == null ? "Unknown (" + brr.ToString("x") + ")" : RecFlags1[i];
                }
            }

            return desc;
        }
    }
}