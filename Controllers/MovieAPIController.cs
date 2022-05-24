using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using MvcMovie.Response;
using Newtonsoft.Json;

namespace MvcMovie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieAPIController : ControllerBase
    {
        private readonly MvcMovieContext _context;

        public MovieAPIController(MvcMovieContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetGenre()
        {
            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;
            List<string> Genres = await genreQuery.Distinct().ToListAsync();

            return Ok(Genres);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> SearchMovies(string sortOrder, int? pageNumber, int pageSize, string movieGenre, string searchString)
        {

            var movies = from m in _context.Movie
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            if (sortOrder == "name_desc")
            {
                movies = movies.OrderByDescending(s => s.Title);
            }
            PaginatedList<Movie> listMovies = await PaginatedList<Movie>.CreateAsync(movies.AsNoTracking(), pageNumber ?? 1, pageSize);

            return Ok(listMovies);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> DetailMovies(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> EditMovie(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> DeleteMovie(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        private static void Dump(object o)
        {
            string json = JsonConvert.SerializeObject(o, Formatting.Indented);
            Console.WriteLine(json);
        }

    }
}
