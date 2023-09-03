using AI_Chess;
using AI_Chess.Activation;
using AI_Chess.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(nameof(ChessController), c => c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36"));
builder.Services.Configure<GameConfig>(builder.Configuration.GetSection(nameof(GameConfig)));


/* var testDownload = Path.Combine(Directory.GetCurrentDirectory(),"Games");
GameHelper.DownloadGames("gukesh-d", testDownload).Wait();
var final = GameHelper.GetGameInfo(testDownload);
Random random = new();
double[][] numbers = new double[300][];
double[][] y = new double[300][];
for (int m = 0; m < numbers.Length ; m++)
{
    numbers[m] = new double[10];
    for (int n = 0; n < 10 ; n++)
    {
        numbers[m][n] = random.NextDouble();
    }
    var maxValue = numbers[m].Max();
    y[m] = new double[10];
    for (int n = 0; n < 10 ; n++)
    {
        y[m][n] = numbers[m][n] == maxValue ? 1 : 0;
    }
}
Node[] nodes = new Node[4];
nodes[0] = new Node(){
    Activation = new Sigmoid(),
    NbHiddenNode = 500
};

nodes[1] = new Node(){
    Activation = new Relu(),
    NbHiddenNode = 300
};
nodes[2] = new Node(){
    Activation = new Sigmoid(),
    NbHiddenNode = 100
};
nodes[3] = new Node(){
    Activation = new Softmax(),
    NbHiddenNode = 10
};
var nn = new NeuralNetwork(10,0.00001, nodes);
var debut = DateTime.Now;
var loss = nn.Train(numbers,y,250);
var fin = DateTime.Now;
Console.WriteLine("Dernier Loss generer: " + loss.Last());
Console.WriteLine("Temps pour le générer: " + (fin-debut));

var test = nn.Predict(numbers);
var nbreReussi = 0;
for (int m = 0; m < test.Length ; m++){
    if(Array.IndexOf(test[m], test[m].Max()) == Array.IndexOf(y[m], y[m].Max()) ) nbreReussi++;
}
Console.WriteLine("nbre reussi: " + nbreReussi); */
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
