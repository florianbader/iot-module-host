#! "net6.0"
#r "nuget: Microsoft.Azure.Devices.Client,1.*"

using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Devices.Client;

string GetTypeName(Type type)
{
    if (type.Name == "Void")
        return "void";

    if (type.IsGenericType)
    {
        var genericParameters = string.Join(", ", type.GetGenericArguments().Select(g => g.IsGenericType ? GetTypeName(g) : g.FullName));
        return $"{type.Namespace}.{type.Name.Substring(0, type.Name.IndexOf("`"))}<{genericParameters}>";
    }

    return type.FullName;
}

string Generate(bool isInterface = false)
{
    var moduleClientType = typeof(ModuleClient);

    var sourceCodeBuilder = new StringBuilder();

    var properties = moduleClientType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    foreach (var property in properties)
    {
        if (property.CanRead && property.CanWrite)
        {
            sourceCodeBuilder.AppendLine($"     {(!isInterface ? "public " : "")}{GetTypeName(property.PropertyType)} {property.Name}{(!isInterface ? $" {{ get => _moduleClient.{property.Name}; set => _moduleClient.{property.Name} = value; }}" : " { get; set; }")}");
        }
        else
        {
            sourceCodeBuilder.AppendLine($"     {(!isInterface ? "public " : "")}{GetTypeName(property.PropertyType)} {property.Name}{(!isInterface ? $" {{ get => _moduleClient.{property.Name}; }}" : " { get; }")}");
        }
    }

    var methods = moduleClientType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object));
    foreach (var method in methods)
    {
        var parameters = string.Join(", ", method.GetParameters()
            .Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"));

        var parameters2 = string.Join(", ", method.GetParameters()
            .Select(p => $"{p.Name}"));

        sourceCodeBuilder.AppendLine($"     {(!isInterface ? "public " : "")}{GetTypeName(method.ReturnType)} {method.Name}({parameters}){(!isInterface ? $" => _moduleClient.{method.Name}({parameters2});" : ";")}");
    }

    return sourceCodeBuilder.ToString();
}

// Class
File.WriteAllText("ModuleClient.cs", $@"// THIS DOCUMENT IS GENERATED, ALL CHANGES WILL GET OVERWRITTEN!
#nullable disable

namespace Bader.Edge.ModuleHost;

[System.CodeDom.Compiler.GeneratedCode(""ModuleClientGenerator.csx"", ""1.0"")]
public class ModuleClient : IModuleClient
{{
    private Microsoft.Azure.Devices.Client.ModuleClient _moduleClient;

    public ModuleClient(Microsoft.Azure.Devices.Client.ModuleClient moduleClient) => _moduleClient = moduleClient;

{Generate()}
}}");

// Interface
File.AppendAllText("ModuleClient.cs", $@"

[System.CodeDom.Compiler.GeneratedCode(""ModuleClientGenerator.csx"", ""1.0"")]
public interface IModuleClient
{{
{Generate(true)}
}}
");
