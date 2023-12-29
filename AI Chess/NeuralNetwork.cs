using System.Linq.Expressions;
using AI_Chess.Context;
using AI_Chess.Model;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace AI_Chess
{
    public class NeuralNetwork
    {
        private readonly int NbInputNodes;
        private readonly double LearningRate;
        //private double[][][] W;
        //private double[][] B;
        //private readonly double[][][] A;
        //private readonly double[][][] Z;
        private readonly List<double> Loss;
        private readonly Node[] Nodes;
        private readonly ChessDbContext _chessDbContext;
        private readonly ILogger<NeuralNetwork> _logger;

        public NeuralNetwork(int nbInputNodes, double learningRate, Node[] nodes, ChessDbContext chessDbContext, ILogger<NeuralNetwork> logger)
        {

            this.NbInputNodes = nbInputNodes;
            this.LearningRate = learningRate;
            this.Loss = new List<double>();
            this.Nodes = nodes;
            _chessDbContext = chessDbContext;
            _logger = logger;

            /*
                    this.Z = new double[this.Nodes!.Length][][];
            this.A = new double[this.Nodes!.Length+1][][];
            this.W = new double[Nodes!.Length][][];
            this.B = new double[this.Nodes!.Length][];

            var random = new Random();
            for (int i = 0; i < this.Nodes!.Length; i++){
                this.W[i] = MatrixOperation.GenerateRandomNormal(random, 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i-1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                this.B[i] = new double[this.Nodes[i].NbHiddenNode].Select(j => j = random.NextDouble()).ToArray();
            }
            */
        }

        public double EntropyLoss(double[][] y, double[][] y_pred){
            double result = 0;
            double outputLength = y_pred[0].Length;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < outputLength; j++)
                {
                    //Prevent taking the log of 0 with double.Epsilon
                    result += -y[i][j] * Math.Log(y_pred[i][j] + double.Epsilon);
                }
            }
            return result;
        }
        public double CostFunction(double[][] y, double[][] y_pred){
            double result = 0;
            double outputLength = y_pred[0].Length;
            for (int i = 0; i < y.Length; i++)
            {
                for (int j = 0; j < outputLength; j++)
                {
                    result += Math.Pow(y_pred[i][j] -y[i][j], 2) * 1/(y_pred[i].Length*2);
                }
            }
            return result;
        }

        public async Task<double[][]> Forward(double[][] x){
            await UpdateContent(0, x, _chessDbContext.AContents, true);
            double[][] dotProduct;

            for (int i = 0; i < this.Nodes.Length; i++){
                var w = await GetContents(i, _chessDbContext.WContents);
                var b = await GetBContents(i);
                var a = await GetContents(i, _chessDbContext.AContents);
                dotProduct = MatrixOperation.DotProduct(a, w);
                var z = MatrixOperation.Add(dotProduct, b);
                await UpdateContent(i, z, _chessDbContext.ZContents);
                await UpdateContent(i+1, this.Nodes[i].Activation!.Activation(z), _chessDbContext.AContents);
                 await _chessDbContext.SaveChangesAsync();
            }
            var lastA = await GetContents(this.Nodes.Length, _chessDbContext.AContents);
            return lastA;
        }

        public async Task Backward(double[][] y){
            double[][][] dw = new double[this.Nodes.Length][][];
            double[][] db = new double[this.Nodes.Length][];
            double[][][] dz = new double[this.Nodes.Length][][];
            double[][] dA;
            double[][] aAfter;
            int aLength;
            for (int i = this.Nodes.Length-1; i >=0; i--){
                var a = await GetContents(i, _chessDbContext.AContents);
                var z = await GetContents(i, _chessDbContext.ZContents);
                if(i == this.Nodes.Length-1){
                    aAfter = await GetContents(i+1, _chessDbContext.AContents);
                    aLength = aAfter.Length;
                    dA = new double[aLength][];
                    for (int l = 0; l < aLength; l++)
                    {
                        dA[l] = new double[aAfter[0].Length];
                        for (int j = 0; j < aAfter[0].Length; j++)
                        {
                            dA[l][j] = (aAfter[l][j] - y[l][j]) / aLength;     
                        }
                    }
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(z, y));
                } else {
                    var w = await GetContents(i + 1, _chessDbContext.WContents);
                    dA = MatrixOperation.DotProduct(dz[i+1], MatrixOperation.Transpose(w!));
                    dz[i] = MatrixOperation.DotElementWise(dA, this.Nodes[i].Activation!.Derivative(z, y));
                }
                dw[i] = MatrixOperation.DotProduct(MatrixOperation.Transpose(a), dz[i]);
                db[i] = MatrixOperation.SumColumn(dz[i]);
            }

            for(int i = 0; i < Nodes!.Length; i++){
                var w = await GetContents(i, _chessDbContext.WContents);
                var b = await GetBContents(i);
                var newW = MatrixOperation.Diff(w, MatrixOperation.DotConstant(dw[i], this.LearningRate));
                var fakeB = new double[][] {b};
                var fakeDB = new double[][] {db[i]};
                var newB = MatrixOperation.Diff(fakeB, MatrixOperation.DotConstant(fakeDB, this.LearningRate))[0];
                await UpdateDatabase(i, newW, newB);
            }
        }

        public async Task<List<double>> Train(double[][] input, double[][] output, int nbreIterations){
            if(input[0].Length != this.NbInputNodes) throw new Exception("Invalid input");
            if(output[0].Length != this.Nodes.Last().NbHiddenNode) throw new Exception("Invalid output");

            var wContentLength = await _chessDbContext.WContents.CountAsync();

            if(wContentLength == 0)
            {
                _logger.LogInformation("Initialize database");
                await InitializeDatabase();
            }
            _logger.LogInformation("Training start with {iterations} iterations", nbreIterations);
            for (int i = 1; i <= nbreIterations; i++){
                _logger.LogInformation("Iteration {iteration}", i);
                var y_pred = await this.Forward(input);
                var loss = this.EntropyLoss(output, y_pred);
                this.Loss.Add(loss);
                await this.Backward(output);
            }
            _logger.LogInformation("Training end");
            return this.Loss;
        }

        public Task<double[][]> Predict(double[][] x){
            return Forward(x);
        }

        private static T[] ConvertToContent<T>(int position, double[][] content) where T : Content, new()
        {
            var lists = new List<T>();
            for(int i = 0; i < content.Length; i++)
            {
                for(int y = 0; y < content[i].Length; y++)
                {
                    lists.Add(new T()
                    {
                        Position = position,
                        From = i,
                        To = y,
                        Value = content[i][y]
                    });
                }
            }
            return lists.ToArray();
        }

        private BContent[] ConvertToBContent(int position, double[] b)
        {
            var lists = new List<BContent>();
            for (int i = 0; i < b.Length; i++)
            {
                    lists.Add(new BContent()
                    {
                        Position = position,
                        To = i,
                        Value = b[i]
                    });
            }
            return lists.ToArray();
        }

        private async Task InitializeDatabase()
        {
            var wContent = new List<WContent>();
            var bContent = new List<BContent>();

            var random = new Random();
            for (int i = 0; i < Nodes.Length; i++)
            {
                var w = MatrixOperation.GenerateRandomNormal(random, 0, 1, i == 0 ? this.NbInputNodes : this.Nodes[i - 1].NbHiddenNode, this.Nodes[i].NbHiddenNode);
                var wContentArray = ConvertToContent<WContent>(i, w);
                wContent.AddRange(wContentArray);

                var b = new double[this.Nodes[i].NbHiddenNode].Select(j => j = random.NextDouble()).ToArray();
                var bContentArray = ConvertToBContent(i, b);
                bContent.AddRange(bContentArray);
            }
            await _chessDbContext.WContents.AddRangeAsync(wContent);
            await _chessDbContext.BContents.AddRangeAsync(bContent);

            _chessDbContext.ZContents.RemoveRange(_chessDbContext.ZContents);
            _chessDbContext.AContents.RemoveRange(_chessDbContext.AContents);
            
            await _chessDbContext.SaveChangesAsync();
        }

        private async Task UpdateDatabase(int position, double[][] w, double[] b)
        {
            await UpdateContent(position, w, _chessDbContext.WContents, false);

            var bContents = ConvertToBContent(position, b);
            foreach (var bContent in bContents)
            {
                var db = _chessDbContext.BContents.Single(w => w.Position == bContent.Position && w.To == bContent.To);
                db.Value = bContent.Value;
            }

            await _chessDbContext.SaveChangesAsync();
        }

        private async Task UpdateContent<T>(int position, double[][] content, DbSet<T> dbSet, bool save = false) where T : Content, new()
        {
            var contents = ConvertToContent<T>(position,content);

            var batchSize = 100;
            var existingEntities = new List<T>();

            for (int i = 0; i < contents.Length; i += batchSize)
            {
                var batch = contents.Skip(i).Take(batchSize);

                // Create a predicate that matches any of the keys in the batch
                var predicate = PredicateBuilder.New<T>(false);
                foreach (var wContent in batch)
                {
                    var positionContent = wContent.Position;
                    var from = wContent.From;
                    var to = wContent.To;
                    predicate = predicate.Or(p => p.Position == positionContent && p.From == from && p.To == to);
                }

                // Get all existing entities in the batch
                var batchExistingEntities = dbSet.AsExpandable()
                    .Where(predicate)
                    .AsEnumerable().ToArray();

                existingEntities.AddRange(batchExistingEntities);
            }


            var entitiesToCreate = new List<T>();
            var entitiesToUpdate = new List<T>();
            foreach (var existingEntity in existingEntities)
            {
                var wContent = contents.First(w => w.Position == existingEntity.Position && w.From == existingEntity.From && w.To == existingEntity.To);
                dbSet.Entry(existingEntity).CurrentValues.SetValues(wContent);
                entitiesToUpdate.Add(existingEntity);

                // Remove the updated entities from wContents
                contents = contents.Where(w => w.Position != existingEntity.Position || w.From != existingEntity.From || w.To != existingEntity.To).ToArray();
            }

            entitiesToCreate.AddRange(contents);

            dbSet.AddRange(entitiesToCreate);
            dbSet.UpdateRange(entitiesToUpdate);

            if(save){
                await _chessDbContext.SaveChangesAsync();
            }
        }
        private static async Task<double[][]> GetContents<T>(int postition, DbSet<T> dbSet) where T : Content
        {
            var xLength = await dbSet.CountAsync(c => c.Position == postition && c.To == 0);
            var yLength = await dbSet.CountAsync(c => c.Position == postition && c.From == 0);

            var contents = await dbSet.Where(c => c.Position == postition).ToArrayAsync();
            var w = new double[xLength][];

            for(int i = 0; i < xLength; i++)
            {
                w[i] = new double[yLength];
            }
            foreach(var content in contents)
            {
                var x = content.From;
                var y = content.To;
                w[x][y] = content.Value;
            }
            return w;
        }

        private async Task<double[]> GetBContents(int postition)
        {
            var xLength = await _chessDbContext.BContents.CountAsync(b => b.Position == postition);

            var bContents = await _chessDbContext.BContents.Where(b => b.Position == postition).ToArrayAsync();
            var b = new double[xLength];

            foreach (var bContent in bContents)
            {
                var x = bContent.To;
                b[x] = bContent.Value;
            }
            return b;
        }
    }
}
