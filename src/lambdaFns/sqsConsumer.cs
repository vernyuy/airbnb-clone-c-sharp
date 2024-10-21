using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Function
{
    private static readonly AmazonDynamoDBClient ddbClient = new AmazonDynamoDBClient();
    private static string tableName = Environment.GetEnvironmentVariable("TABLE_NAME");

    public async Task Handler(SQSEvent sqsEvent, ILambdaContext context)
    {
        try
        {
            var bookingDetails = JsonConvert.DeserializeObject<Dictionary<string, AttributeValue>>(sqsEvent.Records[0].Body);
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = "AcmsDynamoDBDatabaseTable";
            }

            var putRequest = new PutItemRequest
            {
                TableName = tableName,
                Item = bookingDetails
            };

            context.Logger.LogLine($"PutItem request: {JsonConvert.SerializeObject(putRequest)}");

            await ddbClient.PutItemAsync(putRequest);
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"An error occurred during PutItem: {ex.Message}");
        }
    }
}
