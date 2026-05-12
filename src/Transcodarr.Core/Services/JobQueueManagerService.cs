namespace Transcodarr.Core.Services;

public class JobQueueManagerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            
            //check active loads for progress
                //if no progress reported from node and expiry timed out, reset job
                //if progress reported, reset expiry to x minutes in future
            
            //get status of running jobs and check for completions / failures
                //finish completed jobs (copy over, or prepare review)
                //set failures to pending or dlq
            
            //get predicted available processing slots
            //load predicted count pending jobs from db
            //while job can be enqueued on any node, iterate oder pending jobs and update
                //set job to dispatched and save
                //if http call fails reset dispatched state
            
        }
    }
}