namespace SharpGVGP.Decisors.ArrayDecisors;
public abstract class ArrayDecisor : IDecisor<double[], double[]>
{
    public readonly int InputSize;
    public readonly int OutputSize;

    public ArrayDecisor(int inputSize, int outputSize)
    {
        InputSize = inputSize;
        OutputSize = outputSize;
    }

    public abstract double[] Decide(double[] input);
}
