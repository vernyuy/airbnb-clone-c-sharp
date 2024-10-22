using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

public class BuildingStacksProps : StackProps
{
    public GraphqlApi AirbnbGraphqlApi { get; set; }
    public Table AirbnbDatabase { get; set; }
}

public class BuildingStacks : Stack
{
    public BuildingStacks(Construct scope, string id, BuildingStacksProps props) : base(scope, id, props)
    {
        var airbnbGraphqlApi = props.AirbnbGraphqlApi;
        var airbnbDatabase = props.AirbnbDatabase;

        // Add the DynamoDB DataSource
        var airbnbDataSource = airbnbGraphqlApi.AddDynamoDbDataSource(
            "airbnbdbsrc", airbnbDatabase
        );

         // Create User Account Function
        var createBuildingFunction = new AppsyncFunction(this, "createBuilding", new AppsyncFunctionProps
        {
            Name = "createBuilding",
            Api = airbnbGraphqlApi,
            DataSource = airbnbDataSource,
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/building/createBuilding.js")),
            Runtime = FunctionRuntime.JS_1_0_0
        });

         // Create User Resolver
        new Resolver(this, "createBuildingResolver", new ResolverProps
        {
            Api = airbnbGraphqlApi,
            TypeName = "Mutation",
            FieldName = "createBuilding",
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
            Runtime = FunctionRuntime.JS_1_0_0,
            PipelineConfig = new[] { createBuildingFunction }
        });
    }

    // Helper function to bundle AppSync resolver files
    private static string BundleAppSyncResolver(string path)
    {
        // Implement your bundling logic or resolve the absolute path for the resolver file
        return Path.Combine(Directory.GetCurrentDirectory(), path);
    }
}
