using Amazon.CDK;
using Constructs;
using Amazon.CDK.Pipelines;

namespace Airbnb
{
    public class AirbnbStack : Stack
    {
        internal AirbnbStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            
        var pipeline = new CodePipeline(this, "AirbnbPipeline", new CodePipelineProps
        {
            Synth = new ShellStep("Synth", new ShellStepProps
            {
                Input = CodePipelineSource.GitHub("vernyuy/airbnb-clone-c-sharp", "master"),
                Commands = new[] { "npm install -g aws-cdk", "cdk synth" }
            })
        });

        /*********************************
         *    Add development stage
         *********************************/
        var devStage = pipeline.AddStage(new PipelineStage(this, "AirbnbPipelineDevStage", new StageProps
        {
            StageName = "dev"
        }));

        var prodStage = pipeline.AddStage(new PipelineStage(this, "AirbnbPipelineProdStage", new StageProps
        {
            StageName = "prod"
        }));

        devStage.AddPost(new ManualApprovalStep("Manual approval before production"));
        }
    }
}
