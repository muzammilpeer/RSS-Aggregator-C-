

Project Description
RSS Aggregator is the RSS feeds downloader with multi-threaded async request and store the feeds in database.

Features

-Download the RSS feeds from list of channels and store them in database (e.g:SQL Server 2008)

- Multi-threaded RSS aggregator.

- Web-Request in Async fashion.

- List Down the host can't reply within "Timeout" duration.

- Support of Proxy Settings to request through the proxy server. 

 

Website: http://www.triadslabs.com

Last edited Sep 10 at 9:28 PM by muzammilpeer, version 5




    Follow (2)
    Subscribe

Setup the Database with the SQL-Script Provided in Download section.

For Application to setup change the Connection string of the database in 

"RTSMS Server Phase.vshost.exe.config" file find this file in C:\ProgoramFiles\appname\RTSMS Server Phase.vshost.exe.config

Under Connection String tag

replace this tag with  your connection string.

 <add name="RTSMS_Server_Phase.Properties.Settings.SQLServerRTSMSDBConnectionString"            connectionString="Data Source=./;Initial Catalog=rtsmsdb;Integrated Security=True"            providerName="System.Data.SqlClient" />

 

For Source Code change the Connection String from the Projects -> Properties -> Settings.Settings -> "SQLServerRTSMSDBConnectionString"

Replace it with your connection string.

Last edited Mar 29, 2011 at 10:48 AM by muzammilpeer, version 2