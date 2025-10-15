using altasplato_satinalma.Data;
using altasplato_satinalma.Models;
 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 using System.Diagnostics;
using Newtonsoft.Json.Linq;


using System.Xml.Linq;

namespace altasplato_satinalma.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AltasPlatoDbContext _db;
        private readonly string API_KEY = "7bc0d1b4855f4faa8ea7bdc8be965bac"; // kendi API keyini buraya yaz
        private readonly string symbol = "ALMMF";       // Alüminyum sembolü


        public HomeController(ILogger<HomeController> logger, AltasPlatoDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {

            return View();
        }


        public IActionResult Plato()
        {

            var bugun = DateTime.Today;

            // UserBirthday null olmayan ve doğum günü bugün olan çalışanları çekiyoruz
            var dogumGunuOlanlar = _db.CalisanBilg
               .Where(c => c.UserBirthday.HasValue &&
                            c.UserBirthday.Value.Month == bugun.Month &&
                            c.UserBirthday.Value.Day == bugun.Day)
                .ToList();

            // Mesajları oluşturuyoruz
            var mesajlar = dogumGunuOlanlar.Select(c =>
            {
                var isim = !string.IsNullOrEmpty(c.UserName) && !string.IsNullOrEmpty(c.UserLastname)
                   ? $"{c.UserName} {c.UserLastname}"
                   : c.UserId;
                return $"Mutlu Yıllar {isim}! İyi ki doğdun!";
            }).ToList();

            ViewBag.DogumGunuMesajlari = mesajlar;
            KurCek();
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult KurCek()
        {
            string tcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
            decimal dolarKuru = 0;
            decimal euroKuru = 0;
            //string sonGuncellemeTarihi = string.Empty;


            try
            {
                XDocument xml = XDocument.Load(tcmbUrl);
                var dolarElement = xml.Descendants("Currency")
                                      .FirstOrDefault(x => x.Attribute("CurrencyCode").Value == "USD");
                var euroElement = xml.Descendants("Currency")
                                      .FirstOrDefault(x => x.Attribute("CurrencyCode").Value == "EUR");
                //var tarihElement = xml.Descendants("Tarih").FirstOrDefault();

                if (dolarElement != null && euroElement != null)
                {
                    dolarKuru = Convert.ToDecimal(dolarElement.Element("ForexSelling").Value) / 10000;
                    euroKuru = Convert.ToDecimal(euroElement.Element("ForexSelling").Value) / 10000;
                    //sonGuncellemeTarihi = DateTime.ParseExact(tarihElement.Value, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd MMMM yyyy");

                }
                else
                {
                    throw new Exception("Güncel kur bilgileri alınamadı.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Kur bilgileri alınamadı: " + ex.Message;
            }

            ViewBag.DolarKuru = dolarKuru.ToString("N4") + " TL";
            ViewBag.EuroKuru = euroKuru.ToString("N4") + " TL";
            //ViewBag.SonGuncellemeTarihi = sonGuncellemeTarihi;
            var date = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy");

            ViewBag.Date = date;



            return View();

        }

        public async Task<IActionResult> lme()
        {
           

            return View();
        }

    }
}       





