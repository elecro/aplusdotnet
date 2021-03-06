﻿using System;

using Microsoft.Scripting.Utils;

using AplusCore.Compiler;
using AplusCore.Runtime.Function.Monadic.NonScalar.Other;
using AplusCore.Types;

namespace AplusCore.Runtime.Function.Dyadic.NonScalar.Other
{
    class ValueInContext : AbstractDyadicFunction
    {
        #region DLR entrypoint

        public override AType Execute(AType right, AType left, Aplus environment)
        {
            // Environment is required!
            Assert.NotNull(environment);

            Value.CheckArgument<ValueInContext>(right);
            Value.CheckArgument<ValueInContext>(left);

            // get the contextparts, (context, variablename) string pair
            string[] contextparts = VariableHelper.CreateContextParts(left.asString, right.asString);

            Func<AType> method = VariableHelper.BuildVariableAccessMethod(environment, contextparts).Compile();

            return method();
        }

        #endregion

        #region Assignment Helper

        public static AType Assign(AType target, AType contextName, AType value, Aplus environment)
        {
            // Environment is required!
            Assert.NotNull(environment);

            if ((target.Type != ATypes.ASymbol) || (contextName.Type != ATypes.ASymbol) ||
                (target.Rank != 0) || (contextName.Rank != 0))
            {
                throw new Error.Domain("assign");
            }

            // Get the context parts, (context, variablename) string pairs
            string[] contextParts = VariableHelper.CreateContextParts(contextName.asString, target.asString);

            // Build the method
            Func<AType> method = VariableHelper.BuildVariableAssignMethod(environment, contextParts, value).Compile();

            return method();
        }

        #endregion
    }
}
