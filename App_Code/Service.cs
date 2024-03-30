using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml;
using MS.DataManagementLibrary;
using MS.DataObjects;

[ServiceContract]
public class Service
{   
    private string connString = ConfigurationManager.ConnectionStrings["rss_ConnectionString_sql"].ConnectionString;    
    private bool toBool(string arg)
    {
        bool s = true;
        if (arg.Length == 0)
        {
            s = false;
        }

        return s;
    }

    private string removeHtmlTags(string html, bool allowHarmlessTags)
    {
        if (html == null || html == string.Empty)
        {
            return string.Empty;
        }

        if (allowHarmlessTags)
        {
            return System.Text.RegularExpressions.Regex.Replace(html, "", string.Empty);
        }

        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
    }

    public List<RSSfeed> ProcessRSSItem(string rssURL)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; /* handle request from https */
        WebRequest myRequest = WebRequest.Create(rssURL);
        WebResponse myResponse = myRequest.GetResponse();
        Stream rssStream = myResponse.GetResponseStream();
        XmlDocument rssDoc = new XmlDocument();
                    rssDoc.Load(rssStream);

        XmlNodeList rssItems = rssDoc.SelectNodes("rss/channel/item");
        List<RSSfeed> listOfFeeds = new List<RSSfeed>();

        for (int i = 0; i < 4; i++) /* limit to 4 */
        {
            RSSfeed feeds = new RSSfeed();
                    feeds.title = rssItems.Item(i).SelectSingleNode("title").InnerText.ToString();
                    feeds.link = rssItems.Item(i).SelectSingleNode("link").InnerText.ToString();
                    feeds.descrition = removeHtmlTags(rssItems.Item(i).SelectSingleNode("description").InnerText, false).Replace("\"", "");

            listOfFeeds.Add(feeds);
        }
        return listOfFeeds;
    }

    [OperationContract]
    [WebInvoke(Method = "*", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]

    public Sources getSources()
    {
        DataManager DataManager = new DataManager();
        SqlDataReader s = DataManager.SQLGetData("SELECT rss_sources.srcID, rss_sources.sourceName, rss_userssources.userid FROM rss_sources LEFT OUTER JOIN rss_userssources ON (rss_userssources.srcID = rss_sources.srciD AND rss_userssources.userID =1) ORDER BY rss_sources.ordr", connString);

        Sources sources = new Sources();
        if (s.HasRows)
        {
            while (s.Read())
            {
                {
                    Source source = new Source();
                           source.ID = Convert.ToInt32(s["srcID"]);
                           source.name = s["sourceName"].ToString();
                           source.isActive = toBool(s["userid"].ToString());
                           sources.sources.Add(source);
                }
            }

            Status st = new Status();
                   st.detail = "__success__";
                   st.status = true;

            sources.status = st;
        }
        return sources;
    }

    [OperationContract]
    [WebInvoke(Method = "*", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]

    public Channels getChannels()
    {
        DataManager DataManager = new DataManager();
        string sqlString = "SELECT rss_channels.chnlID, rss_channels.channelName, rss_userschannels.userid " +
                           "FROM rss_channels LEFT OUTER JOIN rss_userschannels ON (rss_userschannels.chnlID = rss_channels.chnlID AND rss_userschannels.userID = 1) " +
                           "WHERE rss_channels.chnlID <> 5 " +
                           "ORDER BY rss_channels.ordr";

        SqlDataReader s = DataManager.SQLGetData(sqlString, connString);
        
        Channels channels = new Channels();

        if (s.HasRows)
        {
            while (s.Read())
            {
                {
                    Channel channel = new Channel();
                            channel.ID = Convert.ToInt32(s["chnlID"]);
                            channel.name = s["channelName"].ToString();
                            channel.isActive = toBool(s["userid"].ToString());
                            channels.channels.Add(channel);
                }
            }

            Status st = new Status();
                   st.detail = "__success__";
                   st.status = true;

            channels.status = st;
        }
        return channels;
    }

    [OperationContract]
    [WebInvoke(Method = "*", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
    public MasterFeed getFeeds(string srcCollection, string chnlCollection)
    {

        string od = " rss_sources.ordr, rss_channels.ordr";

        DataManager DataManager = new DataManager();
        string sqlString = "SELECT rss_sources.sourceName, rss_channels.channelName, rss_sourceschannels.userChnlID, rss_sourceschannels.chnlURL " +
                           "FROM rss_sourceschannels " +
                           "INNER JOIN rss_sources ON(rss_sources.srcID = rss_sourceschannels.srcID) " +
                           "INNER JOIN rss_channels on (rss_channels.chnlID = rss_sourceschannels.chnlID)  " +
                           "WHERE rss_sourceschannels.srcID IN (" + srcCollection + ") " +
                           "AND rss_sourceschannels.chnlID IN(" + chnlCollection + ") " +
                           "ORDER BY " + od;

        SqlDataReader s = DataManager.SQLGetData(sqlString, connString);
        MasterFeed masterFeed = new MasterFeed();

        if (s.HasRows)
        {
            while (s.Read())
            {
                {

                    RSSfeeds rssfeeds = new RSSfeeds();
                             rssfeeds.sourceName = s["sourceName"].ToString();
                             rssfeeds.channelName = s["channelName"].ToString();
                             rssfeeds.feeds = ProcessRSSItem(s["chnlURL"].ToString());

                    masterFeed.all.Add(rssfeeds);

                    Status st = new Status();
                           st.detail = "__success__";
                           st.status = true;

                    masterFeed.status = st;
                }
            }
        }
        return masterFeed;
    }
}
