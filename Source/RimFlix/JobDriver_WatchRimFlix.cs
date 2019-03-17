using Verse;
using RimWorld;

namespace RimFlix
{
    class JobDriver_WatchRimFlix : JobDriver_WatchTelevision
    {        
        protected override void WatchTickAction()
        {
            Building thing = (Building) base.TargetA.Thing;

            // Todo: add try-catch (TryGetComp?) in case another mod adds a new tv type
            RimFlix.CompScreen screen = thing.GetComp<RimFlix.CompScreen>();
            screen.SleepTimer = 10;

            base.WatchTickAction();
        }
    }
}
