using Microsoft.AspNetCore;
using Coravel;
using AI_Chess;
namespace ProgramAI
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            host.Services.UseScheduler(scheduler =>
            {
                scheduler.Schedule<TournamentWorker>()
                .DailyAtHour(8) //UTC TIME
                .PreventOverlapping(nameof(TournamentWorker));
            });
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}