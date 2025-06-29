namespace SharpGVGP.Decisors;
public interface IDecisor<in Tin, out TOut>
{
    TOut Decide(Tin input);
}
