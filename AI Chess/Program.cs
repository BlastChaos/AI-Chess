using AI_Chess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Random random = new();
double[][] numbers = new double[8000][];
double[][] y = new double[8000][];
for (int m = 0; m < 8000 ; m++)
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

var nn = new NeuralNetwork(10,100,10,0.0001);
var debut = DateTime.Now;
var loss = nn.Train(numbers,y,10);
var fin = DateTime.Now;
Console.WriteLine("Dernier Loss generer: " + loss.Last());
Console.WriteLine("Temps pour le générer: " + (fin-debut));
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
