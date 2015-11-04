using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Product
{
    public class OPResidue : OuterProduct
    {
        protected override AType Calculate(AType left, AType right, Aplus env)
        {
            return DyadicFunctionInstance.Residue.Execute(right, left, env);
        }
    }
}
