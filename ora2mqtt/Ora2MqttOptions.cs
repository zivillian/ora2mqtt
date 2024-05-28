namespace ora2mqtt;

public class Ora2MqttOptions
{
    public string DeviceId { get; set; }

    public string Country { get; set; }

    public Ora2MqttAccountOptions Account { get; set; } = new();

    public Ora2MqttMqttOptions Mqtt { get; set; } = new();
}

public class Ora2MqttAccountOptions
{
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public string GwId { get; set; }

    public string BeanId { get; set; }
}

public class Ora2MqttMqttOptions
{
    public string Host { get; set; }
    
    public string Username { get; set; }

    public string Password { get; set; }

    public bool UseTls { get; set; }
}