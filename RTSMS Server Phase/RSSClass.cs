using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSMS_Server_Phase
{
    class RSSClass
    {
    }
    public class NewsItem
    {
        private string _title;
        private string _description;
        private string _link;
        private DateTime _pubdate;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Link
        {
            get { return _link; }
            set { _link = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public DateTime pubDate
        {
            get { return _pubdate; }
            set { _pubdate = value; }
        }

    }

}
