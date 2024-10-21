using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;


public class ApartmentStacksProps
{
    // public GraphqlApi AirbnbGraphqlApi { get; set; }
    // public Table AirbnbDatabase { get; set; }
}

public class ApartmentStacks: Stack
{
    private PipelineStage pipelineStage;
    private string v;
    private Table airbnbDB;
    private GraphqlApi airbnbApi;

    public ApartmentStacks(PipelineStage pipelineStage, string v, Table airbnbDB, GraphqlApi airbnbApi)
    {
        this.pipelineStage = pipelineStage;
        this.v = v;
        this.airbnbDB = airbnbDB;
        this.airbnbApi = airbnbApi;
    }

    public ApartmentStacks(Construct scope, string id, ApartmentStacksProps props, GraphqlApi AirbnbGraphqlApi, Table AirbnbDatabase){
        
        var airbnbGraphqlApi = AirbnbGraphqlApi;
        var airbnbDatabase = AirbnbDatabase;

        // Add the DynamoDB DataSource
        var airbnbDataSource = airbnbGraphqlApi.AddDynamoDbDataSource(
            "airbnbdbs", airbnbDatabase
        );

         // Create User Account Function
        var createApartmentFunction = new AppsyncFunction(this, "createApartment", new AppsyncFunctionProps
        {
            Name = "createApartment",
            Api = airbnbGraphqlApi,
            DataSource = airbnbDataSource,
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/apartment/createApartment.js")),
            Runtime = FunctionRuntime.JS_1_0_0
        });

        var getSingleApartment = new AppsyncFunction(this, "getSingleApartment", new AppsyncFunctionProps
        {
            Name = "getSingleApartment",
            Api = airbnbGraphqlApi,
            DataSource = airbnbDataSource,
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/apartment/createApartment.js")),
            Runtime = FunctionRuntime.JS_1_0_0
        });

         // Create User Resolver
        new Resolver(this, "createApartmentResolver", new ResolverProps
        {
            Api = airbnbGraphqlApi,
            TypeName = "Mutation",
            FieldName = "createApartment",
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
            Runtime = FunctionRuntime.JS_1_0_0,
            PipelineConfig = new[] { createApartmentFunction }
        });

        new Resolver(this, "getSingleApartmentResolver", new ResolverProps
        {
            Api = airbnbGraphqlApi,
            TypeName = "Query",
            FieldName = "getSingleApartment",
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
            Runtime = FunctionRuntime.JS_1_0_0,
            PipelineConfig = new[] { createApartmentFunction }
        });

    }
}