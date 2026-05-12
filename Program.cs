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
        decimal sellprice = decimal.Parse(args[1]); //valor de venda 
        decimal buyprice = decimal.Parse(args[2]); //valor de compra
        List<string> tokens = new List<string>();//dados para configurar as APIS

        //lê o arquivo de config para as APIS e emails 
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
        //Se o arquivo não pôde ser lido, impossível continuar
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
            return;
        }
        //API para pegar a cotação das bolsas
        string tokenBrapi = tokens[0]; //token da API de cotação
        string ticker = args[0]; //ativo a ser monitorado
        string url = $"https://brapi.dev/api/quote/{ticker}?token={tokenBrapi}";

        using HttpClient client = new HttpClient();


        // Variáveis para controlar o envio de e-mails
        bool isOverSellPrice = false;
        bool isUnderBuyPrice = false;
        bool isInPrincerange = false;
        // Variável para controlar o loop de monitoramento do mercado
        bool isMarketOpen = true;
        // Clarificação do invervalo de monitoramento, para não sobrecarregar a API,
        const int ONE_MINUTE = 60000; // 1 minuto em milissegundos
        // Horário de abertura e fechamento do mercado (10:00 às 17:00)
        TimeSpan open = new TimeSpan(10, 0, 0);
        TimeSpan close = new TimeSpan(17, 0, 0);

        

        while (isMarketOpen)
        {
            //Obtendo o horário atual e verificando se o mercado está aberto
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            if (currentTime <= open || currentTime >= close)
            {
                isMarketOpen = false;
            }
            string response = await client.GetStringAsync(url);

            if (response == null)
            {   
                //Se a resposta da API for nula, impossível continuar
                Console.WriteLine("Unable to get stock data");
                return;
            }
            //Transforma resposta do JSON em formato mais legivel e utilizável 
            Stock? stock = JsonSerializer.Deserialize<Stock>(response);
            decimal currentPrice = stock?.results[0].regularMarketPrice ?? 0;
            string plainTextContent = ""; 
            //Preço dentro da área de venda
            if(currentPrice >= sellprice && !isOverSellPrice)
            {
                plainTextContent = $"O preço do ativo {ticker} está acima do valor de venda!(R${currentPrice})";
                Console.WriteLine(plainTextContent);
                await SendMail(tokens[1],plainTextContent,tokens[2], tokens[3]);
                isOverSellPrice = true; //previnir spam de email 
                isInPrincerange = false;

            //Preço dentro da área de compra
            } else if (currentPrice <= buyprice && !isUnderBuyPrice)
            {
                plainTextContent = $"O preço do ativo {ticker} está abaixo do valor de compra!(R${currentPrice})";
                Console.WriteLine(plainTextContent);
                await SendMail(tokens[1],plainTextContent,tokens[2], tokens[3]);
                isUnderBuyPrice = true; //previnir spam de email 
                isInPrincerange = false;
            }
            //Preço fora da área de interesse
            else if (currentPrice < sellprice && currentPrice > buyprice && !isInPrincerange)
                {
                    Console.WriteLine($"O preço do ativo {ticker} está fora da área de interesse.(R${currentPrice})");
                    isOverSellPrice = false;
                    isUnderBuyPrice = false;
                    isInPrincerange = true;
                }
            //Não há um else geral para evitar spam de mensagens no terminal
            //Delay de 1 minuto para não sobrecarregar a API e evitar spam de mensagens
            await Task.Delay(ONE_MINUTE);
        }
        Console.WriteLine("Mercado fechado. O programa será encerrado.");
    }
    //Função para enviar email utilizando a API do SendGrid
    static async Task SendMail(string token, string plainTextContent, string senderEmail, string recipientEmail)
    {
        var apiKey = token;  
        var emailClient = new SendGridClient(apiKey);
        var from = new EmailAddress(senderEmail, "StockMonitor");
        var subject = "Alerta de Cotação";
        var to = new EmailAddress(recipientEmail, "User");
        var htmlContent = $"<strong>{plainTextContent}</strong>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var responseEmail = await emailClient.SendEmailAsync(msg);
        Console.WriteLine($"Status Code: {responseEmail.StatusCode}");

    }
}