using SharpGVGP.Decisors.ArrayDecisors;

namespace SharpGVGP.ConsoleSample;

internal class Program
{
    static void Main(string[] args)
    {
        ArrayDecisor decisor = new RandomArrayDecisor(1, 1);

        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine($"Decision #{i + 1}: {decisor.Decide([0.5d])[0]}");
        }
    }
}
