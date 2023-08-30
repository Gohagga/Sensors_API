using Microsoft.AspNetCore.Mvc;
using Sensors.API.Models;
using Sensors.Data.Models;
using Sensors.Data.Repos;

namespace Sensors.API.Controllers
{
    [ApiController]
    [Route("sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly ILogger<SensorsController> _logger;
        private readonly SensorRepository _repo;
        
        //private static List<SensorInfo>? _sensorInfos;
        //private static Dictionary<string, SensorData>? _sensorData;

        public SensorsController(
            ILogger<SensorsController> logger,
            SensorRepository repo)
        {
            _logger = logger;
            _repo = repo;

            //_sensorInfos ??= Enumerable.Range(1, 5).Select(index =>
            //    new SensorInfo(
            //        Id: Guid.NewGuid().ToString(),
            //        Label: $"SENZOR{index}"))
            //    .ToList();

            //_sensorData ??= _sensorInfos.ToDictionary(k => k.Id, v => new SensorData(
            //    SensorId: v.Id,
            //    HitsPerPeriod: Enumerable.Range(0, 10000).Select(y => Random.Shared.Next(0, 40)).ToList()));
        }

        [HttpGet("get-sensors")]
        public ActionResult<GetSensorsResponse> GetSensors()
        {
            var retVal = new GetSensorsResponse();
            retVal.AddRange(_repo.GetSensors());
            return retVal;
        }

        [HttpPost("get-sensor-data")]
        public ActionResult<GetSensorDataResponse> GetSensorData([FromBody] GetSensorDataRequest request)
        {
            var retVal = new GetSensorDataResponse();

            retVal.AddRange(_repo.GetSensorData(request.From, request.To, request.Period, request.SensorsToReturn));

            return retVal;
        }

        //[HttpPost("get-sensor-data")]
        //public ActionResult<GetSensorDataResponse> GetSensorData([FromBody] GetSensorDataRequest request)
        //{
        //    var retVal = new GetSensorDataResponse();

        //    foreach (var sensorId in request.SensorsToReturn)
        //    {
        //        var data = _sensorData[sensorId];

        //        var hitsPerPeriod = request.Period switch
        //        {
        //            TimePeriod.Minute => data.HitsPerPeriod,
        //            TimePeriod.Hour => SumEveryN(data.HitsPerPeriod, 60),
        //            TimePeriod.Day => SumEveryN(data.HitsPerPeriod, 1440),
        //            TimePeriod.Month => SumEveryN(data.HitsPerPeriod, 43200),
        //            TimePeriod.Quarter => SumEveryN(data.HitsPerPeriod, 129600),
        //            _ => data.HitsPerPeriod
        //        };

        //        var sensorData = new SensorData(
        //            SensorId: data.SensorId,
        //            HitsPerPeriod: hitsPerPeriod);

        //        retVal.Add(sensorData);
        //    }

        //    return retVal;
        //}

        private static List<int> SumEveryN(List<int> data, int n)
        {
            List<int> result = new List<int>();
            int sum = 0;

            for (int i = 0; i < data.Count; i++)
            {
                sum += data[i];

                if ((i + 1) % n == 0)
                {
                    result.Add(sum);
                    sum = 0;
                }
            }

            // If there's a remainder, add it to the results
            if (data.Count % n != 0)
            {
                result.Add(sum);
            }

            return result;
        }
    }
}