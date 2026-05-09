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
        float sellprice = float.Parse(args[1]);
        float buyprice = float.Parse(args[2]);
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
        string response = await client.GetStringAsync(url);
        Console.WriteLine($"Buy: {buyprice}, Sell: {sellprice}\n");
        Console.WriteLine(response);
        //aqui tbm
        var apiKey = tokens[1];  
        var emailClient = new SendGridClient(apiKey);
        //aqui tambem
        var from = new EmailAddress("rafaelbamansur@gmail.com", "rafael");
        var subject = "Alerta de Cotação";
        //botar o email no documento de configuração depois
        var to = new EmailAddress("rafaelbamansur@usp.br", "Destinatário");
        var plainTextContent = "O preço do ativo PETR4 está acima do limite de venda!";
        var htmlContent = "<strong>O preço do ativo PETR4 está acima do limite de venda!</strong>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        // Enviar o e-mail
        //ver se da para fazer uma classe bonitinha para o corpo 
        var responseEmail = await emailClient.SendEmailAsync(msg);
        Console.WriteLine($"Status Code: {responseEmail.StatusCode}");
    }
}