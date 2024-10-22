using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

public class SharedStack : Stack
{
    public Table AirbnbDB { get; private set; }
    public GraphqlApi AirbnbApi { get; private set; }
    public CfnGraphQLSchema AirbnbApiSchema { get; private set; }

    public SharedStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
 var userPool = new UserPool(this, "ACMSCognitoUserPool", new UserPoolProps
        {
            SelfSignUpEnabled = true,
            AccountRecovery = AccountRecovery.PHONE_AND_EMAIL,
            UserVerification = new UserVerificationConfig
            {
                EmailStyle = VerificationEmailStyle.CODE
            },
            AutoVerify = new AutoVerifiedAttrs
            {
                Email = true
            },
            StandardAttributes = new StandardAttributes
            {
                Email = new StandardAttribute
                {
                    Required = true,
                    Mutable = true
                }
            }
        });

        var userPoolClient = new UserPoolClient(this, "ACMSUserPoolClient", new UserPoolClientProps
        {
            UserPool = userPool
        });

        // GraphQL API
        this.AirbnbApi = new GraphqlApi(this, "Api", new GraphqlApiProps
        {
            Name = "airbnbApi",
            Definition = Definition.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "graphql/schema.graphql")),
            // Schema = SchemaFile.FromAsset("graphql/schema.graphql"),
            AuthorizationConfig = new AuthorizationConfig
            {
                DefaultAuthorization = new AuthorizationMode
                {
                    AuthorizationType = AuthorizationType.API_KEY
                },
                AdditionalAuthorizationModes = new[]
                {
                        new AuthorizationMode
                        {
                            AuthorizationType = AuthorizationType.USER_POOL,
                            UserPoolConfig = new UserPoolConfig
                            {
                                UserPool = userPool
                            }
                        }
                    }
            },
            XrayEnabled = true,
            LogConfig = new LogConfig
            {
                FieldLogLevel = FieldLogLevel.ALL
            }
        });


        // DynamoDB Table
        this.AirbnbDB = new Table(this, "ACMSDynamoDbTable", new TableProps
        {
            TableName = "AcmsDynamoDBDatabaseTable",
            PartitionKey = new Attribute
            {
                Name = "PK",
                Type = AttributeType.STRING
            },
            SortKey = new Attribute
            {
                Name = "SK",
                Type = AttributeType.STRING
            },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            Stream = StreamViewType.NEW_IMAGE,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Adding Global Secondary Index
        this.AirbnbDB.AddGlobalSecondaryIndex(new GlobalSecondaryIndexProps
        {
            IndexName = "getAllApartmentsPerUser",
            PartitionKey = new Attribute
            {
                Name = "GSI1PK",
                Type = AttributeType.STRING
            },
            SortKey = new Attribute
            {
                Name = "GSI1SK",
                Type = AttributeType.STRING
            },
            ProjectionType = ProjectionType.ALL
        });

// Outputs
        new CfnOutput(this, "UserPoolId", new CfnOutputProps
        {
            Value = userPool.UserPoolId
        });

        new CfnOutput(this, "UserPoolClientId", new CfnOutputProps
        {
            Value = userPoolClient.UserPoolClientId
        });

        new CfnOutput(this, "GraphQLAPI ID", new CfnOutputProps
        {
            Value = this.AirbnbApi.ApiId
        });

        new CfnOutput(this, "GraphQLAPI URL", new CfnOutputProps
        {
            Value = this.AirbnbApi.GraphqlUrl
        });
      
    }
}