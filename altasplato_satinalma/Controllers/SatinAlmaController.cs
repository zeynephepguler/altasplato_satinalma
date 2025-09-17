using altasplato_satinalma.Data;
using altasplato_satinalma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text;
using altasplato_satinalma.Services;

namespace altasplato_satinalma.Controllers
{
   
    public class SatinAlma : Controller
    {
        private readonly AltasPlatoDbContext _db;
        private readonly PdfService _pdfService;

        public SatinAlma(AltasPlatoDbContext db) { _db = db; _pdfService = new PdfService();
        }

        //public IActionResult _SASidebar()
        //{
        //    int okunmayan = _db.Bildirimler.Count(b => b.Okundu == false);

        //    return View(model: okunmayan); // int model olarak gönderiliyor
        //}



        public IActionResult UrunGecmis(string UrunId) 
        {
            var parts = UrunId.Split('-');
            var normalized = string.Join("-", parts.Skip(1));

            var depo =_db.SatinAlmaDepoUrun_Es.Where(t=>t.UrunId.Equals(UrunId)||t.UrunNormalizedId.Equals(normalized)).Select(t=>t.DepoId).ToList();
            var depoList = new List<SatinAlmaDepos>();

            foreach (var item in depo) {

                var depoes = _db.satinAlmaDepos.FirstOrDefault(t => t.Id.ToString() == item);

                if (depoes != null)
                {
                    var ted = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.Id == depoes.TedarikciId);

                    depoes.Kategori = ted != null && !string.IsNullOrEmpty(ted.TedarikciAdi)
                        ? ted.TedarikciAdi
                        : "Sayim";

                    depoList.Add(depoes);
                }


            }
            return View(depoList);
        
        }
        [HttpPost]
        public JsonResult TeklifKapat(int id)
        {
            // FormId'yi alırız
            var form = _db.MalzemeTalep.FirstOrDefault(t => t.FormId == id);

            if (form == null)
            {
                // Eğer form bulunamazsa hata mesajı döndür
                return Json(new { success = false, message = "Form bulunamadı!" });
            }

            // Durumu "Teklif Bekleniyor" olarak güncelleriz
            form.Durum = "Sipariş Verildi";

            // Veritabanına kaydedilir
            _db.SaveChanges();

            // Başarı mesajı iletilir
            return Json(new { success = true, message = "Durum başarıyla güncellendi: Teklif Bekleniyor." });
        }


        [HttpPost("convert")]
        public async Task<IActionResult> ConvertHtmlToPdf([FromForm] string htmlContent, IFormFile pdf, string dosyaAdi)
        {
            try
            {
                // Dosya adı eksikse hata mesajı döndür
                if (string.IsNullOrEmpty(dosyaAdi))
                {
                    TempData["Error"] = "Dosya Formlara Kaydedilmedi: DOSYA ADI EKSİK";
                    return View("Mesaj");
                }

                // HTML içeriğini PDF'ye dönüştür
                var outputPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "output.pdf");
                _pdfService.ConvertHtmlToPdf(htmlContent, outputPdfPath);

                // Dönüştürülen PDF dosyasını döndür
                var pdfBytes = System.IO.File.ReadAllBytes(outputPdfPath);

                // Eğer PDF dosyası yüklenmişse, onu veritabanına kaydet
                if (pdf != null && pdf.Length > 0)
                {
                    // IFormFile'dan byte dizisine dönüştür
                    byte[] pdfFileBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await pdf.CopyToAsync(memoryStream);
                        pdfFileBytes = memoryStream.ToArray();
                    }

                    // PDF'yi veritabanına kaydet
                    var yeniDosya = new SiparisTalepPdf
                    {
                        DosyaAdi = dosyaAdi + ".pdf",  // Dosya adını istediğiniz şekilde düzenleyebilirsiniz
                        DosyaVerisi = pdfFileBytes
                    };

                    _db.SiparisTalepPdf.Add(yeniDosya);
                    await _db.SaveChangesAsync();
                }

                // PDF dosyasını döndür
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }

        public IActionResult GetSiparisData()
        {
            var siparisler = _db.MalzemeTalep.Select(m => new
            {
                m.Id,
                TerminTarihi = m.TerminTarihi.HasValue
                    ? m.TerminTarihi.Value.ToString("dd-MM-yyyy")
                    : null,  // Ensures null safety
                m.MalzemeAdi,
                m.TalepMiktari,
                m.MevcutStok,
                m.Aciklama,
                m.Durum
            }).ToList();

            return Json(siparisler);
        }


        public IActionResult Index() {
            try
            {
                TreeColor treeColorService = new TreeColor();
                var rowsToNotify = treeColorService.CheckAmbalajTuru();

                if (rowsToNotify != null && rowsToNotify.Any())
                {
                    var today = DateTime.Now.Date;
                    var sonGt = rowsToNotify.Where(row => DateTime.TryParse(row.SonGirisTarih, out DateTime sonGirisTarih) && sonGirisTarih.Date == today).ToList();
                    var rowsToDisplay = sonGt
                        .GroupBy(row => row.KartNo) 
                        .Select(group => group.First()) 
                        .Select(row => new { row.KartNo, row.AmbalajTuru, row.SonGirisTarih }) 
                        .ToList();
                    var bildirimList = new List<Bildirimler>();

                    foreach (var row in rowsToDisplay)
                    {
                        var existingBildirim = _db.Bildirimler
                            .FirstOrDefault(b => b.KartNo.ToString()==row.KartNo);

                        if (existingBildirim == null)
                        {
                            var newBildirim = new Bildirimler
                            {
                                KartNo = Convert.ToInt32((row.KartNo)),
                                Mesaj = "TreeColor"+"-"+row.AmbalajTuru, 
                                OlusturmaTarihi = DateTime.Now, 
                                Okundu = false
                            };

                            //bildirimList.Add(newBildirim);
                            _db.Bildirimler.Add(newBildirim);

                        }
                    }
                    try
                    {
                        _db.SaveChanges(); 
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine($"Error: {ex.Message}");
                    }


                    var bildirim = _db.Bildirimler
                        .Where(t => t.Okundu == false && t.Mesaj.StartsWith("TreeColor"))
                        .ToList()
                        .Count;
                    var stokBildirim = _db.Bildirimler
                        .Where(t => t.Okundu == false && t.Mesaj.StartsWith("Yeni Stok İsteği"))
                        .ToList()
                        .Count;

                    if (bildirim !=null || bildirim > 0)
                    {
                        ViewBag.Bildirim = bildirim;
                    }
                    else
                    {
                        ViewBag.Bildirim = 0;
                    }
                    if (stokBildirim != null || stokBildirim > 0)
                    {
                        ViewBag.StokBildirim = stokBildirim;
                    }
                    else
                    {
                        ViewBag.StokBildirim = 0;
                    }

                    // Veritabanına ekleme
                    //if (bildirimList.Any())
                    //{
                    //    _db.Bildirimler.AddRange(bildirimList); // Bildirimleri veritabanına ekle
                    //    _db.SaveChanges(); // Değişiklikleri kaydet
                    //}
                    //ViewBag.KartNoMessage = rowsToDisplay.Count;
                }

            }
            catch (Exception ex)
            {
                return BadRequest($"Hata oluştu: {ex.Message}");
            }
            return View(); }
        public IActionResult Raporlar()
        {
           
            return View();
        }
        public IActionResult OdemeVeFYonetim()
        {
            var siparisler = _db.satinAlmaSiparisler.ToList();
            var depoList = _db.satinAlmaDepos.ToList(); // DefaultIfEmpty() burada gerekmez
            var formList = _db.Formlar.ToList();        // Aynı şekilde burada da
            var malzemeList = _db.MalzemeTalep.ToList();

            var modelList = new List<SatinAlmaSiparisDepo>();

            foreach (var item in siparisler)
            {
                // İlgili malzeme talebini al
                var ilgiliMalzeme = malzemeList.FirstOrDefault(t => t.Id == item.TalepID);
                var formId = ilgiliMalzeme?.FormId;

                // İlgili formu bul: item.TalepID veya formId eşleşmesi
                var form = formList.FirstOrDefault(f => f.FormNo == item.TalepID || f.FormNo == formId);

                // İlgili depo kaydı
                var depo = depoList.FirstOrDefault(d => d.Id == item.StokID);

                var model = new SatinAlmaSiparisDepo
                {
                    SiparisID = item.SiparisID,
                    TalepID = item.TalepID,
                    UrunAdi = item.UrunAdi,
                    SiparisTarihi = item.SiparisTarihi,
                    OnayDurumu = item.OnayDurumu,
                    Konum = item.Konum,
                    SiparisNotu = item.SiparisNotu,
                    DosyaVerisi = item.DosyaVerisi,
                    TedarikciAdi = item.TedarikciAdi,
                    Aciklama = depo?.Aciklama,
                    Karar = depo?.Karar,
                    TalepEden = form?.TalepEden
                };

                modelList.Add(model);
            }

            return View(modelList);
        }

        public class SatinAlmaSiparisDepo
        {
             
            public int SiparisID { get; set; }

             public int TalepID { get; set; }

             public string UrunAdi { get; set; }
             public string TalepEden { get; set; }
            public string TedarikciAdi { get; set; }

            public int StokID { get; set; }


            public string UrunAdedi { get; set; }
            public string BirimFiyat { get; set; }


             public DateTime SiparisTarihi { get; set; }
            public DateTime SiparisTerminTarihi { get; set; }

 
            public string OnayDurumu { get; set; }

          
            public string Durum { get; set; }

            public string Konum { get; set; }

            public string SiparisNotu { get; set; }
            public string Aciklama { get; set; }
            public string Karar { get; set; }


            public byte[]? DosyaVerisi { get; set; }
        }

        public IActionResult FaturaIndirPdf(int siparisID)
        {
            // Sipariş ID'sine göre veriyi alıyoruz
            var siparis = _db.satinAlmaSiparisler.FirstOrDefault(s => s.SiparisID == siparisID);

            if (siparis != null && siparis.DosyaVerisi != null)
            {
                // Dosya verisini byte array olarak alıyoruz
                var dosyaVerisi = siparis.DosyaVerisi;

                // PDF olarak indirilecek dosyanın MIME türünü belirliyoruz
                return File(dosyaVerisi, "application/pdf", "siparis_" + siparisID + ".pdf");
            }

            // Eğer dosya bulunmazsa hata döndürüyoruz
            return NotFound("Dosya bulunamadı.");
        }
        public IActionResult DosyaGoster(int siparisID)
        {
            var siparis = _db.satinAlmaSiparisler.Find(siparisID);

            if (siparis == null || siparis.DosyaVerisi == null)
                return NotFound();

            // MIME türü tespiti (basit içerik tespitiyle yapılabilir)
            var dosya = siparis.DosyaVerisi;
            string contentType;
            string extension;

            // İlk birkaç byte'a bakarak MIME tespiti
            if (dosya.Take(4).SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 })) // %PDF
            {
                contentType = "application/pdf";
                extension = ".pdf";
            }
            else if (dosya.Take(3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF })) // JPEG
            {
                contentType = "image/jpeg";
                extension = ".jpg";
            }
            else if (dosya.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) // PNG
            {
                contentType = "image/png";
                extension = ".png";
            }
            else
            {
                contentType = "application/octet-stream";
                extension = "";
            }

            return File(dosya, contentType, $"dosya_{siparisID}{extension}");
        }


        [HttpPost]
        public async Task<IActionResult> SiparisEkle([FromForm] SatinAlmaSiparis model, IFormFile dosyaVerisi, string BirimFiyat)
        {
            try
            {
                // 1. Dosya yükleme (PDF ve görsel dosyalar destekleniyor)
                if (dosyaVerisi != null && dosyaVerisi.Length > 0)
                {
                    var extension = Path.GetExtension(dosyaVerisi.FileName).ToLower();
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        return BadRequest("Yalnızca PDF veya görsel (.jpg, .jpeg, .png) dosyaları yüklenebilir.");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await dosyaVerisi.CopyToAsync(memoryStream);
                        model.DosyaVerisi = memoryStream.ToArray();
                     }
                }

                // 2. Sipariş notu boşsa doldur
                if (string.IsNullOrWhiteSpace(model.SiparisNotu))
                    model.SiparisNotu = "-";

                model.BirimFiyat = BirimFiyat;
                model.TedarikciAdi = model.TedarikciAdi?.ToUpper();

                // 3. MalzemeTalep kontrolü
                var form = _db.MalzemeTalep.FirstOrDefault(t => t.FormId == model.TalepID);

                if (form == null)
                {
                    var yeniForm = new Formlar
                    {
                        SirketAdi = "ALTAS",              // Gerekirse modelden alınabilir
                        FormTuru = "Malzeme Talep Formu",
                        TalepEden = "SATIN ALMA",                // Varsa modelden alın
                        Onaylayan = "SATIN ALMA",
                        Tarih = DateTime.Now,
                        Durum = "YENİ"
                    };
                    _db.Formlar.Add(yeniForm);
                    await _db.SaveChangesAsync(); // FormNo şimdi oluşmuş olacak

                    // 3.2 Yeni MalzemeTalep satırı oluştur
                    var yeniTalep = new Malzemeler
                    {
                        FormId=yeniForm.FormNo,
                        MalzemeAdi = model.UrunAdi,              // Gerekirse modelden alınabilir
                        MevcutStok = "0",
                        TalepMiktari = "1",                // Varsa modelden alın
                        TerminTarihi = model.SiparisTerminTarihi,
                        Aciklama = model.SiparisNotu,
                        Durum = "Sipariş Verildi",
                        Urun = true
                    };

                    _db.MalzemeTalep.Add(yeniTalep);
                    await _db.SaveChangesAsync();

                    model.TalepID = yeniTalep.FormId;


                    model.TalepID = yeniTalep.Id; // Yeni ID’yi siparişe yaz
                    
                }
                else
                {
                    // 3.3 Kayıt varsa durumu güncelle
                    form.Durum = "Sipariş Verildi";
                    form.Urun = true;
                }

                // 4. Siparişi ekle
                _db.satinAlmaSiparisler.Add(model);
                await _db.SaveChangesAsync();

                TempData["Message"] = "Sipariş başarıyla eklendi.";
                return View("Mesaj");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }


        public IActionResult SiparisOlustur(int id)
        {
            var model = new SatinAlmaSiparis
            {
                TalepID = id  
            };
            var urunler = _db.satinAlmaUrunBilgileris.ToList();
            foreach (var iten in urunler)
            {
                var tedarikci = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.Id.ToString().Equals(iten.Tedarikci));
                if (tedarikci != null)
                {
                    iten.Tedarikci = tedarikci.TedarikciAdi;

                }
                else
                {
                    iten.Tedarikci = "Tedarikçi Bulunamadı";

                }
            }
            ViewBag.Urunler = urunler.Select(u => new 
            {
                Text = u.Tedarikci+"->"+u.UrunAdi ,
                Tedarikci=u.Tedarikci,
                Value = u.UrunID.ToString()  
            }).ToList();
            return View(model);
        }
        public ActionResult SiparisOlusturAsıl(int id)
        {
            // Create an instance of Class1 to hold the data
            SatinAlmaFaturaView cs = new SatinAlmaFaturaView();

            // Get data for Faturalars and FaturaKalems from the database and assign to the properties
            cs.deger1 = _db.satinAlmaFatura.ToList();
            cs.deger2 = _db.satinAlmaFaturaKalem.ToList();
            
            // Return the data to the view
            return View(cs);
        }
     

