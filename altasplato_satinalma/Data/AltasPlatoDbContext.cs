using Microsoft.EntityFrameworkCore;
using altasplato_satinalma.Models;

namespace altasplato_satinalma.Data
{
    public class AltasPlatoDbContext : DbContext
    {
        public AltasPlatoDbContext(DbContextOptions<AltasPlatoDbContext> options) : base(options) { }

        public DbSet<CalisanBilg> CalisanBilg { get; set; }
        public DbSet<Malzemeler> MalzemeTalep { get; set; }
        public DbSet<Formlar> Formlar { get; set; }
        public DbSet<Degerlendir> satinAlmaDegerlendir { get; set; }
        public DbSet<SiparisTalepPdf> SiparisTalepPdf { get; set; }
        public DbSet<satinAlmaTalep> satinAlmaTaleps { get; set; }
        public DbSet<SatinAlmaDepos> satinAlmaDepos { get; set; }
        public DbSet<SatinAlmaTedarikciler> satinAlmaTedarikcilers { get; set; }
        public DbSet<satinAlmaUrunBilgileri> satinAlmaUrunBilgileris { get; set; }
        public DbSet<SatinAlmaSiparis> satinAlmaSiparisler { get; set; }
        public DbSet<Bildirimler> Bildirimler { get; set; }
        public  DbSet<satinAlmaFatura> satinAlmaFatura { get; set; }
        public DbSet<satinAlmaFaturaKalem> satinAlmaFaturaKalem { get; set; }
        public DbSet<SatinAlmaDepoUrun_Es> SatinAlmaDepoUrun_Es { get; set; }

        //maliyet hesabı
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<satinAlmaFatura>()
                .HasKey(f => f.Faturaid);

            builder.Entity<satinAlmaFaturaKalem>()
                .HasKey(f => f.FaturaKalemid);

            base.OnModelCreating(builder);
        }
    }
}
