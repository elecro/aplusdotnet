using System.Collections.Generic;

using AplusCore.Runtime.Function.Monadic;
using AplusCore.Types;
using System.Dynamic;
using System;
using System.Linq;
using Microsoft.Scripting.Runtime;
using System.Reflection;
using Microsoft.Scripting;
using System.Diagnostics;

namespace AplusCore.Runtime
{
    public class Helpers
    {

        /// <summary>
        /// Creates a strand from the input arguments. Boxes each element.
        /// </summary>
        /// <param name="arguments">The array containing the strand arguments, in REVERSE order!</param>
        /// <returns>AArray with ABox elements</returns>
        public static AType BuildStrand(AType[] arguments)
        {
            AType result = AArray.Create(ATypes.ABox);

            for (int i = arguments.Length - 1; i >= 0; i--)
            {
                //There is no environment now!
                result.AddWithNoUpdate(MonadicFunctionInstance.Enclose.Execute(arguments[i]));
            }
            result.UpdateInfo();

            return result;
        }

        /// <summary>
        /// Creates an AArray from the input arguments.
        /// </summary>
        /// <param name="arguments">The array containing the ATypes, in REVERSE order!</param>
        /// <returns></returns>
        public static AType BuildArray(AType[] arguments)
        {
            AType result = AArray.Create(ATypes.AArray);

            for (int i = arguments.Length - 1; i >= 0; i--)
            {
                result.AddWithNoUpdate(arguments[i]);
            }
            result.UpdateInfo();

            return result;
        }

        /// <summary>
        /// Creates an List of ATypes from the input arguments, for indexing
        /// </summary>
        /// <param name="arguments">The array containing the ATypes, in REVERSE order!</param>
        /// <returns></returns>
        public static List<AType> BuildIndexerArray(AType[] arguments)
        {
            List<AType> result = new List<AType>();

            for (int i = arguments.Length - 1; i >= 0; i--)
            {
                result.Add(arguments[i]);
            }

            return result;
        }

        /// <summary>
        /// Creates an AArray from the input string with AChar type or
        /// a simple AChar if the input is of length 1
        /// </summary>
        /// <param name="text">input string</param>
        /// <returns>AArray of AChars or a single AChar</returns>
        public static AType BuildString(string text)
        {
            if (text.Length == 1)
            {
                return AChar.Create(text[0]);
            }

            AType characterArray = AArray.Create(ATypes.AChar);
            foreach (char ch in text)
            {
                characterArray.Add(AChar.Create(ch));
            }
            characterArray.UpdateInfo();

            return characterArray;
        }

        /// <summary>
        /// Test if the input AType is not equals to 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool BooleanTest(AType value)
        {
            AType scalar;
            int number;

            if (value.TryFirstScalar(out scalar, true) && scalar.ConvertToRestrictedWholeNumber(out number))
            {
                return (number != 0);
            }

            throw new Error.Domain("Condition fail");
        }

        #region Binder replacements

        internal static MethodInfo GetVariableMethod = typeof(Helpers).GetMethod("GetVariable");
        public static AType GetVariable(IDynamicMetaObjectProvider metaObject, string context, string name)
        {
            Scope scope = metaObject as Scope;
            if (scope != null)
            {
                object result;
                try
                {
                    result = scope.Storage[context][name];
                }
                catch (KeyNotFoundException)
                {
                    string message = String.Format("GetMemberBinder: cannot bind member '{0}' on object '{1}'",
                             context, name);

                    throw new Error.Value(message);
                }

                return ConvertToAType(result);
            }

            return Utils.ANull();
        }

        public static MethodInfo SetVariableMethod = typeof(Helpers).GetMethod("SetVariable");
        public static AType SetVariable(IDynamicMetaObjectProvider metaObject, string context, string name, AType value)
        {
            Scope scope = metaObject as Scope;
            if (scope != null)
            {
                dynamic contextStore;
                try
                {
                    contextStore = scope.Storage[context];
                }
                catch (KeyNotFoundException)
                {
                    contextStore = scope.Storage[context] = new ScopeStorage();
                }

                contextStore[name] = value;
            }

            return value;
        }

        internal static MethodInfo GetVariableFunctionMethod = typeof(Helpers).GetMethod("GetVariableFunction");
        public static object GetVariableFunction(IDictionary<string, object> functionScope, string name)
        {
            try
            {
                return functionScope[name];
            }
            catch (KeyNotFoundException)
            {
                string message = String.Format("Can't find {0} in function scope", name);

                throw new Error.Value(message);
            }
        }

