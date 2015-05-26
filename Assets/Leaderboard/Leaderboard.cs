using System.Collections.Generic;
using System.IO;
using System.Net;

public class Leaderboard
{
    public string gameID = "gamelab2015_prismus";

	private string uri = "http://drz-games.drz.ethz.ch:50100/lb/";

    public string PostScore(Score score)
    {
        var reqDict = new Dictionary<string, object>();
        reqDict.Add("scoreEntries",
            new List<object>() {
                new Dictionary<string, object>() 
                { 
                    {"name", "highscore" },
                    { "value", score.Value.ToString()},                
                }  
 		});
        string url = uri + gameID + @"/" + score.Name + @"/score";

        return PostJsonRequest(
           MiniJSON.Json.Serialize(reqDict),
           url);
    }

    public Scores GetScores()
    {
        /*
         * [
         *  {"playerID":"mickey","highscore":"48"},
         *  {"playerID":"12","highscore":"32"},
         *  {"playerID":"sdfsdf","highscore":"2"}
         *  ]
         */
        var scores = new Scores();

        string url = uri + gameID + @"/rankedlist";

        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Accept = "application/json";

        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        {
            if (response.StatusCode != HttpStatusCode.OK)
                throw new System.Exception(string.Format(
                "Server error (HTTP {0}: {1}).",
                response.StatusCode,
                response.StatusDescription));
            else
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var text = sr.ReadToEnd();

                    var jsonDic = MiniJSON.Json.Deserialize(text) as List<object>;

                    if (jsonDic != null)
                    {
                        int c = 1;
                        foreach (var k in jsonDic)
                        {
                            var dict = k as Dictionary<string, object>;

                            string name = (string)dict["playerID"];
                            int score = int.Parse((string)dict["highscore"]);
                                                        
                            scores.Add(new Score(name, score, c++));
                        }

                        return scores;
                    }
                }
            }
        }

        return null;
    }

    string PostJsonRequest(string jsonRequest, string url)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Accept = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(jsonRequest);
            streamWriter.Flush();
            streamWriter.Close();

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                return result;
            }
        }
    }
}
