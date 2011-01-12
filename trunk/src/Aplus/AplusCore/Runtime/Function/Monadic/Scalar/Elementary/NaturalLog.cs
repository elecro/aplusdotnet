﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Types;

namespace AplusCore.Runtime.Function.Monadic.Scalar.Elementary
{
    class NaturalLog : MonadicScalar
    {
        public override AType ExecutePrimitive(AInteger argument, AplusEnvironment environment = null)
        {
            return calculateLog(argument.asFloat);
        }

        public override AType ExecutePrimitive(AFloat argument, AplusEnvironment environment = null)
        {
            return calculateLog(argument.asFloat);
        }

        private AType calculateLog(double a)
        {
            if (a >= 0)
            {
                return AFloat.Create(Math.Log(a, Math.E));
            }
            else
            {
                throw new Error.Domain(DomainErrorText);
            }
        }
    }
}