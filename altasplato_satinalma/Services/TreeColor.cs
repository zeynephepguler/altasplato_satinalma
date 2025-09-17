using Microsoft.Data.SqlClient;

namespace altasplato_satinalma.Services
{
    public class TreeColor
    {
        private readonly string _sourceConnString = "Server=sqlsrv2016;User=SA;Password=4Ltas2019*;Database=ALTAS_2014;TrustServerCertificate=True;";

        public List<DepoRow> CheckAmbalajTuru()
        {
            var ambalajTuruListesi = new List<string> { "shrink", "koruma bandı", "karton", "etiket" };

            string query = @"
                SELECT * 
                FROM ALTAS_2014.dbo.depo
                WHERE (" + string.Join(" OR ", ambalajTuruListesi.Select((ambalaj, index) => $"AmbalajTuru LIKE @AmbalajTuru{index}")) + @")
                AND SonGirisTarih >= @StartDate AND SonGirisTarih <= @EndDate";

            var result = GetRowsToNotify(query, ambalajTuruListesi);

            // Eğer sonuç boşsa null döndür
            return result.Any() ? result : null;
        }

        private List<DepoRow> GetRowsToNotify(string query, List<string> ambalajTuruListesi)
        {
            var rows = new List<DepoRow>();

            using (var connection = new SqlConnection(_sourceConnString))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    for (int i = 0; i < ambalajTuruListesi.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@AmbalajTuru{i}", $"%{ambalajTuruListesi[i]}%");
                    }

                    command.Parameters.AddWithValue("@StartDate", DateTime.Now.AddDays(-5).Date);  
                    command.Parameters.AddWithValue("@EndDate", DateTime.Now.Date); 

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (DateTime.TryParse(reader["SonGirisTarih"].ToString(), out DateTime sonGirisTarih))
                            {
                                if ((sonGirisTarih - DateTime.Now).TotalDays <= 3)
                                {
                                    var row = new DepoRow
                                    {
                                        KartNo = reader["KartNo"].ToString(),
                                        AmbalajTuru = reader["AmbalajTuru"].ToString(),
                                        SonGirisTarih = sonGirisTarih.ToString("yyyy-MM-dd") 
                                    };
                                    rows.Add(row);
                                }
                            }
                        }
                    }
                }
            }

            return rows;
        }
    }

    // Depo verisini temsil eden model
    public class DepoRow
    {
        public string KartNo { get; set; }
        public string AmbalajTuru { get; set; }
        public string SonGirisTarih { get; set; }
    }
}
