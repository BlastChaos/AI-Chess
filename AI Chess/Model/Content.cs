using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chess.Model
{
    public abstract class Content
    {
        [Key]
        public string Id { get; set; }
        public string NeuralNetworkId { get; set; }
        public int Position { get; set; }

        public int Row { get; set; }

        public double[] Values { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            var objContent = obj as Content;
            if (objContent == null) return false;
            return objContent.Values == Values && objContent.Position == Position;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() + Values.GetHashCode();
        }
    }
}
