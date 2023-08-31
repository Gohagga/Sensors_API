using Microsoft.AspNetCore.Mvc;
using Sensors.API.Models;
using Sensors.Data.Models;
using Sensors.Data.Repos;
using Sensors.Data.Services;

namespace Sensors.API.Controllers
{
    [ApiController]
    [Route("sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly ILogger<SensorsController> _logger;
        private readonly SensorRepository _repo;
        private readonly PredictionService _predictionService;

        public SensorsController(
            ILogger<SensorsController> logger,
            SensorRepository repo,
            PredictionService predictionService)
        {
            _logger = logger;
            _repo = repo;
            _predictionService = predictionService;
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

            if (request.PerfectlyPredictedData)
            {
                retVal.AddRange(_predictionService.GetPredictions(request.From, request.To, request.Period, request.SensorsToReturn));
            }
            else
            {
                retVal.AddRange(_repo.GetSensorData(request.From, request.To, request.Period, request.SensorsToReturn));
            }

            return retVal;
        }

        [HttpPost("calculate-predictions")]
        public ActionResult CalculatePredictions()
        {
            _predictionService.CalculatePredictions();

            return this.Ok();
        }
    }
}