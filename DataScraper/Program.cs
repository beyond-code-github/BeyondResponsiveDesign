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

        public string TimeSlotId { get; set; }

        public string TrackId { get; set; }

        public string Abstract { get; set; }

        public string Speaker { get; set; }

        public string SpeakerId { get; set; }
    }

    public class Speaker
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Bio { get; set; }

        public IEnumerable<Link> Links { get; set; }

        public IEnumerable<Session> Sessions { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var imageClient = new HttpClient();
            imageClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.91 Safari/537.36");

            var htmlClient = new HttpClient { BaseAddress = new Uri("http://voxxeddaysvienna2015.sched.org/") };
            htmlClient.DefaultRequestHeaders.Add("Accept", "text/html");
            htmlClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/40.0.2214.91 Safari/537.36");

            ScrapeSpeakers(htmlClient, imageClient);

            Console.WriteLine("Process completed, press any key to exit");
            Console.ReadKey();
        }

        private static void ScrapeSpeakers(HttpClient htmlClient, HttpClient imageClient)
        {
            const string SpeakersUrl = "directory/speakers";
            const string OutputDir = "D:\\Code\\BeyondResponsiveDesign\\BeyondResponsiveDesign.Menus\\images\\speakers";

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlClient.GetStringAsync(SpeakersUrl).Result);

            var speakerContainers = htmlDoc.DocumentNode.Descendants("div").Where(o => o.HasClass("sched-person"));

            var speakers = new ConcurrentQueue<Speaker>();
            var allSessions = new ConcurrentQueue<Session>();

            //foreach (var container in speakerContainers)
            //{

            Task.WaitAll(
               speakerContainers.Skip(4).Select(
                   container => Task.Run(
                       async () =>
                       {
                           var link = container.Descendants("h2").First().Descendants("a").First();
                           var href = link.Attributes["href"].Value;
                           var speakerPage = new HtmlDocument();
                           speakerPage.LoadHtml(await htmlClient.GetStringAsync(href));

                           var speakerContainer =
                               speakerPage.DocumentNode.Descendants("div").First(o => o.Id == "sched-page-me-profile");

                           var titleElement = speakerContainer.Descendants("h1").First(o => o.Id == "sched-page-me-name");
                           var linkContainer =
                               speakerContainer.Descendants("div").FirstOrDefault(o => o.Id == "sched-page-me-networks");

                           IEnumerable<HtmlNode> linkElements = new List<HtmlNode>();
                           if (linkContainer != null)
                           {
                               linkElements =
                                   linkContainer.Descendants("div")
                                       .Where(o => o.HasClass("sched-network-link"))
                                       .Select(o => o.Element("a"));
                           }

                           var roleCountryContainer = speakerContainer.Descendants("div")
                                   .FirstOrDefault(o => o.Id == "sched-page-me-profile-data");

                           HtmlNode roleCountryElement;
                           if (roleCountryContainer != null)
                           {
                               roleCountryElement = roleCountryContainer.Element("strong");
                           }

                           var bioContainer = speakerContainer.Elements("div").FirstOrDefault(o => o.Id == "sched-page-me-profile-about");
                           var sessionContainer =
                               speakerPage.DocumentNode.Descendants("div").FirstOrDefault(o => o.Id == "sched-page-me-schedule");

                           var id = href.Split('/').Last();
                           var name = titleElement.Element("#text").InnerHtml.Trim();
                           var links = linkElements.Select(o => new Link { Url = o.Attributes["href"].Value, Icon = o.Element("img").Attributes["alt"].Value });
                           var bio = bioContainer == null ? string.Empty : bioContainer.InnerHtml.Trim();

                           var sessions = new List<Session>();
                           foreach (
                               var sessionLinkContainer in
                                   sessionContainer.Descendants("div").Where(o => o.HasClass("sched-container-inner")))
                           {
                               var sessionLinkElement = sessionLinkContainer.Descendants("a").First();
                               var sessionHref = sessionLinkElement.Attributes["href"].Value;
                               var sessionId = sessionHref.Split('/').Last();
                               var title = sessionLinkElement.Element("#text").InnerText.Trim();

                               var sessionPage = new HtmlDocument();
                               sessionPage.LoadHtml(await htmlClient.GetStringAsync(sessionHref));

                               var abstractElement =
                                   sessionPage.DocumentNode.Descendants("div").FirstOrDefault(o => o.HasClass("tip-description"));

                               var @abstract = string.Empty;
                               if (abstractElement != null)
                               {
                                   @abstract = abstractElement.InnerText.Trim();
                               }

                               var slotElement =
                                   sessionPage.DocumentNode.Descendants("div")
                                       .First(o => o.HasClass("sched-container-dates"));

                               var roomElement =
                                   sessionPage.DocumentNode.Descendants("div")
                                       .First(o => o.HasClass("sched-event-details-timeandplace")).Element("a");

                               var slot =
                                   slotElement.Elements("#text")
                                       .Last()
                                       .InnerText.Split("&bull;".ToCharArray())
                                       .Last()
                                       .Trim();

                               var room = roomElement.InnerText.Trim();

                               var session = new Session
                                                 {
                                                     Id = sessionId,
                                                     Title = title,
                                                     TimeSlotId = slot,
                                                     TrackId = room,
                                                     Abstract = @abstract,
                                                     Speaker = name,
                                                     SpeakerId = id
                                                 };

                               allSessions.Enqueue(session);
                               sessions.Add(session);
                           }

                           speakers.Enqueue(new Speaker { Id = id, Name = name, Links = links, Bio = bio, Sessions = sessions });

                           var avatarContainer =
                               speakerContainer.Descendants("span").FirstOrDefault(o => o.Id == "sched-page-me-profile-avatar");

                           if (avatarContainer != null)
                           {
                               var imageTag = avatarContainer.Descendants("img").FirstOrDefault();
                               if (imageTag != null)
                               {
                                   var rawUrl = imageTag.Attributes["src"].Value;
                                   var imageUrl = rawUrl.StartsWith("//") ? rawUrl.Replace("//", "http://") : rawUrl;

                                   var stream = await imageClient.GetStreamAsync(imageUrl);
                                   var file = File.OpenWrite(string.Format("{0}\\{1}.jpg", OutputDir, id));

                                   await stream.CopyToAsync(file);
                                   file.Close();
                               }
                           }
                       })).ToArray());

            var streamWriter = new StreamWriter(string.Format("{0}\\speakers.json", OutputDir));
            streamWriter.Write(JsonConvert.SerializeObject(speakers.OrderBy(o => o.Name)));
            streamWriter.Close();

            var agendaWriter = new StreamWriter(string.Format("{0}\\agenda.json", OutputDir));
            agendaWriter.Write(JsonConvert.SerializeObject(allSessions.OrderBy(o => o.TrackId).ThenBy(o => o.TimeSlotId)));
            agendaWriter.Close();
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
