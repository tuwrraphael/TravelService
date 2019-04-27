using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tile38.WienerLinienOpenData
{
    class Program
    {
        static void Main(string[] args)
        {
            Insert().Wait();
        }

        private class Haltestelle
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public string DIVA { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public static async Task Insert()
        {
            var haltestellenUri = new Uri("https://data.wien.gv.at/csv/wienerlinien-ogd-haltestellen.csv");
            var steigeUri = new Uri("https://data.wien.gv.at/csv/wienerlinien-ogd-steige.csv");
            var linienUri = new Uri("https://data.wien.gv.at/csv/wienerlinien-ogd-linien.csv");

            var client = new HttpClient();
            List<Haltestelle> haltestellen = new List<Haltestelle>();
            using (var stream = await (await client.GetAsync(haltestellenUri)).Content.ReadAsStreamAsync())
            {
                using (var reader = new StreamReader(stream))
                {
                    string s;
                    bool skipFirst = true;
                    while (null != (s = await reader.ReadLineAsync()))
                    {
                        if (skipFirst)
                        {
                            skipFirst = false;
                            continue;
                        }
                        var values = s.Split(';');
                        try
                        {
                            var parse = new Haltestelle()
                            {
                                ID = values[0],
                                DIVA = values[2],
                                Name = values[3].Replace("\"", ""),
                                Lat = double.Parse(values[6], CultureInfo.InvariantCulture),
                                Lng = double.Parse(values[7], CultureInfo.InvariantCulture)
                            };
                            haltestellen.Add(parse);
                        }
                        catch
                        {
                            Console.WriteLine($"Could not parse {s}");
                        }
                    }
                }
            }
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:9852"))
            {
                IDatabase db = redis.GetDatabase();
                foreach (var haltestelle in haltestellen)
                {
                    db.Execute("SET",
                        "haltestelle", haltestelle.ID,
                        "field", "diva", haltestelle.DIVA,
                        "point", haltestelle.Lat.ToString(CultureInfo.InvariantCulture), haltestelle.Lng.ToString(CultureInfo.InvariantCulture));
                    db.Execute("SET",
                        "haltestelle", $"{haltestelle.ID}:name",
                        "string", haltestelle.Name);
                }
            }
        }
    }
}
