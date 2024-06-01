using System.Text.RegularExpressions;

namespace Bader.Edge.ModuleHost;

public class ExtendedModuleClient : ModuleClient, IExtendedModuleClient
{
    private static readonly Regex DeviceIdRegex = new("DeviceId=([^;]+)", RegexOptions.Compiled);
    private static readonly Regex ModuleIdRegex = new("ModuleId=([^;]+)", RegexOptions.Compiled);

    private static string? _deviceId;
    private static string? _moduleId;

    public ExtendedModuleClient(Microsoft.Azure.Devices.Client.ModuleClient moduleClient) : base(moduleClient)
    {
    }

    public static string DeviceId
    {
        get
        {
            if (_deviceId is not null)
            {
                return _deviceId;
            }

            _deviceId = Environment.GetEnvironmentVariable("IOTEDGE_DEVICEID");
            if (!string.IsNullOrEmpty(_deviceId))
            {
                return _deviceId;
            }

            var edgeHubConnectionString = Environment.GetEnvironmentVariable("EdgeHubConnectionString");
            if (edgeHubConnectionString is null)
            {
                return _deviceId;
            }

            var match = DeviceIdRegex.Match(edgeHubConnectionString);
            _deviceId = match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;

            return _deviceId ?? throw new InvalidOperationException("Could not find device id");
        }
    }

    public static string ModuleId
    {
        get
        {
            if (_moduleId is not null)
            {
                return _moduleId;
            }

            _moduleId = Environment.GetEnvironmentVariable("IOTEDGE_MODULEID");
            if (!string.IsNullOrEmpty(_moduleId))
            {
                return _moduleId;
            }

            var edgeHubConnectionString = Environment.GetEnvironmentVariable("EdgeHubConnectionString");
            if (edgeHubConnectionString is null)
            {
                return _moduleId;
            }

            var match = ModuleIdRegex.Match(edgeHubConnectionString);
            _moduleId = match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;

            return _moduleId ?? throw new InvalidOperationException("Could not find module id");
        }
    }

    string IExtendedModuleClient.DeviceId => DeviceId;

    string IExtendedModuleClient.ModuleId => ModuleId;
}

public interface IExtendedModuleClient
{
    string DeviceId { get; }

    string ModuleId { get; }
}
