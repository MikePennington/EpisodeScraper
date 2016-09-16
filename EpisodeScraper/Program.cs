using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace EpisodeScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            CommandLine.Parser.Default.ParseArguments(args, options);

            XmlDocument doc = new XmlDocument();
            doc.Load(options.InputFilePath);

            var programNodes = doc.SelectNodes("/tv/programme");
            if (programNodes == null || programNodes.Count == 0)
            {
                Console.WriteLine("No programs found!");
                return;
            }

            // Read XML
            Console.WriteLine("{0} programs found.", programNodes.Count);
            Console.WriteLine("Grabbing episode information.");
            var programs = new List<Programme>();
            foreach (XmlNode node in programNodes)
            {
                var p = BuildProgramFromXmlNode(node);
                programs.Add(p);

                var hoursFromNow = (p.StartDate - DateTime.Now).TotalHours;
                if (p.ProgramId.StartsWith("EP") && hoursFromNow >= 0 && hoursFromNow < options.HoursAheadToGetDetail)
                {
                    var detailUrl = "http://tvschedule.zap2it.com/tvlistings/gridDetailService?pgmId=" + p.ProgramId.Replace(".", "");
                    var response = GetUrl(detailUrl);
                    ParseResponse(response, p);
                    //Console.WriteLine(p);
                    if (p.Episode > 0 && p.Season > 0)
                    {
                        var episodeNode = node.SelectSingleNode("episode-num");
                        episodeNode.InnerText = p.EpisodeInfoInXmltvnsFormat;
                        var systemAttr = episodeNode.Attributes["system"];
                        systemAttr.Value = "xmltv_ns";
                    }
                }
            }

            doc.Save(options.OutputFilePath);

            Console.WriteLine("Done");
        }

        private static Programme BuildProgramFromXmlNode(XmlNode node)
        {
            var p = new Programme();

            if (node.Attributes != null)
            {
                var startAttr = node.Attributes["start"];
                if (startAttr != null)
                {
                    var value = startAttr.Value;
                    if (value.EndsWith("00"))
                    {
                        value = value.Remove(value.Length - 2);
                    }
                    DateTime startTime;
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss zz", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                    {
                        p.StartDate = startTime;
                    }
                }

            }

            foreach (XmlNode c in node.ChildNodes)
            {
                if (c.Name == "title")
                {
                    p.Title = c.InnerText;
                }
                if (c.Name == "episode-num")
                {
                    if (node.Attributes != null)
                    {
                        var system = c.Attributes["system"];
                        if (system != null && system.Value == "dd_progid")
                        {
                            p.ProgramId = c.InnerText;
                        }
                    }
                }
            }

            return p;
        }

        private static void ParseResponse(string response, Programme p)
        {
            response = response.Substring(response.IndexOf('{'));
            JObject detailJson = JObject.Parse(response);

            var seasonNumber = detailJson["program"]["seasonNumber"];
            var episodeNumber = detailJson["program"]["episodeNumber"];
            if (seasonNumber != null && episodeNumber != null)
            {
                int seasonInt;
                int episodeInt;
                if(int.TryParse(seasonNumber.ToString().Replace("S", ""), out seasonInt)
                    && int.TryParse(episodeNumber.ToString().Replace("E", ""), out episodeInt))
                {
                    p.Season = seasonInt;
                    p.Episode = episodeInt;
                }
            }

            var description = detailJson["program"]["description"];
            if (description != null)
            {
                p.Description = description.ToString();
            }
        }

        private static string GetUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    Console.WriteLine(errorText);
                }
                throw;
            }
        }
    }
}
