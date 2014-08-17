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
        public int Id { get; set; }

        public string Title { get; set; }
    }

    public class Speaker
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Bio { get; set; }

        public IEnumerable<Link> Links { get; set; }

        public IEnumerable<Session> Sessions { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            const string AgendaUrl = "/Agenda";
            const string OutputDir =
                "D:\\Code\\BeyondResponsiveDesign\\BeyondResponsiveDesign.Menus\\images\\speakers";

            var imageClient = new HttpClient();
            var htmlClient = new HttpClient { BaseAddress = new Uri("http://www.dddeastanglia.com") };

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlClient.GetStringAsync(AgendaUrl).Result);

            var speakerLinks =
                htmlDoc.DocumentNode.Descendants("a")
                    .Where(o => o.Attributes.Contains("class") && o.Attributes["class"].Value == "speakerName");

            var speakers = new ConcurrentQueue<Speaker>();
            Task.WaitAll(speakerLinks.Select(link => Task.Run(async () =>
                {
                    var href = link.Attributes["href"].Value;
                    var speakerPage = new HtmlDocument();
                    speakerPage.LoadHtml(await htmlClient.GetStringAsync(href));

                    var container =
                        speakerPage.DocumentNode.Descendants("div")
                            .First(
                                o => o.Attributes.Contains("class") && o.Attributes["class"].Value.Contains("speaker"));

                    var titleElement = container.Descendants("h3").First();
                    var linkElements = container.Descendants("section").First().Descendants("p");
                    var paragraphElements = container.Elements("p");
                    var sessionLinkElements = container.Element("ul").Descendants("a");

                    var id = int.Parse(href.Split('/').Last());
                    var name = titleElement.Element("#text").InnerHtml;
                    var bio = string.Join(string.Empty, paragraphElements.Select(o => o.InnerHtml));
                    var links = (from linkElement in linkElements
                                 let icon = linkElement.Descendants("i").First().Attributes["class"].Value
                                 let url = linkElement.Descendants("a").First().Attributes["href"].Value
                                 select new Link { Icon = icon, Url = url }).ToList();
                    var sessions = (from sessionLinkElement in sessionLinkElements
                                    let sessionId =
                                        int.Parse(sessionLinkElement.Attributes["href"].Value.Split('/').Last())
                                    let title = sessionLinkElement.InnerText
                                    select new Session { Id = sessionId, Title = title }).ToList();

                    speakers.Enqueue(
                        new Speaker { Id = id, Name = name, Links = links, Bio = bio, Sessions = sessions });

                    var imageTag = titleElement.Descendants("img").First();
                    var imageUrl = imageTag.Attributes["src"].Value.Replace("s=50", "s=300");

                    var stream = await imageClient.GetStreamAsync(imageUrl);
                    var file = File.OpenWrite(string.Format("{0}\\{1}.jpg", OutputDir, id));
                    await stream.CopyToAsync(file);

                    file.Close();
                })).ToArray());

            var streamWriter = new StreamWriter(string.Format("{0}\\speakers.json", OutputDir));
            streamWriter.Write(JsonConvert.SerializeObject(speakers.OrderBy(o => o.Name)));
            streamWriter.Close();

            Console.WriteLine("Process completed, press any key to exit");
            Console.ReadKey();
        }
    }
}
