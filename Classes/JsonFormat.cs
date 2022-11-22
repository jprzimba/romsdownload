using Newtonsoft.Json;
using romsdownload.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace romsdownload.Classes
{
    internal class JsonFormat
    {
        public List<GameList> GameList { get; set; }

        public JsonFormat(List<GameList> gameList)
        {
            this.GameList = gameList;
        }

        public static JsonFormat Import(string file)
        {
            using (var sr = new StreamReader(file))
            {
                var jso = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<JsonFormat>(jso);
            }
        }

        public static void Export(string file, List<GameList> gameList)
        {
            using (var sw = new StreamWriter(file))
            {
                var jso = JsonConvert.SerializeObject(new JsonFormat(gameList), Formatting.Indented);
                sw.WriteLine(jso);
            }
        }
    }
}
