using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.Scalar.Bitwise
{
    /// <summary>
    /// Bitwise Less Equal for input x,y => (~x) | y
    /// </summary>
    public class BitwiseLessEqual : DyadicScalar
    {
        [DyadicScalarMethod]
        public AType ExecutePrimitive(AInteger rightArgument, AInteger leftArgument)
        {
            return AInteger.Create((~leftArgument.asInteger) | rightArgument.asInteger);
        }
    }
}
