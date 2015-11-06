using System.Collections.Generic;

using AplusCore.Runtime.Function.Monadic;
using AplusCore.Types;
using System.Dynamic;
using System;
using Microsoft.Scripting.Runtime;
using System.Reflection;
using Microsoft.Scripting;

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

        internal static MethodInfo SetVariableMethod = typeof(Helpers).GetMethod("SetVariable");
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

        internal static MethodInfo ConvertToATypeMethod = typeof(Helpers).GetMethod("ConvertToAType");
        public static AType ConvertToAType(object obj)
        {
            if (obj is AType)
            {
                return (AType)obj;
            }

            throw new NotImplementedException();
        }

        internal static MethodInfo InvokeMethod = typeof(Helpers).GetMethod("Invoker");
        public static AType Invoker(AType func, Aplus runtime, AType[] callArgs)
        {
            List<object> args = new List<object>();
            args.Add(runtime);
            args.AddRange(callArgs);

            object function = ((AFunc)func.Data).Method;
            Type functionType = function.GetType();
            MethodInfo methodInfo = functionType.GetMethod("Invoke");

            object result = methodInfo.Invoke(function, args.ToArray());
            //object result = ((Func<Aplus, AType, AType>)((AFunc)func.Data).Method)(runtime, callArgs[0]);

            return (AType)result;
        }

        #endregion

    }
}
