using MQTTnet;
using MQTTnet.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Shared.Services;

public interface IMqttService
{
    Task PublishAsync(string topic, string payload);
    Task ConnectAsync();
    Task DisconnectAsync();
}

public class MqttService : IMqttService
{
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<MqttService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _serviceName;

    public MqttService(ILogger<MqttService> logger, IConfiguration configuration, string serviceName = "Service")
    {
        _logger = logger;
        _configuration = configuration;
        _serviceName = serviceName;
        _mqttClient = new MqttFactory().CreateMqttClient();
    }

    public async Task ConnectAsync()
    {
        try
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_configuration["Mqtt:Host"] ?? "localhost", int.Parse(_configuration["Mqtt:Port"] ?? "1883"))
                .WithClientId($"{_serviceName}-{Guid.NewGuid().ToString()}")
                .Build();

            await _mqttClient.ConnectAsync(options, CancellationToken.None);
            _logger.LogInformation("MQTT client connected from {ServiceName}", _serviceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT broker");
            throw;
        }
    }

    public async Task PublishAsync(string topic, string payload)
    {
        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
            _logger.LogInformation("Published message to topic: {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic: {Topic}", topic);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("MQTT client disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from MQTT broker");
        }
    }
}
