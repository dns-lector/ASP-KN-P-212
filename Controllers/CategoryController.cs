using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using ASP_KN_P_212.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/category")]
    [ApiController]
    // IActionFilter - засоби для дій, що виконуються до та після Action-ів
    public class CategoryController : ControllerBase, IActionFilter
    {
        private readonly DataAccessor _dataAccessor;
        private readonly ILogger<CategoryController> _logger;
        private bool isAuthenticated;
        private bool isAdmin;
        public CategoryController(DataAccessor dataAccessor, ILogger<CategoryController> logger)
        {
            _dataAccessor = dataAccessor;
            _logger = logger;
        }

        // метод IActionFilter, що виконується ДО дій контролера (DoGet, DoPost,...)
        [NonAction]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            /* У проєкті є дві авторизації: через сесії та через токени. 
               Первинна авторизація за сесією (в силу того, що з неї починали)
               Дані авторизації за токеном шукаємо за типом авторизації, яку ми
               встановили як назва класу (AuthTokenMiddleware)
            */
            // по-перше шукаємо схему з AuthSessionMiddleware
            var identity = User.Identities
                .FirstOrDefault(i => i.AuthenticationType == nameof(AuthSessionMiddleware));
            
            // якщо не знаходимо, то шукаємо з AuthTokenMiddleware
            identity ??= User.Identities
                .FirstOrDefault(i => i.AuthenticationType == nameof(AuthTokenMiddleware));

            this.isAuthenticated = identity != null;

            String? userRole = identity?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            this.isAdmin = "Admin".Equals(userRole);
        }

        private String? getAdminAuthMessage()
        {
            if (!isAuthenticated)
            {
                // якщо авторизація не пройдена, то повідомлення в Items
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return HttpContext.Items[nameof(AuthTokenMiddleware)]?.ToString() ?? "Auth required";
            }
            if (!isAdmin)
            {
                Response.StatusCode = StatusCodes.Status403Forbidden;
                return "Access to API forbidden";
            }
            return null;
        }

        [HttpGet]
        public List<Category> DoGet()
        {
            return _dataAccessor.ContentDao.GetCategories(includeDeleted: isAdmin);
        }

        [HttpPost]
        public String DoPost([FromForm] CategoryPostModel model)
        {
            if (getAdminAuthMessage() is String msg) return msg;

            try
            {
                String? fileName = null;
                if (model.Photo != null)
                {
                    String ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/img/content/";
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while (System.IO.File.Exists(pathName));
                    using var stream = System.IO.File.OpenWrite(pathName);
                    model.Photo.CopyTo(stream);
                }
                _dataAccessor.ContentDao
                    .AddCategory(model.Name, model.Description, fileName, model.Slug);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Ok";
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Error";
            }
        }

        [HttpPut]  // Update category
        public String DoPut([FromForm] CategoryPostModel model)
        {
            if (getAdminAuthMessage() is String msg) return msg;

            // перевіряємо CategoryId на наявність
            if (model.CategoryId == null || model.CategoryId == default(Guid))
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return "Missing required parameter: 'category-id'";
            }
            // перевіряємо чи є взагалі така категорія
            Category? category = _dataAccessor.ContentDao.GetCategoryById(model.CategoryId.Value);
            if (category == null)
            {
                Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                return $"Parameter 'category-id' ({model.CategoryId.Value}) belongs to no entity";
            }
            // Оновлюємо дані за принципом: якщо даних немає, то залишаються попередні значення
            if (!String.IsNullOrEmpty(model.Name)) category.Name = model.Name;
            if (!String.IsNullOrEmpty(model.Description)) category.Description = model.Description;
            if (!String.IsNullOrEmpty(model.Slug)) category.Slug = model.Slug;
            if (model.Photo != null)   // передається новий файл - зберігаємо новий, видаляємо старий
            {
                try
                {
                    String? fileName = null;
                    String ext = Path.GetExtension(model.Photo.FileName);
                    String path = Directory.GetCurrentDirectory() + "/wwwroot/img/content/";
                    String pathName;
                    do
                    {
                        fileName = Guid.NewGuid() + ext;
                        pathName = path + fileName;
                    }
                    while (System.IO.File.Exists(pathName));
                    using var stream = System.IO.File.OpenWrite(pathName);
                    model.Photo.CopyTo(stream);
                    // новий файл успішно завантажений - видаляємо старий (якщо є)
                    if (!String.IsNullOrEmpty(category.PhotoUrl))
                    {
                        try { System.IO.File.Delete(path + category.PhotoUrl); }
                        catch { _logger.LogWarning(category.PhotoUrl + " not deleted"); }
                    }
                    // зберігаємо нове ім'я 
                    category.PhotoUrl = fileName;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.Message);
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return "Error uploading file";
                }
            }
            _dataAccessor.ContentDao.UpdateCategory(category);
            Response.StatusCode = StatusCodes.Status200OK;
            return "Updated";
        }

        [HttpDelete("{id}")]
        public String DoDelete(Guid id)
        {
            if (getAdminAuthMessage() is String msg) return msg;

            _dataAccessor.ContentDao.DeleteCategory(id);
            Response.StatusCode = StatusCodes.Status202Accepted;
            return "Ok";
        }

        // метод, НЕ позначений атрибутом, буде викликано, якщо не знайдеться
        // необхідний з позначених. Це дозволяє прийняти нестандартні запити
        public Object DoOther()
        {
            // дані про метод запиту - у Request.Method
            if (Request.Method == "RESTORE")
            {
                return DoRestore();
            }
            Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return "Method Not Allowed";
        }
        // Другий НЕ позначений метод має бути private щоб не було конфлікту
        private String DoRestore()
        {
            if (getAdminAuthMessage() is String msg) return msg;

            // Через відсутність атрибутів, автоматичного зв'язування параметрів
            // немає, параметри дістаємо з колекцій Request
            String? id = Request.Query["id"].FirstOrDefault();
            try
            {
                _dataAccessor.ContentDao.RestoreCategory( Guid.Parse(id!) );
            }
            catch
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return "Empty or invalid id";
            }
            Response.StatusCode = StatusCodes.Status202Accepted;
            return "RESTORE OK for id = " + id;
        }

        // метод IActionFilter, що виконується ПІСЛЯ дій контролера (DoGet, DoPost,...)
        [NonAction]  // Якщо неможна зробити метод private, то позначаємо [NonAction]
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }
    }
    public class CategoryPostModel
    {
        [FromForm(Name = "category-name")]
        public String Name { get; set; } = null!;


        [FromForm(Name = "category-description")]
        public String Description { get; set; } = null!;


        [FromForm(Name = "category-slug")]
        public String Slug { get; set; } = null!;


        [FromForm(Name = "category-photo")]
        public IFormFile? Photo { get; set; }


        [FromForm(Name = "category-id")]
        public Guid? CategoryId { get; set; }
    }
}
/* Д.З. Реалізувати адмін-інструменти для Location 
 * (за зразком з Category)
 * - додавання нової локації
 * - редагування наявної
 * - видалення (м'яке)
 * - відновлення видаленої
 * ! усі засоби доступні лише користувачам з роллю адміністратора
 * 
 */