using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

public class UserStackProps : StackProps
{
    public GraphqlApi AirbnbGraphqlApi { get; set; }
    public Table AirbnbDatabase { get; set; }
}

public class UserStacks : Stack
{
    public UserStacks(Construct scope, string id, UserStackProps props) : base(scope, id, props)
    {
        var airbnbGraphqlApi = props.AirbnbGraphqlApi;
        var airbnbDatabase = props.AirbnbDatabase;

        // Add the DynamoDB DataSource
        var airbnbDataSource = airbnbGraphqlApi.AddDynamoDbDataSource(
            "airbnbdbs", airbnbDatabase
        );

        // Create User Account Function
        var airbnbUserFunction = new AppsyncFunction(this, "createUserAccount", new AppsyncFunctionProps
        {
            Name = "createUserAccount",
            Api = airbnbGraphqlApi,
            DataSource = airbnbDataSource,
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/user/createUserAccount.js")),
            Runtime = FunctionRuntime.JS_1_0_0
        });

        // // Get User Account Function
        // var getUserAccount = new AppsyncFunction(this, "getUserAccount", new AppsyncFunctionProps
        // {
        //     Name = "getUserAccount",
        //     Api = airbnbGraphqlApi,
        //     DataSource = airbnbDataSource,
        //     Code = Code.FromAsset(BundleAppSyncResolver("src/resolvers/user/getUserAccount.ts")),
        //     Runtime = FunctionRuntime.JS_1_0_0
        // });

        // // Get User Account Resolver
        // new Resolver(this, "getUserAccountResolver", new ResolverProps
        // {
        //     Api = airbnbGraphqlApi,
        //     TypeName = "Query",
        //     FieldName = "getUserAccount",
        //     Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "./js_resolvers/_before_and_after_mapping_template.js")),
        //     Runtime = FunctionRuntime.JS_1_0_0,
        //     PipelineConfig = new[] { getUserAccount }
        // });

        // Create User Resolver
        new Resolver(this, "createUserResolver", new ResolverProps
        {
            Api = airbnbGraphqlApi,
            TypeName = "Mutation",
            FieldName = "createUserAccount",
            Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
            Runtime = FunctionRuntime.JS_1_0_0,
            PipelineConfig = new[] { airbnbUserFunction }
        });
    
    }

    // Helper function to bundle AppSync resolver files
    private static string BundleAppSyncResolver(string path)
    {
        // Implement your bundling logic or resolve the absolute path for the resolver file
        return Path.Combine(Directory.GetCurrentDirectory(), path);
    }
}
