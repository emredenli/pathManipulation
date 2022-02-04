using Microsoft.AspNetCore.Mvc;
using pathManApp.Models;
using System.Diagnostics;
using System.Text;

namespace pathManApp.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment environment;

        public HomeController(IWebHostEnvironment _environment)
        {
            environment = _environment;
        }

        public IActionResult Index()
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

        //Path Manipulation zafiyetin olduğu kod bloğunu "PathManipulationActive" action altına yazdım.
        //Projeyi çalıştırıp localhost sonrası
        //(~/Home/PathManipulationActive?fileName=photo.jpg) yolumuzu yazıp kod bloğunun çalışmasını bekliyoruz.
        //Bu action altında yapılan işlemler sırasıyla şu şekilde:
        //Projenin dosya yolunu bulup "contentPath" içerisine gönderiyor.
        //("D:\\Proje\\pathManApp\\pathManApp\\")
        //"contentPath" içerisindeki yolu "Files\\Images" ile birleştirip "knownPath" içerisine gönderiyor.
        //("D:\\Proje\\pathManApp\\pathManApp\\Files\\Images")
        //"knownPath" ve parametre olarak gönderdiğimiz "fileName" birleştirip "filePath" içerisine gönderiyor.
        //("D:\\Proje\\pathManApp\\pathManApp\\Files\\Images\\photo.jpg")
        //Son olarak return içerisine girip download işlemini yapıyor.
        //!Burada zafiyetin oluşma sebebiyse dosya yolunu işledikten sonra hiçbir şekilde kontrol etmiyor oluşumuz.
        //!Kontrol edemediğimiz için "Files" altındaki tüm dosyalar saldırganlar tarafından erişime açılmış oluyor.
        //!(~/Home/PathManipulationActive?fileName=../Documents/image.jpg) yolunu kullanarak
        //Documents klasörünede erişim sağlamış oluyorlar.
        public IActionResult PathManipulationActive(string fileName)
        {
            //projenin yolu
            string contentPath = this.environment.ContentRootPath;
            
            String knownPath = Path.Combine(contentPath, "Files\\Images");
            string filePath = Path.Combine(knownPath, fileName);

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        //Path Manipulation zafiyetinin çözüldüğü kod bloğunu "PathManipulationPassive" action altına yazdım.
        //Projeyi çalıştırıp localhost sonrası
        //(~/Home/PathManipulationPassive?fileName=photo.jpg) yolumuzu yazıp kod bloğunun çalışmasını bekliyoruz.
        //Bu action altında yapılan işlemler sırasıyla şu şekilde:
        //Projenin dosya yolunu bulup "contentPath" içerisine gönderiyor.
        //("D:\\Proje\\pathManApp\\pathManApp\\")
        //"contentPath" içerisindeki yolu "Files\\Images" ile birleştirip "knowPath" içerisine gönderiyor.
        //("D:\\Proje\\pathManApp\\pathManApp\\Files\\Images")
        //DirectoryInfo satırına gelince "knowPath"'in gittiği yolu doğrulayıp "di" içerisine gönderiyoruz.
        //"knownPath" ve parametre olarak gönderdiğimiz "fileName" birleştirip "filePath" içerisine gönderiyor.
        //("D:\\Proje\\pathManApp\\pathManApp\\Files\\Images\\photo.jpg")
        //"controlFilename" ile "filePath" içerisindeki yoldaki istenilen dosyanın adını alıyoruz.
        //"controlFilename" deki ismi search edip ilk bulduğunu "di"nin sonuna ekleyip "files" dizisine gönderiyoruz.
        //Eğer path verdiyse onu da Getfileinfo ile kontrol ediyoruz.
        //Dizide eleman varsa download işlemini yapıyoruz, yoksa null değer döndürüyoruz.
        public FileResult PathManipulationPassive(string fileName)
        {
            //projenin yolu
            string contentPath = this.environment.ContentRootPath;

            String knownPath =  Path.Combine(contentPath, "Files\\Images");
            DirectoryInfo di = new DirectoryInfo(knownPath);
            string filePath = Path.Combine(knownPath, fileName);

            string controlFilename = Path.GetFileName(filePath);

            FileInfo[] files = di.GetFiles(controlFilename, SearchOption.TopDirectoryOnly);

            if (files.Length > 0)
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            else
            {
                return null;
            }
        }

    }
}