﻿using System.Globalization;
using CommandLine;
using libgwmapi;
using MQTTnet.Client;
using MQTTnet;
using YamlDotNet.Serialization;
using MQTTnet.Server;
using libgwmapi.DTO.UserAuth;

namespace ora2mqtt;

[Verb("run", true, HelpText = "default")]
public class RunCommand:BaseCommand
{
    [Option('i', "interval", Default = 10, HelpText = "GWM API polling interval")]
    public int Intervall { get; set; }

    public async Task<int> Run(CancellationToken cancellationToken)
    {
        if (!File.Exists(ConfigFile))
        {
            await Console.Error.WriteLineAsync($"config file ({ConfigFile}) missing");
        }
        Ora2MqttOptions config;
        var deserializer = new Deserializer();
        using (var file = File.OpenText(ConfigFile))
        {
            config = deserializer.Deserialize<Ora2MqttOptions>(file);
        }

        using var mqtt = await ConnectMqttAsync(config.Mqtt, cancellationToken);

        var api = GetGwmApiClient(config, cancellationToken);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await RefreshTokenAsync(api, config, cancellationToken);
                await PublishStatusAsync(mqtt, api, cancellationToken);
                await Task.Delay(Intervall * 1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            //ignore
        }
        return 0;
    }

    private async Task<IMqttClient> ConnectMqttAsync(Ora2MqttMqttOptions options,CancellationToken cancellationToken)
    {
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(options.Host);
        if (!String.IsNullOrEmpty(options.Username) && !String.IsNullOrEmpty(options.Password))
        {
            builder = builder.WithCredentials(options.Username, options.Password);
        }

        client.DisconnectedAsync += async e =>
        {
            if (e.ClientWasConnected)
            {
                await client.ConnectAsync(client.Options, cancellationToken);
            }
        };

        await client.ConnectAsync(builder.Build(), cancellationToken);
        return client;
    }

    private GwmApiClient GetGwmApiClient(Ora2MqttOptions options, CancellationToken cancellationToken)
    {
        var client = ConfigureApiClient(options);
        client.SetAccessToken(options.Account.AccessToken);
        return client;
    }

    private async Task RefreshTokenAsync(GwmApiClient client, Ora2MqttOptions options, CancellationToken cancellationToken)
    {
        try
        {
            //check token
            await client.GetUserBaseInfoAsync(cancellationToken);
            return;
        }
        catch (GwmApiException e)
        {
            await Console.Error.WriteLineAsync($"Access token expired ({e.Message}). Trying to refresh token...");
        }

        var refresh = new RefreshTokenRequest
        {
            DeviceId = options.DeviceId,
            AccessToken = options.Account.AccessToken,
            RefreshToken = options.Account.RefreshToken,
        };
        client.SetAccessToken("");
        var response = await client.RefreshTokenAsync(refresh, cancellationToken);
        options.Account.AccessToken = response.AccessToken;
        options.Account.RefreshToken = response.RefreshToken;
        await SaveConfigAsync(options, cancellationToken);
        client.SetAccessToken(options.Account.AccessToken);
    }

    private async Task PublishStatusAsync(IMqttClient mqtt, GwmApiClient gwm, CancellationToken cancellationToken)
    {
        var vehicles = await gwm.AquireVehiclesAsync(cancellationToken);
        foreach (var vehicle in vehicles)
        {
            var status = await gwm.GetLastVehicleStatusAsync(vehicle.Vin, cancellationToken);
            var topicPrefix = $"GWM/{vehicle.Vin}/status";
            await PublishMessageAsync(mqtt, $"{topicPrefix}/AcquisitionTime", status.AcquisitionTime, cancellationToken);
            await PublishMessageAsync(mqtt, $"{topicPrefix}/UpdateTime", status.UpdateTime, cancellationToken);
            await PublishMessageAsync(mqtt, $"{topicPrefix}/Latitude", status.Latitude, cancellationToken);
            await PublishMessageAsync(mqtt, $"{topicPrefix}/Longitude", status.Longitude, cancellationToken);
            foreach (var item in status.Items)
            {
                if (item.Value is null) continue;
                await PublishMessageAsync(mqtt, $"{topicPrefix}/items/{item.Code}/value", item.Value.ToString(), cancellationToken);
                if (item.Unit is not null)
                    await PublishMessageAsync(mqtt, $"{topicPrefix}/items/{item.Code}/unit", item.Unit, cancellationToken);
            }
        }
    }

    private Task PublishMessageAsync(IMqttClient client, string topic, double payload, CancellationToken cancellationToken)
    {
        return PublishMessageAsync(client, topic, payload.ToString(CultureInfo.InvariantCulture), cancellationToken);
    }

    private Task PublishMessageAsync(IMqttClient client, string topic, long payload, CancellationToken cancellationToken)
    {
        return PublishMessageAsync(client, topic, payload.ToString(CultureInfo.InvariantCulture), cancellationToken);
    }

    private Task PublishMessageAsync(IMqttClient client, string topic, string payload, CancellationToken cancellationToken)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();
        return client.PublishAsync(message, cancellationToken);
    }
}