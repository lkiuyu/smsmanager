using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace ModemManager1.DBus
{
    [DBusInterface("org.freedesktop.DBus.ObjectManager")]
    interface IObjectManager : IDBusObject
    {
        Task<IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>> GetManagedObjectsAsync();
        Task<IDisposable> WatchInterfacesAddedAsync(Action<(ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfacesAndProperties)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchInterfacesRemovedAsync(Action<(ObjectPath objectPath, string[] interfaces)> handler, Action<Exception> onError = null);
    }

    [DBusInterface("org.freedesktop.ModemManager1")]
    interface IModemManager1 : IDBusObject
    {
        Task ScanDevicesAsync();
        Task SetLoggingAsync(string Level);
        Task ReportKernelEventAsync(IDictionary<string, object> Properties);
        Task InhibitDeviceAsync(string Uid, bool Inhibit);
        Task<T> GetAsync<T>(string prop);
        Task<ModemManager1Properties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class ModemManager1Properties
    {
        private string _version = default(string);
        public string Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = (value);
            }
        }
    }

    static class ModemManager1Extensions
    {
        public static Task<string> GetVersionAsync(this IModemManager1 o) => o.GetAsync<string>("Version");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Simple")]
    interface ISimple : IDBusObject
    {
        Task<ObjectPath> ConnectAsync(IDictionary<string, object> Properties);
        Task DisconnectAsync(ObjectPath Bearer);
        Task<IDictionary<string, object>> GetStatusAsync();
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Location")]
    interface ILocation : IDBusObject
    {
        Task SetupAsync(uint Sources, bool SignalLocation);
        Task<IDictionary<uint, object>> GetLocationAsync();
        Task SetSuplServerAsync(string Supl);
        Task InjectAssistanceDataAsync(byte[] Data);
        Task SetGpsRefreshRateAsync(uint Rate);
        Task<T> GetAsync<T>(string prop);
        Task<LocationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class LocationProperties
    {
        private uint _capabilities = default(uint);
        public uint Capabilities
        {
            get
            {
                return _capabilities;
            }

            set
            {
                _capabilities = (value);
            }
        }

        private uint _supportedAssistanceData = default(uint);
        public uint SupportedAssistanceData
        {
            get
            {
                return _supportedAssistanceData;
            }

            set
            {
                _supportedAssistanceData = (value);
            }
        }

        private uint _enabled = default(uint);
        public uint Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                _enabled = (value);
            }
        }

        private bool _signalsLocation = default(bool);
        public bool SignalsLocation
        {
            get
            {
                return _signalsLocation;
            }

            set
            {
                _signalsLocation = (value);
            }
        }

        private IDictionary<uint, object> _location = default(IDictionary<uint, object>);
        public IDictionary<uint, object> Location
        {
            get
            {
                return _location;
            }

            set
            {
                _location = (value);
            }
        }

        private string _suplServer = default(string);
        public string SuplServer
        {
            get
            {
                return _suplServer;
            }

            set
            {
                _suplServer = (value);
            }
        }

        private string[] _assistanceDataServers = default(string[]);
        public string[] AssistanceDataServers
        {
            get
            {
                return _assistanceDataServers;
            }

            set
            {
                _assistanceDataServers = (value);
            }
        }

        private uint _gpsRefreshRate = default(uint);
        public uint GpsRefreshRate
        {
            get
            {
                return _gpsRefreshRate;
            }

            set
            {
                _gpsRefreshRate = (value);
            }
        }
    }

    static class LocationExtensions
    {
        public static Task<uint> GetCapabilitiesAsync(this ILocation o) => o.GetAsync<uint>("Capabilities");
        public static Task<uint> GetSupportedAssistanceDataAsync(this ILocation o) => o.GetAsync<uint>("SupportedAssistanceData");
        public static Task<uint> GetEnabledAsync(this ILocation o) => o.GetAsync<uint>("Enabled");
        public static Task<bool> GetSignalsLocationAsync(this ILocation o) => o.GetAsync<bool>("SignalsLocation");
        public static Task<IDictionary<uint, object>> GetLocationAsync(this ILocation o) => o.GetAsync<IDictionary<uint, object>>("Location");
        public static Task<string> GetSuplServerAsync(this ILocation o) => o.GetAsync<string>("SuplServer");
        public static Task<string[]> GetAssistanceDataServersAsync(this ILocation o) => o.GetAsync<string[]>("AssistanceDataServers");
        public static Task<uint> GetGpsRefreshRateAsync(this ILocation o) => o.GetAsync<uint>("GpsRefreshRate");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Signal")]
    interface ISignal : IDBusObject
    {
        Task SetupAsync(uint Rate);
        Task<T> GetAsync<T>(string prop);
        Task<SignalProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class SignalProperties
    {
        private uint _rate = default(uint);
        public uint Rate
        {
            get
            {
                return _rate;
            }

            set
            {
                _rate = (value);
            }
        }

        private IDictionary<string, object> _cdma = default(IDictionary<string, object>);
        public IDictionary<string, object> Cdma
        {
            get
            {
                return _cdma;
            }

            set
            {
                _cdma = (value);
            }
        }

        private IDictionary<string, object> _evdo = default(IDictionary<string, object>);
        public IDictionary<string, object> Evdo
        {
            get
            {
                return _evdo;
            }

            set
            {
                _evdo = (value);
            }
        }

        private IDictionary<string, object> _gsm = default(IDictionary<string, object>);
        public IDictionary<string, object> Gsm
        {
            get
            {
                return _gsm;
            }

            set
            {
                _gsm = (value);
            }
        }

        private IDictionary<string, object> _umts = default(IDictionary<string, object>);
        public IDictionary<string, object> Umts
        {
            get
            {
                return _umts;
            }

            set
            {
                _umts = (value);
            }
        }

        private IDictionary<string, object> _lte = default(IDictionary<string, object>);
        public IDictionary<string, object> Lte
        {
            get
            {
                return _lte;
            }

            set
            {
                _lte = (value);
            }
        }

        private IDictionary<string, object> _nr5g = default(IDictionary<string, object>);
        public IDictionary<string, object> Nr5g
        {
            get
            {
                return _nr5g;
            }

            set
            {
                _nr5g = (value);
            }
        }
    }

    static class SignalExtensions
    {
        public static Task<uint> GetRateAsync(this ISignal o) => o.GetAsync<uint>("Rate");
        public static Task<IDictionary<string, object>> GetCdmaAsync(this ISignal o) => o.GetAsync<IDictionary<string, object>>("Cdma");
        public static Task<IDictionary<string, object>> GetEvdoAsync(this ISignal o) => o.GetAsync<IDictionary<string, object>>("Evdo");
        public static Task<IDictionary<string, object>> GetGsmAsync(this ISignal o) => o.GetAsync<IDictionary<string, object>>("Gsm");
        public static Task<IDictionary<string, object>> GetUmtsAsync(this ISignal o) => o.GetAsync<IDictionary<string, object>>("Umts");
        public static Task<IDictionary<string, object>> GetLteAsync(this ISignal o) => o.GetAsync<IDictionary<string, object>>("Lte");
        public static Task<IDictionary<string, object>> GetNr5gAsync(this ISignal o) => o.GetAsync<IDictionary<string, object>>("Nr5g");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Modem3gpp.Ussd")]
    interface IUssd : IDBusObject
    {
        Task<string> InitiateAsync(string Command);
        Task<string> RespondAsync(string Response);
        Task CancelAsync();
        Task<T> GetAsync<T>(string prop);
        Task<UssdProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class UssdProperties
    {
        private uint _state = default(uint);
        public uint State
        {
            get
            {
                return _state;
            }

            set
            {
                _state = (value);
            }
        }

        private string _networkNotification = default(string);
        public string NetworkNotification
        {
            get
            {
                return _networkNotification;
            }

            set
            {
                _networkNotification = (value);
            }
        }

        private string _networkRequest = default(string);
        public string NetworkRequest
        {
            get
            {
                return _networkRequest;
            }

            set
            {
                _networkRequest = (value);
            }
        }
    }

    static class UssdExtensions
    {
        public static Task<uint> GetStateAsync(this IUssd o) => o.GetAsync<uint>("State");
        public static Task<string> GetNetworkNotificationAsync(this IUssd o) => o.GetAsync<string>("NetworkNotification");
        public static Task<string> GetNetworkRequestAsync(this IUssd o) => o.GetAsync<string>("NetworkRequest");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Messaging")]
    interface IMessaging : IDBusObject
    {
        Task<ObjectPath[]> ListAsync();
        Task DeleteAsync(ObjectPath Path);
        Task<ObjectPath> CreateAsync(IDictionary<string, object> Properties);
        Task<IDisposable> WatchAddedAsync(Action<(ObjectPath path, bool received)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchDeletedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<MessagingProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class MessagingProperties
    {
        private ObjectPath[] _messages = default(ObjectPath[]);
        public ObjectPath[] Messages
        {
            get
            {
                return _messages;
            }

            set
            {
                _messages = (value);
            }
        }

        private uint[] _supportedStorages = default(uint[]);
        public uint[] SupportedStorages
        {
            get
            {
                return _supportedStorages;
            }

            set
            {
                _supportedStorages = (value);
            }
        }

        private uint _defaultStorage = default(uint);
        public uint DefaultStorage
        {
            get
            {
                return _defaultStorage;
            }

            set
            {
                _defaultStorage = (value);
            }
        }
    }

    static class MessagingExtensions
    {
        public static Task<ObjectPath[]> GetMessagesAsync(this IMessaging o) => o.GetAsync<ObjectPath[]>("Messages");
        public static Task<uint[]> GetSupportedStoragesAsync(this IMessaging o) => o.GetAsync<uint[]>("SupportedStorages");
        public static Task<uint> GetDefaultStorageAsync(this IMessaging o) => o.GetAsync<uint>("DefaultStorage");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem")]
    interface IModem : IDBusObject
    {
        Task EnableAsync(bool Enable);
        Task<ObjectPath[]> ListBearersAsync();
        Task<ObjectPath> CreateBearerAsync(IDictionary<string, object> Properties);
        Task DeleteBearerAsync(ObjectPath Bearer);
        Task ResetAsync();
        Task FactoryResetAsync(string Code);
        Task SetPowerStateAsync(uint State);
        Task SetCurrentCapabilitiesAsync(uint Capabilities);
        Task SetCurrentModesAsync((uint, uint) Modes);
        Task SetCurrentBandsAsync(uint[] Bands);
        Task SetPrimarySimSlotAsync(uint SimSlot);
        Task<string> CommandAsync(string Cmd, uint Timeout);
        Task<IDisposable> WatchStateChangedAsync(Action<(int old, int @new, uint reason)> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<ModemProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class ModemProperties
    {
        private ObjectPath _sim = default(ObjectPath);
        public ObjectPath Sim
        {
            get
            {
                return _sim;
            }

            set
            {
                _sim = (value);
            }
        }

        private ObjectPath[] _simSlots = default(ObjectPath[]);
        public ObjectPath[] SimSlots
        {
            get
            {
                return _simSlots;
            }

            set
            {
                _simSlots = (value);
            }
        }

        private uint _primarySimSlot = default(uint);
        public uint PrimarySimSlot
        {
            get
            {
                return _primarySimSlot;
            }

            set
            {
                _primarySimSlot = (value);
            }
        }

        private ObjectPath[] _bearers = default(ObjectPath[]);
        public ObjectPath[] Bearers
        {
            get
            {
                return _bearers;
            }

            set
            {
                _bearers = (value);
            }
        }

        private uint[] _supportedCapabilities = default(uint[]);
        public uint[] SupportedCapabilities
        {
            get
            {
                return _supportedCapabilities;
            }

            set
            {
                _supportedCapabilities = (value);
            }
        }

        private uint _currentCapabilities = default(uint);
        public uint CurrentCapabilities
        {
            get
            {
                return _currentCapabilities;
            }

            set
            {
                _currentCapabilities = (value);
            }
        }

        private uint _maxBearers = default(uint);
        public uint MaxBearers
        {
            get
            {
                return _maxBearers;
            }

            set
            {
                _maxBearers = (value);
            }
        }

        private uint _maxActiveBearers = default(uint);
        public uint MaxActiveBearers
        {
            get
            {
                return _maxActiveBearers;
            }

            set
            {
                _maxActiveBearers = (value);
            }
        }

        private uint _maxActiveMultiplexedBearers = default(uint);
        public uint MaxActiveMultiplexedBearers
        {
            get
            {
                return _maxActiveMultiplexedBearers;
            }

            set
            {
                _maxActiveMultiplexedBearers = (value);
            }
        }

        private string _manufacturer = default(string);
        public string Manufacturer
        {
            get
            {
                return _manufacturer;
            }

            set
            {
                _manufacturer = (value);
            }
        }

        private string _model = default(string);
        public string Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = (value);
            }
        }

        private string _revision = default(string);
        public string Revision
        {
            get
            {
                return _revision;
            }

            set
            {
                _revision = (value);
            }
        }

        private string _carrierConfiguration = default(string);
        public string CarrierConfiguration
        {
            get
            {
                return _carrierConfiguration;
            }

            set
            {
                _carrierConfiguration = (value);
            }
        }

        private string _carrierConfigurationRevision = default(string);
        public string CarrierConfigurationRevision
        {
            get
            {
                return _carrierConfigurationRevision;
            }

            set
            {
                _carrierConfigurationRevision = (value);
            }
        }

        private string _hardwareRevision = default(string);
        public string HardwareRevision
        {
            get
            {
                return _hardwareRevision;
            }

            set
            {
                _hardwareRevision = (value);
            }
        }

        private string _deviceIdentifier = default(string);
        public string DeviceIdentifier
        {
            get
            {
                return _deviceIdentifier;
            }

            set
            {
                _deviceIdentifier = (value);
            }
        }

        private string _device = default(string);
        public string Device
        {
            get
            {
                return _device;
            }

            set
            {
                _device = (value);
            }
        }

        private string[] _drivers = default(string[]);
        public string[] Drivers
        {
            get
            {
                return _drivers;
            }

            set
            {
                _drivers = (value);
            }
        }

        private string _plugin = default(string);
        public string Plugin
        {
            get
            {
                return _plugin;
            }

            set
            {
                _plugin = (value);
            }
        }

        private string _primaryPort = default(string);
        public string PrimaryPort
        {
            get
            {
                return _primaryPort;
            }

            set
            {
                _primaryPort = (value);
            }
        }

        private (string, uint)[] _ports = default((string, uint)[]);
        public (string, uint)[] Ports
        {
            get
            {
                return _ports;
            }

            set
            {
                _ports = (value);
            }
        }

        private string _equipmentIdentifier = default(string);
        public string EquipmentIdentifier
        {
            get
            {
                return _equipmentIdentifier;
            }

            set
            {
                _equipmentIdentifier = (value);
            }
        }

        private uint _unlockRequired = default(uint);
        public uint UnlockRequired
        {
            get
            {
                return _unlockRequired;
            }

            set
            {
                _unlockRequired = (value);
            }
        }

        private IDictionary<uint, uint> _unlockRetries = default(IDictionary<uint, uint>);
        public IDictionary<uint, uint> UnlockRetries
        {
            get
            {
                return _unlockRetries;
            }

            set
            {
                _unlockRetries = (value);
            }
        }

        private int _state = default(int);
        public int State
        {
            get
            {
                return _state;
            }

            set
            {
                _state = (value);
            }
        }

        private uint _stateFailedReason = default(uint);
        public uint StateFailedReason
        {
            get
            {
                return _stateFailedReason;
            }

            set
            {
                _stateFailedReason = (value);
            }
        }

        private uint _accessTechnologies = default(uint);
        public uint AccessTechnologies
        {
            get
            {
                return _accessTechnologies;
            }

            set
            {
                _accessTechnologies = (value);
            }
        }

        private (uint, bool) _signalQuality = default((uint, bool));
        public (uint, bool) SignalQuality
        {
            get
            {
                return _signalQuality;
            }

            set
            {
                _signalQuality = (value);
            }
        }

        private string[] _ownNumbers = default(string[]);
        public string[] OwnNumbers
        {
            get
            {
                return _ownNumbers;
            }

            set
            {
                _ownNumbers = (value);
            }
        }

        private uint _powerState = default(uint);
        public uint PowerState
        {
            get
            {
                return _powerState;
            }

            set
            {
                _powerState = (value);
            }
        }

        private (uint, uint)[] _supportedModes = default((uint, uint)[]);
        public (uint, uint)[] SupportedModes
        {
            get
            {
                return _supportedModes;
            }

            set
            {
                _supportedModes = (value);
            }
        }

        private (uint, uint) _currentModes = default((uint, uint));
        public (uint, uint) CurrentModes
        {
            get
            {
                return _currentModes;
            }

            set
            {
                _currentModes = (value);
            }
        }

        private uint[] _supportedBands = default(uint[]);
        public uint[] SupportedBands
        {
            get
            {
                return _supportedBands;
            }

            set
            {
                _supportedBands = (value);
            }
        }

        private uint[] _currentBands = default(uint[]);
        public uint[] CurrentBands
        {
            get
            {
                return _currentBands;
            }

            set
            {
                _currentBands = (value);
            }
        }

        private uint _supportedIpFamilies = default(uint);
        public uint SupportedIpFamilies
        {
            get
            {
                return _supportedIpFamilies;
            }

            set
            {
                _supportedIpFamilies = (value);
            }
        }
    }

    static class ModemExtensions
    {
        public static Task<ObjectPath> GetSimAsync(this IModem o) => o.GetAsync<ObjectPath>("Sim");
        public static Task<ObjectPath[]> GetSimSlotsAsync(this IModem o) => o.GetAsync<ObjectPath[]>("SimSlots");
        public static Task<uint> GetPrimarySimSlotAsync(this IModem o) => o.GetAsync<uint>("PrimarySimSlot");
        public static Task<ObjectPath[]> GetBearersAsync(this IModem o) => o.GetAsync<ObjectPath[]>("Bearers");
        public static Task<uint[]> GetSupportedCapabilitiesAsync(this IModem o) => o.GetAsync<uint[]>("SupportedCapabilities");
        public static Task<uint> GetCurrentCapabilitiesAsync(this IModem o) => o.GetAsync<uint>("CurrentCapabilities");
        public static Task<uint> GetMaxBearersAsync(this IModem o) => o.GetAsync<uint>("MaxBearers");
        public static Task<uint> GetMaxActiveBearersAsync(this IModem o) => o.GetAsync<uint>("MaxActiveBearers");
        public static Task<uint> GetMaxActiveMultiplexedBearersAsync(this IModem o) => o.GetAsync<uint>("MaxActiveMultiplexedBearers");
        public static Task<string> GetManufacturerAsync(this IModem o) => o.GetAsync<string>("Manufacturer");
        public static Task<string> GetModelAsync(this IModem o) => o.GetAsync<string>("Model");
        public static Task<string> GetRevisionAsync(this IModem o) => o.GetAsync<string>("Revision");
        public static Task<string> GetCarrierConfigurationAsync(this IModem o) => o.GetAsync<string>("CarrierConfiguration");
        public static Task<string> GetCarrierConfigurationRevisionAsync(this IModem o) => o.GetAsync<string>("CarrierConfigurationRevision");
        public static Task<string> GetHardwareRevisionAsync(this IModem o) => o.GetAsync<string>("HardwareRevision");
        public static Task<string> GetDeviceIdentifierAsync(this IModem o) => o.GetAsync<string>("DeviceIdentifier");
        public static Task<string> GetDeviceAsync(this IModem o) => o.GetAsync<string>("Device");
        public static Task<string[]> GetDriversAsync(this IModem o) => o.GetAsync<string[]>("Drivers");
        public static Task<string> GetPluginAsync(this IModem o) => o.GetAsync<string>("Plugin");
        public static Task<string> GetPrimaryPortAsync(this IModem o) => o.GetAsync<string>("PrimaryPort");
        public static Task<(string, uint)[]> GetPortsAsync(this IModem o) => o.GetAsync<(string, uint)[]>("Ports");
        public static Task<string> GetEquipmentIdentifierAsync(this IModem o) => o.GetAsync<string>("EquipmentIdentifier");
        public static Task<uint> GetUnlockRequiredAsync(this IModem o) => o.GetAsync<uint>("UnlockRequired");
        public static Task<IDictionary<uint, uint>> GetUnlockRetriesAsync(this IModem o) => o.GetAsync<IDictionary<uint, uint>>("UnlockRetries");
        public static Task<int> GetStateAsync(this IModem o) => o.GetAsync<int>("State");
        public static Task<uint> GetStateFailedReasonAsync(this IModem o) => o.GetAsync<uint>("StateFailedReason");
        public static Task<uint> GetAccessTechnologiesAsync(this IModem o) => o.GetAsync<uint>("AccessTechnologies");
        public static Task<(uint, bool)> GetSignalQualityAsync(this IModem o) => o.GetAsync<(uint, bool)>("SignalQuality");
        public static Task<string[]> GetOwnNumbersAsync(this IModem o) => o.GetAsync<string[]>("OwnNumbers");
        public static Task<uint> GetPowerStateAsync(this IModem o) => o.GetAsync<uint>("PowerState");
        public static Task<(uint, uint)[]> GetSupportedModesAsync(this IModem o) => o.GetAsync<(uint, uint)[]>("SupportedModes");
        public static Task<(uint, uint)> GetCurrentModesAsync(this IModem o) => o.GetAsync<(uint, uint)>("CurrentModes");
        public static Task<uint[]> GetSupportedBandsAsync(this IModem o) => o.GetAsync<uint[]>("SupportedBands");
        public static Task<uint[]> GetCurrentBandsAsync(this IModem o) => o.GetAsync<uint[]>("CurrentBands");
        public static Task<uint> GetSupportedIpFamiliesAsync(this IModem o) => o.GetAsync<uint>("SupportedIpFamilies");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Voice")]
    interface IVoice : IDBusObject
    {
        Task<ObjectPath[]> ListCallsAsync();
        Task DeleteCallAsync(ObjectPath Path);
        Task<ObjectPath> CreateCallAsync(IDictionary<string, object> Properties);
        Task HoldAndAcceptAsync();
        Task HangupAndAcceptAsync();
        Task HangupAllAsync();
        Task TransferAsync();
        Task CallWaitingSetupAsync(bool Enable);
        Task<bool> CallWaitingQueryAsync();
        Task<IDisposable> WatchCallAddedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchCallDeletedAsync(Action<ObjectPath> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<VoiceProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class VoiceProperties
    {
        private ObjectPath[] _calls = default(ObjectPath[]);
        public ObjectPath[] Calls
        {
            get
            {
                return _calls;
            }

            set
            {
                _calls = (value);
            }
        }

        private bool _emergencyOnly = default(bool);
        public bool EmergencyOnly
        {
            get
            {
                return _emergencyOnly;
            }

            set
            {
                _emergencyOnly = (value);
            }
        }
    }

    static class VoiceExtensions
    {
        public static Task<ObjectPath[]> GetCallsAsync(this IVoice o) => o.GetAsync<ObjectPath[]>("Calls");
        public static Task<bool> GetEmergencyOnlyAsync(this IVoice o) => o.GetAsync<bool>("EmergencyOnly");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Time")]
    interface ITime : IDBusObject
    {
        Task<string> GetNetworkTimeAsync();
        Task<IDisposable> WatchNetworkTimeChangedAsync(Action<string> handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<TimeProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class TimeProperties
    {
        private IDictionary<string, object> _networkTimezone = default(IDictionary<string, object>);
        public IDictionary<string, object> NetworkTimezone
        {
            get
            {
                return _networkTimezone;
            }

            set
            {
                _networkTimezone = (value);
            }
        }
    }

    static class TimeExtensions
    {
        public static Task<IDictionary<string, object>> GetNetworkTimezoneAsync(this ITime o) => o.GetAsync<IDictionary<string, object>>("NetworkTimezone");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Modem3gpp")]
    interface IModem3gpp : IDBusObject
    {
        Task RegisterAsync(string OperatorId);
        Task<IDictionary<string, object>[]> ScanAsync();
        Task SetEpsUeModeOperationAsync(uint Mode);
        Task SetInitialEpsBearerSettingsAsync(IDictionary<string, object> Settings);
        Task DisableFacilityLockAsync((uint, string) Properties);
        Task<T> GetAsync<T>(string prop);
        Task<Modem3gppProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class Modem3gppProperties
    {
        private string _imei = default(string);
        public string Imei
        {
            get
            {
                return _imei;
            }

            set
            {
                _imei = (value);
            }
        }

        private uint _registrationState = default(uint);
        public uint RegistrationState
        {
            get
            {
                return _registrationState;
            }

            set
            {
                _registrationState = (value);
            }
        }

        private string _operatorCode = default(string);
        public string OperatorCode
        {
            get
            {
                return _operatorCode;
            }

            set
            {
                _operatorCode = (value);
            }
        }

        private string _operatorName = default(string);
        public string OperatorName
        {
            get
            {
                return _operatorName;
            }

            set
            {
                _operatorName = (value);
            }
        }

        private uint _enabledFacilityLocks = default(uint);
        public uint EnabledFacilityLocks
        {
            get
            {
                return _enabledFacilityLocks;
            }

            set
            {
                _enabledFacilityLocks = (value);
            }
        }

        private uint _subscriptionState = default(uint);
        public uint SubscriptionState
        {
            get
            {
                return _subscriptionState;
            }

            set
            {
                _subscriptionState = (value);
            }
        }

        private uint _epsUeModeOperation = default(uint);
        public uint EpsUeModeOperation
        {
            get
            {
                return _epsUeModeOperation;
            }

            set
            {
                _epsUeModeOperation = (value);
            }
        }

        private (uint, bool, byte[])[] _pco = default((uint, bool, byte[])[]);
        public (uint, bool, byte[])[] Pco
        {
            get
            {
                return _pco;
            }

            set
            {
                _pco = (value);
            }
        }

        private ObjectPath _initialEpsBearer = default(ObjectPath);
        public ObjectPath InitialEpsBearer
        {
            get
            {
                return _initialEpsBearer;
            }

            set
            {
                _initialEpsBearer = (value);
            }
        }

        private IDictionary<string, object> _initialEpsBearerSettings = default(IDictionary<string, object>);
        public IDictionary<string, object> InitialEpsBearerSettings
        {
            get
            {
                return _initialEpsBearerSettings;
            }

            set
            {
                _initialEpsBearerSettings = (value);
            }
        }
    }

    static class Modem3gppExtensions
    {
        public static Task<string> GetImeiAsync(this IModem3gpp o) => o.GetAsync<string>("Imei");
        public static Task<uint> GetRegistrationStateAsync(this IModem3gpp o) => o.GetAsync<uint>("RegistrationState");
        public static Task<string> GetOperatorCodeAsync(this IModem3gpp o) => o.GetAsync<string>("OperatorCode");
        public static Task<string> GetOperatorNameAsync(this IModem3gpp o) => o.GetAsync<string>("OperatorName");
        public static Task<uint> GetEnabledFacilityLocksAsync(this IModem3gpp o) => o.GetAsync<uint>("EnabledFacilityLocks");
        public static Task<uint> GetSubscriptionStateAsync(this IModem3gpp o) => o.GetAsync<uint>("SubscriptionState");
        public static Task<uint> GetEpsUeModeOperationAsync(this IModem3gpp o) => o.GetAsync<uint>("EpsUeModeOperation");
        public static Task<(uint, bool, byte[])[]> GetPcoAsync(this IModem3gpp o) => o.GetAsync<(uint, bool, byte[])[]>("Pco");
        public static Task<ObjectPath> GetInitialEpsBearerAsync(this IModem3gpp o) => o.GetAsync<ObjectPath>("InitialEpsBearer");
        public static Task<IDictionary<string, object>> GetInitialEpsBearerSettingsAsync(this IModem3gpp o) => o.GetAsync<IDictionary<string, object>>("InitialEpsBearerSettings");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Firmware")]
    interface IFirmware : IDBusObject
    {
        Task<(string selected, IDictionary<string, object>[] installed)> ListAsync();
        Task SelectAsync(string Uniqueid);
        Task<T> GetAsync<T>(string prop);
        Task<FirmwareProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class FirmwareProperties
    {
        private (uint, IDictionary<string, object>) _updateSettings = default((uint, IDictionary<string, object>));
        public (uint, IDictionary<string, object>) UpdateSettings
        {
            get
            {
                return _updateSettings;
            }

            set
            {
                _updateSettings = (value);
            }
        }
    }

    static class FirmwareExtensions
    {
        public static Task<(uint, IDictionary<string, object>)> GetUpdateSettingsAsync(this IFirmware o) => o.GetAsync<(uint, IDictionary<string, object>)>("UpdateSettings");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Modem.Modem3gpp.ProfileManager")]
    interface IProfileManager : IDBusObject
    {
        Task<IDictionary<string, object>[]> ListAsync();
        Task<IDictionary<string, object>> SetAsync(IDictionary<string, object> RequestedProperties);
        Task DeleteAsync(IDictionary<string, object> Properties);
        Task<IDisposable> WatchUpdatedAsync(Action handler, Action<Exception> onError = null);
    }

    [DBusInterface("org.freedesktop.ModemManager1.Sim")]
    interface ISim : IDBusObject
    {
        Task SendPinAsync(string Pin);
        Task SendPukAsync(string Puk, string Pin);
        Task EnablePinAsync(string Pin, bool Enabled);
        Task ChangePinAsync(string OldPin, string NewPin);
        Task SetPreferredNetworksAsync((string, uint)[] PreferredNetworks);
        Task<T> GetAsync<T>(string prop);
        Task<SimProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class SimProperties
    {
        private bool _active = default(bool);
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                _active = (value);
            }
        }

        private string _simIdentifier = default(string);
        public string SimIdentifier
        {
            get
            {
                return _simIdentifier;
            }

            set
            {
                _simIdentifier = (value);
            }
        }

        private string _imsi = default(string);
        public string Imsi
        {
            get
            {
                return _imsi;
            }

            set
            {
                _imsi = (value);
            }
        }

        private string _eid = default(string);
        public string Eid
        {
            get
            {
                return _eid;
            }

            set
            {
                _eid = (value);
            }
        }

        private string _operatorIdentifier = default(string);
        public string OperatorIdentifier
        {
            get
            {
                return _operatorIdentifier;
            }

            set
            {
                _operatorIdentifier = (value);
            }
        }

        private string _operatorName = default(string);
        public string OperatorName
        {
            get
            {
                return _operatorName;
            }

            set
            {
                _operatorName = (value);
            }
        }

        private string[] _emergencyNumbers = default(string[]);
        public string[] EmergencyNumbers
        {
            get
            {
                return _emergencyNumbers;
            }

            set
            {
                _emergencyNumbers = (value);
            }
        }

        private (string, uint)[] _preferredNetworks = default((string, uint)[]);
        public (string, uint)[] PreferredNetworks
        {
            get
            {
                return _preferredNetworks;
            }

            set
            {
                _preferredNetworks = (value);
            }
        }
    }

    static class SimExtensions
    {
        public static Task<bool> GetActiveAsync(this ISim o) => o.GetAsync<bool>("Active");
        public static Task<string> GetSimIdentifierAsync(this ISim o) => o.GetAsync<string>("SimIdentifier");
        public static Task<string> GetImsiAsync(this ISim o) => o.GetAsync<string>("Imsi");
        public static Task<string> GetEidAsync(this ISim o) => o.GetAsync<string>("Eid");
        public static Task<string> GetOperatorIdentifierAsync(this ISim o) => o.GetAsync<string>("OperatorIdentifier");
        public static Task<string> GetOperatorNameAsync(this ISim o) => o.GetAsync<string>("OperatorName");
        public static Task<string[]> GetEmergencyNumbersAsync(this ISim o) => o.GetAsync<string[]>("EmergencyNumbers");
        public static Task<(string, uint)[]> GetPreferredNetworksAsync(this ISim o) => o.GetAsync<(string, uint)[]>("PreferredNetworks");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Sms")]
    interface ISms : IDBusObject
    {
        Task SendAsync();
        Task StoreAsync(uint Storage);
        Task<T> GetAsync<T>(string prop);
        Task<SmsProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class SmsProperties
    {
        private uint _state = default(uint);
        public uint State
        {
            get
            {
                return _state;
            }

            set
            {
                _state = (value);
            }
        }

        private uint _pduType = default(uint);
        public uint PduType
        {
            get
            {
                return _pduType;
            }

            set
            {
                _pduType = (value);
            }
        }

        private string _number = default(string);
        public string Number
        {
            get
            {
                return _number;
            }

            set
            {
                _number = (value);
            }
        }

        private string _text = default(string);
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = (value);
            }
        }

        private byte[] _data = default(byte[]);
        public byte[] Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = (value);
            }
        }

        private string _sMSC = default(string);
        public string SMSC
        {
            get
            {
                return _sMSC;
            }

            set
            {
                _sMSC = (value);
            }
        }

        private (uint, object) _validity = default((uint, object));
        public (uint, object) Validity
        {
            get
            {
                return _validity;
            }

            set
            {
                _validity = (value);
            }
        }

        private int _class = default(int);
        public int Class
        {
            get
            {
                return _class;
            }

            set
            {
                _class = (value);
            }
        }

        private uint _teleserviceId = default(uint);
        public uint TeleserviceId
        {
            get
            {
                return _teleserviceId;
            }

            set
            {
                _teleserviceId = (value);
            }
        }

        private uint _serviceCategory = default(uint);
        public uint ServiceCategory
        {
            get
            {
                return _serviceCategory;
            }

            set
            {
                _serviceCategory = (value);
            }
        }

        private bool _deliveryReportRequest = default(bool);
        public bool DeliveryReportRequest
        {
            get
            {
                return _deliveryReportRequest;
            }

            set
            {
                _deliveryReportRequest = (value);
            }
        }

        private uint _messageReference = default(uint);
        public uint MessageReference
        {
            get
            {
                return _messageReference;
            }

            set
            {
                _messageReference = (value);
            }
        }

        private string _timestamp = default(string);
        public string Timestamp
        {
            get
            {
                return _timestamp;
            }

            set
            {
                _timestamp = (value);
            }
        }

        private string _dischargeTimestamp = default(string);
        public string DischargeTimestamp
        {
            get
            {
                return _dischargeTimestamp;
            }

            set
            {
                _dischargeTimestamp = (value);
            }
        }

        private uint _deliveryState = default(uint);
        public uint DeliveryState
        {
            get
            {
                return _deliveryState;
            }

            set
            {
                _deliveryState = (value);
            }
        }

        private uint _storage = default(uint);
        public uint Storage
        {
            get
            {
                return _storage;
            }

            set
            {
                _storage = (value);
            }
        }
    }

    static class SmsExtensions
    {
        public static Task<uint> GetStateAsync(this ISms o) => o.GetAsync<uint>("State");
        public static Task<uint> GetPduTypeAsync(this ISms o) => o.GetAsync<uint>("PduType");
        public static Task<string> GetNumberAsync(this ISms o) => o.GetAsync<string>("Number");
        public static Task<string> GetTextAsync(this ISms o) => o.GetAsync<string>("Text");
        public static Task<byte[]> GetDataAsync(this ISms o) => o.GetAsync<byte[]>("Data");
        public static Task<string> GetSMSCAsync(this ISms o) => o.GetAsync<string>("SMSC");
        public static Task<(uint, object)> GetValidityAsync(this ISms o) => o.GetAsync<(uint, object)>("Validity");
        public static Task<int> GetClassAsync(this ISms o) => o.GetAsync<int>("Class");
        public static Task<uint> GetTeleserviceIdAsync(this ISms o) => o.GetAsync<uint>("TeleserviceId");
        public static Task<uint> GetServiceCategoryAsync(this ISms o) => o.GetAsync<uint>("ServiceCategory");
        public static Task<bool> GetDeliveryReportRequestAsync(this ISms o) => o.GetAsync<bool>("DeliveryReportRequest");
        public static Task<uint> GetMessageReferenceAsync(this ISms o) => o.GetAsync<uint>("MessageReference");
        public static Task<string> GetTimestampAsync(this ISms o) => o.GetAsync<string>("Timestamp");
        public static Task<string> GetDischargeTimestampAsync(this ISms o) => o.GetAsync<string>("DischargeTimestamp");
        public static Task<uint> GetDeliveryStateAsync(this ISms o) => o.GetAsync<uint>("DeliveryState");
        public static Task<uint> GetStorageAsync(this ISms o) => o.GetAsync<uint>("Storage");
    }

    [DBusInterface("org.freedesktop.ModemManager1.Bearer")]
    interface IBearer : IDBusObject
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task<T> GetAsync<T>(string prop);
        Task<BearerProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class BearerProperties
    {
        private string _interface = default(string);
        public string Interface
        {
            get
            {
                return _interface;
            }

            set
            {
                _interface = (value);
            }
        }

        private bool _connected = default(bool);
        public bool Connected
        {
            get
            {
                return _connected;
            }

            set
            {
                _connected = (value);
            }
        }

        private (string, string) _connectionError = default((string, string));
        public (string, string) ConnectionError
        {
            get
            {
                return _connectionError;
            }

            set
            {
                _connectionError = (value);
            }
        }

        private bool _suspended = default(bool);
        public bool Suspended
        {
            get
            {
                return _suspended;
            }

            set
            {
                _suspended = (value);
            }
        }

        private bool _multiplexed = default(bool);
        public bool Multiplexed
        {
            get
            {
                return _multiplexed;
            }

            set
            {
                _multiplexed = (value);
            }
        }

        private IDictionary<string, object> _ip4Config = default(IDictionary<string, object>);
        public IDictionary<string, object> Ip4Config
        {
            get
            {
                return _ip4Config;
            }

            set
            {
                _ip4Config = (value);
            }
        }

        private IDictionary<string, object> _ip6Config = default(IDictionary<string, object>);
        public IDictionary<string, object> Ip6Config
        {
            get
            {
                return _ip6Config;
            }

            set
            {
                _ip6Config = (value);
            }
        }

        private IDictionary<string, object> _stats = default(IDictionary<string, object>);
        public IDictionary<string, object> Stats
        {
            get
            {
                return _stats;
            }

            set
            {
                _stats = (value);
            }
        }

        private uint _ipTimeout = default(uint);
        public uint IpTimeout
        {
            get
            {
                return _ipTimeout;
            }

            set
            {
                _ipTimeout = (value);
            }
        }

        private uint _bearerType = default(uint);
        public uint BearerType
        {
            get
            {
                return _bearerType;
            }

            set
            {
                _bearerType = (value);
            }
        }

        private int _profileId = default(int);
        public int ProfileId
        {
            get
            {
                return _profileId;
            }

            set
            {
                _profileId = (value);
            }
        }

        private IDictionary<string, object> _properties = default(IDictionary<string, object>);
        public IDictionary<string, object> Properties
        {
            get
            {
                return _properties;
            }

            set
            {
                _properties = (value);
            }
        }
    }

    static class BearerExtensions
    {
        public static Task<string> GetInterfaceAsync(this IBearer o) => o.GetAsync<string>("Interface");
        public static Task<bool> GetConnectedAsync(this IBearer o) => o.GetAsync<bool>("Connected");
        public static Task<(string, string)> GetConnectionErrorAsync(this IBearer o) => o.GetAsync<(string, string)>("ConnectionError");
        public static Task<bool> GetSuspendedAsync(this IBearer o) => o.GetAsync<bool>("Suspended");
        public static Task<bool> GetMultiplexedAsync(this IBearer o) => o.GetAsync<bool>("Multiplexed");
        public static Task<IDictionary<string, object>> GetIp4ConfigAsync(this IBearer o) => o.GetAsync<IDictionary<string, object>>("Ip4Config");
        public static Task<IDictionary<string, object>> GetIp6ConfigAsync(this IBearer o) => o.GetAsync<IDictionary<string, object>>("Ip6Config");
        public static Task<IDictionary<string, object>> GetStatsAsync(this IBearer o) => o.GetAsync<IDictionary<string, object>>("Stats");
        public static Task<uint> GetIpTimeoutAsync(this IBearer o) => o.GetAsync<uint>("IpTimeout");
        public static Task<uint> GetBearerTypeAsync(this IBearer o) => o.GetAsync<uint>("BearerType");
        public static Task<int> GetProfileIdAsync(this IBearer o) => o.GetAsync<int>("ProfileId");
        public static Task<IDictionary<string, object>> GetPropertiesAsync(this IBearer o) => o.GetAsync<IDictionary<string, object>>("Properties");
    }
}