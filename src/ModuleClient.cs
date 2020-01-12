// THIS DOCUMENT IS GENERATED, ALL CHANGES WILL GET OVERWRITTEN!
#nullable disable
namespace AIT.Devices
{
    public class ModuleClient : IModuleClient
    {
        private Microsoft.Azure.Devices.Client.ModuleClient _moduleClient;

        public System.Int32 DiagnosticSamplingPercentage { get => _moduleClient.DiagnosticSamplingPercentage; set => _moduleClient.DiagnosticSamplingPercentage = value; }
        public System.UInt32 OperationTimeoutInMilliseconds { get => _moduleClient.OperationTimeoutInMilliseconds; set => _moduleClient.OperationTimeoutInMilliseconds = value; }
        public System.String ProductInfo { get => _moduleClient.ProductInfo; set => _moduleClient.ProductInfo = value; }
        public void SetRetryPolicy(Microsoft.Azure.Devices.Client.IRetryPolicy retryPolicy) => _moduleClient.SetRetryPolicy(retryPolicy);
        public System.Threading.Tasks.Task OpenAsync() => _moduleClient.OpenAsync();
        public System.Threading.Tasks.Task OpenAsync(System.Threading.CancellationToken cancellationToken) => _moduleClient.OpenAsync(cancellationToken);
        public System.Threading.Tasks.Task CloseAsync() => _moduleClient.CloseAsync();
        public System.Threading.Tasks.Task CloseAsync(System.Threading.CancellationToken cancellationToken) => _moduleClient.CloseAsync(cancellationToken);
        public System.Threading.Tasks.Task CompleteAsync(System.String lockToken) => _moduleClient.CompleteAsync(lockToken);
        public System.Threading.Tasks.Task CompleteAsync(System.String lockToken, System.Threading.CancellationToken cancellationToken) => _moduleClient.CompleteAsync(lockToken, cancellationToken);
        public System.Threading.Tasks.Task CompleteAsync(Microsoft.Azure.Devices.Client.Message message) => _moduleClient.CompleteAsync(message);
        public System.Threading.Tasks.Task CompleteAsync(Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken) => _moduleClient.CompleteAsync(message, cancellationToken);
        public System.Threading.Tasks.Task AbandonAsync(System.String lockToken) => _moduleClient.AbandonAsync(lockToken);
        public System.Threading.Tasks.Task AbandonAsync(System.String lockToken, System.Threading.CancellationToken cancellationToken) => _moduleClient.AbandonAsync(lockToken, cancellationToken);
        public System.Threading.Tasks.Task AbandonAsync(Microsoft.Azure.Devices.Client.Message message) => _moduleClient.AbandonAsync(message);
        public System.Threading.Tasks.Task AbandonAsync(Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken) => _moduleClient.AbandonAsync(message, cancellationToken);
        public System.Threading.Tasks.Task SendEventAsync(Microsoft.Azure.Devices.Client.Message message) => _moduleClient.SendEventAsync(message);
        public System.Threading.Tasks.Task SendEventAsync(Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken) => _moduleClient.SendEventAsync(message, cancellationToken);
        public System.Threading.Tasks.Task SendEventBatchAsync(System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages) => _moduleClient.SendEventBatchAsync(messages);
        public System.Threading.Tasks.Task SendEventBatchAsync(System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages, System.Threading.CancellationToken cancellationToken) => _moduleClient.SendEventBatchAsync(messages, cancellationToken);
        public System.Threading.Tasks.Task SetMethodHandlerAsync(System.String methodName, Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext) => _moduleClient.SetMethodHandlerAsync(methodName, methodHandler, userContext);
        public System.Threading.Tasks.Task SetMethodHandlerAsync(System.String methodName, Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken) => _moduleClient.SetMethodHandlerAsync(methodName, methodHandler, userContext, cancellationToken);
        public System.Threading.Tasks.Task SetMethodDefaultHandlerAsync(Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext) => _moduleClient.SetMethodDefaultHandlerAsync(methodHandler, userContext);
        public System.Threading.Tasks.Task SetMethodDefaultHandlerAsync(Microsoft.Azure.Devices.Client.MethodCallback methodHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken) => _moduleClient.SetMethodDefaultHandlerAsync(methodHandler, userContext, cancellationToken);
        public void SetConnectionStatusChangesHandler(Microsoft.Azure.Devices.Client.ConnectionStatusChangesHandler statusChangesHandler) => _moduleClient.SetConnectionStatusChangesHandler(statusChangesHandler);
        public void Dispose() => _moduleClient.Dispose();
        public System.Threading.Tasks.Task SetDesiredPropertyUpdateCallbackAsync(Microsoft.Azure.Devices.Client.DesiredPropertyUpdateCallback callback, System.Object userContext) => _moduleClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext);
        public System.Threading.Tasks.Task SetDesiredPropertyUpdateCallbackAsync(Microsoft.Azure.Devices.Client.DesiredPropertyUpdateCallback callback, System.Object userContext, System.Threading.CancellationToken cancellationToken) => _moduleClient.SetDesiredPropertyUpdateCallbackAsync(callback, userContext, cancellationToken);
        public System.Threading.Tasks.Task<Microsoft.Azure.Devices.Shared.Twin> GetTwinAsync() => _moduleClient.GetTwinAsync();
        public System.Threading.Tasks.Task<Microsoft.Azure.Devices.Shared.Twin> GetTwinAsync(System.Threading.CancellationToken cancellationToken) => _moduleClient.GetTwinAsync(cancellationToken);
        public System.Threading.Tasks.Task UpdateReportedPropertiesAsync(Microsoft.Azure.Devices.Shared.TwinCollection reportedProperties) => _moduleClient.UpdateReportedPropertiesAsync(reportedProperties);
        public System.Threading.Tasks.Task UpdateReportedPropertiesAsync(Microsoft.Azure.Devices.Shared.TwinCollection reportedProperties, System.Threading.CancellationToken cancellationToken) => _moduleClient.UpdateReportedPropertiesAsync(reportedProperties, cancellationToken);
        public System.Threading.Tasks.Task SendEventAsync(System.String outputName, Microsoft.Azure.Devices.Client.Message message) => _moduleClient.SendEventAsync(outputName, message);
        public System.Threading.Tasks.Task SendEventAsync(System.String outputName, Microsoft.Azure.Devices.Client.Message message, System.Threading.CancellationToken cancellationToken) => _moduleClient.SendEventAsync(outputName, message, cancellationToken);
        public System.Threading.Tasks.Task SendEventBatchAsync(System.String outputName, System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages) => _moduleClient.SendEventBatchAsync(outputName, messages);
        public System.Threading.Tasks.Task SendEventBatchAsync(System.String outputName, System.Collections.Generic.IEnumerable<Microsoft.Azure.Devices.Client.Message> messages, System.Threading.CancellationToken cancellationToken) => _moduleClient.SendEventBatchAsync(outputName, messages, cancellationToken);
        public System.Threading.Tasks.Task SetInputMessageHandlerAsync(System.String inputName, Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext) => _moduleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
        public System.Threading.Tasks.Task SetInputMessageHandlerAsync(System.String inputName, Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken) => _moduleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext, cancellationToken);
        public System.Threading.Tasks.Task SetMessageHandlerAsync(Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext) => _moduleClient.SetMessageHandlerAsync(messageHandler, userContext);
        public System.Threading.Tasks.Task SetMessageHandlerAsync(Microsoft.Azure.Devices.Client.MessageHandler messageHandler, System.Object userContext, System.Threading.CancellationToken cancellationToken) => _moduleClient.SetMessageHandlerAsync(messageHandler, userContext, cancellationToken);
        public System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest);
        public System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest, System.Threading.CancellationToken cancellationToken) => _moduleClient.InvokeMethodAsync(deviceId, methodRequest, cancellationToken);
        public System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, System.String moduleId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest) => _moduleClient.InvokeMethodAsync(deviceId, moduleId, methodRequest);
        public System.Threading.Tasks.Task<Microsoft.Azure.Devices.Client.MethodResponse> InvokeMethodAsync(System.String deviceId, System.String moduleId, Microsoft.Azure.Devices.Client.MethodRequest methodRequest, System.Threading.CancellationToken cancellationToken) => _moduleClient.InvokeMethodAsync(deviceId, moduleId, methodRequest, cancellationToken);

        internal void SetInstance(Microsoft.Azure.Devices.Client.ModuleClient moduleClient)
        {
            _moduleClient = moduleClient ?? throw new System.ArgumentNullException(nameof(moduleClient));
        }
    }
}
