using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sensors.Data.Entities;
using Sensors.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sensors.Data.Repos
{
    public class SensorRepository
    {
        private readonly IDbContextFactory<SensorsDbContext> _dbContextFactory;
        private readonly string _prolaz = "PROLAZ";

        public SensorRepository(
            IDbContextFactory<SensorsDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        private IQueryable<Device> GetDevices(SensorsDbContext dbContext)
        {
            return dbContext.Devices.Where(x => x.Name.StartsWith(_prolaz));
        }

        public List<SensorInfo> GetSensors()
        {
            using var db = _dbContextFactory.CreateDbContext();

            var data = GetDevices(db)
                .Select(x => new SensorInfo
                {
                    Id = x.Code,
                    Label = x.Name
                })
                .ToList();

            return data;
        }

        public List<SensorData> GetSensorData(DateTime from, DateTime to, TimePeriod period, List<string> sensorIds)
        {
            using var db = _dbContextFactory.CreateDbContext();

            var periodName = period switch
            {
                TimePeriod.Minute => "MINUTE",
                TimePeriod.Hour => "HOUR",
                TimePeriod.Day => "DAY",
                TimePeriod.Month => "MONTH",
                TimePeriod.Quarter => throw new NotImplementedException(),
                _ => "MINUTE"
            };

            var sensorIdList = string.Join(",", sensorIds.Select(x => "'" + x + "'"));

            var commandText = @$"
                SELECT
	                d.Code as deviceId,
	                d.[Name] as name,
	                count(c.Id) as count,
	                dateadd({periodName}, datediff({periodName}, 0, c.Created), 0) as countDatetime
                FROM dbo.Devices d
                LEFT JOIN Counters c ON c.DeviceId = d.Id
                WHERE d.Code IN ({sensorIdList})
                    AND c.Created > @from
                    AND c.Created < @to
                group by
	                d.Code,
	                d.[Name],
	                dateadd({periodName}, datediff({periodName}, 0, c.Created), 0)
                ORDER BY countDatetime";

            var result = db.CounterQuery.FromSqlRaw(commandText,
                new SqlParameter("@periodName", periodName),
                new SqlParameter("@from", from),
                new SqlParameter("@to", to))
                .ToList();

            var sensors = new Dictionary<string, SensorData>();
            foreach (var row in result)
            {
                var deviceId = row.DeviceId.ToString();
                if (!sensors.TryGetValue(deviceId, out var sensorData))
                    sensorData = sensors[deviceId] = new SensorData()
                    {
                        SensorId = deviceId,
                        HitsPerPeriod = new List<int>(),
                        LastDateTime = from
                    };

                while (sensorData.LastDateTime < row.CountDateTime)
                {
                    sensorData.HitsPerPeriod.Add(0);
                    sensorData.LastDateTime = period switch
                    {
                        TimePeriod.Minute => sensorData.LastDateTime.AddMinutes(1),
                        TimePeriod.Hour => sensorData.LastDateTime.AddHours(1),
                        TimePeriod.Day => sensorData.LastDateTime.AddDays(1),
                        TimePeriod.Month => sensorData.LastDateTime.AddMonths(1),
                        TimePeriod.Quarter => throw new NotImplementedException(),
                        _ => sensorData.LastDateTime.AddMinutes(1)
                    };
                }

                sensorData.HitsPerPeriod.Add(row.Count);
            }

            return sensors.Values.ToList();
        }
    }
}
