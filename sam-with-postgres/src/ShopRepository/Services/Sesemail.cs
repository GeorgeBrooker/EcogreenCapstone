using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ShopRepository.Services;

public class Sesemail
{
    private readonly IAmazonSimpleEmailServiceV2 email;
    private readonly IConfiguration config;
    public Sesemail(IAmazonSimpleEmailServiceV2 email, IConfiguration config)
    {
        this.email = email;
        this.config = config;
    }
    public async Task<bool> SendEmail([FromBody] EmailInput emailInput)
    {
        Console.WriteLine($"{emailInput.email}, {emailInput.name}, {emailInput.message}");
        Console.WriteLine($"{config["Email:From"]}, {config["Email:To"]}");
        var request = new SendEmailRequest
        {
            FromEmailAddress = config["Email:From"],
            ReplyToAddresses = [emailInput.email],
            Destination = new Destination { ToAddresses = [config["Email:To"]] },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Body = new Body { Text = new Content { Charset = "UTF-8", Data = emailInput.message + $"\n Sent from: {emailInput.email}" } },
                    Subject = new Content { Charset = "UTF-8", Data = "Message from customer: " + emailInput.name }
                }
            }
        };

        try
        {
            await email.SendEmailAsync(request);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public class EmailInput
    {
        public string email { get; set; }
        public string name { get; set; }
        public string message { get; set; }
    }
}