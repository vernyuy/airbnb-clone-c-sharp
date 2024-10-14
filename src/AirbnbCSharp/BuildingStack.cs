using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;


public class BuildingStackProps
{
    public GraphqlApi AirbnbGraphqlApi { get; set; }
    public Table AirbnbDatabase { get; set; }
}

public class BuildingStacks: Stack
{
    public BuildingStacks(Construct scope, string id, BuildingStackProps props){
        
        var airbnbGraphqlApi = props.AirbnbGraphqlApi;
        var airbnbDatabase = props.AirbnbDatabase;

        // Add the DynamoDB DataSource
        var airbnbDataSource = airbnbGraphqlApi.AddDynamoDbDataSource(
            "airbnbdbs", airbnbDatabase
        );

         // Create User Account Function
        var createBuildingFunction = new AppsyncFunction(this, "createUserAccount", new AppsyncFunctionProps
        {
            Name = "createUserAccount",
            Api = airbnbGraphqlApi,
            DataSource = airbnbDataSource,
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/building/createBuilding.js")),
            Runtime = FunctionRuntime.JS_1_0_0
        });

         // Create User Resolver
        new Resolver(this, "createUserResolver", new ResolverProps
        {
            Api = airbnbGraphqlApi,
            TypeName = "Mutation",
            FieldName = "createBuilding",
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
            Runtime = FunctionRuntime.JS_1_0_0,
            PipelineConfig = new[] { createBuildingFunction }
        });

    }
}