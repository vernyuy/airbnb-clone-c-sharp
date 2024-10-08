using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

public class AirbnbSharedStack : Stack
{
    public Table AcmsDatabase { get; private set; }
    public GraphqlApi AcmsGraphqlApi { get; private set; }
    public CfnGraphQLSchema ApiSchema { get; private set; }

    public AirbnbSharedStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // Example setup for Cognito User Pool and AppSync API (you can add your actual resources and logic here)

        // DynamoDB Table example setup
        AcmsDatabase = new Table(this, "AirbnbDB", new TableProps
        {
            PartitionKey = new Attribute
            {
                Name = "ID",
                Type = AttributeType.STRING
            },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // AppSync GraphQL API setup example
        AcmsGraphqlApi = new GraphqlApi(this, "AirbnbApi", new GraphqlApiProps
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
        ApiSchema = new CfnGraphQLSchema(this, "APISchema", new CfnGraphQLSchemaProps
        {
            ApiId = AcmsGraphqlApi.ApiId,
            Definition = File.ReadAllText("graphql/schema.graphql") // Assuming schema is in the graphql folder
        });

        // Example: Output the GraphQL API URL
        _ = new CfnOutput(this, "GraphQLAPIURL", new CfnOutputProps
        {
            Value = AcmsGraphqlApi.GraphqlUrl
        });
    }
}
