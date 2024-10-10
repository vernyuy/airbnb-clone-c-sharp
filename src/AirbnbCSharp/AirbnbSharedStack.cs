using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

public class AirbnbSharedStack : Stack
{
    public Table AirbnbDB { get; private set; }
    public GraphqlApi AirbnbApi { get; private set; }
    public CfnGraphQLSchema AirbnbApiSchema { get; private set; }

    public AirbnbSharedStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // Example setup for Cognito User Pool and AppSync API (you can add your actual resources and logic here)

        // DynamoDB Table example setup
        AirbnbDB = new Table(this, "AirbnbDB", new TableProps
        {
            PartitionKey = new Attribute
            {
                Name = "PK",
                Type = AttributeType.STRING
            },
            SortKey = new Attribute {
                Name = "SK",
                Type = AttributeType.STRING
            },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // AppSync GraphQL API setup example
        AirbnbApi = new GraphqlApi(this, "AirbnbApi", new GraphqlApiProps
        {
            Name = "AirbnbApi",
            Definition = Definition.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "graphql/schema.graphql")),
            AuthorizationConfig = new AuthorizationConfig
            {
                DefaultAuthorization = new AuthorizationMode
                {
                    AuthorizationType = AuthorizationType.API_KEY
                }
            },
            XrayEnabled = true
        });

        // Define the GraphQL Schema for AppSync
        AirbnbApiSchema = new CfnGraphQLSchema(this, "AirbnbApiSchema", new CfnGraphQLSchemaProps
        {
            ApiId = AirbnbApi.ApiId,
            Definition = File.ReadAllText("graphql/schema.graphql") // Assuming schema is in the graphql folder
        });

        // Example: Output the GraphQL API URL
        _ = new CfnOutput(this, "GraphQLAPIURL", new CfnOutputProps
        {
            Value = AirbnbApi.GraphqlUrl
        });
    }
}
