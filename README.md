# Transcodarr
--- 
An Automated Transcoding System with multi node support.

## About
Transcodarr is deployed as a core that can utilize multiple nodes to dispatch transcode jobs to. The nodes are kept intentially slim such that they are easy to deploy on almost any system without needing too much infrastructure.

A node opens a connection to the core and tells the core its capabilities, i.e., the available codecs. Through the configuration made via the web ui of the core, the desired settings to be used by the all nodes when transcoding can be specified.

The core only orchestrates the jobs and does not do any media file specific handling itself. So even for a single system deployment a core and node will need to be running.


## Setup
To get Transcodarr running you will need to deploy both the core and node and configure the node to have the endpoint of the core. Either through the appsettings.json by setting the `NodeConfiguration:CoreUrl` to the address where the core is hosted at, or by setting `NodeConfiguration__CoreUrl` as an environment variable to the cores address.

The core ships and hosts the web ui itself and only needs to have its path for the sqlite db configured. Either in the appsetting.json `ConnectionStrings:TranscodarrDb` or as an environmment variable `ConnectionStrings__TranscodarrDb`
