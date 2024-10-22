using Amazon.CDK;
using Constructs;

public class PipelineStage : Stage
{
    public PipelineStage(Construct scope, string id, IStageProps props = null) : base(scope, id, props)
    {
        /***********************************
         *    Instantiate the shared stack
         ***********************************/
        var sharedStack = new SharedStack(this, "SharedStack");
        

        new UserStacks(this, "UserStacks", new UserStackProps
        {
            AirbnbDatabase = sharedStack.AirbnbDB,
            AirbnbGraphqlApi = sharedStack.AirbnbApi,
        });

        new BuildingStacks(this, "BuildingStacks", new BuildingStacksProps{
            AirbnbDatabase = sharedStack.AirbnbDB,
            AirbnbGraphqlApi = sharedStack.AirbnbApi,
        });

        new ApartmentStacks(this, "ApartmentStacks", new ApartmentStackProps{
            AirbnbDatabase = sharedStack.AirbnbDB,
            AirbnbGraphqlApi = sharedStack.AirbnbApi,
        });

        // var bs = new BookingStack(this, "BookingStack", new BookingStackProps{
        //     AirbnbDatabase = sharedStack.AirbnbDB,
        //     AirbnbGraphqlApi = sharedStack.AirbnbApi,
        // });

        new RatingsAndFeedbackStack(this, "RatingsAndFeedbackStack", new RatingsAndFeedbackStackProps
        {
            AcmsDatabase = sharedStack.AirbnbDB,
            AcmsGraphqlApi = sharedStack.AirbnbApi,
        });

        // new ApartmentStacks(this, "ApartmentStacks", new ApartmentStacksProps
        // {
        //     AirbnbDatabase = sharedStack.AirbnbDB,
        //     AirbnbGraphqlApi = sharedStack.AirbnbApi,
        // });

        // sharedStack.AddDependency(bs);
    }
}