[HttpPost]
    public IActionResult FaturaKaydet([FromBody] JsonElement data)
    {
        try
        {
            // JSON verisini JObject'a dönüştürün
            var faturaData = JObject.Parse(data.ToString());

            // Dinamik veri üzerinden doğrudan erişim
            string faturaSeriNo = faturaData["FaturaSeriNo"]?.ToString();
            string faturaSiraNo = faturaData["FaturaSıraNo"]?.ToString();
            DateTime tarih = DateTime.Parse(faturaData["Tarih"]?.ToString());
            string saat = faturaData["Saat"]?.ToString();
            string vergiDairesi = faturaData["VergiDairesi"]?.ToString();
            string teslimEden = faturaData["TeslimEden"]?.ToString();
            string teslimAlan = faturaData["TeslimAlan"]?.ToString();
            decimal toplam = decimal.Parse(faturaData["Toplam"]?.ToString() ?? "0");
            var kalemler = faturaData["kalemler"]?.ToObject<List<Dictionary<string, object>>>();

            // Fatura oluşturma
            var fatura = new satinAlmaFatura
            {
                FaturaSeriNo = faturaSeriNo,
                FaturaSıraNo = faturaSiraNo,
                Tarih = tarih,
                Saat = saat,
                VergiDairesi = vergiDairesi,
                TeslimEden = teslimEden,
                TeslimAlan = teslimAlan,
                Toplam = toplam
            };

            _db.satinAlmaFatura.Add(fatura);
            _db.SaveChanges();

            // Kalemleri kaydetme
            if (kalemler != null)
            {
                foreach (var kalem in kalemler)
                {
                    var faturaKalem = new satinAlmaFaturaKalem
                    {
                        Faturaid = fatura.Faturaid,
                        Aciklama = kalem["Aciklama"]?.ToString(),
                        Miktar = Convert.ToInt32(kalem["Miktar"]),
                        BirimFiyat = Convert.ToDecimal(kalem["BirimFiyat"]),
                        Tutar = Convert.ToDecimal(kalem["Tutar"])
                    };
                    _db.satinAlmaFaturaKalem.Add(faturaKalem);
                }
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Fatura başarıyla kaydedildi." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Hata: {ex.Message}" });
        }
    }






    //[HttpPost]
    //public ActionResult FaturaKaydet([FromBody] satinAlmaFatura model, satinAlmaFaturaKalem[] kalemler)
    //{
    //    satinAlmaFatura f = new satinAlmaFatura();
    //    f.FaturaSeriNo = model.FaturaSeriNo;
    //    f.FaturaSıraNo = model.FaturaSıraNo;
    //    f.Tarih = model.Tarih;
    //    f.VergiDairesi = model.VergiDairesi;
    //    f.Saat = model.Saat;
    //    f.TeslimEden = model.TeslimEden;
    //    f.TeslimAlan = model.TeslimAlan;
    //    f.Toplam = model.Toplam;
    //    _db.satinAlmaFatura.Add(f);
    //    foreach (var x in kalemler)
    //    {
    //        satinAlmaFaturaKalem fk = new satinAlmaFaturaKalem();
    //        fk.Aciklama = x.Aciklama;
    //        fk.BirimFiyat = x.BirimFiyat;
    //        fk.Faturaid = x.FaturaKalemid;
    //        fk.Miktar = x.Miktar;
    //        fk.Tutar = x.Tutar;
    //        _db.satinAlmaFaturaKalem.Add(fk);
    //    }
    //    _db .SaveChanges();
    //    return Json("İşlem Başarılı");
    //}

    [HttpPost]
        public async Task<ActionResult> PdfKaydet(IFormFile pdf, string htmlContent, string dosyaAdi)
        {
            // Dosya adı yoksa, veritabanına kaydetme işlemi yapma
            if (string.IsNullOrEmpty(dosyaAdi))
            {
                TempData["Error"] = "Dosya Formlara Kaydedilmedi : DOSYA ADI EKSİK";
                return View("Mesaj");
            }

            //if (pdf == null || pdf.Length == 0)
            //{
            //    return BadRequest("No PDF file uploaded.");
            //}

            // Convert IFormFile to byte array
            byte[] pdfBytes;
            using (var memoryStream = new MemoryStream())
            {
                await pdf.CopyToAsync(memoryStream);
                pdfBytes = memoryStream.ToArray();
            }

            // Save PDF to the database
            var yeniDosya = new SiparisTalepPdf
            {
                DosyaAdi = dosyaAdi + ".pdf", // You can modify this as needed
                DosyaVerisi = pdfBytes
            };

            _db.SiparisTalepPdf.Add(yeniDosya);
            await _db.SaveChangesAsync();

            return Ok("PDF başarıyla oluşturulup veritabanına kaydedildi.");
        }





        public class HtmlContentModel
        {
            public string Html { get; set; }
        }


        //[HttpPost]
        //    public async Task<IActionResult> PdfKaydet(string dosyaAdi, IFormFile pdfDosya)
        //    {
        //        if (pdfDosya != null && pdfDosya.Length > 0)
        //        {
        //            // Dosya adı kontrolü
        //            if (string.IsNullOrEmpty(dosyaAdi))
        //            {
        //                ModelState.AddModelError("", "Dosya adı boş olamaz.");
        //                return View(); // Hata mesajı ile aynı sayfada kalacak
        //            }

        //            using (var memoryStream = new MemoryStream())
        //            {
        //                await pdfDosya.CopyToAsync(memoryStream);

        //                var yeniDosya = new SiparisTalepPdf
        //                {
        //                    DosyaAdi = dosyaAdi+".pdf", // Kullanıcıdan alınan dosya adı
        //                    DosyaVerisi = memoryStream.ToArray() // PDF dosyasının byte verisi
        //                };

        //                _db.SiparisTalepPdf.Add(yeniDosya);
        //                await _db.SaveChangesAsync();
        //            }

        //            // Başarılı bir şekilde kaydedildikten sonra, yönlendirme yapılır.
        //            return RedirectToAction("KayitliSiparisForm");
        //        }

        //        // Dosya seçilmediyse veya başka bir hata oluştuysa.
        //        ModelState.AddModelError("", "Lütfen geçerli bir dosya seçin.");
        //        return View(); // Hata mesajı ile aynı sayfada kalacak
        //    }


        public IActionResult KayitliSiparisForm()
        {
            var dosyalar = _db.SiparisTalepPdf.ToList();
            return View(dosyalar);
        }

        public async Task<IActionResult> PdfIndir(int id)
        {
            var dosya = await _db.SiparisTalepPdf.FindAsync(id);
            if (dosya == null) return NotFound();

            return File(dosya.DosyaVerisi, "application/pdf", dosya.DosyaAdi);
        }
        [HttpPost]
        public JsonResult TeklifBekle(int id,string adi)
        {
            var message = " ";
            try
            {
                var form = _db.MalzemeTalep.FirstOrDefault(t => t.Id == id);

                if (form == null)
                {
                    return Json(new { success = false, message = "Form bulunamadı!" });
                }
                if(form.Durum.Equals("Teklif Bekleniyor"))
                {
                    form.Durum =adi + " "+ "tedarikcisine aktarıldı";
                    message = "aktarildi";
                }
                else 
                { 
                form.Durum = "Teklif Bekleniyor";
                    message = "bekleniyor";
                }
                int changes = _db.SaveChanges();
                

                if (changes > 0)
                {
                    return Json(new { success = true, message });
                }

                return Json(new { success = false, message = "Bir hata oluştu! Veritabanına kaydedilemedi." });
            }
            catch (Exception ex)
            {
                // Log the exception or use a logging framework
                return Json(new { success = false, message = "Bir hata oluştu! " + ex.Message });
            }
        }
        

        [HttpPost]
        public async Task<IActionResult> SavePdf(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Define the path where the file will be saved on the server
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", file.FileName);

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save the file to the specified path
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return a success response
                return Json(new { success = true, message = "File saved successfully!" });
            }

            // Return an error if the file wasn't uploaded
            return Json(new { success = false, message = "No file uploaded." });
        }

        [HttpPost]
        public IActionResult Olustur(int id)
        {
            // Retrieve the data from the database based on the form ID
            var talep = _db.MalzemeTalep.Where(t => t.FormId == id).ToList();

            // Return the view with the retrieved data
            return View(talep);
        }

        [HttpPost]
        public IActionResult Olustura([FromBody] JsonElement data)
        {
            // Access the 'id' from the JsonElement
            if (data.TryGetProperty("id", out JsonElement idElement))
            {
                int id = idElement.GetInt32(); // Convert the id to an integer

                // Retrieve the data based on the id
                var talep = _db.MalzemeTalep.Where(t => t.FormId == id).ToList();

                // Check if there are any results for the given id
                if (talep.Any())
                {
                    // You can perform other operations if needed (e.g., create a form, send emails, etc.)

                    // Set the data into ViewBag for passing to the view (if needed)
                    ViewBag.Talep = talep;

                    // Optionally, you can return a view with the data here
                    // return View("SomeView", talep);

                    // For redirection after the operation, you can do:
                    return RedirectToAction("Olustur", new { id = id }); // Redirecting to another action
                }
                else
                {
                    // If no data found for the given id
                    return Json(new { success = false, message = "No records found for the given ID." });
                }
            }

            // If the 'id' was not found in the request data
            return Json(new { success = false, message = "ID parameter is missing." });
        }


        public IActionResult Olustur2(int id)
        {
            var talep = _db.MalzemeTalep.Where(t => t.FormId == id).ToList();



            return View(talep);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TalepSil(int id)
        {
            // Find the item in the MalzemeTalep table
            var item = _db.MalzemeTalep.FirstOrDefault(t => t.FormId == id);

            if (item == null)
            {
                return NotFound();
            }

            // Remove the item from the MalzemeTalep table
            _db.MalzemeTalep.Remove(item);
            _db.SaveChanges();

            // Check if there are any other records in MalzemeTalep with the same FormId
            var relatedItems = _db.MalzemeTalep.Where(t => t.FormId == id).ToList();

            // If no other records are found, remove the corresponding record from the Formlar table
            if (!relatedItems.Any())
            {
                var formRecord = _db.Formlar.FirstOrDefault(f => f.FormNo == id);

                if (formRecord != null)
                {
                    _db.Formlar.Remove(formRecord);
                    _db.SaveChanges();
                }
            }
            TempData["Message"] = "Silme işlemi başarılı.";
            return View("Mesaj");
        }   [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SiparisSil(int id)
        {
            var item = _db.satinAlmaSiparisler.FirstOrDefault(t => t.SiparisID == id);

            if (item == null)
            {
                return NotFound();
            }

            _db.satinAlmaSiparisler.Remove(item);
            _db.SaveChanges();

           
            TempData["Message"] = "Silme işlemi başarılı.";
            return View("Mesaj");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PdfSil(int id)
        {
            // Find the item in the MalzemeTalep table
            var item = _db.SiparisTalepPdf.FirstOrDefault(t => t.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            // Remove the item from the MalzemeTalep table
            _db.SiparisTalepPdf.Remove(item);
            _db.SaveChanges();

            TempData["message"] = "Dosya başarı ile silindi";
            return RedirectToAction("KayitliSiparisForm");
        }

        public IActionResult Degerlendir(int id)
        {
            var tedarikci = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.Id == id);

            if (tedarikci == null)
            {
                return NotFound("Tedarikçi bulunamadı.");
            }

            ViewBag.TedarikciId = tedarikci.Id;
            ViewBag.TedarikciAdi = tedarikci.TedarikciAdi;
            ViewBag.TedarikciMail = tedarikci.TedarikciMail;

            ViewBag.TedarikciAdres = tedarikci.TedarikciAdres;
            ViewBag.TedarikciTelefon = tedarikci.TedarikciTelefon;
            var mevcutRapor = _db.satinAlmaDegerlendir.DefaultIfEmpty().FirstOrDefault(t => t.Id == id);

            return View(mevcutRapor);
        }


        [HttpPost]
        public IActionResult Dgrlendir(Degerlendir model)
        {
            if (model == null)
            {
                return BadRequest("Geçersiz veri gönderildi.");
            }

            var mevcutRapor = _db.satinAlmaDegerlendir.DefaultIfEmpty().FirstOrDefault(t => t.Id == model.Id);

            if (mevcutRapor != null)
            {
                mevcutRapor.DokumanNo = model.DokumanNo;
                mevcutRapor.RevizyonNo = model.RevizyonNo;
                mevcutRapor.RevizyonTarihi = model.RevizyonTarihi;
                mevcutRapor.DegerlendirmeTarihi = model.DegerlendirmeTarihi;
                mevcutRapor.Adi = model.Adi;
                mevcutRapor.TelefonNo = model.TelefonNo;
                mevcutRapor.EPosta = model.EPosta;
                mevcutRapor.IlgiliKisi = model.IlgiliKisi;
                mevcutRapor.FaaliyetKonusu = model.FaaliyetKonusu;
                mevcutRapor.Fiyat = model.Fiyat;
                mevcutRapor.OdemeEsnekligi = model.OdemeEsnekligi;
                mevcutRapor.UrunKalitesi = model.UrunKalitesi;
                mevcutRapor.Zamanindalik = model.Zamanindalik;
                mevcutRapor.Uzmanlik = model.Uzmanlik;
                mevcutRapor.DestekDokumanlar = model.DestekDokumanlar;
                mevcutRapor.SatisSonrasiHizmet = model.SatisSonrasiHizmet;
                mevcutRapor.Sureklilik = model.Sureklilik;
                mevcutRapor.KaliteYonetimSistemi = model.KaliteYonetimSistemi;

                mevcutRapor.DofAcilsinMi = model.DofAcilsinMi;
                mevcutRapor.DofAciklama = model.DofAciklama;
                mevcutRapor.Siniflandirma = model.Siniflandirma;
                mevcutRapor.DegerlendirmeSonucu = model.DegerlendirmeSonucu;
                mevcutRapor.DegerlendirenAdSoyad = model.DegerlendirenAdSoyad;
            }
            else
            {
                
                var rapor = new Degerlendir
                {
                    Id = model.Id, 
                    DokumanNo = model.DokumanNo,
                    RevizyonNo = model.RevizyonNo,
                    RevizyonTarihi = model.RevizyonTarihi,
                    DegerlendirmeTarihi = model.DegerlendirmeTarihi,
                    Adi = model.Adi,
                    TelefonNo = model.TelefonNo,
                    EPosta = model.EPosta,
                    IlgiliKisi = model.IlgiliKisi,
                    FaaliyetKonusu = model.FaaliyetKonusu,
                    Fiyat = model.Fiyat,
                    OdemeEsnekligi = model.OdemeEsnekligi,
                    UrunKalitesi = model.UrunKalitesi,
                    Zamanindalik = model.Zamanindalik,
                    Uzmanlik = model.Uzmanlik,
                    DestekDokumanlar = model.DestekDokumanlar,
                    SatisSonrasiHizmet = model.SatisSonrasiHizmet,
                    Sureklilik = model.Sureklilik,
                    KaliteYonetimSistemi = model.KaliteYonetimSistemi,
                    DofAcilsinMi = model.DofAcilsinMi,
                    DofAciklama = model.DofAciklama,
                    Siniflandirma = model.Siniflandirma,
                    DegerlendirmeSonucu = model.DegerlendirmeSonucu,
                    DegerlendirenAdSoyad = model.DegerlendirenAdSoyad
                };

                _db.satinAlmaDegerlendir.Add(rapor);
            }

            _db.SaveChanges();
            var tedarikci = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.Id == model.Id);
            int toplamPuan = model.Fiyat +
                         model.OdemeEsnekligi +
                         model.UrunKalitesi +
                         model.Zamanindalik +
                         model.Uzmanlik +
                         model.DestekDokumanlar +
                         model.SatisSonrasiHizmet +
                         model.Sureklilik +
                         model.KaliteYonetimSistemi;
            tedarikci.Score = toplamPuan;
            _db.SaveChanges();


            TempData["Message"] = "Değerlendirme kaydedildi veya güncellendi.";
            return View("Mesaj");
        }


        [HttpPost]
        public IActionResult UpdateTedarikci(int id, string tedarikciAdi, string tedarikciTelefon, string tedarikciAdres,string tedarikciNot)
        {
            try
            {
                var tedarikci = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.Id == id);

                if (tedarikci != null)
                {
                    tedarikci.TedarikciAdi = tedarikciAdi;
                    tedarikci.TedarikciTelefon = tedarikciTelefon;
                    tedarikci.TedarikciAdres = tedarikciAdres;
                    tedarikci.TedarikciNot = tedarikciNot;

                    _db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Supplier not found." });
                }
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult TedarikciSil(int id)
        {
            var tedarikci = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.Id == id);
            if (tedarikci != null)
            {
                _db.satinAlmaTedarikcilers.Remove(tedarikci);
                _db.SaveChanges();
            }
            TempData["Message"] = "Tedarikci Silindi.";
            return View("Mesaj");
        }
        public IActionResult EskiIstek(int id)
        {
            var eski = _db.Formlar.Where(t => t.FormNo == id).ToList();

            foreach (var form in eski)
            {
                form.Durum = "ESKİ";
            }

            _db.SaveChanges();

            return RedirectToAction("BekleyenStok");
        }

        public IActionResult BekleyenStok()
        {
            try
            {
                var display=new List<Malzemeler>();
                var model = _db.MalzemeTalep                               
                               .GroupBy(t => t.FormId)
                               .ToList();

                    if (model.Any() && model!=null)
                {
                   
                
                var yeniFormlar =  _db.Formlar
                .Where(f => f.Durum.Equals
                ("YENİ")  )
                .Select(f => f.FormNo) 
                .ToList();

                display = model.SelectMany(group => group)
                .Where(talep => yeniFormlar.Contains(talep.FormId))  
                .Select(talep => new Malzemeler
                {
                    FormId = talep.FormId,
                    TerminTarihi = talep.TerminTarihi,
                    MalzemeAdi = talep.MalzemeAdi,
                    TalepMiktari = talep.TalepMiktari,
                    MevcutStok = talep.MevcutStok,
                    Aciklama = talep.Aciklama
                })
                .ToList();
                }
                else
                {
                    display = new List<Malzemeler>();
                }

                
                TreeColor treeColorService = new TreeColor();
                var rowsToNotify = treeColorService.CheckAmbalajTuru();

                if (rowsToNotify != null && rowsToNotify.Any())
                {
                    var today = DateTime.Now.Date;
                    var sonGt = rowsToNotify
                                .Where(row => DateTime.TryParse(row.SonGirisTarih, out DateTime sonGirisTarih) && sonGirisTarih.Date == today)
                                .ToList();

                    var rowsToDisplay = sonGt
                        .GroupBy(row => row.KartNo)
                        .Select(group => group.First())
                        .Select(row => new { row.KartNo, row.AmbalajTuru, row.SonGirisTarih })
                        .ToList();

                    var bildirimSayisi = _db.Bildirimler
                        .Count(t => t.Okundu == false && t.Mesaj.StartsWith("TreeColor"));

                    ViewBag.Bildirim = bildirimSayisi > 0 ? bildirimSayisi : 0;
                }
               

                return View(display); 
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata oluştu: {ex.Message}");
            }
        }
        public IActionResult YeniStokYonetim(Malzemeler model)
        {
            var talep = _db.MalzemeTalep.DefaultIfEmpty().Where(t => t.FormId.Equals(model.FormId)).ToList();

            var form = _db.Formlar.Where(t => t.FormNo.Equals(model.FormId)).ToList();

            var siparis = _db.satinAlmaSiparisler.FirstOrDefault(t => t.TalepID.Equals(model.FormId));

      

            var viewModel = new YeniStok
            {
                Malzeme=model.MalzemeAdi,
                TalepListesi = talep,
                FormListesi = form
            };

            return View(viewModel);
        }

        public class YeniStok
        {
            public List<Malzemeler> TalepListesi { get; set; }
            public string Malzeme { get; set; }
            public List<Formlar> FormListesi { get; set; }
        }
        public IActionResult DepoYonetimCikis()
        {
            var talepler = _db.satinAlmaTaleps
                           .Select(t => new satinAlmaTalep
                           {
                               TalepNo = t.TalepNo,
                               TalepTarihi = t.TalepTarihi,
                               TalepEdenBolum = t.TalepEdenBolum,
                               TalepEdilenUrun = t.TalepEdilenUrun,
                               TalepEdilenMiktar = t.TalepEdilenMiktar,
                               Birim = t.Birim
                           }).OrderByDescending(t => t.TalepNo)
                           .ToList();

           

            return View(talepler);
        }
        [AllowAnonymous]

        [HttpPost]
        public IActionResult KaydetTalepFormu(Formlar model)
        {
            if (model!=null)
            {
                
                var yeniForm = new Formlar
                {
                    SirketAdi = model.SirketAdi,
                    FormTuru = model.FormTuru,
                    TalepEden = model.TalepEden,
                    Onaylayan = model.Onaylayan,
                    Durum ="YENİ",
                    Tarih = DateTime.Now
                };
                _db.Formlar.Add(yeniForm);
                _db.SaveChanges();

               
                foreach (var malzeme in model.Malzemeler)
                {
                    var yeniMalzeme = new Malzemeler
                    {
                        FormId = yeniForm.FormNo,
                        MalzemeAdi = malzeme.MalzemeAdi,
                        MevcutStok = malzeme.MevcutStok,
                        TalepMiktari = malzeme.TalepMiktari,
                        TerminTarihi = malzeme.TerminTarihi,
                        Durum = "Beklemede",
                        Aciklama = string.IsNullOrEmpty(malzeme.Aciklama) ? "-" : malzeme.Aciklama,
                    };
                    _db.MalzemeTalep.Add(yeniMalzeme);
                }
                
                _db.SaveChanges();

                
                string mesaj = $"Yeni Stok İsteği - {yeniForm.TalepEden} - {yeniForm.Onaylayan}";

                var yeniBildirim = new Bildirimler
                {
                    Mesaj = mesaj,
                    OlusturmaTarihi = DateTime.Now,
                    Okundu = false
                };

                _db.Bildirimler.Add(yeniBildirim);
                _db.SaveChanges();

                TempData["Message"] = "İstek başarıyla eklendi.";
                return View("Mesaj");
            }
            TempData["Error"] = "Hata :: 0x10H-I";

            return View("YeniStokIstek",model);
        }






        //public IActionResult _SASidebar()
        //{
        //    return View();
        //}
        public IActionResult Bildirimler()
        {
            var bildirimList = _db.Bildirimler.OrderByDescending(t=>t.Id).ToList(); 
            return View(bildirimList);
            
        }
        [HttpPost]
        public IActionResult OkunduYap(int id)
        {
            var bildirim = _db.Bildirimler.FirstOrDefault(b => b.Id == id);
            if (bildirim != null)
            {
                bildirim.Okundu = true;
                _db.SaveChanges();
                return Ok();
            }
            return BadRequest("Bildirim bulunamadı");
        }
        [AllowAnonymous]
        public async Task<IActionResult> Talep() {

            var products = await _db.satinAlmaUrunBilgileris
        .Where(p => p.StokMiktari > 0)  
        .Select(p => new
        {
            p.UrunAdi,
            p.UrunID,
            p.UrunKodu,
            p.Kategori,
            p.StokMiktari,
            p.Aciklama
        })
        .ToListAsync();

            return View(products);

            }
        [AllowAnonymous]
        public IActionResult TalepEkle(int urunId) {
            var urun = _db.satinAlmaUrunBilgileris
                          .Where(u => u.UrunID == urunId)
                          .Select(u => new
                          {
                              u.UrunAdi,
                              u.UrunKodu,
                              u.Birim,
                              u.Tedarikci,
                              u.Aciklama

                          })
                          .FirstOrDefault();

            if (urun == null)
            {
                return NotFound();
            }

            // Ürün bilgilerini view'a gönderiyoruz
            return View(urun);
        }
        public static string NormalizeUrunAdi(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var replaced = input
                .Replace("*", "x")
                .Replace("/", "x")
                .Replace("-", "x")
                .Replace("×", "x");

            string normalized = replaced.Normalize(NormalizationForm.FormD);
            string ascii = new string(normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return ascii.ToLower(new CultureInfo("tr-TR")).Trim();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> TalepEklePost(satinAlmaTalep model, int TedarikciId)
        {
            if (ModelState.IsValid)
            {
                var talepUrun = NormalizeUrunAdi(model.TalepEdilenUrun);

                // satinAlmaUrunBilgileris tablosundaki toplam stok
                var tumUrunler =  _db.satinAlmaUrunBilgileris
                    .AsEnumerable()
                    .Where(t => NormalizeUrunAdi(t.UrunAdi) == talepUrun)
                    .ToList();
              
                var toplamStok = tumUrunler.Sum(t => t.StokMiktari);

                if (model.TalepEdilenMiktar > toplamStok)
                {
                    TempData["Error"] = "Talep edilen miktar, sistemdeki toplam stoğu aşıyor!";
                    return View("Mesaj");
                }

                var UrunKodu = GenerateProductCode(TedarikciId, model.TalepEdilenUrun);

                // Talep kaydı oluştur
                var talep = new satinAlmaTalep
                {
                    UrunKodu = UrunKodu,
                    TalepTarihi = model.TalepTarihi,
                    TalepEdenBolum = model.TalepEdenBolum,
                    TalepEdilenUrun = model.TalepEdilenUrun,
                    TalepEdilenMiktar = model.TalepEdilenMiktar,
                    Birim = model.Birim
                };

                _db.satinAlmaTaleps.Add(talep);
                await _db.SaveChangesAsync();

                int kalanMiktar = model.TalepEdilenMiktar;
                int kalanMiktarurun = model.TalepEdilenMiktar;

                // Önce satinAlmaDepos içindeki KalanAdet'lerden düşelim (öncelikli depo)
                var depoStoklar = _db.satinAlmaDepos
                                        .AsEnumerable()

                    .Where(t => NormalizeUrunAdi(t.UrunAdi) == talepUrun)
                    .OrderBy(t => t.Id)
                    .ToList();

                foreach (var stok in depoStoklar)
                {
                    if (kalanMiktar <= 0)
                        break;

                    if (stok.KalanAdet > 0)
                    {
                        int azaltma = Math.Min(stok.KalanAdet, kalanMiktar);
                        stok.KalanAdet -= azaltma;
                        kalanMiktar -= azaltma;
                    }
                }
              

                // Kalan varsa satinAlmaUrunBilgileris stoklarından da düş
                foreach (var urun in tumUrunler.OrderBy(x => x.KayitTarihi))
                {
                    if (kalanMiktarurun <= 0)
                        break;

                    if (urun.StokMiktari > 0)
                    {
                        decimal azaltma = Math.Min(urun.StokMiktari, kalanMiktarurun);
                        urun.StokMiktari -= azaltma;
                        kalanMiktarurun -= (int)azaltma;
                    }
                }

                await _db.SaveChangesAsync();

                TempData["Message"] = "Talep başarıyla eklendi.";
                return View("Mesaj");
            }

            return View("TalepEkle", model);
        }


        public IActionResult Profil() { return View(); }

        public IActionResult TedarikciYonetim() 
        {

            var ted = _db.satinAlmaTedarikcilers.OrderByDescending(t=>t.Id).ToList();
            if (!ted.Any())
            {
                ted = new List<SatinAlmaTedarikciler>();
            }


            return View(ted); 
        }
        public IActionResult TedarikciEkle()
        {
           
            return View();
        }
        [HttpPost]
        public IActionResult TedarikciAdd(SatinAlmaTedarikciler tedarikci)
        {
            if (tedarikci!=null)
            {
                // Tedarikci modelini veritabanına ekleyin
                var model = new SatinAlmaTedarikciler
                {
                    TedarikciAdi = tedarikci.TedarikciAdi,
                    TedarikciTelefon = tedarikci.TedarikciTelefon,
                    TedarikciAdres = tedarikci.TedarikciAdres,
                    TedarikciMail = tedarikci.TedarikciMail,
                    TedarikciNot =tedarikci.TedarikciNot
                    // Don't manually set 'Id'
                };

                _db.satinAlmaTedarikcilers.Add(model);
                _db.SaveChanges();


                // Başarılı ekleme sonrası başka bir sayfaya yönlendirebilirsiniz
                return RedirectToAction("TedarikciYonetim");  // Örneğin, tedarikçi yönetim sayfasına yönlendirme
            }

            // Formda bir hata varsa, tekrar formu döndür
            return View(tedarikci);
        }
        [HttpPost]
        public async Task<IActionResult> EditUrun(SatinAlmaDepos urun)
        {
            if (ModelState.IsValid)
            {
                var existingUrun = await _db.satinAlmaDepos
                    .FirstOrDefaultAsync(u => u.Id == urun.Id);

                if (existingUrun != null)
                {
                    // Veritabanındaki mevcut ürünü güncelliyoruz
                    existingUrun.KalanAdet = urun.KalanAdet;
                    existingUrun.Kategori = urun.Kategori;
                    existingUrun.BirimFiyat = urun.BirimFiyat;
                    existingUrun.GuncellemeTarihi = DateTime.Now;

                    // Değişiklikleri kaydediyoruz
                    await _db.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction("DepoYonetim");
                }

                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("DepoYonetim");
            }

            return View(urun);
        }
        public IActionResult GetUrunDetaylari(string runAdi)
        {
            var urunAdi = runAdi.Split("->");

           
            var ted = _db.satinAlmaTedarikcilers
             .Where(t => t.TedarikciAdi.ToString().Equals(urunAdi[0])) // Tedarikci bir sayıysa doğrudan eşleşir
             .Select(t => new
             {
                 t.TedarikciAdi,
                 t.Id
             })
             .FirstOrDefault(); // İlk eşleşen tedarikçiyi döndür
            var urun = _db.satinAlmaUrunBilgileris
                              .Where(u => u.UrunAdi == urunAdi[1]&&u.Tedarikci==ted.Id.ToString())
                              .Select(u => new
                              {
                                  u.Tedarikci,
                                  u.Birim,
                                  u.Kategori,
                                  u.BirimFiyati
                              })
                              .FirstOrDefault();

            if (urun != null)
            {
                return Json(new
                {
                    Tedarikci = ted.TedarikciAdi,
                    Kategori = urun.Kategori,
                    BirimFiyat = urun.BirimFiyati
                });
            }

            return Json(null);
        }

        public IActionResult GetSiparisDetaylari(string urunAdi)
        {
            var siparis = _db.satinAlmaSiparisler
                             .Where(u => u.UrunAdi == urunAdi)
                             .Select(u => new
                             {
                                 u.SiparisTarihi,
                                 u.SiparisTerminTarihi,
                                 u.SiparisID,
                                 u.OnayDurumu,
                                 u.Konum,
                                 u.UrunAdedi,
                                 u.BirimFiyat
                             })
                             .FirstOrDefault();

            if (siparis != null)
            {
                return Json(new
                {
                    siparis.SiparisTarihi,
                    siparis.SiparisID,

                    siparis.SiparisTerminTarihi,
                    siparis.OnayDurumu,
                    siparis.Konum,
                    siparis.UrunAdedi,
                    siparis.BirimFiyat
                });
            }

            return Json(null);
        }
    

        public IActionResult DepoEk()
        {
            var ted = _db.satinAlmaTedarikcilers.ToList();
            ViewBag.Tedarikci = ted.Any() ? ted : null; // Eğer tedarikçi varsa atama yap, yoksa null ata

            var urun = _db.satinAlmaUrunBilgileris.DefaultIfEmpty().ToList();
            ViewBag.UrunList = urun.Any() ? urun : null; // Eğer ürün varsa atama yap, yoksa null ata

            var siparis = _db.satinAlmaSiparisler.Where(t => t.Konum == "FATURA KESİLDİ").ToList();
            ViewBag.siparis = siparis.Any() ? siparis : null; // Eğer sipariş varsa atama yap, yoksa null ata

            return View();
        }
        public IActionResult DepoEkSayim(string urunAdi)
        {
            var ted = _db.satinAlmaTedarikcilers.ToList();
            ViewBag.Tedarikci = ted.Any() ? ted : null; // Eğer tedarikçi varsa atama yap, yoksa null ata

            var urunGruplanmis = _db.satinAlmaUrunBilgileris
    .GroupBy(u => u.UrunAdi)
    .Select(g => new
    {
        UrunAdi = g.Key,
        UrunKodu = g.FirstOrDefault().UrunKodu,
        Kategori = g.FirstOrDefault().Kategori,
        BirimFiyati = g.FirstOrDefault().BirimFiyati,
        Tedarikci = g.FirstOrDefault().Tedarikci,
        StokMiktari = g.Sum(x => x.StokMiktari),
        ToplamAdet = g.Sum(x => x.StokMiktari)
    })
    .ToList();

            ViewBag.UrunList = urunGruplanmis.Any() ? urunGruplanmis : null;


            var siparis = _db.satinAlmaSiparisler.FirstOrDefault();
            ViewBag.siparis = siparis; // Eğer sipariş varsa atama yap, yoksa null ata

            return View();
        }

        [HttpPost]
        public IActionResult DepoUrunEkle(SatinAlmaDepos model, string TedarikciAdi, string Konum, int siparisID, IFormFile Foto, string BirimFiyat)
        {
            if (model != null)
            {
                model.UrunAdi = model.UrunAdi.Split("->")[1];
                var tedarikci = _db.satinAlmaTedarikcilers.FirstOrDefault(t => t.TedarikciAdi.Equals(TedarikciAdi));
                var urun = _db.satinAlmaUrunBilgileris.DefaultIfEmpty().ToList();
                if (urun.Any())
                {
                    ViewBag.UrunList = urun;
                }

                if (tedarikci != null)
                {
                    var fiyat = BirimFiyat.Split(" ")[0];
                    // Hatalı satır:
                    // fiyat = (decimal)(fiyat);

                    // Doğru şekilde string'i decimal'e çevir:
                    if (decimal.TryParse(fiyat, out decimal fiyatDecimal))
                    {
                        fiyat = fiyatDecimal.ToString();
                    }
                    else
                    {
                        // Hatalıysa varsayılan değer ata veya hata yönetimi ekle
                        fiyat = "0";
                    }
                    model.TedarikciId = tedarikci.Id;                    
                    model.BirimFiyat = fiyatDecimal;

                    model.KalanAdet = model.GelenUrunMiktari;
                    model.GuncellemeTarihi = DateTime.Now;
                    model.EklemeTarihi = DateTime.Now;
                    model.UrunAdi = model.UrunAdi.ToUpper();  // Büyük harfe dönüştürme
                    model.Aciklama = string.IsNullOrEmpty(model.Aciklama) ? "-" : model.Aciklama;
                    // Fotoğrafı kaydetme
                    if (Foto != null && Foto.Length > 0)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                        // Dizin yoksa oluştur
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        var filePath = Path.Combine(uploadDir, Foto.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            Foto.CopyTo(stream);
                        }

                        model.FotoPath = "/uploads/" + Foto.FileName;  // Veritabanına kaydedilecek fotoğraf yolu
                    }

                    _db.satinAlmaDepos.Add(model);
                    _db.SaveChanges();

                    var siparis = _db.satinAlmaSiparisler.FirstOrDefault(t => t.SiparisID == siparisID);
                    siparis.Konum = Konum;
                    siparis.Durum = "TESLİM ALINDI";
                    int sonKayitId = model.Id;
                    siparis.StokID = sonKayitId;
                    _db.SaveChanges();

                    var teda = _db.satinAlmaTedarikcilers.ToList();
                    if (teda.Any())
                    {
                        ViewBag.Tedarikci = teda;
                    }

                    var depoUrunler = _db.satinAlmaDepos
                                         .OrderByDescending(t => t.Id)
                                         .ToList();

                    var depoTedarikciList = depoUrunler.Select(item => new DepoTed
                    {
                        Depo = item,
                        TedarikciAdi = _db.satinAlmaTedarikcilers
                            .Where(t => t.Id == item.TedarikciId)
                            .Select(t => t.TedarikciAdi)
                            .FirstOrDefault()
                    }).ToList();
                    var yeniUrunKodu = GenerateProductCode(model.TedarikciId, model.UrunAdi);

                    var parts = yeniUrunKodu.Split('-');
                    var normalized = string.Join("-", parts.Skip(1));

                    var depoUrunes = new SatinAlmaDepoUrun_Es
                    {
                        DepoId = model.Id.ToString(),
                        UrunId = yeniUrunKodu,
                        UrunNormalizedId = normalized
                    };
                    _db.Add(depoUrunes);

                    _db.SaveChanges();
                    return View("DepoYonetim", depoTedarikciList);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tedarikçi bulunamadı.");
                }
            }

            var ted = _db.satinAlmaTedarikcilers.ToList();
            if (ted.Any())
            {
                ViewBag.Tedarikci = ted;
            }

            return View("DepoEk", model);
        }
        [HttpPost]
        public IActionResult DepoUrunEkleSayim(SatinAlmaDepos model, string TedarikciAdi, string Konum, int siparisID, IFormFile Foto, string BirimFiyat)
        {
            if (model != null){
               

               
                    var fiyat = 0;
                 
                    model.TedarikciId = 0;
                    model.BirimFiyat = 0;

                    model.KalanAdet = model.GelenUrunMiktari;
                    model.GuncellemeTarihi = DateTime.Now;
                    model.EklemeTarihi = DateTime.Now;
                    model.UrunAdi = model.UrunAdi.ToUpper();  // Büyük harfe dönüştürme
                    model.Aciklama = string.IsNullOrEmpty(model.Aciklama) ? "-" : model.Aciklama;
                    // Fotoğrafı kaydetme
                    if (Foto != null && Foto.Length > 0)
                    {
                        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                        // Dizin yoksa oluştur
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        var filePath = Path.Combine(uploadDir, Foto.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            Foto.CopyTo(stream);
                        }

                        model.FotoPath = "/uploads/" + Foto.FileName;  // Veritabanına kaydedilecek fotoğraf yolu
                    }

                    _db.satinAlmaDepos.Add(model);
                    _db.SaveChanges();

                 

                    var teda = _db.satinAlmaTedarikcilers.ToList();
                    if (teda.Any())
                    {
                        ViewBag.Tedarikci = teda;
                    }

                    var depoUrunler = _db.satinAlmaDepos
                                         .OrderByDescending(t => t.Id)
                                         .ToList();

                    var depoTedarikciList = depoUrunler.Select(item => new DepoTed
                    {
                        Depo = item,
                        TedarikciAdi = _db.satinAlmaTedarikcilers
                            .Where(t => t.Id == item.TedarikciId)
                            .Select(t => t.TedarikciAdi)
                            .FirstOrDefault()
                    }).ToList();
                    var yeniUrunKodu = GenerateProductCode(0, model.UrunAdi);

                    var parts = yeniUrunKodu.Split('-');
                    var normalized = string.Join("-", parts.Skip(1));

                    var depoUrunes = new SatinAlmaDepoUrun_Es
                    {
                        DepoId = model.Id.ToString(),
                        UrunId = yeniUrunKodu,
                        UrunNormalizedId = normalized
                    };
                    _db.Add(depoUrunes);

                    _db.SaveChanges();
                int kalanMiktar = model.GelenUrunMiktari;

                var sayim = _db.satinAlmaDepos
                               .Where(t => t.UrunAdi == model.UrunAdi)
                               .OrderBy(t => t.Id) // FIFO için
                               .ToList();

                foreach (var item in sayim)
                {
                    if (kalanMiktar == 0)
                        break;

                    if (item.KalanAdet != 0)
                    {
                        if (kalanMiktar < 0) // ürün çıkışı
                        {
                            int azaltilacak = Math.Min(item.KalanAdet, Math.Abs(kalanMiktar));
                            item.KalanAdet -= azaltilacak;
                            kalanMiktar += azaltilacak; // negatif değeri sıfıra yaklaştır
                        }
                        else // ürün girişi
                        {
                            item.KalanAdet += kalanMiktar;
                            kalanMiktar = 0;
                        }
                    }
                }

                _db.SaveChanges();

                var urun = _db.satinAlmaUrunBilgileris.FirstOrDefault(t => t.UrunAdi == model.UrunAdi);
                var depo = _db.satinAlmaDepos.FirstOrDefault(t => t.UrunAdi == model.UrunAdi);
                var ted = Convert.ToInt32(urun.Tedarikci);



                EkleUrun(depo.Id, ted, model.UrunAdi);



                return View("DepoYonetim", depoTedarikciList);
                }









          

            return View("DepoEk", model);
        }


        [HttpPost]
        public async Task<IActionResult> SilStok(int urunId, int TedarikciId)
        {
            var depo = await _db.satinAlmaDepos
                .FirstOrDefaultAsync(u => u.Id == urunId);
            var yeniUrunKodu = GenerateProductCode(depo.TedarikciId, depo.UrunAdi);
            var urun = await _db.satinAlmaUrunBilgileris.FirstOrDefaultAsync(u => u.UrunKodu == yeniUrunKodu);

            if (urun != null)
            {
                var depoUrun = await _db.satinAlmaDepos.FirstOrDefaultAsync(d => d.Id == urunId);

                if (depoUrun != null)
                {
                    if (depoUrun.KalanAdet == urun.StokMiktari)
                    {
                        _db.satinAlmaUrunBilgileris.Remove(urun);
                        TempData["SuccessMessage"] = "Ürün ve ilişkili veriler başarıyla silindi.";
                    }
                    else
                    {
                        urun.StokMiktari -= depoUrun.KalanAdet;
                        _db.satinAlmaUrunBilgileris.Update(urun);
                        TempData["SuccessMessage"] = "Ürünün stoğu güncellendi.";
                    }

                    _db.satinAlmaDepos.Remove(depoUrun);
                }
                else
                {
                    TempData["ErrorMessage"] = "Depo kaydı bulunamadı.";
                }

                await _db.SaveChangesAsync();
            }
            else
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
            }

            return RedirectToAction("DepoYonetim");
        }


        public IActionResult DepoYonetim()
        {
            var depoUrunler = _db.satinAlmaDepos
                .OrderByDescending(t => t.Id)
                .ToList();

            var depoTedarikciList = depoUrunler.Select(item => new DepoTed
            {
                Depo = item,
                TedarikciAdi = _db.satinAlmaTedarikcilers
                    .Where(t => t.Id == item.TedarikciId)
                    .Select(t => t.TedarikciAdi)
                    .FirstOrDefault()
            }).ToList();

            return View(depoTedarikciList);
        }

        public class DepoTed
        {
            public SatinAlmaDepos Depo { get; set; }
            public string TedarikciAdi { get; set; } 
        }


        [HttpPost]
        public async Task<IActionResult> EkleUrun(int urunId, int TedarikciId, string urunAdi)
        {
            
            // Toplam stok miktarını hesaplıyoruz
            var toplamStok = await _db.satinAlmaDepos
                .Where(t => t.TedarikciId == TedarikciId && t.UrunAdi == urunAdi)
                .SumAsync(t => t.KalanAdet);

            // Urun bilgilerini alıyoruz
            var urun = await _db.satinAlmaDepos
                .FirstOrDefaultAsync(u => u.Id == urunId);

            if (urun != null)
            {
               
                    var yeniUrunKodu = GenerateProductCode(urun.TedarikciId, urun.UrunAdi);

                    // Ürünün var olup olmadığını kontrol ediyoruz
                    var existingProduct = await _db.satinAlmaUrunBilgileris
                        .FirstOrDefaultAsync(u => u.UrunKodu == yeniUrunKodu);

                    if (existingProduct != null)
                    {
                        // Eğer ürün mevcutsa, mevcut stok miktarını güncelliyoruz
                        existingProduct.StokMiktari = toplamStok;
                        existingProduct.BirimFiyati = urun.BirimFiyat;
                        _db.Update(existingProduct);
                        await _db.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Ürün stok miktarı başarıyla güncellendi.";
                        //var parts = yeniUrunKodu.Split('-');
                        //var normalized = string.Join("-", parts.Skip(1));

                        //var depoUrunes = new SatinAlmaDepoUrun_Es
                        //{
                        //    DepoId = urun.Id.ToString(),
                        //    UrunId = yeniUrunKodu,
                        //    UrunNormalizedId = normalized
                        //};
                        //_db.Add(depoUrunes);

                        //_db.SaveChanges();

                    }
                    else
                    {
                        // Eğer ürün hiç eklenmemişse ve kalan stok miktarı > 0 ise yeni ürünü ekliyoruz
                        if (toplamStok > 0)
                        {
                            var yeniUrun = new satinAlmaUrunBilgileri
                            {
                                UrunAdi = urun.UrunAdi,
                                UrunKodu = yeniUrunKodu,
                                Kategori = urun.Kategori,
                                Aciklama = urun.Aciklama,
                                StokMiktari = toplamStok,  // Toplam stok miktarını kullanıyoruz
                                Birim = "-",
                                Tedarikci = urun.TedarikciId.ToString(),
                                BirimFiyati = urun.BirimFiyat,
                                UrunKart= "TekUrunK",
                                SonKullanmaTarihi = DateTime.Now,
                                KayitTarihi = DateTime.Now,
                                GuncellemeTarihi = DateTime.Now
                            };

                            // Yeni ürünü ekliyoruz
                            _db.Add(yeniUrun);
                            await _db.SaveChangesAsync();

                            TempData["SuccessMessage"] = "Yeni ürün başarıyla eklendi.";

                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Ürün eklenemedi.";
                            return RedirectToAction("DepoYonetim");
                        }
                    }

                    // DepoYonetim sayfasına yönlendiriyoruz
                    return RedirectToAction("DepoYonetim");
                }
               
            

            // Eğer ürün bulunamazsa, NotFound döndürüyoruz
            return NotFound();
        }



        [AllowAnonymous]
        public IActionResult YeniStokIstek() {
            var urunler = _db.satinAlmaUrunBilgileris.Select(t=>t.UrunAdi).ToList();
            if (urunler.Count > 0)
            {
                ViewBag.Urunler = urunler;
            }
            else
            {
                ViewBag.Urunler = null;
            }
                return View(); 
        }


        [AllowAnonymous]
        public IActionResult sOnay()
        {
            var talep = _db.MalzemeTalep.DefaultIfEmpty().Where(t => t.Durum == "Teklif Bekleniyor" || t.Durum == "Beklemede").ToList();
            ViewBag.talep = talep;
            var durum = _db.satinAlmaSiparisler.OrderByDescending(t=>t.SiparisID).ToList(); 
            return View(durum); 
        }
        [HttpPost]
        public async Task<IActionResult> YeniStokBildirim(string MesajBaslik, string UrunAdi, string IstekVeren, string OlusturmaTarihi)
        {
            string mesaj = $"{MesajBaslik} - {UrunAdi} - {IstekVeren}";

            var yeniBildirim = new Bildirimler
            {
                Mesaj = mesaj,
                OlusturmaTarihi = DateTime.Parse(OlusturmaTarihi), 
                Okundu = false 
            };

            _db.Bildirimler.Add(yeniBildirim);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Yeni stok İsteği başarıyla eklendi.";

            return View("Mesaj");
        }

        public IActionResult TreeBildirim() {
            try
            {

                var bildirim = _db.Bildirimler
                    .Where(t => t.Okundu == false && t.Mesaj.StartsWith("TreeColor"))
                    .ToList();
                foreach (var item in bildirim)
                {
                    item.Okundu = true;
                    _db.SaveChanges();
                }
                TreeColor treeColorService = new TreeColor();
                var rowsToNotify = treeColorService.CheckAmbalajTuru();

                if (rowsToNotify != null && rowsToNotify.Any())
                {
                    var kartNoList = rowsToNotify.GroupBy(row => row.KartNo)
                        .Select(group => group.First()).Select(row => row.KartNo).ToList();

                    var bildirimSayisi = _db.Bildirimler
                    .Where(t => t.Okundu == false && t.Mesaj.StartsWith("TreeColor"))
                    .ToList().Count;

                    if (bildirimSayisi != null || bildirimSayisi > 0)
                    {
                        ViewBag.Bildirim = bildirimSayisi;
                    }
                    else
                    {
                        ViewBag.Bildirim = 0;
                    }
                    rowsToNotify = rowsToNotify.GroupBy(row => row.KartNo)
                        .Select(group => group.First()).OrderByDescending(t => t.SonGirisTarih).ToList();
                    return View(rowsToNotify);

                }
                else
                {
                    var service = new TreeColor();
                    var rows = service.CheckAmbalajTuru();

                    
                        TempData["Message"] = "Kriterlere uyan kayıt bulunamadı.";
                        rows = new List<DepoRow>();
                        return View(rows);

                    
                }

            }
            catch (Exception ex)
            {
                var service = new TreeColor();
                var rows = service.CheckAmbalajTuru();

                if (rows == null)
                {
                    TempData["Message"] = "Kriterlere uyan kayıt bulunamadı.";
                    rows = new List<DepoRow>();
                    return View(rows);

                }
                return BadRequest($"Hata oluştu: {ex.Message}");
            }

             }

        public IActionResult SiparisYonetim()
        {
            try
            {
                var display = new List<Malzemeler>();

                // Beklemede olanları al
                var beklemedeGruplari = _db.MalzemeTalep
                    .Where(t => t.Durum == "Beklemede")  // Beklemede olanları al
                    .ToList() // Veritabanından alıp belleğe taşı
                    .GroupBy(t => t.FormId)  // FormId'ye göre grupla
                    .OrderByDescending(g => g.First().Id)  // Grupları Id'ye göre azalan sırayla sırala
                    .ToList();

                // Teklif Bekleniyor olanları al
                var teklifBekleniyorGruplari = _db.MalzemeTalep
                    .Where(t => t.Durum.Equals("Teklif Bekleniyor")||t.Durum.Contains(" Aktarıldı"))  // Teklif Bekleniyor olanları al
                    .ToList() // Veritabanından alıp belleğe taşı
                    .GroupBy(t => t.FormId)  // FormId'ye göre grupla
                    .OrderByDescending(g => g.First().Id)  // Grupları Id'ye göre azalan sırayla sırala
                    .ToList();

                // "Beklemede" olanlar üstte, "Teklif Bekleniyor" olanlar altta olacak şekilde birleştir
                var tumGruplar = beklemedeGruplari.Concat(teklifBekleniyorGruplari).ToList();

                // Grupları açıp, gerekli alanları alarak display listesine ekliyoruz
                if (tumGruplar.Any())
                {
                    display = tumGruplar.SelectMany(group => group)
                        .Select(talep => new Malzemeler
                        {
                            FormId = talep.FormId,
                            Id = talep.Id,

                            TerminTarihi = talep.TerminTarihi,
                            MalzemeAdi = talep.MalzemeAdi,
                            TalepMiktari = talep.TalepMiktari,
                            MevcutStok = talep.MevcutStok,
                            Aciklama = talep.Aciklama,
                            Durum = talep.Durum,
                            Urun=talep.Urun
                        })
                        .ToList();
                }

                // TreeColor servis işlemleri
                TreeColor treeColorService = new TreeColor();
                var rowsToNotify = treeColorService.CheckAmbalajTuru();

                if (rowsToNotify != null && rowsToNotify.Any())
                {
                    var today = DateTime.Now.Date;
                    var sonGt = rowsToNotify
                                .Where(row => DateTime.TryParse(row.SonGirisTarih, out DateTime sonGirisTarih) && sonGirisTarih.Date == today)
                                .ToList();

                    var rowsToDisplay = sonGt
                        .GroupBy(row => row.KartNo)
                        .Select(group => group.First())
                        .Select(row => new { row.KartNo, row.AmbalajTuru, row.SonGirisTarih })
                        .ToList();

                    var bildirimSayisi = _db.Bildirimler
                        .Count(t => t.Okundu == false && t.Mesaj.StartsWith("TreeColor"));

                    ViewBag.Bildirim = bildirimSayisi > 0 ? bildirimSayisi : 0;
                }

                return View(display);
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata oluştu: {ex.Message}");
            }
        }

        public IActionResult SiparisYonetimTermin()
        {
            try
            {
                
               
                // Teklif Bekleniyor veya Tedarikçiye Aktarıldı olanlar
                var sipasris = _db.satinAlmaSiparisler
                    .Where(t => t.Durum == "Sipariş Verildi")
                    .ToList()
                        .OrderBy(t => t.SiparisTerminTarihi) // Termin tarihi yaklaşan en önce gelir
                    .ToList();

                // Grupları birleştir
                
                // TreeColor servisi
                TreeColor treeColorService = new TreeColor();
                var rowsToNotify = treeColorService.CheckAmbalajTuru();

                if (rowsToNotify != null && rowsToNotify.Any())
                {
                    var today = DateTime.Now.Date;
                    var sonGt = rowsToNotify
                        .Where(row => DateTime.TryParse(row.SonGirisTarih, out DateTime sonGirisTarih) && sonGirisTarih.Date == today)
                        .ToList();

                    var rowsToDisplay = sonGt
                        .GroupBy(row => row.KartNo)
                        .Select(group => group.First())
                        .Select(row => new { row.KartNo, row.AmbalajTuru, row.SonGirisTarih })
                        .ToList();

                    var bildirimSayisi = _db.Bildirimler
                        .Count(t => t.Okundu == false && t.Mesaj.StartsWith("TreeColor"));

                    ViewBag.Bildirim = bildirimSayisi > 0 ? bildirimSayisi : 0;
                }

                return View(sipasris);
            }
            catch (Exception ex)
            {
                return BadRequest($"Hata oluştu: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult GorulduYap([FromBody] List<SatinAlmaSiparis> selectedSiparisler)
        {
            if (selectedSiparisler == null || !selectedSiparisler.Any())
            {
                return BadRequest("Hiçbir sipariş seçilmedi.");
            }

            try
            {
                foreach (var siparis in selectedSiparisler)
                {
                    var yeniSiparis = new SatinAlmaSiparis
                    {
                        TalepID = siparis.TalepID,
                        // Gelen tarihi direkt kullanıyoruz, 'ToShortDateString()' yerine DateTime'ı direkt alıyoruz
                        SiparisTarihi = siparis.SiparisTarihi,
                        OnayDurumu = siparis.OnayDurumu,
                        Durum = siparis.Durum,
                        SiparisNotu = siparis.SiparisNotu
                    };

                    // Siparişleri veritabanına ekle
                    _db.satinAlmaSiparisler.Add(yeniSiparis);
                }

                // Değişiklikleri kaydet
                _db.SaveChanges();

                // Başarılı yanıt döndür
                return Ok(new { message = "Siparişler başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                // Hata durumunda geri bildirim
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }
        public class UrunYonetimViewModel
        {
            public int UrunID { get; set; }
            public string UrunAdi { get; set; }
            public string UrunKodu { get; set; }
            public string Kategori { get; set; }
            public string Aciklama { get; set; }
            public decimal ToplamStok { get; set; }
            public string Birim { get; set; }
            public string Tedarikci { get; set; }
            public decimal? BirimFiyati { get; set; }
            public DateTime? SonKullanmaTarihi { get; set; }
            public DateTime? KayitTarihi { get; set; }
            public DateTime? GuncellemeTarihi { get; set; }
            public decimal? MinStok { get; set; }
            public string UrunKart { get; set; }
        }

        public IActionResult UrunYonetim()
        {
            var Urunler = _db.satinAlmaUrunBilgileris.DefaultIfEmpty()
     .ToList() // Veriyi EF'den belleğe al
     .GroupBy(t => t.UrunAdi)
     .Select(g =>
     {
         var sonKayit = g.OrderByDescending(x => x.KayitTarihi).FirstOrDefault();

         return new UrunYonetimViewModel
         {
             UrunID = sonKayit.UrunID,
             UrunAdi = g.Key,
             UrunKodu = sonKayit.UrunKodu,
             Kategori = sonKayit.Kategori,
             Aciklama = sonKayit.Aciklama,
             ToplamStok = g.Sum(x => x.StokMiktari),
             Birim = sonKayit.Birim,
             Tedarikci = sonKayit.Tedarikci,
             BirimFiyati = sonKayit.BirimFiyati,
             SonKullanmaTarihi = sonKayit.SonKullanmaTarihi,
             KayitTarihi = sonKayit.KayitTarihi,
             GuncellemeTarihi = sonKayit.GuncellemeTarihi,
             MinStok = sonKayit.MinStok,
             UrunKart = sonKayit.UrunKart
         };
     })
     .OrderByDescending(x => x.UrunID)
     .ToList();


            bool bildirimEklendi = false;

            foreach (var item in Urunler)
            {
                if (item.MinStok.HasValue && item.ToplamStok < item.MinStok.Value)
                {
                    string mesaj = $"Min Stok Uyarısı - {item.UrunAdi} - Min Stok Değerinin Altında";

                    var mevcutBildirim = _db.Bildirimler.FirstOrDefault(b => b.Mesaj == mesaj);

                    if (mevcutBildirim == null)
                    {
                        var yeniBildirim = new Bildirimler
                        {
                            Mesaj = mesaj,
                            OlusturmaTarihi = DateTime.Now,
                            Okundu = false
                        };

                        _db.Bildirimler.Add(yeniBildirim);
                        bildirimEklendi = true;
                    }
                }
            }

            if (bildirimEklendi)
            {
                _db.SaveChanges();
            }

            return View(Urunler); // View tipi: List<UrunYonetimViewModel>
        }



        [HttpPost]
        public IActionResult KytMinStok([FromBody] MinStokModel model)
        {
            // Model doğrulama
            if (model == null || model.Id <= 0 || model.MinStok < 0)
            {
                return Json(new { success = false, message = "Geçersiz veri gönderildi." });
            }

            // Ürünü ID ile al
            var ID = _db.satinAlmaUrunBilgileris.FirstOrDefault(t => t.UrunID == model.Id);
            if (ID == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı (ID ile)." });
            }

            // UrunKodu'nu normalize et
            var parts = ID.UrunKodu?.Split('-');
            if (parts == null || parts.Length < 2)
            {
                return Json(new { success = false, message = "Ürün kodu formatı geçersiz." });
            }

            var normalized = string.Join("-", parts.Skip(1)); // İlk parçayı at

            // Normalized kodu ile eşleşen ürünleri bul
            var urunler = _db.satinAlmaUrunBilgileris
                             .Where(u => u.UrunKodu != null && u.UrunKodu.EndsWith(normalized))
                             .ToList();

            if (urunler == null || !urunler.Any())
            {
                return Json(new { success = false, message = "Eşleşen ürün bulunamadı." });
            }

            // Her biri için MinStok güncelle
            foreach (var urun in urunler)
            {
                urun.MinStok = model.MinStok;
            }

            _db.SaveChanges();

            return Json(new { success = true });
        }

        public class MinStokModel
        {
            public int Id { get; set; }
            public int MinStok { get; set; }
        }

        [HttpPost]
        public IActionResult DevamliUrun(Malzemeler model)
        {
            if (model != null)
            {
                var yeniUrun = new satinAlmaUrunBilgileri
                {
                    UrunAdi = model.MalzemeAdi.ToUpper(),
                    UrunKart = "DevamliK",
                    Aciklama = model.Aciklama,
                    StokMiktari = 0,
                    Birim = "Adet",
                    BirimFiyati = 0,
                    KayitTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    SonKullanmaTarihi = model.TerminTarihi
                };
                var tedarikci = _db.satinAlmaTedarikcilers.ToList();
                if (tedarikci.Any()) { ViewBag.Ted = tedarikci; } else { ViewBag.Ted = "Kayıtlı tedarikçi yok."; }
                ViewBag.Ted = tedarikci;
                return View("UrunEkle", yeniUrun);
            }
            TempData["Error"] = "Ürüne Kart Açılamaz ";
            return View("Mesaj");
        }
        [HttpPost]
        public IActionResult TekSeferlikUrun(Malzemeler model)
        {
            if (model!=null)
            {
                var yeniUrun = new satinAlmaUrunBilgileri
                {
                    UrunAdi = model.MalzemeAdi.ToUpper(),
                    UrunKart = "TekUrunK",

                    Aciklama = model.Aciklama,
                    StokMiktari = 0,
                    Birim = "Adet",
                    BirimFiyati = 0,
                    KayitTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    SonKullanmaTarihi = model.TerminTarihi
                };
                var tedarikci = _db.satinAlmaTedarikcilers.ToList();
                if (tedarikci.Any()) { ViewBag.Ted = tedarikci; } else { ViewBag.Ted = "Kayıtlı tedarikçi yok."; }
                ViewBag.Ted = tedarikci;
                return View("UrunEkle", yeniUrun);
            }
            TempData["Error"] = "Ürüne Kart Açılamaz EXP::0xop58*-74";
            return View("Mesaj");
        }


        public IActionResult UrunEkle() {
            var tedarikci = _db.satinAlmaTedarikcilers.ToList();
            if (tedarikci.Any()) { ViewBag.Ted = tedarikci; } else { ViewBag.Ted = "Kayıtlı tedarikçi yok."; }
            ViewBag.Ted = tedarikci;
           
           
            return View(); }

        [HttpPost]
        public IActionResult urnEkle(satinAlmaUrunBilgileri Urun,int Id)
        {
            if (Urun == null)
            {
                ViewBag.Message = "Ürün bilgileri eksik.";
                return View("UrunYonetim");
            }

            try
            {
                int tdrk = Convert.ToInt32(Urun.Tedarikci);
                Urun.UrunKodu = GenerateProductCode(tdrk, Urun.UrunAdi);

                var mevcutUrun = _db.satinAlmaUrunBilgileris
                    .FirstOrDefault(u => u.UrunKodu == Urun.UrunKodu && u.Kategori == Urun.Kategori);

                if (mevcutUrun != null)
                {
                    TempData["Error"] = $"Bu ürün kodu ve kategoriye sahip bir ürün zaten mevcut. -->> {mevcutUrun.UrunKodu} Kodlu Ürün ";
                    return View("Mesaj"); 
                }

                Urun.BirimFiyati = 0;
                Urun.SonKullanmaTarihi = DateTime.Now;
                var yeniUrun = new satinAlmaUrunBilgileri
                {
                    UrunAdi = Urun.UrunAdi.ToUpper(new System.Globalization.CultureInfo("en-US")),
                    UrunKodu = Urun.UrunKodu,
                    UrunKart = Urun.UrunKart,

                    Aciklama = Urun.Aciklama,
                    Kategori = Urun.Kategori,
                    Tedarikci = Urun.Tedarikci,
                    StokMiktari = 0,
                    Birim = Urun.Birim,
                    BirimFiyati = 0,
                    KayitTarihi = DateTime.Now,
                    GuncellemeTarihi = DateTime.Now,
                    SonKullanmaTarihi = DateTime.Now
                };

                _db.satinAlmaUrunBilgileris.Add(yeniUrun);
                if (Id != 0)
                {
                    var malzeme = _db.MalzemeTalep.Find(Id);

                    malzeme.Urun = true;
                }
               
                _db.SaveChanges(); 

                TempData["Message"] = "Ürün başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ürün ekleme sırasında bir hata oluştu: {ex.Message}";
            }

            return View("Mesaj"); 
        }

        [HttpPost]
        public IActionResult tAta([FromBody] TedarikciUpdateModel model)
        {
            if (string.IsNullOrEmpty(model?.id) || string.IsNullOrEmpty(model?.yTedarikci))
            {
                // Eğer ID veya yeni tedarikçi adı boşsa hata mesajı döndür
                return Json(new { success = false, message = "Lütfen geçerli bir ürün ID'si ve tedarikçi adı girin." });
            }

            // ID ve yeni tedarikçi adı doğru geliyorsa, devam et
            Console.WriteLine($"Received Product ID: {model.id} | Supplier: {model.yTedarikci}");

            // Veritabanından ilgili ürünü al
            var urun = _db.satinAlmaUrunBilgileris.DefaultIfEmpty().FirstOrDefault(t => t.UrunKodu.Equals(model.id));

            if (urun == null)
            {
                // Eğer ürün bulunamazsa hata mesajı döndür
                return Json(new { success = false, message = $"Ürün ID'si {model.id} ile eşleşen bir kayıt bulunamadı." });
            }

            // Tedarikçi bilgisini güncelle
            urun.Tedarikci = model.yTedarikci;

            // Değişiklikleri kaydet
            _db.SaveChanges();

            // Başarı mesajını döndür
            return Json(new { success = true, message = "Tedarikçi başarıyla güncellendi." });
        }

        // Model class
        public class TedarikciUpdateModel
        {
            public string? id { get; set; }
            public string yTedarikci { get; set; }
        }


        private string GenerateProductCode(int tId, string productName)
        {
            string sanitizedProductName = SanitizeProductName(productName);

            string startPart = sanitizedProductName.Length >= 2 ? sanitizedProductName.Substring(0, 2).ToLowerInvariant() : "xx";
            string middlePart = sanitizedProductName.Length >= 4 ? sanitizedProductName[sanitizedProductName.Length / 2].ToString().ToLowerInvariant() : "x";
            string endPart = sanitizedProductName.Length >= 2 ? sanitizedProductName.Substring(sanitizedProductName.Length - 2, 2).ToLowerInvariant() : "xx";

            string startAscii = ConvertToAscii(startPart);
            string middleAscii = ConvertToAscii(middlePart);
            string endAscii = ConvertToAscii(endPart);

            return $"{tId}-{startAscii}-{middleAscii}-{endAscii}";
        }

        private string SanitizeProductName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "XXX";

            // Türkçe karakterleri sadeleştir
            text = text
                .Replace("ç", "c").Replace("Ç", "C")
                .Replace("ğ", "g").Replace("Ğ", "G")
                .Replace("ı", "x").Replace("I", "X")  // 'ı' ve 'I' yerine 'x'
                .Replace("İ", "X").Replace("i", "x")  // 'i' yerine 'x'
                .Replace("ö", "o").Replace("Ö", "O")
                .Replace("ş", "s").Replace("Ş", "S")
                .Replace("ü", "u").Replace("Ü", "U");

            // Yalnızca harf ve rakamlar, 'i' türevleri hiç kalmaz
            text = new string(text
                .ToUpperInvariant()
                .Where(c => char.IsLetterOrDigit(c))
                .ToArray());

            return text;
        }

        private string ConvertToAscii(string input)
        {
            return string.Join("", input.Select(c => ((int)c).ToString()));
        }






    }
}
