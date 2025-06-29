namespace SharpGVGP.Decisors.ArrayDecisors;
public class RandomArrayDecisor : ArrayDecisor
{

    private Random Random;

    public RandomArrayDecisor(int inputSize, int outputSize) : base(inputSize, outputSize)
    {
        Random = new Random();
    }

    public override double[] Decide(double[] input)
    {
        var result = new double[OutputSize];
        for (int i = 0; i < OutputSize; i++)
        {
            result[i] = Random.NextDouble();
        }
        return result;
    }
}
