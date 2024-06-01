using System.Net;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Bader.Edge.ModuleHost.Mqtt;

public sealed class MqttModuleClient : IModuleClient, IAsyncDisposable, IDisposable
{
    private readonly List<Func<TwinCollection, Task>> _desiredPropertyUpdateCallbacks = new();
    private readonly string _deviceId;
    private readonly ILogger _logger;
    private readonly Dictionary<string, List<Func<Message, Task<MessageResponse>>>> _messageHandlers = new();
    private readonly string _messagesTopic;
    private readonly Dictionary<string, Func<MethodRequest, Task<MethodResponse>>> _methodCallbacks = new();
    private readonly string _methodsTopic;
    private readonly string _methodsWildcardTopic;
    private readonly string _moduleId;
    private readonly IManagedMqttClient _mqttClient;
    private readonly ManagedMqttClientOptions _mqttClientOptions;
    private readonly MqttRpcClient _mqttRpcClient;
    private readonly IEnumerable<MqttRoute> _routes;
    private readonly Twin _twin = new();
    private readonly string _twinDesiredTopic;
    private readonly string _twinReportedTopic;
    private readonly string _twinTopic;
    private bool _isDisposed;
    private IRetryPolicy? _retryPolicy;
    private ConnectionStatusChangesHandler? _statusChangesHandler;

    internal MqttModuleClient(string deviceId, string moduleId, IManagedMqttClient mqttClient, ManagedMqttClientOptions options, IEnumerable<MqttRoute> routes, ILogger logger)
    {
        _mqttClientOptions = options;
        _mqttClient = mqttClient;

        _mqttClient.ConnectedAsync += Connected;
        _mqttClient.DisconnectedAsync += Disconnected;
        _mqttClient.ApplicationMessageReceivedAsync += MessageReceived;
        _mqttClient.ApplicationMessageReceivedAsync += MethodRequestReceived;
        _mqttClient.ApplicationMessageReceivedAsync += TwinUpdateReceived;

        _deviceId = deviceId;
        _moduleId = moduleId;
        _routes = routes;
        _logger = logger;

        _messagesTopic = $"{_deviceId}/{_moduleId}/messages";

        _methodsTopic = $"{_deviceId}/{_moduleId}/methods";
        _methodsWildcardTopic = $"{_methodsTopic}/+";

        _twinTopic = $"{_deviceId}/{_moduleId}/twin";
        _twinDesiredTopic = $"{_twinTopic}/desired";
        _twinReportedTopic = $"{_twinTopic}/reported";

        var mqttRpcClientOptions = new MqttRpcClientOptionsBuilder()
            .WithTopicGenerationStrategy(new ModuleMqttRpcClientTopicGenerationStrategy("response"))
            .Build();
        _mqttRpcClient = new MqttRpcClient(_mqttClient.InternalClient, mqttRpcClientOptions);
    }

    public int DiagnosticSamplingPercentage { get; set; }

    public uint OperationTimeoutInMilliseconds { get; set; }

    public string ProductInfo { get; set; } = "Bader.Edge.ModuleHost.Mqtt";

    public Task AbandonAsync(string lockToken) => AbandonAsync(lockToken, default);

