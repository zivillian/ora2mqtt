using CommandLine;
using libgwmapi;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace ora2mqtt;

public abstract class BaseCommand
{
    [Option('d', "debug", Default = false, HelpText = "enable debug logging")]
    public bool Debug { get; set; }

    [Option('c', "config", Default = "ora2mqtt.yml", HelpText = "path to yaml config file")]
    public string ConfigFile { get; set; }

    protected ILoggerFactory LoggerFactory { get; private set; }

    protected void Setup()
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(x => x.SetMinimumLevel(Debug ? LogLevel.Trace : LogLevel.Error).AddConsole());
    }

    protected GwmApiClient ConfigureApiClient(Ora2MqttOptions options)
    {
        var certHandler = new CertificateHandler();
        var httpHandler = new HttpClientHandler();
        httpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
        using (var cert = certHandler.CertificateWithPrivateKey)
        {
            var pkcs12 = new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
            httpHandler.ClientCertificates.Add(pkcs12);
        }

        //TODO check how this behaves on linux and mac
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //add intermediates to local cert store, so they get sent with the request
            //https://github.com/dotnet/runtime/issues/55368#issuecomment-876775809
            var store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            var certs = certHandler.Chain;
            foreach (var cert in certs)
            {
                if (cert.Issuer != cert.Subject)
                {
                    store.Add(cert);
                }
            }
        }

        var httpLogger = LoggerFactory.CreateLogger<HttpClient>();
        var httpOptions = new HttpClientFactoryOptions
        {
            ShouldRedactHeaderValue = x => "accessToken".Equals(x, StringComparison.InvariantCultureIgnoreCase)
        };
        var h5Client = new HttpClient(new LoggingHttpMessageHandler(httpLogger, httpOptions)
        {
            InnerHandler = new HttpClientHandler()
        });
        var appClient = new HttpClient(new LoggingHttpMessageHandler(httpLogger, httpOptions)
        {
            InnerHandler = httpHandler
        });
        return new GwmApiClient(h5Client, appClient, LoggerFactory)
        {
            Country = options.Country
        };
    }

    protected async Task SaveConfigAsync(Ora2MqttOptions options, CancellationToken cancellationToken)
    {
        var serializer = new Serializer();
        await using var configFile = File.OpenWrite(ConfigFile);
        configFile.SetLength(0);
        await using var writer = new StreamWriter(configFile);
        serializer.Serialize(writer, options);
        await writer.FlushAsync();
        await configFile.FlushAsync(cancellationToken);
    }
}