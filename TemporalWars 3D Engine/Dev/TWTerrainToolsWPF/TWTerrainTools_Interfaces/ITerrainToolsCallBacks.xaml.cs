using System.ServiceModel;

namespace TWTerrainTools_Interfaces
{
    // 6/30/2010 - Callbacks for WCF (Essentially Events)
    /// <summary>
    /// The <see cref="ITerrainToolsCallBacks"/> interface describes the Callbacks supported by this library via WCF.
    /// </summary>
    public interface ITerrainToolsCallBacks
    {
        /// <summary>
        /// Occurs when the specified timer interval has elapsed and
        /// the timer is enabled.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void TickCallBack();

        /// <summary>
        /// Occurs when the button 'ApplyHeightMap' is clicked.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ApplyHeightMapCallBack();

        /// <summary>
        /// Occurs when the button 'CreateMap1024x1024' is clicked.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void CreateMap1024CallBack();

        /// <summary>
        /// Occurs when the button 'GeneratePerlinNoise'is clicked.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void GeneratePerlinNoiseCallBack();
    }
}