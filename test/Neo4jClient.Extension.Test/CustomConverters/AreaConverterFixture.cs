using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UnitsNet;

namespace Neo4jClient.Extension.Test.CustomConverters
{
    [TestFixture]
    public class AreaConverterFixture
    {
        [Test]
        public void Converts()
        {
            Area? area1 = Area.FromSquareMeters(20);
            Area? area2 = null;
            TestConversion(area1, Area.FromSquareMeters(20));
            TestConversion(area2, null);
        }

        private void TestConversion(Area? input, Area? expected)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new AreaJsonConverter());
            settings.Formatting = Formatting.Indented;
            
            var json = JsonConvert.SerializeObject(input, settings);

            Console.WriteLine(json);

            var areaReloaded = JsonConvert.DeserializeObject<Area?>(json, settings);

            Assert.AreEqual(expected, areaReloaded);
        }
    }
}
