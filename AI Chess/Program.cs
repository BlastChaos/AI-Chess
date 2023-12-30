using AI_Chess;
using AI_Chess.Activation;
using AI_Chess.Context;
using AI_Chess.Controllers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<ChessController>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(nameof(ChessController), c => c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36"));
builder.Services.Configure<NeuralConfig>(builder.Configuration.GetSection(nameof(NeuralConfig)));
builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection(nameof(SmtpConfig)));


var neuralConfig = builder.Configuration.GetSection(nameof(NeuralConfig)).Get<NeuralConfig>();

string connectionString = $"Data Source={neuralConfig.BrainFileName}";
builder.Services.AddDbContext<ChessDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddHostedService<Worker>();


builder.Services.AddScoped<NeuralNetwork>(provider =>
{
    List<Node> nodes = new()
    {
        new Node()
        {
            Activation = new LeakyRelu(),
            NbHiddenNode = 69, // number of cases on the chessboard
        },
        new Node()
        {
            Activation = new LeakyRelu(),
            NbHiddenNode = 50
        },
        new Node()
        {
            Activation = new Sigmoid(),
            NbHiddenNode = 25
        },
        new Node()
        {
            Activation = new LeakyRelu(),
            NbHiddenNode = 50
        },
        new Node()
        {
            Activation = new LeakyRelu(),
            NbHiddenNode = 50
        },
        new Node()
        {
            Activation = new Sigmoid(),
            NbHiddenNode = 1
        }
    };

    var nbInputNodes = 69; //8*8 + Original postion (2) => New Position(2) + turn(1)
    var neuralConfig = provider.GetRequiredService<IOptions<NeuralConfig>>();
    var logger = provider.GetRequiredService<ILogger<NeuralNetwork>>();
    var learningRate = neuralConfig.Value.LearningRate;
    var chessDbContext = provider.GetRequiredService<ChessDbContext>();
    return new NeuralNetwork(nbInputNodes, learningRate, nodes.ToArray(), chessDbContext, logger);
});

var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<ChessDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
