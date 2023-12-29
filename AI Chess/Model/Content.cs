namespace AI_Chess.Model
{
    public abstract class Content
    {
        public int Position { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public double Value { get; set; }
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            var objContent = obj as Content;
            if (objContent == null) return false;
            return objContent.From == From && objContent.To == To && objContent.Value == Value && objContent.Position == Position;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() + From.GetHashCode() + To.GetHashCode(); //or id.GetHashCode();
        }
    }
}