    public Task AbandonAsync(string lockToken, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task AbandonAsync(Message message) => AbandonAsync(message, default);

    public Task AbandonAsync(Message message, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task CloseAsync() => CloseAsync(default);

    public Task CloseAsync(CancellationToken cancellationToken) => _mqttClient.StopAsync();

    public Task CompleteAsync(string lockToken) => CompleteAsync(lockToken, default);

    public Task CompleteAsync(string lockToken, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task CompleteAsync(Message message) => CompleteAsync(message, default);

    public Task CompleteAsync(Message message, CancellationToken cancellationToken)
    {
        message.Properties[""]
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected", Justification = "It's created by the builder so we are responsible for disposing it")]
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _mqttRpcClient.Dispose();

        _mqttClient.ApplicationMessageReceivedAsync -= TwinUpdateReceived;
        _mqttClient.ApplicationMessageReceivedAsync -= MethodRequestReceived;
        _mqttClient.ApplicationMessageReceivedAsync -= MessageReceived;
        _mqttClient.ConnectedAsync -= Connected;
        _mqttClient.DisconnectedAsync -= Disconnected;

        _mqttClient.Dispose();
        _isDisposed = true;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return default;
    }

    public Task<Twin> GetTwinAsync() => GetTwinAsync(default);

    public Task<Twin> GetTwinAsync(CancellationToken cancellationToken) => Task.FromResult(_twin);

    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest) => InvokeMethodAsync(deviceId, methodRequest, default);

    public Task<MethodResponse> InvokeMethodAsync(string deviceId, MethodRequest methodRequest, CancellationToken cancellationToken) => InvokeMethodAsync(deviceId, string.Empty, methodRequest, cancellationToken);

    public Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId, MethodRequest methodRequest) => InvokeMethodAsync(deviceId, moduleId, methodRequest, default);

    public async Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId, MethodRequest methodRequest, CancellationToken cancellationToken)
    {
        var methodName = string.IsNullOrEmpty(moduleId)
            ? $"{deviceId}/methods/{methodRequest.Name}"
            : $"{deviceId}/{moduleId}/methods/{methodRequest.Name}";

        _logger.LogTrace("Invoking method {MethodName}", methodName);
        var response = await _mqttRpcClient.ExecuteAsync(methodName, methodRequest.Data, MqttQualityOfServiceLevel.ExactlyOnce, cancellationToken: cancellationToken);
        _logger.LogTrace("Invoked method {MethodName}", methodName);

        return new MethodResponse(response, (int)HttpStatusCode.OK);
    }

