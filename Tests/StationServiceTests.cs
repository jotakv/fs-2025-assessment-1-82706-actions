using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Models;
using fs_2025_a_api_demo_002.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Tests
{
    public class StationServiceTests
    {
        private StationService BuildService(List<BikeModel> bikes)
        {
            var data = new BikeData(bikes);
            var cache = new MemoryCache(new MemoryCacheOptions());
            return new StationService(data, cache);
        }

        [Fact]
        public void Filters_By_Status_And_MinBikes()
        {
            var bikes = new List<BikeModel>
            {
                new BikeModel { number = 1, name = "One", address = "Main", status = "OPEN", bike_stands = 10, available_bikes = 5 },
                new BikeModel { number = 2, name = "Two", address = "Side", status = "CLOSED", bike_stands = 8, available_bikes = 6 },
                new BikeModel { number = 3, name = "Three", address = "Other", status = "OPEN", bike_stands = 12, available_bikes = 2 }
            };

            var service = BuildService(bikes);
            var result = service.GetStations("OPEN", 3, null, "name", "asc", 1, 10);

            Assert.Single(result);
            Assert.Equal(1, result[0].number);
        }

        [Fact]
        public void Searches_By_Name_Or_Address()
        {
            var bikes = new List<BikeModel>
            {
                new BikeModel { number = 1, name = "River Park", address = "Nice Street", status = "OPEN", bike_stands = 10, available_bikes = 5 },
                new BikeModel { number = 2, name = "Hill", address = "River Road", status = "OPEN", bike_stands = 10, available_bikes = 5 }
            };

            var service = BuildService(bikes);
            var result = service.GetStations(null, null, "river", "name", "asc", 1, 10);

            Assert.Equal(2, result.Count);
        }
    }
}
