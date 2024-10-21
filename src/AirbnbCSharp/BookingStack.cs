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

    private PipelineStage pipelineStage;
    private string v;
    private Table airbnbDB;
    private GraphqlApi airbnbApi;
    public BookingStack(PipelineStage pipelineStage, string v, Table airbnbDB, GraphqlApi airbnbApi)
    {
        this.pipelineStage = pipelineStage;
        this.v = v;
        this.airbnbDB = airbnbDB;
        this.airbnbApi = airbnbApi;
    }
    public BookingStack(Construct scope, string id, Table airbnbDB, GraphqlApi airbnbApi)
    {
        var api = airbnbApi;
        var ddbTable = airbnbDB;

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
                    {"ACMS_DB", ddbTable.TableName}
                },
            Role = lambdaRole
        });
        ddbTable.GrantWriteData(lambdaFn);
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

        // Adding Lambda as a data source in AppSync
        var lambdaDs = api.AddLambdaDataSource("lambdaDatasource", lambdaFn);

        // Adding SQS event source to sqsConsumer Lambda
        var eventSource = new SqsEventSource(queue);
        sqsConsumer.AddEventSource(eventSource);

        // Creating AppSync resolver
        lambdaDs.CreateResolver("booking-resolver", new ResolverProps
        {
            TypeName = "Mutation",
            FieldName = "createApartmentBooking"
        });

        // Permissions for Lambda to interact with SQS
        queue.GrantSendMessages(lambdaFn);
        queue.GrantConsumeMessages(sqsConsumer);
    }
}