        internal static MethodInfo SetVariableFunctionMethod = typeof(Helpers).GetMethod("SetVariableFunction");
        public static object SetVariableFunction(IDictionary<string, object> functionScope, string name, object value)
        {
            functionScope[name] = value;
            return value;
        }

        internal static MethodInfo ConvertToATypeMethod = typeof(Helpers).GetMethod("ConvertToAType");
        public static AType ConvertToAType(object obj)
        {
            if (obj is AType)
            {
                return (AType)obj;
            }
            else if (obj is int)
            {
                return AInteger.Create((int)obj);
            }

            throw new NotImplementedException();
        }

        internal static MethodInfo InvokeMethod = typeof(Helpers).GetMethod("Invoker");
        public static AType Invoker(AType func, Aplus runtime, AType[] callArgs)
        {
            Debug.Assert(func is AReference);

            AFunc afunction = func.Data as AFunc;

            if (afunction == null)
            {
                // We are trying to invoke a non-method which is not possible, throw error
                throw new Error.NonFunction(func.ToString());
            }

            // Get the callable method
            object function = afunction.Method;
            Type functionType = function.GetType();
            MethodInfo methodInfo = functionType.GetMethod("Invoke");

            // Build call arguments
            // First we add the basic arguments we have and then extend it if needed
            List<object> args = new List<object>();
            args.Add(runtime);
            args.AddRange(callArgs);

            if (afunction.IsBuiltin)
            {
                switch (callArgs.Length)
                {
                    case 1:
                        args.Add(null);
                        break;
                    case 2:
                        break;
                    default:
                        throw new Error.Valence(afunction.Name);
                }
            }
            else
            {
                if (afunction.Valence != callArgs.Length + 1)
                {
                    throw new Error.Valence(afunction.Name);
                }
            }

            try
            {
                object result = methodInfo.Invoke(function, args.ToArray());
                return (AType)result;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is Error)
                {
                    // we need to forward the runtime Errors
                    throw e.InnerException;
                }

                throw e;
            }
        }

        internal static MethodInfo CallbackMethod = typeof(Helpers).GetMethod("Callback");
        public static AType Callback(Callback.CallbackItem callbackItem, Aplus runtime, AType valueParam, AType index, AType path)
        {
            AFunc callbackFunction = callbackItem.CallbackFunction.Data as AFunc;

            if (callbackFunction == null)
            {
                throw new Error.NonFunction(callbackItem.VariableName);
            }

            // Get the callable method
            object function = callbackFunction.Method;
            Type functionType = function.GetType();
            MethodInfo methodInfo = functionType.GetMethod("Invoke");

            // Build argument list
            IEnumerable<AType> baseArgs = new AType[] {
                callbackItem.StaticData,        // static data
                valueParam,                     // new value
                index,                          // index/set of indices
                path,                           // path (left argument of pick)
                callbackItem.Context,           // context of the global variable
                callbackItem.UnqualifiedName   // name of the global variable
            }.Where((item, i) => i < callbackFunction.Valence - 1).Reverse();

            List<object> args = new List<object>();
            args.Add(runtime);
            args.AddRange(baseArgs);

            try
            {
                object result = methodInfo.Invoke(function, args.ToArray());
                return (AType)result;
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is Error)
                {
                    // we need to forward the runtime Errors
                    throw e.InnerException;
                }

                throw e;
            }

            throw new NotImplementedException();
        }

        internal static MethodInfo GetIndexerMethod = typeof(Helpers).GetMethod("GetIndexer");
        public static AType GetIndexer(AType target, List<AType> indexerParam)
        {
            if (indexerParam.Count == 0)
            {
                // In case of 'target[]'
                return target;
            }
            else
            {
                if (target.Rank < indexerParam.Count)
                {
                    throw new Error.Rank("Invalid get indexer arguments for target value");
                }

                return target[indexerParam];
            }
        }

        internal static MethodInfo SetIndexerMethod = typeof(Helpers).GetMethod("SetIndexer");
        public static AType SetIndexer(AType target, List<AType> indexerParam, AType value)
        {
            if (indexerParam.Count == 0)
            {
                Utils.PerformAssign(target, value);

                return value;
            }
            else
            {
                if (target.Rank < indexerParam.Count)
                {
                    throw new Error.Rank("Invalid set indexer arguments for target value");
                }

                target[indexerParam] = value;

                return value;
            }
        }

        #endregion

    }
}
