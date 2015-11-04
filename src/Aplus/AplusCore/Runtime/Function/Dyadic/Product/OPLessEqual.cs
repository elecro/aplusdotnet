using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Product
{
    public class OPLessEqual : OuterProduct
    {
        protected override AType Calculate(AType left, AType right, Aplus env)
        {
            return DyadicFunctionInstance.LessThanOrEqualTo.Execute(right, left, env);
        }
    }
}
