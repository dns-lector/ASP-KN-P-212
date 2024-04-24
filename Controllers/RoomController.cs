﻿using ASP_KN_P_212.Data.DAL;
using ASP_KN_P_212.Data.Entities;
using ASP_KN_P_212.Models.Content.Location;
using ASP_KN_P_212.Models.Content.Room;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_KN_P_212.Controllers
{
    [Route("api/room")]
    [ApiController]
    public class RoomController(DataAccessor dataAccessor, ILogger<RoomController> logger) : ControllerBase

    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<RoomController> _logger = logger;

        [HttpGet("all/{id}")]
        public List<Room> GetRooms(String id)
        {
            // var location = _dataAccessor.ContentDao.GetLocationBySlug(id);
            List<Room> rooms;
            {
                rooms = _dataAccessor.ContentDao.GetRooms(id);
            }
            return rooms;
        }

        [HttpGet("{id}")]
        public Room? GetRoom([FromRoute] String id/*, 
            [FromQuery] int? year, [FromQuery] int? month*/)
        {
            var room = _dataAccessor.ContentDao.GetRoomBySlug(id);
            if(room == null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }
            room.Reservations = room.Reservations
                .Where(r => r.Date >= DateTime.Today).ToList();

            // room.Reservations.ForEach(r => { r.Room = null!; r.User = null!; });

            return room;
        }

        [HttpPost]
        public String DoPost(RoomFormModel model)
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
                if (fileName == null)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return "File Image required";
                }
                _dataAccessor.ContentDao.AddRoom(
                    name: model.Name,
                    description: model.Description,
                    photoUrl: fileName,
                    slug: model.Slug,
                    locationId: model.LocationId,
                    stars: model.Stars,
                    dailyPrice: model.DailyPrice);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Added";
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return ex.Message;
            }            
        }

        [HttpPost("reserve")]
        public String ReserveRoom([FromBody] ReserveRoomFormModel model)
        {
            try
            {
                _dataAccessor.ContentDao.ReserveRoom(model);
                Response.StatusCode = StatusCodes.Status201Created;
                return "Reserved";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return ex.Message;
            }
        }
    }
}
/*
 Д.З. Забезпечити валідацію даних форми, що надходять до 
    RoomController::DoPost
*/