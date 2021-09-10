using Nest;
using System;

namespace movie_search_api
{
    public class ESClient
    {
        private readonly ConnectionSettings _settings;

        public ESClient()
        {
            var node = new Uri("http://localhost:9200");
            _settings = new ConnectionSettings(node).DefaultMappingFor<Movie>(x => x.IndexName("movies")) ;
        }

        [Obsolete]
        public ElasticClient Get() // refactor to use different indeces to hot swap them
        {
            var client = new ElasticClient(_settings);
            try {
                var response = client.Indices.Create("movies", descriptor => descriptor
                .Mappings(
                        ms => ms.Map<Movie>(
                            m => m.Properties(
                                p => p
                                    .Text(t => t.Name(n => n.MovieName).Analyzer("auto-complete"))
                                    .Text(t => t.Name(n => n.Genre).Analyzer("auto-complete").Fields(ff => ff.Keyword(k => k.Name("keyword")))))))
                        .Settings(f => f.Analysis(
                            analysis => analysis
                                .Analyzers(
                                    analyzers => analyzers
                                        .Custom("auto-complete", a => a.Tokenizer("standard").Filters("lowercase", "auto-complete-filter")))
                                        .TokenFilters(tokenFilter => tokenFilter
                                                                    .EdgeNGram("auto-complete-filter", t => t.MinGram(2).MaxGram(7))
                                                                 ))));
                Console.WriteLine(response.OriginalException);
                Console.WriteLine(response.IsValid);
            } catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            
            return client;
        }
    }
}
