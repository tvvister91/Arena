using PE.Framework.Models;
using System;
using System.Threading.Tasks;

namespace PE.Plugins.Network.Contracts
{
    #region Enumerations

    public enum NetworkConnectionStates
    {
        None,
        CellularSlow,
        CellularFast,
        Wifi,
        Unknown,
        Ethernet
    }

    #endregion Enumerations

    public interface INetworkService
    {
        #region Events

        event EventHandler OnConnectivityChanged;

        #endregion Events

        #region Properties

        NetworkConnectionStates ConnectionState { get; set; }

        string BaseUrl { get; set; }

        string SecondaryUrl { get; set; }

        bool IsOnSecondary { get; set; }

        bool IsFailover { get; set; }

        NetworkConfiguration Configuration { get; }

        #endregion Properties

        #region Methods

        Task GetNetworksAsync(bool repeat, int intervalMs, Action<ServiceResult> complete);

        void StopNetworkMonitor();

        void Reset();

        #endregion Methods
    }
}
