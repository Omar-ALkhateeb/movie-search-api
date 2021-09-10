using LiteDB;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace movie_search_api
{
    public class DBClient
    {
        private readonly ILiteCollection<Movie> _col;
        public DBClient()
        {
            var db = new LiteDatabase(@"Movies.db");
            _col = db.GetCollection<Movie>("movies");
        }
        public ILiteCollection<Movie> Create()
        {
            return _col;
        }
    }
}
