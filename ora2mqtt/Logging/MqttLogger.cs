using Microsoft.Extensions.Logging;
using MQTTnet.Diagnostics.Logger;

namespace ora2mqtt.Logging
{
    internal class MqttLogger: IMqttNetLogger
    {
        private readonly ILogger<MqttLogger> _logger;

        public MqttLogger(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger<MqttLogger>();
        }

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            var level = logLevel switch
            {
                MqttNetLogLevel.Verbose => LogLevel.Debug,
                MqttNetLogLevel.Info => LogLevel.Information,
                MqttNetLogLevel.Warning => LogLevel.Warning,
                MqttNetLogLevel.Error => LogLevel.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
            _logger.Log(level, exception, message, parameters);
        }

        public bool IsEnabled => true;
    }
}
