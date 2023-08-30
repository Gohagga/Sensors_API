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


    // Models
    public record SensorInfo(
        string Id,
        string Label);

    public record SensorData(
        List<int> HitsPerPeriod,
        string SensorId);

    public enum TimePeriod
    {
        Minute = 0,
        Hour = 1,
        Day = 2,
        Month = 3,
        Quarter = 4,
    }
}
