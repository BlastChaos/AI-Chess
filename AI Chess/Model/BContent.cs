using System.ComponentModel.DataAnnotations;

namespace AI_Chess.Model
{
    public class BContent
    {
        [Key]
        public string Id { get; set; }
        public string NeuralNetworkId { get; set; }
        public int Position { get; set; }
        public double[] Value { get; set; }
    }
}
