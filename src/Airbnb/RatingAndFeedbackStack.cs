using Amazon.CDK;
using Amazon.CDK.AWS.AppSync;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System.IO;

    public class RatingsAndFeedbackStackProps : StackProps
    {
        public GraphqlApi AcmsGraphqlApi { get; set; }
        public Table AcmsDatabase { get; set; }
    }

    public class RatingsAndFeedbackStack : Stack
    {
        public RatingsAndFeedbackStack(Construct scope, string id, RatingsAndFeedbackStackProps props) 
            : base(scope, id, props)
        {
            var acmsGraphqlApi = props.AcmsGraphqlApi;
            var acmsDatabase = props.AcmsDatabase;

            // AppSync Function for leaveFeedback
            var leaveFeedback = new AppsyncFunction(this, "leaveFeedback", new AppsyncFunctionProps
            {
                Name = "leaveFeedback",
                Api = acmsGraphqlApi,
                DataSource = acmsGraphqlApi.AddDynamoDbDataSource("acmsFeedbackDataSource", acmsDatabase),
                Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/ratingAndFeedback/leaveFeedbackandRate.js")),
                Runtime = FunctionRuntime.JS_1_0_0
            });

            // Resolver for leaveFeedback
            new Resolver(this, "leaveFeedbackResolver", new ResolverProps
            {
                Api = acmsGraphqlApi,
                TypeName = "Mutation",
                FieldName = "leaveFeedback",
                Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
                Runtime = FunctionRuntime.JS_1_0_0,
                PipelineConfig = new IAppsyncFunction[] { leaveFeedback }
            });

            // AppSync Function for getApartmentFeedback
            var getApartmentFeedback = new AppsyncFunction(this, "ApartmentFeedback", new AppsyncFunctionProps
            {
                Name = "getApartmentFeedback",
                Api = acmsGraphqlApi,
                DataSource = acmsGraphqlApi.AddDynamoDbDataSource("FeedbackDataSource", acmsDatabase),
                Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/ratingAndFeedback/getFeedbackandRate.js")),
                Runtime = FunctionRuntime.JS_1_0_0
            });

            // Resolver for getApartmentFeedback
            new Resolver(this, "getApartmentFeedbackResolver", new ResolverProps
            {
                Api = acmsGraphqlApi,
                TypeName = "Query",
                FieldName = "getApartmentFeedback",
                Code = Code.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "resolvers/default.js")),
                Runtime = FunctionRuntime.JS_1_0_0,
                PipelineConfig = new IAppsyncFunction[] { getApartmentFeedback }
            });
        }
    }