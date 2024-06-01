using MQTTnet.Extensions.Rpc;

namespace Bader.Edge.ModuleHost.Mqtt;

public class ModuleMqttRpcClientTopicGenerationStrategy(string? responseSuffix = "response") : IMqttRpcClientTopicGenerationStrategy
{
    public MqttRpcTopicPair CreateRpcTopics(TopicGenerationContext context)
    {
        var topic = context.MethodName;

        return new MqttRpcTopicPair
        {
            RequestTopic = topic,
            ResponseTopic = $"{topic}/{responseSuffix}",
        };
    }
}
