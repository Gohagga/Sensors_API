namespace Sensors.Data.Models
{
    public class SensorInfo
    {
        public string Id { get; set; }
        public string Label { get; set; }
    }

    public class SensorData
    {
        public List<int> HitsPerPeriod { get; set; }
        public string SensorId { get; set; }

        public DateTime LastDateTime { get; set; }
    }

    public enum TimePeriod
    {
        Minute = 0,
        Hour = 1,
        Day = 2,
        Month = 3,
        Quarter = 4,
    }
}
