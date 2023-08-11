namespace AI_Chess
{
    public class ReseauxNeuronal
    {
        private int NbreNoeudEntre;
        private int NbreNoeudCache;
        private int NbreNoeudSortie;
        private double TauxApprentissage;
        private double[][] W1;
        private double[][] W2;
        private double[] b1;
        private double[] b2;


        public ReseauxNeuronal(int nbreNoeudEntre, int nombreCoucheCache, int nombreNoeudSortie, double tauxApprentissage)
        {
            this.NbreNoeudEntre = nbreNoeudEntre;
            this.NbreNoeudCache = nombreCoucheCache;
            this.NbreNoeudSortie = nombreNoeudSortie;
            this.TauxApprentissage = tauxApprentissage;
            SetupValue();
        }
        public void SetupValue()
        {
            this.W1 = new double[this.NbreNoeudEntre][];
            this.b1 = new double[this.NbreNoeudCache];
            var rnd = new Random();
            var oneTimeOnly = true;
            for (int i = 0; i < this.W1.Length; i++)
            {
                this.W1[i] = new double[this.NbreNoeudCache];
                for (int j = 0; j < this.W1[i].Length; j++)
                {
                    this.W1[i][j] = rnd.NextDouble();
                    if (oneTimeOnly) this.b1[j] = 1;
                }
                oneTimeOnly = false;
            }

            this.W2 = new double[this.NbreNoeudCache][];
            this.b2 = new double[this.NbreNoeudSortie];
            oneTimeOnly = true;
            for (int i = 0; i < this.W2.Length; i++)
            {
                this.W2[i] = new double[this.NbreNoeudSortie];
                for (int j = 0; j < this.W2[i].Length; j++)
                {
                    this.W2[i][j] = rnd.NextDouble();
                    if (oneTimeOnly) this.b2[j] = 1;
                }
                oneTimeOnly = false;
            }
        }

    }
}
