using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Threading.Tasks;

public class Function
{
    private static readonly AmazonSQSClient sqsClient = new AmazonSQSClient();
    private static readonly string BOOKING_QUEUE_URL = Environment.GetEnvironmentVariable("BOOKING_QUEUE_URL");
    private static string tableName = Environment.GetEnvironmentVariable("ACMS_DB");

    public async Task<bool> Handler(dynamic appsyncInput)
    {
        string createdOn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        string id = "shdf0-3fjs-4r94wj"; // Could use a GUID generator if needed

        Console.WriteLine(appsyncInput);
        
        var bookingInput = new
        {
            PK = "BOOKING",
            SK = $"USER#{appsyncInput.arguments.input.userId}",
            GS1PK = $"BOOKING#{id}",
            GSI1SK = $"APARTMENT#{appsyncInput.arguments.input.apartmentId}",
            appsyncInput.arguments.input.userId,
            appsyncInput.arguments.input.apartmentId,
            appsyncInput.arguments.input.someOtherField, // Add other fields accordingly
            createdOn
        };

        if (string.IsNullOrEmpty(tableName))
        {
            tableName = "AcmsDynamoDBTable";
        }

        var messageBody = Newtonsoft.Json.JsonConvert.SerializeObject(bookingInput);
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = BOOKING_QUEUE_URL,
            MessageBody = messageBody
        };

        Console.WriteLine($"Create booking input info: {messageBody}");
        
        try
        {
            var response = await sqsClient.SendMessageAsync(sendMessageRequest);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending the message to SQS: {ex.Message}");
            throw new Exception($"An error occurred while sending the message to SQS: {ex.Message}", ex);
        }
    }
}
