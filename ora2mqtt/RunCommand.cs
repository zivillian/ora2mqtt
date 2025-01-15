using System.Globalization;
using System.Text.Json;
using CommandLine;
using libgwmapi;
using MQTTnet;
using YamlDotNet.Serialization;
using libgwmapi.DTO.UserAuth;
using libgwmapi.DTO.Vehicle;
using Microsoft.Extensions.Logging;
using ora2mqtt.Logging;

namespace ora2mqtt;

[Verb("run", true, HelpText = "default")]
public class RunCommand:BaseCommand
{
    private ILogger _logger;

    [Option('i', "interval", Default = 10, HelpText = "GWM API polling interval")]
    public int Intervall { get; set; }

    public async Task<int> Run(CancellationToken cancellationToken)
    {
        Setup();
        _logger = LoggerFactory.CreateLogger<RunCommand>();
        if (!File.Exists(ConfigFile))
        {
            _logger.LogError($"config file ({ConfigFile}) missing");
            return 1;
        }
        Ora2MqttOptions config;
        var deserializer = new Deserializer();
        using (var file = File.OpenText(ConfigFile))
        {
            config = deserializer.Deserialize<Ora2MqttOptions>(file);
        }

        var api = GetGwmApiClient(config);
        using var mqtt = await ConnectMqttAsync(config.Mqtt, api, cancellationToken);

        var publishHaDiscovery = config.Mqtt.HomeAssistantDiscoveryTopic is not null;

        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Intervall));
            while (!cancellationToken.IsCancellationRequested)
            {
                await RefreshTokenAsync(api, config, cancellationToken);
                await PublishStatusAsync(mqtt, api, config.Mqtt, publishHaDiscovery, cancellationToken);
                if (publishHaDiscovery)
                {
                    publishHaDiscovery = false;
                }
                await timer.WaitForNextTickAsync(cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            //ignore
        }
        return 0;
    }

    private async Task<IMqttClient> ConnectMqttAsync(Ora2MqttMqttOptions options, GwmApiClient api, CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory(new MqttLogger(LoggerFactory));
        var client = factory.CreateMqttClient();
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(options.Host)
            .WithTlsOptions(new MqttClientTlsOptions { UseTls = options.UseTls });
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
        if (options.HomeAssistantDiscoveryTopic is not null)
        {
            client.ApplicationMessageReceivedAsync += x => OnMessageAsync(x, client, api, options, cancellationToken);
            await client.SubscribeAsync($"{options.HomeAssistantDiscoveryTopic}/status", cancellationToken: cancellationToken);
        }
        return client;
    }

    private Task OnMessageAsync(MqttApplicationMessageReceivedEventArgs arg, IMqttClient mqtt, GwmApiClient api, Ora2MqttMqttOptions options, CancellationToken cancellationToken)
    {
        if (arg.ApplicationMessage.Topic == $"{options.HomeAssistantDiscoveryTopic}/status")
        {
            return PublishStatusAsync(mqtt, api, options, true, cancellationToken);
        }
        return Task.CompletedTask;
    }

    private GwmApiClient GetGwmApiClient(Ora2MqttOptions options)
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
            _logger.LogError($"Access token expired ({e.Message}). Trying to refresh token...");
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

    private async Task PublishStatusAsync(IMqttClient mqtt, GwmApiClient gwm, Ora2MqttMqttOptions options, bool publishHaDiscovery, CancellationToken cancellationToken)
    {
        var vehicles = await gwm.AcquireVehiclesAsync(cancellationToken);
        foreach (var vehicle in vehicles)
        {
            var status = await gwm.GetLastVehicleStatusAsync(vehicle.Vin, cancellationToken);
            var topicPrefix = $"GWM/{vehicle.Vin}/status";
            if (publishHaDiscovery)
            {
                await PublishHaDiscoveryAsync(mqtt, options, vehicle, status, cancellationToken);
            }
            await PublishMessageAsync(mqtt, $"{topicPrefix}/AcquisitionTime", status.AcquisitionTime, cancellationToken);
            await PublishMessageAsync(mqtt, $"{topicPrefix}/UpdateTime", status.UpdateTime, cancellationToken);
            if (status.Latitude.HasValue && status.Longitude.HasValue)
            {
                await PublishMessageAsync(mqtt, $"{topicPrefix}/Latitude", status.Latitude.Value, cancellationToken);
                await PublishMessageAsync(mqtt, $"{topicPrefix}/Longitude", status.Longitude.Value, cancellationToken);
                await PublishMessageAsync(mqtt, $"{topicPrefix}/Location", JsonSerializer.Serialize(new
                {
                    latitude = status.Latitude.Value,
                    longitude = status.Longitude.Value
                }), cancellationToken);
            }

            foreach (var item in status.Items)
            {
                if (item.Value is null) continue;
                await PublishMessageAsync(mqtt, $"{topicPrefix}/items/{item.Code}/value", item.Value.ToString(), cancellationToken);
                if (item.Unit is not null)
                    await PublishMessageAsync(mqtt, $"{topicPrefix}/items/{item.Code}/unit", item.Unit, cancellationToken);
            }
        }
    }

    private Task PublishHaDiscoveryAsync(IMqttClient mqtt, Ora2MqttMqttOptions options, Vehicle vehicle, VehicleStatus status, CancellationToken cancellationToken)
    {
        var topicPrefix = $"GWM/{vehicle.Vin}/status";
        var json = JsonSerializer.Serialize(new
        {
            dev = new
            {
                ids = vehicle.Vin,
                name = vehicle.AppShowSeriesName,
                mf = vehicle.BrandName,
                mdl = vehicle.Vtype,
                sn = status.DeviceId
            },
            o = new
            {
                name = "ora2mqtt",
                url = "https://github.com/zivillian/ora2mqtt"
            },
            cmps = new
            {
                location = new
                {
                    p="device_tracker",
                    icon="mdi:map-marker",
                    json_attributes_topic=$"{topicPrefix}/Location",
                    unique_id=$"gwm_{vehicle.Vin}_location",
                    name="Location"
                },
                acquisition=new
                {
                    p = "sensor",
                    device_class = "timestamp",
                    unique_id = $"gwm_{vehicle.Vin}_AcquisitionTime",
                    state_topic = $"{topicPrefix}/AcquisitionTime",
                    name = "Acquisition",
                    value_template = "{{ (value|int // 1000) | timestamp_utc }}"
                },
                status_2013021 = new
                {
                    p="sensor",
                    device_class = "battery",
                    unique_id= $"gwm_{vehicle.Vin}_2013021",
                    unit_of_measurement="%",
                    state_topic= $"{topicPrefix}/items/2013021/value",
                    state_class= "measurement",
                    name="SOC"
                },
                status_2011501 = new
                {
                    p="sensor",
                    device_class = "distance",
                    unique_id = $"gwm_{vehicle.Vin}_2011501",
                    unit_of_measurement ="km",
                    state_topic = $"{topicPrefix}/items/2011501/value",
                    state_class = "measurement",
                    name="Range"
                },
                status_2041301 = new
                {
                    p="sensor",
                    unique_id = $"gwm_{vehicle.Vin}_2041301",
                    unit_of_measurement ="%",
                    state_topic = $"{topicPrefix}/items/2041301/value",
                    state_class = "measurement",
                    name="SOCE",
                    icon= "mdi:battery-heart-variant"
                },
                status_2101001 = new
                {
                    p="sensor",
                    device_class = "pressure",
                    unique_id = $"gwm_{vehicle.Vin}_2101001",
                    unit_of_measurement ="kPa",
                    state_topic = $"{topicPrefix}/items/2101001/value",
                    state_class = "measurement",
                    name="Tire Pressure FL",
                    icon= "mdi:car-tire-alert"
                },
                status_2101002 = new
                {
                    p="sensor",
                    device_class = "pressure",
                    unique_id = $"gwm_{vehicle.Vin}_2101002",
                    unit_of_measurement ="kPa",
                    state_topic = $"{topicPrefix}/items/2101002/value",
                    state_class = "measurement",
                    name="Tire Pressure FR",
                    icon= "mdi:car-tire-alert"
                },
                status_2101003 = new
                {
                    p="sensor",
                    device_class = "pressure",
                    unique_id = $"gwm_{vehicle.Vin}_2101003",
                    unit_of_measurement ="kPa",
                    state_topic = $"{topicPrefix}/items/2101003/value",
                    state_class = "measurement",
                    name="Tire Pressure RL",
                    icon= "mdi:car-tire-alert"
                },
                status_2101004 = new
                {
                    p="sensor",
                    device_class = "pressure",
                    unique_id = $"gwm_{vehicle.Vin}_2101004",
                    unit_of_measurement ="kPa",
                    state_topic = $"{topicPrefix}/items/2101004/value",
                    state_class = "measurement",
                    name="Tire Pressure RR",
                    icon= "mdi:car-tire-alert"
                },
                status_2101005 = new
                {
                    p="sensor",
                    device_class = "temperature",
                    unique_id = $"gwm_{vehicle.Vin}_2101005",
                    unit_of_measurement ="°C",
                    state_topic = $"{topicPrefix}/items/2101005/value",
                    state_class = "measurement",
                    name="Tire Temperature FL"
                },
                status_2101006 = new
                {
                    p="sensor",
                    device_class = "temperature",
                    unique_id = $"gwm_{vehicle.Vin}_2101006",
                    unit_of_measurement ="°C",
                    state_topic = $"{topicPrefix}/items/2101006/value",
                    state_class = "measurement",
                    name="Tire Temperature FR"
                },
                status_2101007 = new
                {
                    p="sensor",
                    device_class = "temperature",
                    unique_id = $"gwm_{vehicle.Vin}_2101007",
                    unit_of_measurement ="°C",
                    state_topic = $"{topicPrefix}/items/2101007/value",
                    state_class = "measurement",
                    name="Tire Temperature RL"
                },
                status_2101008 = new
                {
                    p="sensor",
                    device_class = "temperature",
                    unique_id = $"gwm_{vehicle.Vin}_2101008",
                    unit_of_measurement ="°C",
                    state_topic = $"{topicPrefix}/items/2101008/value",
                    state_class = "measurement",
                    name="Tire Temperature RR"
                },
                status_2103010 = new
                {
                    p = "sensor",
                    device_class = "distance",
                    unique_id = $"gwm_{vehicle.Vin}_2103010",
                    unit_of_measurement = "km",
                    state_topic = $"{topicPrefix}/items/2103010/value",
                    state_class = "measurement",
                    name = "Odometer",
                    icon="mdi:counter"
                },
                status_2201001 = new
                {
                    p = "sensor",
                    device_class = "temperature",
                    unique_id = $"gwm_{vehicle.Vin}_2201001",
                    unit_of_measurement = "°C",
                    state_topic = $"{topicPrefix}/items/2201001/value",
                    state_class = "measurement",
                    name = "Interior Temperature",
                    value_template = "{{ value|int / 10 }}"
                },
                status_2202001 = new
                {
                    p = "binary_sensor",
                    unique_id = $"gwm_{vehicle.Vin}_2202001",
                    state_topic = $"{topicPrefix}/items/2202001/value",
                    name = "A/C",
                    payload_off = "0",
                    payload_on = "1",
                    icon= "mdi:air-conditioner"
                },
                status_2208001 = new
                {
                    p = "binary_sensor",
                    device_class = "lock",
                    unique_id = $"gwm_{vehicle.Vin}_2208001",
                    state_topic = $"{topicPrefix}/items/2208001/value",
                    name = "Lock",
                    payload_off = "0",
                    payload_on = "1"
                },
                status_2210001 = new
                {
                    p = "binary_sensor",
                    device_class = "window",
                    unique_id = $"gwm_{vehicle.Vin}_2210001",
                    state_topic = $"{topicPrefix}/items/2210001/value",
                    name = "Window FL",
                    payload_off = "1",
                    payload_on = "3"
                },
                status_2210002 = new
                {
                    p = "binary_sensor",
                    device_class = "window",
                    unique_id = $"gwm_{vehicle.Vin}_2210002",
                    state_topic = $"{topicPrefix}/items/2210002/value",
                    name = "Window FR",
                    payload_off = "1",
                    payload_on = "3"
                },
                status_2210003 = new
                {
                    p = "binary_sensor",
                    device_class = "window",
                    unique_id = $"gwm_{vehicle.Vin}_2210003",
                    state_topic = $"{topicPrefix}/items/2210003/value",
                    name = "Window RL",
                    payload_off = "1",
                    payload_on = "3"
                },
                status_2210004 = new
                {
                    p = "binary_sensor",
                    device_class = "window",
                    unique_id = $"gwm_{vehicle.Vin}_2210004",
                    state_topic = $"{topicPrefix}/items/2210004/value",
                    name = "Window RR",
                    payload_off = "1",
                    payload_on = "3"
                },
                status_2078020 = new
                {
                    p = "binary_sensor",
                    device_class = "running",
                    unique_id = $"gwm_{vehicle.Vin}_2078020",
                    state_topic = $"{topicPrefix}/items/2078020/value",
                    name = "Air Circulation",
                    payload_off = "0",
                    payload_on = "1"
                },
                status_2222001 = new
                {
                    p = "binary_sensor",
                    unique_id = $"gwm_{vehicle.Vin}_2222001",
                    state_topic = $"{topicPrefix}/items/2222001/value",
                    name = "Front defroster",
                    payload_off = "0",
                    payload_on = "1",
                    icon= "mdi:car-defrost-front"
                },
                status_2042082 = new
                {
                    p = "binary_sensor",
                    device_class= "plug",
                    unique_id = $"gwm_{vehicle.Vin}_2042082",
                    state_topic = $"{topicPrefix}/items/2042082/value",
                    name = "Charge plug",
                    payload_off = "0",
                    payload_on = "1",
                },
            }
        });
        return PublishMessageAsync(mqtt, $"{options.HomeAssistantDiscoveryTopic}/device/{vehicle.Vin}/config", json, cancellationToken);
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