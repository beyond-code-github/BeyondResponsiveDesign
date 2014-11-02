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
    }

    public class Session
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Abstract { get; set; }

        public string Level { get; set; }

        public string TimeSlotId { get; set; }

        public string Speaker { get; set; }

        public string SpeakerId { get; set; }

        public string Room { get; set; }
    }

    public class Speaker
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Bio { get; set; }

        public IEnumerable<Link> Links { get; set; }

        public IEnumerable<Session> Sessions { get; set; }

        public string Company { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var imageClient = new HttpClient { BaseAddress = new Uri("http://oredev.org/") };
            var htmlClient = new HttpClient { BaseAddress = new Uri("http://oredev.org/") };
            
            // ScrapeSpeakerInfo(htmlClient, imageClient);
            ScrapeAgenda(htmlClient);

            Console.WriteLine("Process completed, press any key to exit");
            Console.ReadKey();
        }

        private static void ScrapeAgenda(HttpClient htmlClient)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlClient.GetStringAsync("2014/schedule/").Result);

            var dayElements =
                htmlDoc.DocumentNode.Descendants("table")
                    .Where(o => o.HasClass("session-day"));
            
            var days = new Dictionary<string, Dictionary<string, Session>>();
            foreach (var dayElement in dayElements.Skip(1))
            {
                var dayTitle = dayElement.Attributes["id"].Value;

                var sessions = new Dictionary<string, Session>();
                var sessionElements = dayElement.Descendants("tr").Where(o => o.HasClass("session-slot"));
                foreach (var container in sessionElements)
                {
                    var titleElement = container.Descendants("div").First(o => o.HasClass("title"));
                    var roomElement = container.Descendants("div").First(o => o.HasClass("room"));
                    var levelElement = container.Descendants("div").First(o => o.HasClass("session-level"));

                    var title = titleElement.Element("#text").InnerHtml;
                    var room = roomElement.Element("#text").InnerHtml.Replace("Room: ", string.Empty);
                    var level = levelElement.Element("#text").InnerHtml.Replace("Level: ", string.Empty);

                    sessions.Add(title, new Session { Title = title, Room = room, Level = level });
                }

                days.Add(dayTitle, sessions);
            }
            
            htmlDoc.LoadHtml(htmlClient.GetStringAsync("2014/sessions/").Result);
            dayElements = htmlDoc.DocumentNode.Descendants("div").Where(o => o.HasClass("session-day"));

            foreach (var dayElement in dayElements.Skip(1))
            {
                var dayTitle = dayElement.Attributes["id"].Value;
                var sessions = days[dayTitle];

                var sessionElements = dayElement.Descendants("div").Where(o => o.HasClass("session-item"));
                foreach (var container in sessionElements)
                {
                    var timeSlotElement = container.Descendants("h2").First(o => o.HasClass("time-slot"));
                    var timeslot = timeSlotElement.Element("#text").InnerHtml.Split('-').First().Trim();

                    var nameElement = container.Descendants("h3").First(o => o.HasClass("session-name"));
                    var id = nameElement.FirstChild.Attributes["href"].Value.Split('/').Last();
                    var title = nameElement.FirstChild.InnerText;

                    var paragraphElements = container.Descendants("p");
                    var @abstract = string.Join(string.Empty, paragraphElements.Select(o => o.InnerHtml));

                    var speakerElement = container.Descendants("div").First(o => o.HasClass("session-speaker"));
                    var speakerLink = speakerElement.Descendants("a").First();

                    var speakerId = speakerLink.Attributes["href"].Value.Split('/').Last();
                    var speakerName = speakerLink.InnerText;

                    var session = sessions[title];
                    session.Id = id;
                    session.TimeSlotId = timeslot;
                    session.Abstract = @abstract;
                    session.Speaker = speakerName;
                    session.SpeakerId = speakerId;
                }
            }


            const string OutputDir = "D:\\Code\\BeyondResponsiveDesign\\BeyondResponsiveDesign.Menus\\content\\";
            foreach (var day in days)
            {
                var streamWriter = new StreamWriter(string.Format("{0}\\agenda-{1}.json", OutputDir, day.Key));
                streamWriter.Write(JsonConvert.SerializeObject(day.Value.Values));
                streamWriter.Close();
            }
        }

        private static void ScrapeSpeakerInfo(HttpClient htmlClient, HttpClient imageClient)
        {
            const string SpeakersUrl = "2014/speakers/";
            const string OutputDir = "D:\\Code\\BeyondResponsiveDesign\\BeyondResponsiveDesign.Menus\\images\\speakers";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlClient.GetStringAsync(SpeakersUrl).Result);

            var speakerLinks =
                htmlDoc.DocumentNode.Descendants("div")
                    .Where(o => o.Attributes.Contains("class") && o.Attributes["class"].Value == "speaker-info")
                    .Select(o => o.Descendants("a").First());

            var speakers = new ConcurrentQueue<Speaker>();
            Task.WaitAll(
                speakerLinks.Select(
                    link => Task.Run(
                        async () =>
                            {
                                var href = link.Attributes["href"].Value;
                                var speakerPage = new HtmlDocument();
                                speakerPage.LoadHtml(await htmlClient.GetStringAsync(href));

                                var container =
                                    speakerPage.DocumentNode.Descendants("div").First(o => o.HasClass("speaker-info"));

                                var titleElement = container.Descendants("h2").First();
                                var companyElement = container.Descendants("div").First(o => o.HasClass("company"));
                                var tagsElement = container.Descendants("div").First(o => o.HasClass("inline-tags"));
                                var linkElements =
                                    container.Descendants("div")
                                        .Where(o => !o.HasClass("other-sessions"))
                                        .SelectMany(o => o.Descendants("a"));

                                var paragraphElements = container.Elements("p");
                                var sessionLinkElements =
                                    container.Descendants("div")
                                        .Where(o => o.HasClass("other-sessions"))
                                        .SelectMany(o => o.Descendants("a").Where(a => !a.HasClass("btn add-to-schedule")));

                                var id = href.Split('/').Last();

                                var name = titleElement.Element("#text").InnerHtml;
                                var companyContent = companyElement.Element("#text");
                                var company = companyContent == null ? string.Empty : companyContent.InnerHtml;
                                var tagsContent = tagsElement.Element("#text");

                                IEnumerable<string> tags = null;
                                if (tagsContent != null)
                                {
                                    tags = tagsContent.InnerHtml.Split(',').Select(o => o.Trim());
                                }

                                var bio = string.Join(string.Empty, paragraphElements.Select(o => o.InnerHtml));
                                var links = (from linkElement in linkElements
                                             let icon = linkElement.ParentNode.Attributes["class"].Value
                                             let url = linkElement.Attributes["href"].Value
                                             select new Link { Icon = icon, Url = url }).ToList();

                                var sessions = new List<Session>();
                                foreach (var sessionLinkElement in sessionLinkElements)
                                {
                                    var sessionHref = sessionLinkElement.Attributes["href"].Value;
                                    var title = sessionLinkElement.InnerText;

                                    var sessionId = sessionHref.Split('/').Last();

                                    var sessionPage = new HtmlDocument();
                                    sessionPage.LoadHtml(await htmlClient.GetStringAsync(sessionHref));

                                    var sessionContainer =
                                        sessionPage.DocumentNode.Descendants("div").First(o => o.HasClass("session-info"));

                                    var sessionLevelElement =
                                        sessionContainer.Descendants("div").First(o => o.HasClass("session-level"));
                                    var abstractElements = sessionContainer.Descendants("p");

                                    var sessionLevel = sessionLevelElement.Element("#text").InnerHtml;
                                    var @abstract = string.Join(string.Empty, abstractElements.Select(o => o.InnerHtml));
                                    sessions.Add(
                                        new Session
                                            {
                                                Id = sessionId,
                                                Title = title,
                                                Level = sessionLevel,
                                                Abstract = @abstract
                                            });
                                }

                                speakers.Enqueue(
                                    new Speaker
                                        {
                                            Id = id,
                                            Name = name,
                                            Company = company,
                                            Tags = tags,
                                            Links = links,
                                            Bio = bio,
                                            Sessions = sessions
                                        });

                                var imageTag = container.PreviousSibling;
                                var imageUrl = imageTag.Attributes["src"].Value;

                                var stream = await imageClient.GetStreamAsync(imageUrl);
                                var file = File.OpenWrite(string.Format("{0}\\{1}.jpg", OutputDir, id));
                                await stream.CopyToAsync(file);

                                file.Close();
                            }).ContinueWith(
                                r =>
                                    {
                                        // Mute any exceptions
                                    })).ToArray());

            var streamWriter = new StreamWriter(string.Format("{0}\\speakers.json", OutputDir));
            streamWriter.Write(JsonConvert.SerializeObject(speakers.OrderBy(o => o.Name)));
            streamWriter.Close();
        }
    }

    public static class Extensions
    {
        public static bool HasClass(this HtmlNode o, string className)
        {
            return o.Attributes.Contains("class") && o.Attributes["class"].Value.Contains(className);
        }
    }
}
