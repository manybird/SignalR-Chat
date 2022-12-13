using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Chat.Web.Data;
using Chat.Web.Helpers;
using Chat.Web.Hubs;
using Chat.Web.Models;
using Chat.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Chat.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBaseExt
    {
        //private readonly int FileSizeLimit;
        //private readonly string[] AllowedExtensions;
        //private readonly ApplicationDbContext _context;
        //private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        //private readonly IHubContext<ChatHub> _hubContext;
        private readonly IFileValidator _fileValidator;

        public UploadController(ApplicationDbContext context,
            IMapper mapper,
            IWebHostEnvironment environment,
            IHubContext<ChatHub> hubContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configruation,
            IFileValidator fileValidator) : base(context, mapper, hubContext,userManager,roleManager)
        {
            //_context = context;
            //_mapper = mapper;
            _environment = environment;
            //_hubContext = hubContext;
            _fileValidator = fileValidator;
            
        }

        private async Task<IActionResult> UploadAction([FromForm] UploadViewModel uploadViewModel, ApplicationUser user)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (!_fileValidator.IsValid(uploadViewModel.File))
                return BadRequest("Validation failed!");

            var room = _context.Rooms.Where(r => r.Id == uploadViewModel.RoomId).FirstOrDefault();
            if (user == null || room == null)
                return NotFound();

            var now = DateTime.Now;
            var oFileName = uploadViewModel.File.FileName;
            var fileName = now.ToString("HHmmssf") + "_" + Path.GetFileName(oFileName);

            var roomName = string.Join('_', room.Name.Split(Path.GetInvalidPathChars(), StringSplitOptions.RemoveEmptyEntries));

            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", roomName, now.ToString("yyyyMM"));
            var fileFullPath = Path.Combine(folderPath, fileName);

            var relativePath = fileFullPath.Replace(_environment.WebRootPath, PathBase).Replace(Path.DirectorySeparatorChar, '/');

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            using (var fileStream = new FileStream(fileFullPath, FileMode.Create))
            {
                await uploadViewModel.File.CopyToAsync(fileStream);
            }

            var ext = (Path.GetExtension(fileFullPath) ?? "").ToLower();

            string iconPath = _getImageIcons(ext, relativePath);
            string htmlImage = string.Format(
                "<a href='{0}' target='_blank'>" +
                "<img src='{1}' class='post-image'>" +
                "<div>{2}</div></a>", relativePath, iconPath,oFileName);

            var message = new Message()
            {
                Content = Regex.Replace(htmlImage, @"(?i)<(?!img|a|/a|div|/div|/img).*?>", string.Empty),
                Timestamp = DateTime.Now,
                FromUser = user,
                ToRoom = room,
                MessageType = 1,
                FileFullPath = fileFullPath,
                RelativePath = relativePath,
                CaseId = uploadViewModel.CaseId,
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            // Send image-message to group
            var messageViewModel = _mapper.Map<Message, MessageViewModel>(message);
            await _hubContext.Clients.Group(room.Name).SendAsync("newMessage", messageViewModel);

            return Ok();
        }
        private string PathBase { get { return HttpContext.Request.PathBase; } }
        private string _getImageIcons(string ext, string relativePath)
        {
            string iconPath = relativePath;
            var p = PathBase;
            if (string.Equals(ext, ".pdf"))
            {
                iconPath =p + "/images/file-download-64.png";
            }
            else if (string.Equals(ext, ".jpg") || string.Equals(ext, ".jpeg") || string.Equals(ext, ".png"))
            {
                //imgPath = relativePath;
                iconPath = p + "/images/image-64.png";
            }
            else
            {
                iconPath = p + "/images/file-download-64.png";
            }

            return iconPath;
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] UploadViewModel uploadViewModel)
        {
            var user = await base.GetUserByName(User.Identity.Name);
            return await UploadAction(uploadViewModel, user);
        }
        
        [AllowAnonymous]
        [HttpPost("ByAgent/{na1ta}")]
        public async Task<IActionResult> Upload([FromForm] UploadViewModel uploadViewModel, string na1ta)
        {
            var user = await base.GetUserByName(na1ta);
            return await UploadAction(uploadViewModel, user);
        }
    }
}
