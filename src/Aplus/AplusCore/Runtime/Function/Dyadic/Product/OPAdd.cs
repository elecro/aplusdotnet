using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Product
{
    public class OPAdd : OuterProduct
    {
        protected override AType Calculate(AType left, AType right, Aplus env)
        {
            return DyadicFunctionInstance.Add.Execute(right, left, env);
        }
    }
}
