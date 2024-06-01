using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Server;

namespace Bader.Edge.ModuleHost.Mqtt;

public class MqttModuleClientBuilder
{
    private readonly string _deviceId;
    private readonly string _moduleId;
    private readonly ManagedMqttClientOptions _mqttClientOptions;
    private ILogger _logger = NullLogger.Instance;
    private IEnumerable<MqttRoute> _routes = Enumerable.Empty<MqttRoute>();

    public MqttModuleClientBuilder(string deviceId, string moduleId, Uri mqttBrokerUri)
    {
        var clientId = $"{deviceId}.{moduleId}";

        _mqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(1))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithConnectionUri(mqttBrokerUri)
                .WithCleanSession()
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .Build())
            .WithMaxPendingMessages(int.MaxValue)
            .WithPendingMessagesOverflowStrategy(MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage)
            .Build();

        _deviceId = deviceId;
        _moduleId = moduleId;
    }

    public MqttModuleClient Build()
    {
        var factory = new MqttFactory();
        var managedMqttClient = factory.CreateManagedMqttClient();

        var mqttModuleClient = new MqttModuleClient(_deviceId, _moduleId, managedMqttClient, _mqttClientOptions, _routes, _logger);
        return mqttModuleClient;
    }

    public MqttModuleClientBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    public MqttModuleClientBuilder WithMqttOptions(Action<ManagedMqttClientOptions> configure)
    {
        configure(_mqttClientOptions);
        return this;
    }

    public MqttModuleClientBuilder WithRoutes(IEnumerable<MqttRoute> routes)
    {
        _routes = routes;
        return this;
    }
}
