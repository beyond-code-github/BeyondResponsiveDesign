namespace DataScraper
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using HtmlAgilityPack;

    using Newtonsoft.Json;

    public class Link
    {
        public string Icon { get; set; }

        public string Url { get; set; }

        public string Text { get; set; }
    }

    public class Session
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string TimeSlotId { get; set; }

        public string TrackId { get; set; }

        public string Abstract { get; set; }

        public string Speaker { get; set; }

        public string SpeakerId { get; set; }

        public string Day { get; set; }
    }

    public class Speaker
    {
        public Speaker()
        {
            this.Sessions = new List<Session>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public string Bio { get; set; }

        public IEnumerable<Link> Links { get; set; }

        public List<Session> Sessions { get; set; }
    }

    public class SessionInfo
    {
        public string Day { get; set; }

        public string SessionId { get; set; }

        public Speaker Speaker { get; set; }
    }

    public class Program
    {
        private const string OutputDir = "D:\\Code\\BeyondResponsiveDesign\\BeyondResponsiveDesign\\images\\speakers";

        const int Innovation = 1;
        const int Devops = 2;
        const int Mobile = 3;
        const int IoTMakerWearable = 4;
        const int SecurityHack = 5;
        const int Methods = 6;
        const int BigDataCloud = 7;
        const int Languages = 8;
        const int Codelab1 = 9;
        const int Codelab2 = 10;

        public static void Main(string[] args)
        {
            var imageClient = new HttpClient();
            imageClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.91 Safari/537.36");

            var htmlClient = new HttpClient { BaseAddress = new Uri("http://rome2015.codemotionworld.com") };
            htmlClient.DefaultRequestHeaders.Add("Accept", "text/html");
            htmlClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.91 Safari/537.36");

            var speakers = new ConcurrentQueue<Speaker>();
            var sessions = new ConcurrentQueue<Session>();
            var sessionInfos = new ConcurrentQueue<SessionInfo>();

            ScrapeSpeakers(htmlClient, imageClient, speakers, sessionInfos);
            ScrapeSessions(htmlClient, sessionInfos, sessions);

            var sessionDictionary = sessions.ToDictionary(k => k.Id, v => v);
            ScrapeTimeSlots(htmlClient, sessionDictionary);

            var streamWriter = new StreamWriter(string.Format("{0}\\speakers.json", OutputDir));
            streamWriter.Write(JsonConvert.SerializeObject(speakers.OrderBy(o => o.Name)));
            streamWriter.Close();

            var agendaFridayWriter = new StreamWriter(string.Format("{0}\\agenda-friday.json", OutputDir));
            agendaFridayWriter.Write(JsonConvert.SerializeObject(sessions.Where(o => o.Day == "Friday").OrderBy(o => o.TrackId).ThenBy(o => o.TimeSlotId)));
            agendaFridayWriter.Close();

            var agendaSaturdayWriter = new StreamWriter(string.Format("{0}\\agenda-saturday.json", OutputDir));
            agendaSaturdayWriter.Write(JsonConvert.SerializeObject(sessions.Where(o => o.Day == "Saturday").OrderBy(o => o.TrackId).ThenBy(o => o.TimeSlotId)));
            agendaSaturdayWriter.Close();


            Console.WriteLine("Process completed, press any key to exit");
            Console.ReadKey();
        }

        private static void ScrapeSpeakers(
            HttpClient htmlClient,
            HttpClient imageClient,
            ConcurrentQueue<Speaker> speakers,
            ConcurrentQueue<SessionInfo> sessionInfo)
        {
            const string SpeakersUrl = "/speakers";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlClient.GetStringAsync(SpeakersUrl).Result);

            var speakerContainers =
                htmlDoc.DocumentNode.Descendants("div").First(o => o.Id == "speaker").Descendants("article");

            Task.WaitAll(
                speakerContainers.Select(
                    container => Task.Run(
                        async () =>
                        {
                            var link = container.Descendants("a").First();
                            var href = link.Attributes["href"].Value;
                            var speakerPage = new HtmlDocument();
                            speakerPage.LoadHtml(await htmlClient.GetStringAsync(href));

                            var speakerContainer =
                                speakerPage.DocumentNode.Descendants("article").First(o => o.Id == "speaker");

                            var header = speakerContainer.Descendants("header").First();
                            var titleElement = header.Elements("h1").First();
                            var roleElement = header.Elements("h2").First();

                            var linkContainer = speakerContainer.Descendants("ul").First(o => o.HasClass("links"));

                            IEnumerable<HtmlNode> linkElements = new List<HtmlNode>();
                            if (linkContainer != null)
                            {
                                linkElements = linkContainer.Descendants("li").Select(o => o.Element("a"));
                            }

                            var bioContainer = speakerContainer.Descendants("p").First();

                            var id = href.Split('/').Last();
                            var name = titleElement.Element("#text").InnerHtml.Trim();
                            var role = roleElement.Element("#text").InnerHtml.Trim();
                            var links =
                                linkElements.Select(
                                    o =>
                                    new Link
                                        {
                                            Text = o.Element("#text") != null ? o.Element("#text").InnerHtml.Trim() : string.Empty,
                                            Url = o.Attributes.Contains("href") ? o.Attributes["href"].Value : string.Empty,
                                            Icon = o.Attributes.Contains("class") ? o.Attributes["class"].Value : string.Empty
                                        }).ToList();

                            var bio = bioContainer == null ? string.Empty : bioContainer.InnerHtml.Trim();

                            var speaker = new Speaker
                                              {
                                                  Id = id,
                                                  Name = name,
                                                  Role = role,
                                                  Links = links,
                                                  Bio = bio
                                              };
                            speakers.Enqueue(speaker);

                            var linksContainer = speakerContainer.Descendants("p").Skip(1).FirstOrDefault();
                            if (linksContainer != null)
                            {

                                var sessionLinks = linksContainer.Elements("a");

                                foreach (var sessionLink in sessionLinks)
                                {
                                    var rawHref = sessionLink.Attributes["href"].Value;

                                    var day = rawHref.Contains("/22/") ? "Friday" : "Saturday";
                                    var sessionId =
                                        rawHref.Replace(
                                            "/conference/22/?dlink=dtalk",
                                            string.Empty)
                                            .Replace(
                                                "/conference/23/?dlink=dtalk",
                                                string.Empty);
                                    
                                    sessionInfo.Enqueue(
                                        new SessionInfo { Day = day, SessionId = sessionId, Speaker = speaker });
                                }

                                var imageTag = speakerContainer.Descendants("img").FirstOrDefault();
                                if (imageTag != null)
                                {
                                    var rawUrl = imageTag.Attributes["src"].Value;
                                    var imageUrl = rawUrl.StartsWith("//") ? rawUrl.Replace("//", "http://") : rawUrl;

                                    try
                                    {
                                        var stream = await imageClient.GetStreamAsync(imageUrl);
                                        var file = File.OpenWrite(string.Format("{0}\\{1}.jpg", OutputDir, id));

                                        await stream.CopyToAsync(file);
                                        file.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }
                        })).ToArray());
        }

        private static void ScrapeSessions(
            HttpClient htmlClient,
            IEnumerable<SessionInfo> sessionInfos,
            ConcurrentQueue<Session> sessions)
        {
            Task.WaitAll(
                sessionInfos.Select(
                    sessionInfo => Task.Run(
                        async () =>
                        {
                            var day = sessionInfo.Day;
                            var sessionId = sessionInfo.SessionId;
                            var speaker = sessionInfo.Speaker;

                            var sessionHref =
                                "http://rome2015.codemotionworld.com/wp-content/themes/event/detail-talk.php?detail="
                                + sessionId;

                            var sessionPage = new HtmlDocument();
                            sessionPage.LoadHtml(await htmlClient.GetStringAsync(sessionHref));

                            var titleElement = sessionPage.DocumentNode.Descendants("h1").First();
                            var slotElement = sessionPage.DocumentNode.Descendants("h3").First();
                            var abstractElement = sessionPage.DocumentNode.Descendants("p").First();

                            var title = titleElement.Element("#text").InnerHtml.Trim();
                            var @abstract = abstractElement.Element("#text").InnerHtml.Trim();
                            var slot = slotElement.Element("#text").InnerHtml.Trim();

                            var session = new Session
                                              {
                                                  Id = sessionId,
                                                  Day = day,
                                                  Title = title,
                                                  TimeSlotId = slot,
                                                  Abstract = @abstract,
                                                  Speaker = speaker.Name,
                                                  SpeakerId = speaker.Id
                                              };

                            speaker.Sessions.Add(session);
                            sessions.Enqueue(session);
                        })).ToArray());
        }

        private static void ScrapeTimeSlots(HttpClient htmlClient, IReadOnlyDictionary<string, Session> sessionDictionary)
        {
            const string FridayUrl = "/conference/22";
            const string SaturdayUrl = "/conference/23";

            ScrapeDay(htmlClient, sessionDictionary, FridayUrl);
            ScrapeDay(htmlClient, sessionDictionary, SaturdayUrl);
        }

        private static void ScrapeDay(HttpClient htmlClient, IReadOnlyDictionary<string, Session> sessionDictionary, string url)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlClient.GetStringAsync(url).Result);

            var table = htmlDoc.DocumentNode.Descendants("table").FirstOrDefault(o => o.Id == "table_talk");
            if (table != null)
            {
                // skip first row as this is just headers
                var rows = table.Descendants("tr").Skip(1);
                foreach (var row in rows)
                {
                    var columns = row.Descendants("td").ToList();
                    var timeslotSpan = columns.First().Descendants("span").First();
                    var timeslotId = timeslotSpan.Element("#text").InnerHtml.Trim().Replace(" ", string.Empty);

                    foreach (var column in columns.Skip(1))
                    {
                        var link = column.Descendants("a").FirstOrDefault();
                        if (link != null)
                        {
                            Session session;
                            var sessionId = link.Id.Replace("dtalk", string.Empty);
                            if (sessionDictionary.TryGetValue(sessionId, out session))
                            {
                                session.TimeSlotId = timeslotId;
                                session.TrackId = columns.IndexOf(column).ToString();
                            }
                        }
                    }
                }
            }
        }
    }

    public static class Extensions
    {
        public static bool HasClass(this HtmlNode o, string className)
        {
            return o.Attributes.Contains("class") && o.Attributes["class"].Value == className;
        }
    }
}
