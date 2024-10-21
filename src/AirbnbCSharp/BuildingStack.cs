using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;


public class BuildingStackProps
{
    // public GraphqlApi AirbnbGraphqlApi { get; set; }
    // public Table AirbnbDatabase { get; set; }
}

public class BuildingStacks: Stack
{
    private PipelineStage pipelineStage;
    private string v;
    private Table airbnbDB;
    private GraphqlApi airbnbApi;

    public BuildingStacks(PipelineStage pipelineStage, string v, Table airbnbDB, GraphqlApi airbnbApi)
    {
        this.pipelineStage = pipelineStage;
        this.v = v;
        this.airbnbDB = airbnbDB;
        this.airbnbApi = airbnbApi;
    }

    public BuildingStacks(Construct scope, string id, BuildingStackProps props, GraphqlApi AirbnbGraphqlApi, Table AirbnbDatabase){
        
        var airbnbGraphqlApi = AirbnbGraphqlApi;
        var airbnbDatabase = AirbnbDatabase;

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