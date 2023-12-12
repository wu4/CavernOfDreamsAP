using HarmonyLib;

namespace CoDArchipelago
{
    static class Extensions
    {
        static readonly AccessTools.FieldRef<Timer, float> maxAccess = AccessTools.FieldRefAccess<Timer, float>("max");
        static public void SetMax(this Timer timer, float value) => maxAccess(timer) = value;

        static readonly AccessTools.FieldRef<Raise, Timer> raiseTimerAccess = AccessTools.FieldRefAccess<Raise, Timer>("raiseTimer");
        static public Timer GetTimer(this Raise raise) => raiseTimerAccess(raise);

        static readonly AccessTools.FieldRef<GrowFromNothingActivation, Timer> growTimerAccess = AccessTools.FieldRefAccess<GrowFromNothingActivation, Timer>("growTimer");
        static public Timer GetTimer(this GrowFromNothingActivation grow) => growTimerAccess(grow);
    }
}