//Classe para deserializar a resposta da API do Brapi
public class Stock
{
    public List<StockData> results { get; set; } = new List<StockData>();
    public DateTime requestedAt { get; set; }
    public int took { get; set; }
}

public class StockData
{
    public string symbol { get; set; } = "";
    public string shortName { get; set; } = "";
    public string longName { get; set; } = "";
    public string currency { get; set; } = "";
    public decimal regularMarketPrice { get; set; }
    public decimal regularMarketDayHigh { get; set; }
    public decimal regularMarketDayLow { get; set; }
    public string regularMarketDayRange { get; set; } = "";
    public decimal regularMarketChange { get; set; }
    public decimal regularMarketChangePercent { get; set; }
    public DateTime regularMarketTime { get; set; }
    public long marketCap { get; set; }
    public long regularMarketVolume { get; set; }
}