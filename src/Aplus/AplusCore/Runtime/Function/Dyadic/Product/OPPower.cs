using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Product
{
    public class OPPower : OuterProduct
    {
        protected override AType Calculate(AType left, AType right, Aplus env)
        {
            return DyadicFunctionInstance.Power.Execute(right, left, env);
        }
    }
}
