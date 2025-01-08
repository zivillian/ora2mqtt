using CommandLine;
using libgwmapi.DTO.UserAuth;
using libgwmapi;
using MQTTnet.Exceptions;
using MQTTnet;
using Sharprompt;
using Sharprompt.Fluent;
using YamlDotNet.Serialization;
using ora2mqtt.Logging;
using Microsoft.Extensions.Logging;

namespace ora2mqtt
{
    [Verb("configure", HelpText = "run config file wizard")]
    public class ConfigureCommand:BaseCommand
    {
        private ILogger<ConfigureCommand> _logger;

        public async Task<int> Run(CancellationToken cancellationToken)
        {
            Setup();
            _logger = LoggerFactory.CreateLogger<ConfigureCommand>();
            Ora2MqttOptions config;
            if (!File.Exists(ConfigFile))
            {
                config = new Ora2MqttOptions
                {
                    DeviceId = Guid.NewGuid().ToString("N"),
                };
            }
            else
            {
                var deserializer = new Deserializer();
                using (var file = File.OpenText(ConfigFile))
                {
                    config = deserializer.Deserialize<Ora2MqttOptions>(file);
                }
            }
            await SaveConfigAsync(config, cancellationToken);

            SelectCountry(config);
            await SaveConfigAsync(config, cancellationToken);

            var client = ConfigureApiClient(config);

            await LoginAsync(client, config, cancellationToken);
            await SaveConfigAsync(config, cancellationToken);

            while (!await TestMqttAsync(config, cancellationToken))
            {
                ConfigureMqttAsync(config);
            }
            await SaveConfigAsync(config, cancellationToken);

            Console.WriteLine("Configuration successful!");
            return 0;
        }

        private void SelectCountry(Ora2MqttOptions options)
        {
            if (String.IsNullOrEmpty(options.Country))
            {
                options.Country = Prompt.Select<string>(o => o
                    .WithMessage("Please choose your country")
                    .WithItems(new[] { "DE", "GB", "EE" })
                );
            }
        }

        private async Task LoginAsync(GwmApiClient client, Ora2MqttOptions options, CancellationToken cancellationToken)
        {
            if (!String.IsNullOrEmpty(options.Account.AccessToken))
            {
                try
                {
                    client.SetAccessToken(options.Account.AccessToken);
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
                try
                {
                    var response = await client.RefreshTokenAsync(refresh, cancellationToken);
                    options.Account.AccessToken = response.AccessToken;
                    options.Account.RefreshToken = response.RefreshToken;
                    return;
                }
                catch (GwmApiException e)
                {
                    _logger.LogError($"Token refresh failed: {e.Message}");
                }
            }
            var request = new LoginAccountRequest
            {
                Country = options.Country,
                IsEncrypt = false,
                DeviceId = options.DeviceId,
                Model = "ora2mqtt",
                PushToken = "",
                Account = Prompt.Input<string>("Please enter your mail address"),
                Password = Prompt.Password("Please enter your password")
            };
            try
            {
                var token = await client.LoginAccountAsync(request, cancellationToken);
                options.Account.AccessToken = token.AccessToken;
                options.Account.RefreshToken = token.RefreshToken;
                options.Account.GwId = token.GwId;
                options.Account.BeanId = token.BeanId;
            }
            catch (GwmApiException e) when (e.Code == "110641")
            {
                //SMS Login
                await client.GetSmsCodeAsync(new GetSmsCode { Email = request.Account }, cancellationToken);
                var code = Prompt.Password("Code required. Please check your mail and enter the 4 digit code");
                var loginRequest = new LoginWithSmsRequest
                {
                    Email = request.Account,
                    Country = "DE",
                    DeviceId = options.DeviceId,
                    Model = "ora2mqtt",
                    SmsCode = code
                };
                var token = await client.LoginWithSmsAsync(loginRequest, cancellationToken);
                options.Account.AccessToken = token.AccessToken;
                options.Account.RefreshToken = token.RefreshToken;
                options.Account.GwId = token.GwId;
                options.Account.BeanId = token.BeanId;
            }
        }

        private void ConfigureMqttAsync(Ora2MqttOptions oraOptions)
        {
            var options = oraOptions.Mqtt;
            options.Host = Prompt.Input<string>("Please enter your mqtt server host or ip", defaultValue: options.Host);

            if (!Prompt.Confirm("Does your mqtt server require credentials?"))
            {
                options.Username = String.Empty;
                options.Password = String.Empty;
            }
            else
            {
                options.Username = Prompt.Input<string>("Please enter your mqtt username", defaultValue: options.Username);
                options.Password = Prompt.Password("Please enter your mqtt password");
            }

            options.UseTls = Prompt.Confirm("Do you want to use TLS on port 8883?");
        }

        private async Task<bool> TestMqttAsync(Ora2MqttOptions oraOptions, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(oraOptions.Mqtt.Host)) return false;
            var options = oraOptions.Mqtt;

            try
            {
                var factory = new MqttClientFactory(new MqttLogger(LoggerFactory));
                using var client = factory.CreateMqttClient();
                var builder = new MqttClientOptionsBuilder()
                    .WithTcpServer(options.Host)
                    .WithTlsOptions(new MqttClientTlsOptions { UseTls = options.UseTls });
                if (!String.IsNullOrEmpty(options.Username) && !String.IsNullOrEmpty(options.Password))
                {
                    builder = builder.WithCredentials(options.Username, options.Password);
                }

                await client.ConnectAsync(builder.Build(), cancellationToken);
                await client.DisconnectAsync(cancellationToken: cancellationToken);
            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError($"Mqtt connection failed: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
