using Microsoft.Azure.Devices.Client;

var authenticationMethod = new AnonymousAuthenticationMethod();
var moduleClient = ModuleClient.Create("localhost", authenticationMethod, );

public class AnonymousAuthenticationMethod : IAuthenticationMethod
{
    public IotHubConnectionStringBuilder Populate(IotHubConnectionStringBuilder iotHubConnectionStringBuilder) => iotHubConnectionStringBuilder;
}
