// THIS DOCUMENT IS GENERATED, ALL CHANGES WILL GET OVERWRITTEN!
namespace Bader.Edge.ModuleHost
{
    [System.CodeDom.Compiler.GeneratedCode("ModuleClientGenerator.csx", "1.0")]
    public interface IModuleClient
    {
        System.Int32 DiagnosticSamplingPercentage { get; set; }

        System.UInt32 OperationTimeoutInMilliseconds { get; set; }

        System.String ProductInfo { get; set; }

        System.Threading.Tasks.Task AbandonAsync(System.String lockToken);

        System.Threading.Tasks.Task AbandonAsync(System.String lockToken, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task AbandonAsync(Microsoft.Azure.Devices.Client.Message message);

        System.Threading.Tasks.Task AbandonAsync(Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task CloseAsync();

        System.Threading.Tasks.Task CloseAsync(System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task CompleteAsync(System.String lockToken);

        System.Threading.Tasks.Task CompleteAsync(System.String lockToken, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task CompleteAsync(Microsoft.Azure.Devices.Client.Message message);

        System.Threading.Tasks.Task CompleteAsync(Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken);

        void Dispose();

        System.Threading.Tasks.Task<Microsoft.Azure.Devices.Shared.Twin> GetTwinAsync();

        System.Threading.Tasks.Task<Microsoft.Azure.Devices.Shared.Twin> GetTwinAsync(System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest);

        System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, System.String moduleId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest);

        System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, System.String moduleId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task OpenAsync();

        System.Threading.Tasks.Task OpenAsync(System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SendEventAsync(Microsoft.Azure.Devices.Client.Message message);

        System.Threading.Tasks.Task SendEventAsync(Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SendEventAsync(System.String outputName, Microsoft.Azure.Devices.Client.Message message);

        System.Threading.Tasks.Task SendEventAsync(System.String outputName, Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SendEventBatchAsync(System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages);

        System.Threading.Tasks.Task SendEventBatchAsync(System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SendEventBatchAsync(System.String outputName, System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages);

        System.Threading.Tasks.Task SendEventBatchAsync(System.String outputName, System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages, System.Threading.CancellationToken cancellationToken);

        void SetConnectionStatusChangesHandler(Microsoft.Azure.Devices.Client.ConnectionStatusChangesHandler statusChangesHandler);

        System.Threading.Tasks.Task SetDesiredPropertyUpdateCallbackAsync(Microsoft.Azure.Devices.Client.DesiredPropertyUpdateCallback callback, System.Object userContext);

        System.Threading.Tasks.Task SetDesiredPropertyUpdateCallbackAsync(Microsoft.Azure.Devices.Client.DesiredPropertyUpdateCallback callback, System.Object userContext, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SetInputMessageHandlerAsync(System.String inputName, Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext);

        System.Threading.Tasks.Task SetInputMessageHandlerAsync(System.String inputName, Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SetMessageHandlerAsync(Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext);

        System.Threading.Tasks.Task SetMessageHandlerAsync(Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SetMethodDefaultHandlerAsync(Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext);

        System.Threading.Tasks.Task SetMethodDefaultHandlerAsync(Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task SetMethodHandlerAsync(System.String methodName, Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext);

        System.Threading.Tasks.Task SetMethodHandlerAsync(System.String methodName, Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken);

        void SetRetryPolicy(Microsoft.Azure.Devices.Client.IRetryPolicy retryPolicy);

        System.Threading.Tasks.Task UpdateReportedPropertiesAsync(Microsoft.Azure.Devices.Shared.TwinCollection reportedProperties);

        System.Threading.Tasks.Task UpdateReportedPropertiesAsync(Microsoft.Azure.Devices.Shared.TwinCollection reportedProperties, System.Threading.CancellationToken cancellationToken);
    }
}
