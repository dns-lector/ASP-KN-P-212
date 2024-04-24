using ASP_KN_P_212.Data;
using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Models;
using ASP_KN_P_212.Models.Home.FrontendForm;
using ASP_KN_P_212.Models.Home.Ioc;
using ASP_KN_P_212.Models.Home.Signup;
using ASP_KN_P_212.Services.Hash;
using ASP_KN_P_212.Services.Kdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        // інжекція контексту даних - така ж за формою, як інші сервіси
        private readonly DataContext _dataContext;
        private readonly DataAccessor _dataAccessor;
        private readonly IKdfService _kdfService;

        // додаємо до конструктора параметр-залежність і зберігаємо її у тілі
        public HomeController(ILogger<HomeController> logger, IHashService hashService, DataContext dataContext, DataAccessor dataAccessor, IKdfService kdfService)
        {
            _logger = logger;           // Збереження переданих залежностей, що їх
            _hashService = hashService; // передає контейнер при створенні контролера
            _dataContext = dataContext;
            _dataAccessor = dataAccessor;
            _kdfService = kdfService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Intro()
        {
            return View();
        }
        public IActionResult Data()
        {
            ViewData["users-count"] = _dataContext.Users.Count();
            return View();
        }
        public IActionResult Ioc(String? format)  // Inversion of Control
        {
            IocPageModel pageModel = new()
            {
                Title = "Декілька випадкових дайджестів",
                SingleHash = _hashService.Digest("Hello, World!")
            };
            for (int i = 0; i < 5; i++)
            {
                String str = (i + 100500).ToString();
                pageModel.Hashes[str] = _hashService.Digest(str);
            }
            if(format == "json")
            {
                return Json(pageModel);
            }
            return View(pageModel);
        }
        public IActionResult AboutRazor()
        {
            return View();
        }

        public ViewResult Admin()
        {
            return View();
        }

        // Модель форми зазначається параметром методу, заповнення-автоматичне
        public IActionResult Model(Models.Home.Model.FormModel? formModel)
        {
            // Модель представлення створюється і заповнюється самостійно
            Models.Home.Model.PageModel pageModel = new()
            {
                TabHeader = "Моделі",
                PageTitle = "Моделі в ASP",
                FormModel = formModel
            };
            // модель представлення передається аргументом View()
            return View(pageModel);
        }

        [HttpPost]  // атрибут дозволяє запит лише POST-методом
        public JsonResult FrontendForm([FromBody] FrontendFormInput input)
        {
            FrontendFormOutput output = new()
            {
                Code = 200,
                Message = $"{input.UserName} -- {input.UserEmail}"
            };
            _logger.LogInformation(output.Message);
            return Json(output);
        }

        public IActionResult Signup(SignupFormModel? formModel)
        {
            SignupPageModel pageModel = new()
            {
                FormModel = formModel
            };
            if(formModel?.HasData ?? false)
            {
                pageModel.ValidationErrors = _ValidateSignupModel(formModel);
                if(pageModel.ValidationErrors.Count == 0)
                {   // Немає помилок валідації - реєструємо користувача
                    String salt = Guid.NewGuid().ToString();
                    _dataAccessor.UserDao.Signup(new()
                    {
                        Name = formModel.UserName,
                        Email = formModel.UserEmail,
                        Birthdate = formModel.UserBirthdate,
                        AvatarUrl = formModel.SavedAvatarFilename,
                        Salt = salt,
                        DerivedKey = _kdfService.DerivedKey(salt, formModel.Password)
                    });
                }
            }
            // _logger.LogInformation(Directory.GetCurrentDirectory());
            return View(pageModel);
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

        private Dictionary<string, string> _ValidateSignupModel(SignupFormModel? model)
        {
            Dictionary<string, string> result = new();

            if (model == null)
            {
                result["model"] = "Model is null";
            }
            else
            {
                if (String.IsNullOrEmpty(model.UserName))
                {
                    result[nameof(model.UserName)] = "User Name should not be empty";
                }
                if (String.IsNullOrEmpty(model.UserEmail))
                {
                    result[nameof(model.UserEmail)] = "User Email should not be empty";
                }
                if (model.UserBirthdate == default(DateTime))
                {
                    result[nameof(model.UserBirthdate)] = "User Birthdate should not be empty";
                }
                if (model.UserAvatar != null)
                {
                    // є файл, аналізуємо його 
                    // Дізнаємось розширення файлу:
                    int dotPosition = model.UserAvatar.FileName.LastIndexOf('.');
                    if (dotPosition == -1)
                    {
                        result[nameof(model.UserAvatar)] = "File without extension not allowed";
                    }
                    String ext = model.UserAvatar.FileName[dotPosition..];
                    // Перевірити розширення на графічні файли.
                    // ...
                    // _logger.LogInformation(ext);
                    // зберігаємо файл у /wwwroot/img/avatars з новим іменем
                    // (передані імена залишати не варто)
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/img/avatars/";
                    _logger.LogInformation(path);
                    String fileName;
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while(System.IO.File.Exists(pathName));

                    using var stream = System.IO.File.OpenWrite(pathName);
                    model.UserAvatar.CopyTo(stream);

                    model.SavedAvatarFilename = fileName;
                }
            }
            return result;
        }
    }
}
/* Д.З. Впровадити управління контентом у власний курсовий проєкт
 * Реалізувати засоби створення (додавання) та розмежування 
 * контенту за належністю (авторством) -- можливості власника
 * контенту відрізняються від інших користувачів
 *   
 */
