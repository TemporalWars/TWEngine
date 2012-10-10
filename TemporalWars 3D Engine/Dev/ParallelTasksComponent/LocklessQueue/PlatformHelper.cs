using System;

namespace ImageNexus.BenScharbach.TWTools.ParallelTasksComponent.LocklessQueue
{
    internal static class PlatformHelper
    {
        // Fields
        private const int PROCESSOR_COUNT_REFRESH_INTERVAL_MS = 0x7530;
        private static DateTime s_nextProcessorCountRefreshTime = DateTime.MinValue;
        private static int s_processorCount = -1;

        // Properties
        internal static bool IsSingleProcessor
        {
            get
            {
                return (ProcessorCount == 1);
            }
        }

        internal static int ProcessorCount
        {
            get
            {
                if (DateTime.UtcNow.CompareTo(s_nextProcessorCountRefreshTime) >= 0)
                {
                    s_processorCount = Environment.ProcessorCount;
                    s_nextProcessorCountRefreshTime = DateTime.UtcNow.AddMilliseconds(30000.0);
                }
                return s_processorCount;
            }
        }
    }


}
