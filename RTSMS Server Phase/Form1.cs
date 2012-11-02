using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Configuration;
using System.Net;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Threading;
using System.Timers;
using System.IO;
using System.Xml;
using RTSMS_Server_Phase.SQLServerRTSMSDataSetTableAdapters;
using RTSMS_Server_Phase.classes;

namespace RTSMS_Server_Phase
{
    public partial class Form1 : Form
    {
        public int Timer_Diff = 45; // Default is 5 Seconds 
        public bool StopServer = false; 
        public XmlTextReader rssReader;
        public XmlDocument rssDoc;
        public XmlNode nodeRss;
        public XmlNode nodeChannel;
        public XmlNode nodeItem;
        public ListViewItem rowNews;

        String current;
        ArrayList channelset = new ArrayList();
        //private const string list_product = "select pkProductId, ProductName, IntroducedDate from Product where fkCategoryId= {0};";
        private const string listchannel = "select * from [Channel]";
        public Form1()
        {
            InitializeComponent();
            this.rssItemTableAdapter1.Fill(this.sqlServerRTSMSDataSet1.RSSItem);
        //    backgroundWorker1.RunWorkerAsync();
            using ( SqlConnection sql = new SqlConnection())
            {
                sql.ConnectionString = Properties.Settings.Default.SQLServerRTSMSDBConnectionString;
                sql.Open();
                try
                {
                    SqlCommand sqlcommand = new SqlCommand(listchannel, sql);

                    SqlDataReader reader = sqlcommand.ExecuteReader();
                    // RSSListBox.Items.Clear();
                    while (reader.Read())
                    {
                        channelset.Add(reader.GetString(2));
                    }
                    reader.Close();
                    sql.Close();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); throw ex; }
                finally
                {
                    if (sql != null && sql.State == ConnectionState.Open) sql.Close();
                }

            }
        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }
        private void notifyIcon1_DoubleClick(object sender,
                                     System.EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
        private void status_update()
        {
            toolStripStatusLabel1.Text = "Server Started";
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
           // for (int j=0;j<2 ;j++ )
           // {
                if (backgroundWorker1.CancellationPending)//checks for cancel request
                {
                    e.Cancel = true;
                    return;
                }

                // Wait 100 milliseconds.
                /*if (Int32.Parse(time_interval.Text.ToString()) > 0)
                    Timer_Diff = Int32.Parse(time_interval.Text.ToString());
                else
                    Status_Bar.Text = "Invalid Timer Interval...Interval must be greator than 0";
                
                 * */
                //Thread.Sleep(Timer_Diff * 1000);
                Threading_Work();
                Thread.Sleep(2 * 1000);  // 2 Second Wait to report the status. and Reset the Progress Bar.
                backgroundWorker1.ReportProgress(0);
        }

        private void Threading_Work()
        {
            WebProxy proxy = null;//add proxy if needed for example: new WebProxy("some uri : some port");
            if (proxy_support.Checked == true)
            {
                proxy = new WebProxy(proxy_url.ToString());
            }

            if (proxy != null)
            {

                // proxy = new WebProxy("http://10.11.0.22:8080");
                proxy.Credentials = new NetworkCredential("userName", "password", "proxy:port");
            }
            
            int i = 1;
            foreach (string uriString in channelset)
            {
                WebRequest request = HttpWebRequest.Create(uriString);
                request.Method = "GET";
                if (proxy != null)
                {
                    request.Proxy = proxy;
                }
                object data = new object(); //container for our "Stuff"
                // RequestState is a custom class to pass info to the callback
                RequestState state = new RequestState(request, data, uriString);
                IAsyncResult result = request.BeginGetResponse(
                    new AsyncCallback(UpdateItem), state);

                //Register the timeout callback
                ThreadPool.RegisterWaitForSingleObject(
                    result.AsyncWaitHandle,
                    new WaitOrTimerCallback(ScanTimeoutCallback),
                    state,
                    (60 * 1000),  // 15 second timeout
                    true
                    );
                Thread.Sleep(2 * 1000);
            }
            backgroundWorker1.ReportProgress(i * 100);

        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            ProgressBar_ToolStrip.Value = e.ProgressPercentage;
            // Set the text.
            Status_Bar.Text = e.ProgressPercentage.ToString();
            //            this.Text = e.ProgressPercentage.ToString();
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)//it doesn't matter if the BG worker ends normally, or gets cancelled,
            {			   //both cases RunWorkerCompleted is invoked, so we need to check what has happened
                Status_Bar.Text = "You Have Stopped The Server.";

            }
            else
            {
                Status_Bar.Text = "Working...";
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            button2.Enabled = false;
            Startbtn.Enabled = true;
        }


        private void Startbtn_Click(object sender, EventArgs e)
        {
           // status_update();
            backgroundWorker1.RunWorkerAsync();
            Startbtn.Enabled = false;
            button2.Enabled = true;
		}

  
		private  void UpdateItem (IAsyncResult result)
        {

            RequestState state = (RequestState)result.AsyncState;
            WebRequest request = (WebRequest)state.Request;
            // get the Response
           /* HttpWebResponse response;
            response = (HttpWebResponse)request.EndGetResponse(result);
            */

//            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ShowDownHostName("Active Site >>" + state.SiteUrl.ToString());
                        /////////////////////////////////////////////////////////////new code ////////////////////////
                        // grab the custom state object

                        string lastMod = String.Empty;
                        if (response.Headers["last-modified"] != null)
                        {
                            lastMod = response.Headers["last-modified"];
                        }
                        else
                        {
                            //////////////////////////////////////////////////////////////////////////////////

                            // Create a new XmlTextReader from the specified URL (RSS feed)
                            Stream s = (Stream)response.GetResponseStream();
                            StreamReader readStream = new StreamReader(s);
                                // dataString will hold the entire contents of the requested page if we need it.
                             string dataString = readStream.ReadToEnd();

                             s.Close();
                             readStream.Close();
                            
                            //rssReader = new XmlTextReader(state.SiteUrl);
                            rssReader = new XmlTextReader(dataString);

                            rssReader.WhitespaceHandling = WhitespaceHandling.Significant;
                            rssDoc = new XmlDocument();
                            // Load the XML content into a XmlDocument
                            rssDoc.Load(rssReader);
                            // Loop for the <rss> tag
                            for (int i = 0; i < rssDoc.ChildNodes.Count; i++)
                            {
                                // If it is the rss tag
                                if (rssDoc.ChildNodes[i].Name == "rss")
                                {
                                    // <rss> tag found
                                    nodeRss = rssDoc.ChildNodes[i];
                                }
                            }

                            // Loop for the <channel> tag
                            for (int i = 0; i < nodeRss.ChildNodes.Count; i++)
                            {
                                // If it is the channel tag
                                if (nodeRss.ChildNodes[i].Name == "channel")
                                {
                                    // <channel> tag found
                                    nodeChannel = nodeRss.ChildNodes[i];
                                }
                            }
                            lastMod = nodeChannel["lastBuildDate"].InnerText.ToString();
                            //////////////////////////////////////////////////////////////////////////////////
                        }
                        response.Close();
                        //lastBuildDate
                        /* Stream s = (Stream)response.GetResponseStream();
                         StreamReader readStream = new StreamReader(s);
                         // dataString will hold the entire contents of the requested page if we need it.
                         string dataString = readStream.ReadToEnd();

                         s.Close();
                         readStream.Close();
                         */

                        DateTime dbDateTime = new DateTime();
                        DateTime httpDateTime = new DateTime();
                        httpDateTime = DateTime.Parse(lastMod);
                        Int32 ChannelID;

                        string query = "select LastUpdated,ChannelID from Channel where Link='" + state.SiteUrl + "'";
                        using (SqlConnection sql = new SqlConnection())
                        {
                            sql.ConnectionString = Properties.Settings.Default.SQLServerRTSMSDBConnectionString;
                            sql.Open();
                            try
                            {
                                SqlCommand sqlcommand = new SqlCommand(query, sql);
                                SqlDataReader reader = sqlcommand.ExecuteReader();
                                reader.Read();

                                dbDateTime = reader.GetDateTime(0);
                                ChannelID = reader.GetInt32(1);
                                reader.Close();
                                sql.Close();
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); throw ex; }
                            finally
                            {
                                if (sql != null && sql.State == ConnectionState.Open) sql.Close();
                            }

                        }
                        int compare = DateTime.Compare(httpDateTime, dbDateTime);

                        if (compare == 1)
                        {
                            //                 MessageBox.Show(state.SiteUrl);

                            query = String.Empty;
                            query = "update [Channel] set LastUpdated='" + httpDateTime + "' where  Link='" + state.SiteUrl + " ';";

                            using (SqlConnection sql1 = new SqlConnection())
                            {
                                sql1.ConnectionString = Properties.Settings.Default.SQLServerRTSMSDBConnectionString;
                                sql1.Open();
                                try
                                {
                                    SqlCommand sqlcommand1 = new SqlCommand(query, sql1);

                                    sqlcommand1.ExecuteNonQuery();
                                    // sqlcommand1.ExecuteScalar();
                                    sql1.Close();
                                }
                                catch (Exception ex) { MessageBox.Show(ex.Message); throw ex; }
                                finally
                                {
                                    if (sql1 != null && sql1.State == ConnectionState.Open) sql1.Close();
                                }

                            }

                            String url = state.SiteUrl;
                            /////////////////////////////////////////////////////////////////////////////////////////////////////
                            // Create a new XmlTextReader from the specified URL (RSS feed)
                            rssReader = new XmlTextReader(state.SiteUrl);
                            rssDoc = new XmlDocument();
                            // Load the XML content into a XmlDocument
                            rssDoc.Load(rssReader);


                            ////////////////////////////////
                            /*nodeRss.RemoveAll();
                            nodeChannel.RemoveAll();
                            nodeItem.RemoveAll();
                            rowNews.Remove();
                            */
                            ////////////////////////////////



                            // Loop for the <rss> tag
                            for (int i = 0; i < rssDoc.ChildNodes.Count; i++)
                            {
                                // If it is the rss tag
                                if (rssDoc.ChildNodes[i].Name == "rss")
                                {
                                    // <rss> tag found
                                    nodeRss = rssDoc.ChildNodes[i];
                                }
                            }

                            // Loop for the <channel> tag
                            for (int i = 0; i < nodeRss.ChildNodes.Count; i++)
                            {
                                // If it is the channel tag
                                if (nodeRss.ChildNodes[i].Name == "channel")
                                {
                                    // <channel> tag found
                                    nodeChannel = nodeRss.ChildNodes[i];
                                }
                            }

                            // Temp Storage of Title,Links etc...
                            String Titles, Link;
                            String Description;
                            DateTime pubDate;
                            // Array List to Fetch RSS and store.
                            ArrayList arrList = new ArrayList();
                            // Loop for the <title>, <link>, <description> and all the other tags
                            for (int i = 0; i < nodeChannel.ChildNodes.Count; i++)
                            {
                                // If it is the item tag, then it has children tags which we will add as items to the ListView
                                if (nodeChannel.ChildNodes[i].Name == "item")
                                {
                                    nodeItem = nodeChannel.ChildNodes[i];

                                    Titles = nodeItem["title"].InnerText.ToString();
                                    Description = nodeItem["description"].InnerText.ToString();          ///////
                                    HtmlRemoval.source = Description;
                                    HtmlRemoval.ValidateHTML();
                                    Description = HtmlRemoval.StripTagsRegex();
                                    Link = nodeItem["link"].InnerText.ToString();
                                    //pubDate = DateTime.Parse(nodeItem["pubDate"].InnerText.ToString());  ///////
                                    pubDate = DateTime.Now;
                                    // Feed Data In NewsItem Class for ArrayList
                                    NewsItem tempitem = new NewsItem();
                                    tempitem.Title = Titles;
                                    tempitem.Description = Description;
                                    tempitem.Link = Link;
                                    tempitem.pubDate = pubDate;
                                    arrList.Add(tempitem);

                                }
                            }
                            // Array List is filled with XML. NewsItem Class Collections.
                            ArrayList newList = new ArrayList();
                            newList = arrList;
                            foreach (NewsItem currentNewsItem in newList)
                            {
                                string title = currentNewsItem.Title;
                                string description = currentNewsItem.Description;
                                if (title.Length >= 1024)
                                    title = title.Substring(0, 1024);
                                if (description.Length >= 5048)
                                    description = description.Substring(0, 5048);
                                title = title.Replace("'", "");
                                string link = currentNewsItem.Link;
                                string filterExpression = "Link = '" + link + "' and Title='" + title + "'";// and Description='" + description + "'
                                DataRow[] filterNewsItems = sqlServerRTSMSDataSet1.RSSItem.Select(filterExpression);
                                if (filterNewsItems.Length == 0)
                                {
                                    Guid guid = Guid.NewGuid();
                                    SQLServerRTSMSDataSet.RSSItemRow newRow = sqlServerRTSMSDataSet1.RSSItem.NewRSSItemRow();
                                    newRow.RSSItemID = guid.ToString("N");

                                    newRow.Title = title;
                                    newRow.Description = description;
                                    newRow.Link = currentNewsItem.Link;
                                    newRow.ChannelID = ChannelID;
                                    newRow.pubDate = currentNewsItem.pubDate;
                                    sqlServerRTSMSDataSet1.RSSItem.AddRSSItemRow(newRow);
                                }
                                else
                                {
                                }
                            }
                            this.rssItemTableAdapter1.Update(sqlServerRTSMSDataSet1.RSSItem);
                            /////////////////////////////////////////////////////////////////////////////////////////////////////
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                ShowDownHostName(ex.Message + " >> " + state.SiteUrl.ToString());
//                ShowDownHostName(state.SiteUrl.ToString());
                //Just don't do anything. Retry after few seconds
               // throw ex;
            }


        }

        private delegate void stringDelegate(string host);
        private void ShowDownHostName(string host)
        {
            if (listBox1.InvokeRequired)
            {
                stringDelegate sd = new stringDelegate(ShowDownHostName);
                this.Invoke(sd, new object[] { host });
            }
            else
            {
                listBox1.Items.Add(host);
            }


        }
		private static void ScanTimeoutCallback (
			object state, bool timedOut) 
		{ 
			if (timedOut) 
			{
				RequestState reqState = (RequestState)state;
				if (reqState != null) 
					reqState.Request.Abort();
				Console.WriteLine("aborted- timeout") ;
			}
		}

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void restoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }





   }

    class RequestState
    {
        public WebRequest Request;  // holds the request
        public object Data;  // store any data in this
        public string SiteUrl; // holds the UrlString to match up results (Database lookup, etc).
        public RequestState(WebRequest request, object data, string siteUrl)
        {
            this.Request = request;
            this.Data = data;
            this.SiteUrl = siteUrl;
        }
    }
}
