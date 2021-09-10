# movie-search-api
asp.net 3 core api with elasticsearch and liteDB 

## acknowledgements
based on [This stackoverflow answer](https://stackoverflow.com/questions/41711961/elasticsearch-user-clicks-feedback/41716811#41716811)

## usage
start elasticsearch
  ```
  docker run -d -p 9200:9200 -p 5601:5601 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:7.11.2
  ```
go to /swagger to learn the api routes
