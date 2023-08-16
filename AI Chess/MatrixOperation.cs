namespace AI_Chess
{
    public class MatrixOperation
    {
        public static double[][] DotProduct(double[][] a, double[][] b)
        {
            // Vérifier si les dimensions des matrices permettent la multiplication
            if (a[0].Length != b.Length)
            {
                Console.WriteLine("Les dimensions des matrices ne permettent pas la multiplication.");
                return Array.Empty<double[]>();
            }
            // Initialiser la matrice résultante c avec des zéros
            double[][] c = new double[a.Length][];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new double[b[0].Length];
            }

            // Effectuer la multiplication matricielle
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < b[0].Length; j++)
                {
                    for (int k = 0; k < b.Length; k++)
                    {
                        c[i][j] += a[i][k] * b[k][j];
                    }
                }
            }
            return c;
        }


        public static double[][] DotElementWise(double[][] a, double[][] b)
        {
            // Effectuer la multiplication
            double[][] c = new double[a.Length][];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new double[a[0].Length];
                for (int j = 0; j < a[0].Length; j++)
                {
                    c[i][j]=a[i][j] * b[i][j];        
                }
            }
            return c;
        }

        public static double[][] DotConstant(double[][] a, double constant)
        {
            // Effectuer la multiplication
            double[][] c = new double[a.Length][];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new double[a[0].Length];
                for (int j = 0; j < a[0].Length; j++)
                {
                    c[i][j]=a[i][j] * constant;        
                }
            }
            return c;
        }
        public static double[][] Add(double[][] a, double[] b)
        {
            // Effectuer l'addition matricielle
            double[][] c = new double[a.Length][];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new double[a[0].Length];
                for (int j = 0; j < a[0].Length; j++)
                {
                    c[i][j]=a[i][j] + b[j];        
                }
            }
            return c;
        }

        public static double[][] Diff(double[][] a, double[][] b)
        {
            // Effectuer l'addition matricielle
            double[][] c = new double[a.Length][];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new double[a[0].Length];
                for (int j = 0; j < a[0].Length; j++)
                {
                    c[i][j]=a[i][j] - b[i][j];        
                }
            }
            return c;
        }

        public static double[][] Transpose(double[][] matrix)
        {
            int w = matrix.Length;
            int h = matrix[0].Length;

            double[][] result = new double[h][];

            for (int i = 0; i < w; i++)
            {
                result[i] = new double[w];
                for (int j = 0; j < h; j++)
                {
                    result[j][i] = matrix[i][j];
                }
            }

            return result;
        }

       public static double[] SumColumn(double[][] matrix)
        {
            int numRows = matrix.Length;
            int numCols = matrix[0].Length;

            double[] sumByColumn = new double[numCols];

            // Calculer la somme par colonne
            for (int j = 0; j < numCols; j++)
            {
                for (int i = 0; i < numRows; i++)
                {
                    sumByColumn[j] += matrix[i][j];
                }
            }
            return sumByColumn;
        }
    }
}
