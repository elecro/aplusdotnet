using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Product
{
    public class OPGreater : OuterProduct
    {
        protected override AType Calculate(AType left, AType right, Aplus env)
        {
            return DyadicFunctionInstance.GreaterThan.Execute(right, left, env);
        }
    }
}
