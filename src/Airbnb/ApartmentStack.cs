using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

public class ApartmentStackProps : StackProps
{
    public GraphqlApi AirbnbGraphqlApi { get; set; }
    public Table AirbnbDatabase { get; set; }
}

public class ApartmentStacks : Stack
{
    public UserStacks(Construct scope, string id, ApartmentStackProps props) : base(scope, id, props)
    {
        var airbnbGraphqlApi = props.AirbnbGraphqlApi;
        var airbnbDatabase = props.AirbnbDatabase;

        // Add the DynamoDB DataSource
        var airbnbDataSource = airbnbGraphqlApi.AddDynamoDbDataSource(
            "apartment-ds", airbnbDatabase
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

    // Helper function to bundle AppSync resolver files
    private static string BundleAppSyncResolver(string path)
    {
        // Implement your bundling logic or resolve the absolute path for the resolver file
        return Path.Combine(Directory.GetCurrentDirectory(), path);
    }
}
