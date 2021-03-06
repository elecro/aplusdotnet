﻿using System;

using AplusCore.Types;

namespace AplusCore.Runtime.Function.Monadic.NonScalar.Other
{
    class Print : AbstractMonadicFunction
    {
        public override AType Execute(AType argument, Aplus environment = null)
        {
            Console.WriteLine(argument);
            return argument;
        }
    }
}
