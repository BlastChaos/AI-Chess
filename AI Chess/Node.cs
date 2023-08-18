using AI_Chess.Activation;

namespace AI_Chess
{
    public class Node
    {
        public IActivation? Activation {get; set; }
        public int NbHiddenNode {get; set; }
    }
}