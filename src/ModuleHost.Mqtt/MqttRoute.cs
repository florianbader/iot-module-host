namespace Bader.Edge.ModuleHost.Mqtt;

public class MqttRoute(string fromModuleId, string? fromInputName, string? toInputName)
{
    public string? FromInputName { get; private set; } = fromInputName;

    public string FromModuleId { get; private set; } = fromModuleId;

    public string? ToInputName { get; private set; } = toInputName;

    public static MqttRoute From(string moduleId, string? inputName = null)
        => new MqttRoute(moduleId, inputName, null);

    public MqttRoute To(string inputName)
    {
        ToInputName = inputName;
        return this;
    }
}
