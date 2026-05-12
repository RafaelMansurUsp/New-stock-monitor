using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;



class Program
{
    static async Task Main(string[] args)
    {   
        decimal sellprice = decimal.Parse(args[1]);
        decimal buyprice = decimal.Parse(args[2]);
        List<string> tokens = new List<string>();
        //nao esquece de trocar aqui depois
         try
        {
        
            using (StreamReader sr = new StreamReader("ConfigFile.txt"))
            {
                string line;
                while ((line = sr.ReadLine()!) != null)
                {
                    tokens.Add(line);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }

        // tem que mudar aqui para tirar a real token antes de botar no git
        string tokenBrapi = tokens[0];
        string ticker = args[0]; 
        string url = $"https://brapi.dev/api/quote/{ticker}?token={tokenBrapi}";

        using HttpClient client = new HttpClient();


        // Variáveis para controlar o envio de e-mails
        bool isOverSellPrice = false;
        bool isUnderBuyPrice = false;

        // aqui comecarua  o while -----------------
        string response = await client.GetStringAsync(url);

        if (response == null)
        {   
            Console.WriteLine("Unable to get stock data");
            return;
           

        }
        Stock? stock = JsonSerializer.Deserialize<Stock>(response);
            //Console.WriteLine($"Buy: {buyprice}, Sell: {sellprice}\n");
            //Console.WriteLine($"{stock.results[0].regularMarketPrice}\n");

        string plainTextContent = ""; 
        if(stock?.results[0].regularMarketPrice > sellprice && !isOverSellPrice)
        {
            plainTextContent = $"O preço do ativo {ticker} está acima do valor de venda!";
            await SendMail(tokens[1],plainTextContent,tokens[2]);
            isOverSellPrice = true;

        } else if (stock?.results[0].regularMarketPrice < buyprice && !isUnderBuyPrice)
        {
            plainTextContent = $"O preço do ativo {ticker} está abaixo do valor de compra!";
            await SendMail(tokens[1],plainTextContent,tokens[2]);
            isUnderBuyPrice = true;
        }
        else
        {
            isOverSellPrice = false;
            isUnderBuyPrice = false;
        }

        //aqui tbm
        

        // Enviar o e-mail
        //ver se da para fazer uma classe bonitinha para o corpo 
        //var responseEmail = await emailClient.SendEmailAsync(msg);
        //Console.WriteLine($"Status Code: {responseEmail.StatusCode}");
    }

    static async Task SendMail(string token, string plainTextContent, string user)
    {
        var apiKey = token;  
        var emailClient = new SendGridClient(apiKey);
        //aqui tambem
        var from = new EmailAddress(user, "StockMonitor");
        var subject = "Alerta de Cotação";
        //botar o email no documento de configuração depois
        var to = new EmailAddress("rafaelbamansur@usp.br", "User");
        var htmlContent = $"<strong>{plainTextContent}</strong>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        //NAO ESQUECE DE DESCOMENTAR AQUI 
        //var responseEmail = await emailClient.SendEmailAsync(msg);
        //Console.WriteLine($"Status Code: {responseEmail.StatusCode}");

    }
}