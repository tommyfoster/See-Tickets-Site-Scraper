using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace See_Tickets_Site_Scraper
{

    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum EventStatus
    {
        None,
        Postponed,
        Cancelled,
        [EnumMember(Value = "Sold out")]
        [Description("Sold out")]
        Sold_out,
        [EnumMember(Value = "Contact venue for tickets")]
        [Description("Contact venue for tickets")]
        Contact_venue_for_tickets,
        [EnumMember(Value = "Pay on entry (subject to availability)")]
        [Description("Pay on entry (subject to availability)")]
        Pay_on_entry
    }



    class Event
    {
        
        public Event()
        {

        }

        public Event(string eventName, string venue, DateTime eventDate, string imageURL, List<string> artists = null, EventStatus status = 0)
        {
            this.eventName = eventName;
            this.venue = venue;
            this.eventDate = eventDate;
            this.imageURL = imageURL;
            this.artists = artists;
            this.status = status;
        }

        public string eventName { get; set; }
        public string venue { get; set; }
        public DateTime eventDate { get; set; }
        public string imageURL { get; set; }
        public List<string> artists { get; set; }
        public EventStatus status { get; set; }
    }
}
