using AspNetCoreHero.ToastNotification.Notyf.Models;
using AutoMapper;
using Internet_1.Models;
using Internet_1.Repositories;
using Internet_1.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using NETCore.Encrypt.Extensions;
using System.Diagnostics;
using System.Security.Claims;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Internet_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly INotyfService _notyf;
        private readonly IFileProvider _fileProvider;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        public readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, AppDbContext context, IMapper mapper, IConfiguration config, INotyfService notyf, IFileProvider fileProvider, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _mapper = mapper;
            _config = config;
            _notyf = notyf;
            _fileProvider = fileProvider;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(FileUploadViewModel vm)
        {
            //vm.SystemFiles = await _context.SystemFiles.ToListAsync();

            //return View(vm);
            // Oturum açmış kullanıcıyı al
            string currentUser = User.Identity.Name;

            // Sadece oturum açmış kullanıcının dosyalarını getir
            vm.SystemFiles = await _context.SystemFiles
                                           .Where(f => f.UploadedBy == currentUser)
                                           .ToListAsync();

            return View(vm);
        }
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
            _notyf.Success("Dosya Silindi...");

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                _notyf.Error("Girilen Kullanıcı Adı Kayıtlı Değildir!");
                return View(model);
            }
            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.KeepMe, true);

            if (signInResult.Succeeded)
            {

                await _userManager.AddClaimAsync(user, new Claim("PhotoUrl", user.PhotoUrl));
                return RedirectToAction("Index", "Admin");
            }
            if (signInResult.IsLockedOut)
            {
                _notyf.Error("Kullanıcı Girişi " + user.LockoutEnd + " kadar kısıtlanmıştır!");

                return View(model);
            }
            _notyf.Error("Geçersiz Kullanıcı Adı veya Parola Başarısız Giriş Sayısı :" + await _userManager.GetAccessFailedCountAsync(user) + "/3");
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var identityResult = await _userManager.CreateAsync(new() { UserName = model.UserName, Email = model.Email, FullName = model.FullName, PhotoUrl = "no-img.png" }, model.Password);

            if (!identityResult.Succeeded)
            {
                foreach (var item in identityResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);


                    _notyf.Error(item.Description);
                }

                return View(model);
            }

            // default olarak Uye rolü ekleme
            var user = await _userManager.FindByNameAsync(model.UserName);
            var roleExist = await _roleManager.RoleExistsAsync("Uye");
            if (!roleExist)
            {
                var role = new AppRole { Name = "Uye" };
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, "Uye");

            _notyf.Success("Üye Kaydı Yapılmıştır. Oturum Açınız");
            return RedirectToAction("Login");
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        public IActionResult AccessDenied()
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