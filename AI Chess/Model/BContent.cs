using System.ComponentModel.DataAnnotations;

namespace AI_Chess.Model
{
    public class BContent
    {
        [Key]
        public int Id { get; set; }
        public int Position { get; set; }
        public double[] Value { get; set; }
    }
}
