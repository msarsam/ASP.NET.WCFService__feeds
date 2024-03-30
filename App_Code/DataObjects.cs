using System.Collections.Generic;

namespace MS.DataObjects
{
    public class Status
    {
        public bool status { get; set; }
        public string detail { get; set; }
    }

    public class Source
    {
        public int ID { get; set; }
        public string name { get; set; }
        public bool isActive { get; set; }
    }

    public class Sources
    {
        public List<Source> sources = new List<Source>();

        public Status status = new Status();
    }

    public class Channel
    {
        public int ID { get; set; }
        public string name { get; set; }
        public bool isActive { get; set; }
    }

    public class Channels
    {
        public List<Channel> channels = new List<Channel>();
        public Status status = new Status();
    }

    public class RSSfeed
    {
        public string title { get; set; }
        public string link { get; set; }
        public string descrition { get; set; }
    }

    public class RSSfeeds
    {
        public List<RSSfeed> feeds = new List<RSSfeed>();
        public string sourceName { get; set; }
        public string channelName { get; set; }
    }

    public class MasterFeed
    {
        public List<RSSfeeds> all = new List<RSSfeeds>();
        public Status status = new Status();
    }
}

