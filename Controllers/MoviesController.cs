using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;
using Newtonsoft.Json;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        string apiUrl = "https://localhost:7036/api/MovieAPI";
        HttpClient client = new HttpClient();
        private readonly MvcMovieContext _context;

        public MoviesController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, int? pageNumber, int pageSize, string movieGenre, string searchString)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            MovieGenreViewModel? movieGenreVM;
            List<string> listGenres = null;
            PaginatedList<Movie> listMovies = null;
            pageSize = 5;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            HttpResponseMessage responseMovies = client.GetAsync(apiUrl + string.Format("/SearchMovies?sortOrder={0}&pageNumber={1}&pageSize={2}&movieGenre={3}&searchString={4}", sortOrder, pageNumber, pageSize, movieGenre, searchString)).Result;
            if (responseMovies.IsSuccessStatusCode)
            {
                listMovies = JsonConvert.DeserializeObject<PaginatedList<Movie>>(responseMovies.Content.ReadAsStringAsync().Result);
            }

            HttpResponseMessage responseGenres = client.GetAsync(apiUrl + string.Format("/GetGenre")).Result;
            if (responseGenres.IsSuccessStatusCode)
            {
                listGenres = JsonConvert.DeserializeObject<List<string>>(responseGenres.Content.ReadAsStringAsync().Result);
            }

            movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(listGenres),
                Movies = listMovies
            };

            return View(movieGenreVM);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Movie movie = null;

            HttpResponseMessage response = client.GetAsync(apiUrl + string.Format("/DetailMovies?id={0}", id)).Result;
            if (response.IsSuccessStatusCode)
            {
                movie = JsonConvert.DeserializeObject<Movie>(response.Content.ReadAsStringAsync().Result);
            }
            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Movie movie = null;

            HttpResponseMessage response = client.GetAsync(apiUrl + string.Format("/EditMovie?id={0}", id)).Result;
            if (response.IsSuccessStatusCode)
            {
                movie = JsonConvert.DeserializeObject<Movie>(response.Content.ReadAsStringAsync().Result);
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Movie movie = null;

            HttpResponseMessage response = client.GetAsync(apiUrl + string.Format("/DeleteMovie?id={0}", id)).Result;
            if (response.IsSuccessStatusCode)
            {
                movie = JsonConvert.DeserializeObject<Movie>(response.Content.ReadAsStringAsync().Result);
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Movie == null)
            {
                return Problem("Entity set 'MvcMovieContext.Movie'  is null.");
            }
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return "From [HttpPost]Index: filter on " + searchString;
        }

        private bool MovieExists(int id)
        {
            return (_context.Movie?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private static void Dump(object o)
        {
            string json = JsonConvert.SerializeObject(o, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}
