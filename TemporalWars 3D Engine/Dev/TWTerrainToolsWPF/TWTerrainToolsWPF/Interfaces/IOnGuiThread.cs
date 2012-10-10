using System;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Interfaces
{
    // 2/4/2011
    public interface IOnGuiThread
    {
        void OnGuiThreadMethodCall<T>(Action<T> action, T paramItem) where T : class;
        
    }
}