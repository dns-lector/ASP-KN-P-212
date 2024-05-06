﻿using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly DataAccessor _dataAccessor;
        public CategoryController(DataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        [HttpGet]
        public List<Category> DoGet()
        {
            return _dataAccessor.ContentDao.GetCategories();
        }

        [HttpPost]
        public String DoPost([FromForm] CategoryPostModel model)
        {
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
    }
}
