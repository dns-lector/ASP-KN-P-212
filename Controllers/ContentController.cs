using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Models.Content.Category;
using ASP_KN_P_212.Models.Content.Index;
using ASP_KN_P_212.Models.Content.Location;
using ASP_KN_P_212.Models.Content.Room;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    public class ContentController(DataAccessor dataAccessor) : Controller
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            ContentIndexPageModel model = new()
            {
                Categories = _dataAccessor.ContentDao.GetCategories()
            };
            return View(model);
        }

        public IActionResult Category([FromRoute]String id)
        {
            var ctg = _dataAccessor.ContentDao.GetCategoryBySlug(id);

            return ctg == null 
                ? View("NotFound") 
                : View(new ContentCategoryPageModel()
                {
                    Category = ctg,
                    Locations = _dataAccessor.ContentDao.GetLocations(ctg.Slug!)
                });
        }
        public IActionResult Location([FromRoute]String id)
        {
            var loc = _dataAccessor.ContentDao.GetLocationBySlug(id);

            return loc == null 
                ? View("NotFound") 
                : View(new ContentLocationPageModel()
                {
                    Location = loc,
                    Rooms = _dataAccessor.ContentDao.GetRooms(loc.Id)
                });
        }

        public IActionResult Room([FromRoute] String id, [FromQuery]int? year, [FromQuery] int? month)
        {
            var room = _dataAccessor.ContentDao.GetRoomBySlug(id);

            return room == null
                ? View("NotFound")
                : View(new ContentRoomPageModel
                {
                    Room = room,
                    Year = year ?? DateTime.Today.Year,
                    Month = month ?? DateTime.Today.Month,
                });
        }
    }
}
