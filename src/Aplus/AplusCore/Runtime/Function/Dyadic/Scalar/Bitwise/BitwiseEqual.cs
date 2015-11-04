using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Scalar.Bitwise
{
    /// <summary>
    /// Bitwise Equal for input x,y => ~(x ^ y)
    /// </summary>
    public class BitwiseEqual : DyadicScalar
    {
        [DyadicScalarMethod]
        public AType ExecutePrimitive(AInteger rightArgument, AInteger leftArgument)
        {
            return AInteger.Create(~(leftArgument.asInteger ^ rightArgument.asInteger));
        }
    }
}
