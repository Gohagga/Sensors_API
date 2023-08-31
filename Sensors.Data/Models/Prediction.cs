using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensors.Data.Models
{
    public class PredictionData
    {
        public Dictionary<DayOfWeek, PolyFunction> WeekdayFunction { get; set; } = new Dictionary<DayOfWeek, PolyFunction>();
        public Dictionary<int, PolyFunction> MonthFunction { get; set; } = new Dictionary<int, PolyFunction>();
    }

    public class PolyFunction
    {
        private double[] _coefficients;
        private int _degree;

        public PolyFunction(double[] coefficients, int degree)
        {
            _coefficients = coefficients;
            _degree = degree;
        }

        public double SolveFor(double x)
        {
            if (_degree > _coefficients.Length) return 0.0;

            double y = 0;

            for (int i = 0; i <= _degree; i++)
            {
                y += _coefficients[_degree - i] * Math.Pow(x, i);
            }

            return y;
        }
    }
}
