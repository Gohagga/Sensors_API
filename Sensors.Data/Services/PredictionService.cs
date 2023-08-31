using MathNet.Numerics.LinearAlgebra;
using Sensors.Data.Models;
using Sensors.Data.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Sensors.Data.Services
{
    public class PredictionService
    {
        private readonly Dictionary<string, PredictionData> _sensorPredictionData = new Dictionary<string, PredictionData>();
        private readonly SensorRepository _sensorRepository;

        public PredictionService(
            SensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public List<SensorData> GetPredictions(DateTime from, DateTime to, TimePeriod period, List<string> sensorIds)
        {
            var retVal = new Dictionary<string, SensorData>();
            var date = from.AddMinutes(0);

            while (date < to)
            {
                foreach (var sensorId in sensorIds)
                {
                    //var guid = Guid.Parse(sensorId);
                    var guid = sensorId;

                    if (!retVal.TryGetValue(guid, out var sensorData))
                        retVal[guid] = sensorData = new SensorData()
                        {
                            HitsPerPeriod = new List<int>(),
                            SensorId = sensorId.ToLower()
                        };

                    var hits = _sensorPredictionData[guid].WeekdayFunction[date.DayOfWeek].SolveFor((int)date.TimeOfDay.Hours);
                    if (hits < 0) hits = 0;
                    sensorData.HitsPerPeriod.Add((int)hits);
                }

                date = period switch
                {
                    TimePeriod.Minute => date.AddMinutes(1),
                    TimePeriod.Hour => date.AddHours(1),
                    TimePeriod.Day => date.AddDays(1),
                    TimePeriod.Month => date.AddMonths(1),
                    TimePeriod.Quarter => throw new NotImplementedException(),
                    _ => date.AddMinutes(1)
                };
            }

            return retVal.Values.ToList();
        }

        public void CalculatePredictions()
        {
            // Get data from now to two years into the past
            var startDate = DateTime.UtcNow.AddDays(-3);
            var sensorInfos = _sensorRepository.GetSensors();
            var sensorData = _sensorRepository.GetSensorData(
                from: startDate,
                to: DateTime.UtcNow,
                period: TimePeriod.Hour,
                sensorIds: sensorInfos.Select(x => x.Id).ToList());

            foreach (var data in sensorData)
            {
                // Make a pass for each day of the week
                Dictionary<DayOfWeek, (List<double> xValues, List<double> yValues)?> weekdays = new();
                var date = startDate.AddMinutes(0);

                foreach (var hitCount in data.HitsPerPeriod)
                {
                    date = date.AddHours(1);

                    if (!weekdays.TryGetValue(date.DayOfWeek, out var values))
                        weekdays[date.DayOfWeek] = values = (new List<double>(), new List<double>());

                    values.Value.xValues.Add((int)date.TimeOfDay.Hours);
                    values.Value.yValues.Add(hitCount);
                }

                PolyFunction getFunction(DayOfWeek dayOfWeek) {
                    var degree = 2;
                    var maybeValues = weekdays.GetValueOrDefault(dayOfWeek);
                    if (maybeValues == null || maybeValues.Value.xValues == null || maybeValues.Value.yValues == null)
                        return new PolyFunction(new double[] { }, degree);

                    var values = maybeValues.Value;

                    // Perform moving average
                    //values.yValues = MovingAverage(values.yValues, 2);

                    var coef = PolynomialRegression(values.xValues.ToArray(), values.yValues.ToArray(), degree);
                    var poly = new PolyFunction(coef, degree);
                    return poly;
                }

                var prediction = _sensorPredictionData[data.SensorId] = new PredictionData();
                prediction.WeekdayFunction[DayOfWeek.Monday] = getFunction(DayOfWeek.Monday);
                prediction.WeekdayFunction[DayOfWeek.Tuesday] = getFunction(DayOfWeek.Tuesday);
                prediction.WeekdayFunction[DayOfWeek.Wednesday] = getFunction(DayOfWeek.Wednesday);
                prediction.WeekdayFunction[DayOfWeek.Thursday] = getFunction(DayOfWeek.Thursday);
                prediction.WeekdayFunction[DayOfWeek.Friday] = getFunction(DayOfWeek.Friday);
                prediction.WeekdayFunction[DayOfWeek.Saturday] = getFunction(DayOfWeek.Saturday);
                prediction.WeekdayFunction[DayOfWeek.Sunday] = getFunction(DayOfWeek.Sunday);
            }
        }

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

        public double[] PolynomialRegression(double[] xValues, double[] yValues, int degree)
        {
            var vandermondeMatrix = Matrix<double>.Build.Dense(xValues.Length, degree + 1,
                (i, j) => Math.Pow(xValues[i], j));
            var yVector = Vector<double>.Build.Dense(yValues);
            var p = vandermondeMatrix.TransposeThisAndMultiply(vandermondeMatrix)
                .Cholesky()
                .Solve(vandermondeMatrix.TransposeThisAndMultiply(yVector));

            return p.Reverse().ToArray();
        }

        public List<double> MovingAverage(List<double> data, int windowSize)
        {
            int halfWindowSize = windowSize / 2;
            var result = new List<double>();

            for (int i = 0; i < data.Count; i++)
            {
                int start = Math.Max(0, i - halfWindowSize);
                int end = Math.Min(data.Count - 1, i + halfWindowSize);
                double sum = 0;

                for (int j = start; j <= end; j++)
                {
                    sum += data[j];
                }

                result.Add(sum / (end - start + 1));
            }

            return result;
        }
    }
}
