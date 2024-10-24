using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.SQS;
using Constructs;
using System.Collections.Generic;
using System.IO;

public class BookingStack : Stack
{
    public BookingStack(Construct scope, string id) : base(scope, id)
    {
            var apiUrl = Fn.ImportValue("ApiUrl");
            var tableName = Fn.ImportValue("DynamoDBTableName");
            var airbnbDatabase = Table.FromTableAttributes(this, "ImportedTable", new TableAttributes
            {
                TableName = tableName,
                // If you need the table ARN or other attributes, you can set them here.
            });



        // Dead Letter Queue
        var dlq = new Queue(this, "DeadLetterQueue");

        // SQS Queue
        var queue = new Queue(this, "bookingQueue", new QueueProps
        {
            DeadLetterQueue = new DeadLetterQueue
            {
                Queue = dlq,
                MaxReceiveCount = 10
            }
        });

        // Lambda Execution Role
        var lambdaRole = new Role(this, "LambdaExecutionRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });



        lambdaRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"));

        // Lambda Function for handling AppSync requests
        var lambdaFn = new Function(this, "AppSyncLambdaHandler", new FunctionProps
        {
            Runtime = Runtime.NODEJS_20_X,
            Handler = "createApartmentBooking.handler",
            Code = Amazon.CDK.AWS.Lambda.Code.FromAsset(Path.Combine("src/lambdaFns")),
            Environment = new Dictionary<string, string>
                {
                    { "BOOKING_QUEUE_URL", queue.QueueUrl },
                    {"ACMS_DB", tableName}
                },
            Role = lambdaRole
        });
        airbnbDatabase.GrantWriteData(lambdaFn);


        // Lambda Function for consuming SQS
        var sqsConsumer = new Function(this, "sqsConsumer", new FunctionProps
        {
            Runtime = Runtime.NODEJS_20_X,
            Handler = "sqsConsumer.handler",
            Code = Amazon.CDK.AWS.Lambda.Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "src/lambdaFns")),
            Environment = new Dictionary<string, string>
                {
                    { "BOOKING_QUEUE_URL", queue.QueueUrl }
                },
            Role = lambdaRole
        });

        var api = GraphqlApi.FromGraphqlApiAttributes(this, "ImportedAPI", new GraphqlApiAttributes
            {
                GraphqlApiId = Fn.ImportValue("ApiID"),
            });

        // Adding Lambda as a data source in AppSync
        var lambdaDs = new LambdaDataSource(this, "LambdaDataSource", new LambdaDataSourceProps
            {
                Api = api,
                LambdaFunction = lambdaFn
            });
        var eventSource = new SqsEventSource(queue);
        sqsConsumer.AddEventSource(eventSource);

        lambdaDs.CreateResolver("booking-resolver", new ResolverProps
        {
            TypeName = "Mutation",
            FieldName = "createApartmentBooking"
        });

        queue.GrantSendMessages(lambdaFn);
        queue.GrantConsumeMessages(sqsConsumer);
    }
}
