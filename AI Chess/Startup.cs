using AI_Chess;
using AI_Chess.Activation;
using AI_Chess.Context;
using AI_Chess.Controllers;
using Coravel;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    public void ConfigureServices(IServiceCollection services)
    {

        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        services.AddControllers();
        services.AddScoped<ChessController>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpClient(nameof(TournamentWorker), c => c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36"));
        services.Configure<NeuralConfig>(Configuration.GetSection(nameof(NeuralConfig)));
        services.AddTransient<Tournament>();

        var neuralConfig = Configuration.GetSection(nameof(NeuralConfig)).Get<NeuralConfig>() ?? throw new Exception("Neural config cannot be null");

        services.AddDbContext<ChessDbContext>(optionBuilder =>
        {
            optionBuilder.UseNpgsql(connectionString, option => option.EnableRetryOnFailure());
        });

        services.AddTransient<TournamentWorker>();
        services.AddScheduler();

        var numberInput = TurnInfo.GetNumberOfInputNodes();
        List<Node> nodes = new() //Set here since I don't want to manage the case where I don't have a good activation in appsetting.json
        {
            new Node()
            {
                Activation = new Sigmoid(),
                NbHiddenNode = 70, // number of cases on the chessboard
            },
            new Node()
            {
                Activation = new LeakyRelu(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new Sigmoid(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new Sigmoid(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new Sigmoid(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new LeakyRelu(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new LeakyRelu(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new Sigmoid(),
                NbHiddenNode = 30
            },
            new Node()
            {
                Activation = new Sigmoid(),
                NbHiddenNode = 1
            }
        };



        services.AddTransient(provider =>
        {

            var neuralConfig = provider.GetRequiredService<IOptions<NeuralConfig>>();
            var logger = provider.GetRequiredService<ILogger<NeuralNetwork>>();
            var learningRate = neuralConfig.Value.LearningRate;
            var chessDbContext = provider.GetRequiredService<ChessDbContext>();
            var neuralNetwork1 = new NeuralNetwork(numberInput, learningRate, "player 1", nodes.ToArray(), chessDbContext, logger);
            var neuralNetwork2 = new NeuralNetwork(numberInput, learningRate, "player 2", nodes.ToArray(), chessDbContext, logger);
            NeuralTournement tournament = new()
            {
                NeuralNetwork1 = neuralNetwork1,
                NeuralNetwork2 = neuralNetwork2,
            };
            return tournament;
        });

        services.AddTransient(provider =>
        {

            var neuralConfig = provider.GetRequiredService<IOptions<NeuralConfig>>();
            var logger = provider.GetRequiredService<ILogger<NeuralNetwork>>();
            var learningRate = neuralConfig.Value.LearningRate;
            var chessDbContext = provider.GetRequiredService<ChessDbContext>();
            var neuralNetwork = new NeuralNetwork(numberInput, 0, "neuralNetwork", nodes.ToArray(), chessDbContext, logger);
            return neuralNetwork;
        });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));

        using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<ChessDbContext>();
            dbContext.Database.Migrate();
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
