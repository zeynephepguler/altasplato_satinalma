using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace altasplato_satinalma.Models
{
   

    public class satinAlmaFaturaKalem
    {
        [Key]
        public int FaturaKalemid { get; set; }
        public string Aciklama { get; set; }
        public int Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal Tutar { get; set; }
        public int Faturaid { get; set; }

        //public virtual satinAlmaFatura Faturaid { get; set; }
    }
}
