using System.Threading;

namespace ImageNexus.BenScharbach.TWTools.ParallelTasksComponent.LocklessQueue 
{
    /// <summary>
    /// 
    /// </summary>
    public static class SyncHelper
    {
        /// <summary>
        /// Helper method which uses the internal Thread's <see cref="Interlocked.CompareExchange{T}"/> method call.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="location"></param>
        /// <param name="comparand"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static bool CompareAndExchange<T>(ref T location, T comparand, T newValue) where T : class
        {
            return
                comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
        }
    }
}
