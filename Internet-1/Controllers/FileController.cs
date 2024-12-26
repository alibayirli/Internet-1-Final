using Internet_1.Models;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Internet_1.Hubs;

namespace Internet_1.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        public readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly INotyfService _notyf;
        private readonly IHubContext<GeneralHub> _hubContext;

        public FileController(AppDbContext context, IHubContext<GeneralHub> hubContext, IConfiguration configuration, INotyfService notyf)
        {
            _context = context;
            _hubContext = hubContext;
            _configuration = configuration;
            _notyf = notyf;
        }
        [HttpGet]

        [HttpGet]
        public async Task<IActionResult> GetFileCount()
        {
            var count = await _context.SystemFiles.CountAsync();
            return Json(count);
        }
        public async Task<IActionResult> Index(FileUploadViewModel vm)
        {
            string currentUser = User.Identity.Name;

            // Sadece oturum açmış kullanıcının dosyalarını getir
            vm.SystemFiles = await _context.SystemFiles
                                           .Where(f => f.UploadedBy == currentUser)
                                           .ToListAsync();

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile(FileUploadViewModel vm, IFormFile file)
        {
            if (file != null)
            {
                var filename = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + file.FileName;
                var path = $"{_configuration.GetSection("FileManagement:SystemFileUploads").Value}";
                var filepath = Path.Combine(path, filename);
                var fileextention = Path.GetExtension(filename);

                // Fiziksel dosyayı kaydet
                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Kullanıcı bilgilerini ekle
                var uploadfile = new FileModal
                {
                    UploadOn = DateTime.Now,
                    UploadedBy = User.Identity.Name, // Oturum açmış kullanıcının adı
                    FileType = file.ContentType,
                    FileName = filename,
                    Description = vm.Description,
                    FilePath = filepath,
                    Extension = fileextention,
                };

                await _context.AddAsync(uploadfile);
                await _context.SaveChangesAsync();

                // SignalR ile dosya sayısını güncelle
                var fileCount = await _context.SystemFiles.CountAsync();
                await _hubContext.Clients.All.SendAsync("onFileCountUpdate", fileCount);


                _notyf.Success("Dosya Eklendi...");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.SystemFiles.FindAsync(id);
            if (file == null || file.UploadedBy != User.Identity.Name) // Kullanıcı kontrolü
            {
                return Forbid(); // Yetkisiz erişim engellendi
            }

            // Fiziksel dosyayı sil
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }

            // Veritabanından sil
            _context.SystemFiles.Remove(file);
            await _context.SaveChangesAsync();

            // SignalR ile dosya sayısını güncelle
            var fileCount = await _context.SystemFiles.CountAsync();
            await _hubContext.Clients.All.SendAsync("onFileCountUpdate", fileCount);

            _notyf.Success("Dosya Silindi...");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(int id)
        {
            var file = await _context.SystemFiles.FindAsync(id);
            if (file == null || file.UploadedBy != User.Identity.Name) // Kullanıcı kontrolü
            {
                return Forbid(); // Yetkisiz erişim engellendi
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                TempData["Error"] = "Dosya bulunamadı.";
                return RedirectToAction("Index");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.FileType, file.FileName);
        }
    }
}
