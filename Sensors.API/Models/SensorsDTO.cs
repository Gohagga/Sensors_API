using Sensors.Data.Models;

namespace Sensors.API.Models
{
    // Requests
    public record GetSensorDataRequest(
        DateTime From,
        DateTime To,
        TimePeriod Period,
        List<string> SensorsToReturn,
        bool PerfectlyPredictedData = false);

    
    // Responses
    public class GetSensorsResponse : List<SensorInfo> { };

    public class GetSensorDataResponse : List<SensorData> { };
}
