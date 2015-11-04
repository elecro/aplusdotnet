﻿using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.NonScalar.Selection
{
    public class Left : AbstractDyadicFunction
    {
        public override AType Execute(AType right, AType left, Aplus environment = null)
        {
            return left;
        }
    }
}
