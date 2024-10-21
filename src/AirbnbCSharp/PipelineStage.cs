using Amazon.CDK;
using Constructs;

public class PipelineStage : Stage
{
    public PipelineStage(Construct scope, string id, IStageProps props = null) : base(scope, id, props)
    {
        /***********************************
         *    Instantiate the shared stack
         ***********************************/
        var sharedStack = new AirbnbSharedStack(this, "AirbnbSharedStack");

        new BuildingStacks(this, "BuildingStacks", sharedStack.AirbnbDB, sharedStack.AirbnbApi);

        new UserStacks(this, "UserStacks", new UserStackProps
        {
            AirbnbDatabase = sharedStack.AirbnbDB,
            AirbnbGraphqlApi = sharedStack.AirbnbApi,
        });

        _ = new ApartmentStacks(this, "ApartmentStacks", sharedStack.AirbnbDB, sharedStack.AirbnbApi);
        _ = new BookingStack(this, "BookingStacks", sharedStack.AirbnbDB, sharedStack.AirbnbApi);
    }
}
