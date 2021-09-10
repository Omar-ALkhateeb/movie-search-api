using Elasticsearch.Net;
using LiteDB;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace movie_search_api
{
    public class Movie
    {
        public ObjectId Id { get; set; }
        public string MovieName { get; set; }
        public string Year { get; set; }
        public string[] Genre { get; set; }
        public int Clicks { get; set; }
        public int Views { get; set; }

        public static bool IsEmpty(ILiteCollection<Movie> col)
        {
            return col.FindAll().Count() == 0;
        }

        public static void SeedDB(ILiteCollection<Movie> col)
        {
            // read data from dat file
            var lines = File.ReadAllLines("./movies.dat");
            // Console.WriteLine(lines);
            List<Movie> movies = new List<Movie>();

            // serialize the data into Movie objs
            foreach (string line in lines)
            {
                var movieData = line.Split("::").ToList();
                // Console.WriteLine(movieData[1].Split('(')[1].Trim(')'));
                movies.Add(new Movie
                {
                    Clicks = 0,
                    Views = 0,
                    Year = movieData[1].Split('(')[1].Trim(')'),
                    Genre = movieData[2].Split('|'),
                    MovieName = movieData[1].Split('(')[0]
                });
            }

            col.InsertBulk(movies);
        }
        public static Movie AddClick(ILiteCollection<Movie> col, string movieName)
        {
            var movie = col.FindOne(x => x.MovieName == movieName);
            if (movie != null)
            {
                movie.Clicks +=1;
                col.Update(movie);
            }
            return movie;
        }
        public static void AddView(ILiteCollection<Movie> col, string[] movieNames)
        {
            foreach (string movieName in movieNames)
            {
                var movie = col.FindOne(x => x.MovieName == movieName);
                if (movie!=null)
                {
                    movie.Views += 1;
                    col.Update(movie);
                }
            }
        }
        public static void SeedSearch(ElasticClient client, ILiteCollection<Movie> col)
        {
            // delete all data retrive the new values from db then bulk insert it
            client.DeleteByQuery<Movie>(del => del
                .Query(q => q.MatchAll())
            );
            //client.Indices.Delete("movies");
            //client.Indices.Create("movies",
            //      index => index.Map<Movie>(
            //          x => x.AutoMap()
            //      ));
            client.Bulk(b => b
                .IndexMany<Movie>(col.FindAll().ToArray())
                .Refresh(Refresh.WaitFor)
            );
        }
        public static ISearchResponse<Movie> Search(ElasticClient client, string term)
        {
            // TODO optimize these parameters
            //var response = client.Search<Movie>(s => s
            //    .From(0)
            //    .Size(10)
            //    .Query(q => q
            //    .FunctionScore(fs => fs
            //    .ScoreMode(FunctionScoreMode.Max)
            //    .BoostMode(FunctionBoostMode.Multiply)
            //        .Functions(fu => fu
            //            .Weight(w => w
            //                .Filter(wf => wf
            //                    .Term("MovieName", term)
            //                )
            //            )
            //            .ScriptScore(s => s
            //                .Script(ss => ss
            //                    .Source("20*doc['Clicks'].value + 40 * doc['Views].value")
            //                )
            //                .Weight(200)
            //            )
            //        )
            //        .Functions(fu => fu
            //            .Weight(w => w
            //                .Filter(wf => wf
            //                    .Term("Genre", term)
            //                )
            //            )
            //            .ScriptScore(s => s
            //                .Script(ss => ss
            //                    .Source("20*doc['Clicks'].value + 40 * doc['Views].value")
            //                )
            //                .Weight(100)
            //            )
            //        )

            //    )
            //)
            //);
            //var response = client.Search<Movie>(s => s
            //    .Size(10)
            //    .Query(q => q
            //        .QueryString(queryDescriptor => queryDescriptor
            //            .Query(term)
            //            .Fields(fs => fs
            //                .Fields(f1 => f1.MovieName)
            //                .Fields(f1 => f1.Genre)
            //            )
            //            .DefaultOperator(Operator.Or)
            //        )
            //    )
            //    //.Sort(q => q.Descending(u => u.Views))
            //    //.Sort(q => q.Descending(u => u.Clicks))

            //);


            var json = @"{
                ""function_score"": {
                  ""query"": {
                    ""query_string"": {
                      ""query"": """+term+@""",
                      ""fields"": [""movieName^5"", ""genre^2""]
                    }
                  },
                  ""script_score"": {
                    ""script"": {
                      ""lang"": ""painless"",
                      ""inline"": ""_score + 20*doc['clicks'].value + 40 * doc['views'].value""
                    }
                  },
                  ""score_mode"": ""max"",
                  ""boost_mode"": ""multiply""
                }
            }";

            //string query = string.Format(json,Environment.NewLine, term);

            var response = client.Search<Movie>(s => s.Query(q=>q.Raw(json)));

            Console.WriteLine(response.OriginalException);
            // Console.WriteLine(response.IsValid);
            return response;
        }
    }
}
