﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Types;

namespace AplusCore.Runtime.Function.Monadic.Scalar.Miscellaneous
{
    class Sign : MonadicScalar
    {

        public override AType ExecutePrimitive(AInteger argument, AplusEnvironment environment = null)
        {
            return calculateSignum(argument.asFloat);
        }

        public override AType ExecutePrimitive(AFloat argument, AplusEnvironment environment = null)
        {
            return calculateSignum(argument.asFloat);
        }

        private AType calculateSignum(double number)
        {
            return AInteger.Create(Math.Sign(number));
        }
    }
}
