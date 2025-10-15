using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace altasplato_satinalma.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KartNo = table.Column<int>(type: "int", nullable: false),
                    Mesaj = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Okundu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalisanBilg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserLastname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmirIsmi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserBirthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserShort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bolum = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalisanBilg", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Formlar",
                columns: table => new
                {
                    FormNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SirketAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormTuru = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TalepEden = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Onaylayan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formlar", x => x.FormNo);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaDegerlendir",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DokumanNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevizyonNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RevizyonTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DegerlendirmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Adi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelefonNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EPosta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IlgiliKisi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FaaliyetKonusu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fiyat = table.Column<int>(type: "int", nullable: false),
                    OdemeEsnekligi = table.Column<int>(type: "int", nullable: false),
                    UrunKalitesi = table.Column<int>(type: "int", nullable: false),
                    Zamanindalik = table.Column<int>(type: "int", nullable: false),
                    Uzmanlik = table.Column<int>(type: "int", nullable: false),
                    DestekDokumanlar = table.Column<int>(type: "int", nullable: false),
                    SatisSonrasiHizmet = table.Column<int>(type: "int", nullable: false),
                    Sureklilik = table.Column<int>(type: "int", nullable: false),
                    KaliteYonetimSistemi = table.Column<int>(type: "int", nullable: false),
                    DofAcilsinMi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DofAciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Siniflandirma = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DegerlendirmeSonucu = table.Column<int>(type: "int", nullable: true),
                    DegerlendirenAdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaDegerlendir", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaDepos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TedarikciId = table.Column<int>(type: "int", nullable: false),
                    UrunAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    KalanAdet = table.Column<int>(type: "int", nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SiparisOnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SiparisTerminTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UrunGelisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GelenUrunMiktari = table.Column<int>(type: "int", nullable: false),
                    KontrolEden = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Karar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FotoPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaDepos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SatinAlmaDepoUrun_Es",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrunId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UrunNormalizedId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatinAlmaDepoUrun_Es", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaFatura",
                columns: table => new
                {
                    Faturaid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaSeriNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FaturaSıraNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VergiDairesi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Saat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeslimEden = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeslimAlan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Toplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaFatura", x => x.Faturaid);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaFaturaKalem",
                columns: table => new
                {
                    FaturaKalemid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Miktar = table.Column<int>(type: "int", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Faturaid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaFaturaKalem", x => x.FaturaKalemid);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaSiparisler",
                columns: table => new
                {
                    SiparisID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TalepID = table.Column<int>(type: "int", nullable: false),
                    UrunAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TedarikciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StokID = table.Column<int>(type: "int", nullable: false),
                    UrunAdedi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirimFiyat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiparisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SiparisTerminTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OnayDurumu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Konum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiparisNotu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosyaVerisi = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaSiparisler", x => x.SiparisID);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaTaleps",
                columns: table => new
                {
                    TalepNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TalepTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UrunKodu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TalepEdenBolum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TalepEdilenUrun = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TalepEdilenMiktar = table.Column<int>(type: "int", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaTaleps", x => x.TalepNo);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaTedarikcilers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TedarikciAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TedarikciTelefon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TedarikciAdres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TedarikciMail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TedarikciNot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaTedarikcilers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "satinAlmaUrunBilgileris",
                columns: table => new
                {
                    UrunID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UrunKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StokMiktari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tedarikci = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BirimFiyati = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SonKullanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinStok = table.Column<int>(type: "int", nullable: false),
                    UrunKart = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_satinAlmaUrunBilgileris", x => x.UrunID);
                });

            migrationBuilder.CreateTable(
                name: "SiparisTalepPdf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DosyaAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DosyaVerisi = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiparisTalepPdf", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MalzemeTalep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormId = table.Column<int>(type: "int", nullable: false),
                    MalzemeAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MevcutStok = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TalepMiktari = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Urun = table.Column<bool>(type: "bit", nullable: false),
                    TalepEden = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MalzemeTalep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MalzemeTalep_Formlar_FormId",
                        column: x => x.FormId,
                        principalTable: "Formlar",
                        principalColumn: "FormNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MalzemeTalep_FormId",
                table: "MalzemeTalep",
                column: "FormId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "CalisanBilg");

            migrationBuilder.DropTable(
                name: "MalzemeTalep");

            migrationBuilder.DropTable(
                name: "satinAlmaDegerlendir");

            migrationBuilder.DropTable(
                name: "satinAlmaDepos");

            migrationBuilder.DropTable(
                name: "SatinAlmaDepoUrun_Es");

            migrationBuilder.DropTable(
                name: "satinAlmaFatura");

            migrationBuilder.DropTable(
                name: "satinAlmaFaturaKalem");

            migrationBuilder.DropTable(
                name: "satinAlmaSiparisler");

            migrationBuilder.DropTable(
                name: "satinAlmaTaleps");

            migrationBuilder.DropTable(
                name: "satinAlmaTedarikcilers");

            migrationBuilder.DropTable(
                name: "satinAlmaUrunBilgileris");

            migrationBuilder.DropTable(
                name: "SiparisTalepPdf");

            migrationBuilder.DropTable(
                name: "Formlar");
        }
    }
}
