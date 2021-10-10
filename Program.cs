using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace See_Tickets_Site_Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string mainurl = @"https://www.seetickets.com/search?BrowseOrder=Relevance&q=&s=&se=false&c=3&dst=&dend=&l";
            string outputdir = @"C:\Users\Tommy\Desktop\";

            //First get the list of all events on the page
            //This is just the first page, but could be extended to grab from all other pages if needed
            Console.WriteLine($"Beginning scraping of {mainurl}");

            try
            {
                List<Event> events = GetEvents(mainurl);
                try
                {
                    WriteToFile(events, outputdir);
                    Console.WriteLine($"Scraping complete.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in writing to file: {ex.ToString()}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in scraping: {e.ToString()}");
            }



            Console.ReadKey();
        }

        public static void WriteToFile(List<Event> events, string file)
        {
            //This method writes to txt file in JSON format, but this could be changed to write to CSV/XML etc using the same input List<Event>

            file += "output.txt";
            var jsonSerializerSettings = new JsonSerializerSettings();
            //Serialise to json string, using text for enum value
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            string json = JsonConvert.SerializeObject(events,jsonSerializerSettings);
            using (StreamWriter fw = new StreamWriter(file))
            {
                fw.Write(json);
            }
        }

        public static T GetValueFromDescription<T>(string description)
        {
            //Used to check 'Description' attribute when parsing enum
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }


        static List<Event> GetEvents(string url)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            List<Event> returnList = new List<Event>();
            //Load URL into HtmlDocument (Html Agility Pack)
            HtmlDocument htmlDoc = htmlWeb.Load(url);

            //Find all class="g-blocklist-item" nodes
            List<HtmlNode> eventNodes = htmlDoc.DocumentNode.Descendants(0).Where(d => d.GetAttributeValue("class", "") == "g-blocklist-item").ToList();


            foreach (var node in eventNodes)
            {
                try
                {
                    string eventName = node.Descendants(0).Where(d => d.GetAttributeValue("class", "") == "event-title").FirstOrDefault().InnerText;
                    string venueRaw = node.Descendants(0).Where(d => d.GetAttributeValue("class", "").Contains("g-blocklist-sub-text")).FirstOrDefault().InnerHtml;
                    string venue = Regex.Matches(venueRaw, @"(?<=(<br>))([\s\S]*?)(?=<br>)").First().ToString().Trim();
                    string eventDate = node.Descendants(0).Where(d => d.Name == "time").FirstOrDefault().InnerText;
                    string eventTime = node.Descendants(0).Where(d => d.Name == "time").ElementAt(1).InnerText;
                    DateTime eventDateTime = new DateTime();
                    DateTime.TryParse($"{eventDate} {eventTime}", out eventDateTime);
                    string imageUrl = "(none)";
                    //Get image if one exists
                    if (node.Descendants(0).Where(d => d.Name == "img").Count() > 0) imageUrl = node.Descendants(0).Where(d => d.Name == "img").FirstOrDefault().GetAttributeValue("data-src", "");

                    List<string> artists = null;
                    //Get list of artists if list exists
                    if (node.Descendants(0).Where(d => d.GetAttributeValue("class", "") == "g-blocklist-item-extended").Count() > 0)
                    {
                        var artistsNode = node.Descendants(0).Where(d => d.GetAttributeValue("class", "") == "g-blocklist-item-extended").FirstOrDefault();
                        artists = artistsNode.Descendants(0).Where(d => d.Name == "a").Select(e => e.InnerText).ToList();
                    }

                    //Default to none, but try to obtain event status
                    string statusRaw = node.Descendants(0).Where(d => d.GetAttributeValue("class", "") == "g-blocklist-action").FirstOrDefault().InnerText.Trim();
                    EventStatus status = 0;
                    try { status = (EventStatus)Enum.Parse(typeof(EventStatus), GetValueFromDescription<EventStatus>(statusRaw).ToString()); } catch { };

                    Event eventResult = new Event()
                    {
                        eventName = eventName,
                        venue = venue,
                        eventDate = eventDateTime,
                        artists = artists,
                        imageURL = imageUrl,
                        status = status
                    };

                    //Add to list
                    returnList.Add(eventResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error parsing item {eventNodes.IndexOf(node)}: {e.ToString()}");
                }
            }

            //Return list of all events on page
            return returnList;
        }

    }
}
