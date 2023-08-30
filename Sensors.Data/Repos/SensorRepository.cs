using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sensors.Data.Entities;
using Sensors.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Id = x.Id.ToString(),
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
	                d.Id as deviceId,
	                d.[Name] as name,
	                count(c.Id) as count,
	                dateadd({periodName}, datediff({periodName}, 0, c.Created), 0) as countDatetime
                FROM dbo.Devices d
                LEFT JOIN Counters c ON c.DeviceId = d.Id
                WHERE d.Id IN ({sensorIdList})
                    AND c.Created > @from
                    AND c.Created < @to
                group by
	                d.Id,
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
                        HitsPerPeriod = new List<int>()
                    };

                sensorData.HitsPerPeriod.Add(row.Count);
            }

            return sensors.Values.ToList();
        }
    }
}
