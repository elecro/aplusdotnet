using AplusCore.Types;

namespace AplusCore.Runtime.Function.Monadic.NonScalar.Selection
{
    public class Right : AbstractMonadicFunction
    {
        public override AType Execute(AType argument, Aplus environment = null)
        {
            return argument.IsMemoryMappedFile ? argument.Clone() : argument;
        }
    }
}
