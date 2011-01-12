﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Types;

namespace AplusCore.Runtime.Function.Monadic.NonScalar.Structural
{
    class Enclose : AbstractMonadicFunction
    {
        public override AType Execute(AType argument, AplusEnvironment environment = null)
        {
            AType result = ABox.Create(argument);

            if (argument.Type == ATypes.AFunc && !argument.IsFunctionScalar)
            {
                result.Type = ATypes.AFunc;
            }

            return result;
        }
    }
}