    public Task OpenAsync() => OpenAsync(default);

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Opening MQTT connection");
        await _mqttClient.StartAsync(_mqttClientOptions);
        await _mqttClient.SubscribeAsync(_methodsWildcardTopic, MqttQualityOfServiceLevel.ExactlyOnce);
    }

    public Task SendEventAsync(Message message) => SendEventAsync(message, default);

    public Task SendEventAsync(Message message, CancellationToken cancellationToken) => SendEventAsync(string.Empty, message, cancellationToken);

    public Task SendEventAsync(string outputName, Message message) => SendEventAsync(outputName, message, default);

    public async Task SendEventAsync(string outputName, Message message, CancellationToken cancellationToken)
    {
        var topic = string.IsNullOrEmpty(outputName) ? _messagesTopic : $"{_messagesTopic}/{outputName}";
        var mqttMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(message.GetBytes())
            .WithContentType(message.ContentType)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        foreach (var messageProperty in message.Properties)
        {
            mqttMessage.UserProperties.Add(new MqttUserProperty(messageProperty.Key, messageProperty.Value));
        }

        _logger.LogTrace("Enqueuing message to output {OutputName} and topic {TopicName}", string.IsNullOrEmpty(outputName) ? "default" : outputName, topic);
        await _mqttClient.EnqueueAsync(mqttMessage);
    }

    public Task SendEventBatchAsync(IEnumerable<Message> messages) => SendEventBatchAsync(messages, default);

    public Task SendEventBatchAsync(IEnumerable<Message> messages, CancellationToken cancellationToken) => SendEventBatchAsync(string.Empty, messages, cancellationToken);

    public Task SendEventBatchAsync(string outputName, IEnumerable<Message> messages) => SendEventBatchAsync(outputName, messages, default);

    public async Task SendEventBatchAsync(string outputName, IEnumerable<Message> messages, CancellationToken cancellationToken)
    {
        foreach (var message in messages)
        {
            await SendEventAsync(message, cancellationToken);
        }
    }

    public void SetConnectionStatusChangesHandler(ConnectionStatusChangesHandler statusChangesHandler) => _statusChangesHandler = statusChangesHandler;

    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext) => SetDesiredPropertyUpdateCallbackAsync(callback, userContext, default);

    public Task SetDesiredPropertyUpdateCallbackAsync(DesiredPropertyUpdateCallback callback, object userContext, CancellationToken cancellationToken)
    {
        _desiredPropertyUpdateCallbacks.Add((TwinCollection desiredProperties) => callback.Invoke(desiredProperties, userContext));
        return Task.CompletedTask;
    }

    public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext) => SetInputMessageHandlerAsync(inputName, messageHandler, userContext, default);

    public async Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext, CancellationToken cancellationToken)
    {
        var routes = _routes.Where(route => (route.ToInputName ?? string.Empty).Equals(inputName, StringComparison.OrdinalIgnoreCase));
        foreach (var route in routes)
        {
            var topic = GetTopicName($"{route.FromModuleId}/${route.FromInputName ?? string.Empty}");

            _logger.LogDebug("Subscribing to messages with input {InputName} and topic {TopicName}", string.IsNullOrEmpty(inputName) ? "default" : inputName, topic);
            await _mqttClient.SubscribeAsync(topic, MqttQualityOfServiceLevel.ExactlyOnce);

            if (!_messageHandlers.ContainsKey(topic))
            {
                _messageHandlers.Add(topic, new());
            }

            _messageHandlers[topic].Add((Message message) => messageHandler(message, userContext));
        }
    }

    public Task SetMessageHandlerAsync(MessageHandler messageHandler, object userContext) => SetMessageHandlerAsync(messageHandler, userContext, default);

    public Task SetMessageHandlerAsync(MessageHandler messageHandler, object userContext, CancellationToken cancellationToken) => SetInputMessageHandlerAsync(string.Empty, messageHandler, userContext, cancellationToken);

    public Task SetMethodDefaultHandlerAsync(MethodCallback methodHandler, object userContext) => SetMethodDefaultHandlerAsync(methodHandler, userContext, default);

    public Task SetMethodDefaultHandlerAsync(MethodCallback methodHandler, object userContext, CancellationToken cancellationToken) => SetMethodHandlerAsync(string.Empty, methodHandler, userContext, cancellationToken);

    public Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext) => SetMethodHandlerAsync(methodName, methodHandler, userContext, default);

    public Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Subscribing to method {MethodName}", string.IsNullOrEmpty(methodName) ? "default" : methodName);

        _methodCallbacks[methodName] = (MethodRequest request) => methodHandler(request, userContext);
        return Task.CompletedTask;
    }

    public void SetRetryPolicy(IRetryPolicy retryPolicy) => _retryPolicy = retryPolicy;

    public Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties) => UpdateReportedPropertiesAsync(reportedProperties, default);

    public async Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties, CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<string, object> keyValuePair in reportedProperties)
        {
            _twin.Properties.Reported[keyValuePair.Key] = keyValuePair.Value;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_twinReportedTopic)
            .WithPayload(_twin.Properties.Reported.ToJson())
            .Build();

        await _mqttClient.EnqueueAsync(message);
    }

    private static string GetTopicName(string topicName) => topicName.TrimEnd('/') + '/';

    private Task Connected(MqttClientConnectedEventArgs args)
    {
        _logger.LogTrace("MQTT connection connected");
        _statusChangesHandler?.Invoke(ConnectionStatus.Connected, ConnectionStatusChangeReason.Connection_Ok);
        return Task.CompletedTask;
    }

    private Task Disconnected(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogTrace("MQTT connection disconnected because of {Reason} (Reason Code {ReasonCode})", args.ReasonString, args.Reason);
        _statusChangesHandler?.Invoke(ConnectionStatus.Disconnected, ConnectionStatusChangeReason.Communication_Error);
        return Task.CompletedTask;
    }

    private void HandleTwinDesiredPropertiesUpdate(byte[] payload)
    {
        var newDesiredTwinCollection = new TwinCollection(Encoding.UTF8.GetString(payload));
        foreach (KeyValuePair<string, object> keyValuePair in newDesiredTwinCollection)
        {
            _twin.Properties.Desired[keyValuePair.Key] = keyValuePair.Value;
        }

        foreach (var desiredPropertyUpdateCallback in _desiredPropertyUpdateCallbacks)
        {
            InvokeDesiredPropertyUpdateCallback(desiredPropertyUpdateCallback, _twin.Properties.Desired);
        }
    }

    private void InvokeDesiredPropertyUpdateCallback(Func<TwinCollection, Task> desiredPropertiesUpdateCallback, TwinCollection twinCollection)
    {
        try
        {
            desiredPropertiesUpdateCallback.Invoke(twinCollection);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not invoke desired properties update callback");
        }
    }

    private void InvokeMessageHandler(Func<Message, Task<MessageResponse>> messageHandler, Message message)
    {
        try
        {
            messageHandler.Invoke(message);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not invoke message handler");
        }
    }

    private async Task<bool> InvokeMethodHandlerAsync(Func<MethodRequest, Task<MethodResponse>> methodCallback, MethodRequest request, MqttApplicationMessage applicationMessage)
    {
        try
        {
            var response = await methodCallback.Invoke(request);

            var responseMessage = new MqttApplicationMessageBuilder()
                .WithTopic(applicationMessage.ResponseTopic)
                .WithCorrelationData(applicationMessage.CorrelationData)
                .WithPayload(response.Result)
                .Build();

            _logger.LogTrace("Method {MethodName} invoked, responding on topic {ResponseTopicName}", request.Name, applicationMessage.ResponseTopic);
            await _mqttClient.EnqueueAsync(responseMessage);
            return true;
        }
        catch (Exception ex)
        {
            await RespondMethodErrorAsync(ex, request.Name, applicationMessage);
            return false;
        }
    }

    private Task MessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        var applicationMessage = args.ApplicationMessage;
        var topic = GetTopicName(applicationMessage.Topic);

        if (!topic.StartsWith(_messagesTopic, StringComparison.OrdinalIgnoreCase))
        {
            // no telemetry message topic, skipping message
            return Task.CompletedTask;
        }

        if (_messageHandlers.TryGetValue(topic, out var messageHandlers))
        {
            using var message = new Message(applicationMessage.PayloadSegment.ToArray());

            foreach (var messageHandler in messageHandlers)
            {
                InvokeMessageHandler(messageHandler, message);
            }
        }

        return Task.CompletedTask;
    }

    private async Task MethodRequestReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        var applicationMessage = args.ApplicationMessage;
        var topic = GetTopicName(applicationMessage.Topic);

        if (!topic.StartsWith(_methodsTopic, StringComparison.OrdinalIgnoreCase))
        {
            // no method topic, skipping message
            return;
        }

        var methodName = topic[_methodsTopic.Length..];
        if (methodName.Contains('/', StringComparison.Ordinal))
        {
            // return if it seems like to contain a subtopic as it might be the response
            _logger.LogDebug("Received method call but topic contained sub topic, skipping: {TopicName}", topic);
            return;
        }

        var wasSuccessful = true;
        var methodRequest = new MethodRequest(methodName, applicationMessage.PayloadSegment.ToArray());
        if (_methodCallbacks.TryGetValue(topic, out var methodHandler))
        {
            wasSuccessful = await InvokeMethodHandlerAsync(methodHandler, methodRequest, args.ApplicationMessage);
        }

        if (_methodCallbacks.TryGetValue(string.Empty, out var defaultMethodHandler))
        {
            wasSuccessful = await InvokeMethodHandlerAsync(defaultMethodHandler, methodRequest, args.ApplicationMessage);
        }

        if (!wasSuccessful)
        {
            args.ProcessingFailed = true;
            args.ReasonCode = MqttApplicationMessageReceivedReasonCode.ImplementationSpecificError;
        }
    }

    private async Task<bool> RespondMethodErrorAsync(Exception exception, string methodName, MqttApplicationMessage applicationMessage)
    {
        try
        {
            var responseMessage = new MqttApplicationMessageBuilder()
                .WithTopic(applicationMessage.ResponseTopic)
                .WithCorrelationData(applicationMessage.CorrelationData)
                .WithPayload(exception.Message)
                .Build();

            _logger.LogDebug(exception, "Method {MethodName} invocation failed, responding on topic {ResponseTopicName}", methodName, applicationMessage.ResponseTopic);
            await _mqttClient.EnqueueAsync(responseMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not respond error for method invocation of method {MethodName} on topic {ResponseTopicName}", methodName, applicationMessage.ResponseTopic);
            return false;
        }
    }

    private Task TwinUpdateReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        var applicationMessage = args.ApplicationMessage;
        var topic = GetTopicName(applicationMessage.Topic);

        if (!topic.StartsWith(_twinTopic, StringComparison.OrdinalIgnoreCase))
        {
            // no twin topic, skipping message
            return Task.CompletedTask;
        }

        var payload = args.ApplicationMessage.PayloadSegment.ToArray();

        if (topic.StartsWith(_twinDesiredTopic, StringComparison.OrdinalIgnoreCase))
        {
            HandleTwinDesiredPropertiesUpdate(payload);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
