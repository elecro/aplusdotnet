using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Scalar.Bitwise
{
    public class BitwiseOr : DyadicScalar
    {
        [DyadicScalarMethod]
        public AType ExecutePrimitive(AInteger rightArgument, AInteger leftArgument)
        {
            return AInteger.Create(leftArgument.asInteger | rightArgument.asInteger);
        }
    }
}
