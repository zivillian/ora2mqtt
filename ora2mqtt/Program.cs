using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using CommandLine;
using libgwmapi;
using libgwmapi.DTO.UserAuth;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using ora2mqtt;
using Sharprompt;
using Sharprompt.Fluent;
using YamlDotNet.Serialization;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

try
{
    return await Parser.Default.ParseArguments<ConfigureCommand, RunCommand>(args)
        .MapResult(
            (ConfigureCommand x) => x.Run(cts.Token),
            (RunCommand x) => x.Run(cts.Token),
            _ => Task.FromResult(1));
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return -1;
}