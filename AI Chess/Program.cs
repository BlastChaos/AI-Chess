using AI_Chess;
using AI_Chess.Activation;
using AI_Chess.Controllers;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<ChessController>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(nameof(ChessController), c => c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36"));
builder.Services.Configure<NeuralConfig>(builder.Configuration.GetSection(nameof(NeuralConfig)));
builder.Services.AddHostedService<Worker>();
var gameConfig = builder.Configuration.GetSection(nameof(NeuralConfig)).Get<NeuralConfig>();

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
            NbHiddenNode = 20
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

builder.Services.AddSingleton(new NeuralNetwork(69,gameConfig.LearningRate, nodes.ToArray()));
var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
