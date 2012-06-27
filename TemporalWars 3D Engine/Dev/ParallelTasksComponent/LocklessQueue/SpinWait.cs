using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace ParallelTasksComponent.LocklessQueue
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpinWait
    {
        internal const int YIELD_THRESHOLD = 10;
        internal const int SLEEP_0_EVERY_HOW_MANY_TIMES = 5;
        internal const int SLEEP_1_EVERY_HOW_MANY_TIMES = 20;
        private int m_count;

        public int Count
        {
            //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return m_count;
            }
        }
        public bool NextSpinWillYield
        {
            get
            {
                if (m_count <= 10)
                {
                    return PlatformHelper.IsSingleProcessor;
                }
                return true;
            }
        }
        public void SpinOnce()
        {
            if (NextSpinWillYield)
            {
                //CdsSyncEtwBCLProvider.Log.SpinWait_NextSpinWillYield();
                var num = (m_count >= 10) ? (m_count - 10) : m_count;
                if ((num % 20) == 19)
                {
                    Thread.Sleep(1);
                }
                else 
                {
                    Thread.Sleep(0);
                }
                
            }
            else
            {
#if !XBOX360
                Thread.SpinWait(4 << m_count);
#else
                Thread.Sleep(10);
#endif
            }
            m_count = (m_count == 2147483647) ? 10 : (m_count + 1);
        }

        public void Reset()
        {
            m_count = 0;
        }
       
    }


}
