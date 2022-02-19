using System.Collections.Generic;
using VRage.Game.Components;


namespace BeamSystem.SessionComponents
{
    [MySessionComponentDescriptor(updateOrder: MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    class DelayedJobScheduler : MySessionComponentBase
    {
        class DelayedJob
        {
            private static readonly Stack<DelayedJob> cached = new Stack<DelayedJob>();


            long delay;
            System.Action task;

            public static DelayedJob Allocate(long delay, System.Action task)
            {
                DelayedJob instance;
                if (cached.Count > 0)
                    instance = cached.Pop();
                else
                    instance = new DelayedJob();
                instance.delay = delay;
                instance.task = task;
                return instance;
            }

            public bool Do()
            {
                if (--delay > 0) return true;
                task();
                task = null;
                cached.Push(this);
                return false;
            }
        }

        private int tick;
        private static List<DelayedJob> delayedJobs = new List<DelayedJob>();
        private static List<DelayedJob> nextDelayedJobs = new List<DelayedJob>();


        public override void UpdateAfterSimulation()
        {
            tick = (tick + 1) % Settings.UpdateTick;
            if (tick == 0)
            {
                foreach (var job in delayedJobs)
                    if (job.Do())
                        nextDelayedJobs.Add(job);
                delayedJobs.Clear();
                var a = delayedJobs;
                delayedJobs = nextDelayedJobs;
                nextDelayedJobs = a;
            }
        }

        public static void AddDelayedJob(long delay, System.Action task)
            => nextDelayedJobs.Add(DelayedJob.Allocate(delay, task));

    }
}
