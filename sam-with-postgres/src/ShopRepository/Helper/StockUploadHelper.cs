using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Transfer;
using ShopRepository.Data;
using ShopRepository.Dtos;
using Stripe.Forwarding;

namespace ShopRepository.Helper;

public class StockUploadHelper(IConfiguration configuration, IAmazonS3 amazonS3, IShopRepo repo)
{
    private readonly string? _bucketName = configuration["StockUploadBucket"];

    public async Task<bool> UploadImages(byte[][] images, Guid stockId)
    {
        try
        {
            var s = await repo.GetStock(stockId);
            if (s == null) throw new Exception("Cannot find the stock to upload to in the DB");
            
            var uploadName = CleanUploadName(s.Name);
            var uploadFolder = $"stock-photos/{uploadName}-{s.Id.ToString()}/";
            Console.WriteLine(uploadFolder);
            
            Console.WriteLine(images.Length);
            Console.WriteLine(images[0].Length);

            using var transferUtility = new TransferUtility(amazonS3);

            for (int i = 0; i < images.Length; i++)
            {
                var request = new TransferUtilityUploadRequest
                {
                    BucketName = _bucketName,
                    Key = $"{uploadFolder}{i + 1}.jpeg",
                    InputStream = new MemoryStream(images[i])
                };
                
                await transferUtility.UploadAsync(request);
            }

            return true;
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("\n\nError encountered on server. Message:'{0}' when writing an object\n\n", e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return false;
    }
    
    private static string CleanUploadName(string uploadName)
    {
        // Define unsupported characters
       var regex = new Regex("[^a-zA-Z0-9!,_.*'()\\-]");

        // Replace each unsupported character with a hyphen
        uploadName = regex.Replace(uploadName, "-");

        // Return the cleaned upload name
        return uploadName;
    }
}