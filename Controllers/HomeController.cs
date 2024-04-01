using ASP_KN_P_212.Models;
using ASP_KN_P_212.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_KN_P_212.Controllers
{
    public class HomeController : Controller
    {
        /* Інжекція сервісів (залежностей) - запит у контейнера
        на передачу посилань на відповідні об'єкти. Найбільш
        рекомендований спосіб інжекції - через конструктор. Це 
        дозволяє, по-перше, оголосити поля як незмінні (readonly) та,
        по-друге, унеможливити створення об'єктів без передачі
        залежностей. У стартовому проєкті інжекція демонструється 
        на сервісі логування (_logger)
        */
        private readonly ILogger<HomeController> _logger;
        // створюємо поле для посилання на сервіс
        private readonly IHashService _hashService;

        // додаємо до конструктора параметр-залежність і зберігаємо її у тілі
        public HomeController(ILogger<HomeController> logger, IHashService hashService)
        {
            _logger = logger;           // Збереження переданих залежностей, що їх
            _hashService = hashService; // передає контейнер при створенні контролера
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }
        public IActionResult Ioc()  // Inversion of Control
        {
            // користуємось інжектованим сервісом
            ViewData["hash"] = _hashService.Digest("Hello, World!");
            // ViewData - спеціальний об'єкт для передачі даних
            // до представлення. Його ключі на кшталт ["hash"]
            // можна створювати з довільними назвами
            return View();
        }

        public IActionResult AboutRazor()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
