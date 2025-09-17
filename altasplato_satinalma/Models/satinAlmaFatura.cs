using System;
using System.Collections.Generic;
namespace altasplato_satinalma.Models
{
    
    public class satinAlmaFatura
    {
        

        public int Faturaid { get; set; }
        public string FaturaSeriNo { get; set; }
        public string FaturaSıraNo { get; set; }
        public DateTime Tarih { get; set; }
        public string VergiDairesi { get; set; }
        public string Saat { get; set; }
        public string TeslimEden { get; set; }
        public string TeslimAlan { get; set; }
        public decimal Toplam { get; set; }

       
    }
}
