using AI_Chess.Activation;

namespace AI_Chess
{
    public class Node
    {
        public IActivation? Activation {get; set; }
        public int NbHiddenNode {get; set; }
    }
    public class NodeInput
    {
        public Type Activation {get; set; }
        public int NbHiddenNode {get; set; }
        public IActivation? GetActivation() {
            return Activation switch
            {
                Type.Identity => new Identity(),
                Type.LeakyRelu => new LeakyRelu(),
                Type.Relu => new Relu(),
                Type.Sigmoid => new Sigmoid(),
                Type.Softmax => new Softmax(),
                Type.TanH => new TanH(),
                _ => null,
            };
        }
    }

    public enum Type {
        Identity,
        LeakyRelu,
        Relu,
        Sigmoid,
        Softmax,
        TanH
    }
}