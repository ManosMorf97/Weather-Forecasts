using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApplication.IntegrationTesting
{
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class SequentialCollection : ICollectionFixture<WeatherForecastFactory> { }

}
