﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AplusCore.Types;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace AplusCore.Runtime.Function.Dyadic
{
    abstract class DyadicScalar : AbstractDyadicFunction
    {
        #region Variables

        private byte combination;

        /// <summary>
        /// Stores the allowed methods. The key is a combination generated by <see cref="CombinationKey"/>,
        /// which uses the method arguments' type to create a byte key value.
        /// </summary>
        private Dictionary<byte, MethodInfo> allowedMethods;

        #endregion

        #region Constructor

        public DyadicScalar()
        {

            this.allowedMethods = new Dictionary<byte, MethodInfo>();

            MethodInfo[] methods = this.GetType().GetMethods(
                BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public
            );

            // Detect the methods and add a rule for it based on the parameter types
            foreach (MethodInfo method in methods)
            {
                object[] attribs = method.GetCustomAttributes(typeof(Scalar.DyadicScalarMethodAttribute), false);
                // check if the method has the correct attribute
                if (attribs.Length < 1)
                {
                    continue;
                }

                ParameterInfo[] parameterInfo = method.GetParameters();
                
                // Add the method to the allowed method's list
                this.allowedMethods.Add(
                    CombinationKey(
                        Utils.GetATypesFromType(parameterInfo[0].ParameterType),
                        Utils.GetATypesFromType(parameterInfo[1].ParameterType)
                    ),
                    method
                );
            }

        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns an byte mask representing the type combination
        /// </summary>
        /// <param name="rightArgType">converted to int and shifted by 4</param>
        /// <param name="leftArgType">converted to int</param>
        /// <returns>type combination byte mask</returns>
        private byte CombinationKey(ATypes rightArgType, ATypes leftArgType)
        {
            return (byte)((((int)rightArgType) << 4) | ((int)leftArgType));
        }

        #endregion

        #region Entry Point from DLR

        public override AType Execute(AType right, AType left, AplusEnvironment environment = null)
        {
            this.combination = CombinationKey(right.Type, left.Type);
            if (right.Type == ATypes.ANull || left.Type == ATypes.ANull)
            {
                this.combination = CombinationKey(ATypes.ANull, ATypes.ANull);
            }
            // Check if we have a rule for the specific input types
            //Contains(this.combination)
            else if (!this.allowedMethods.ContainsKey(this.combination))
            {
                // Rule not found
                // reset the combination info to the default case
                this.combination = CombinationKey(ATypes.AType, ATypes.AType);
                // throw an error if we don't have a default case
                if (!this.allowedMethods.ContainsKey(this.combination))
                {
                    // throw a type error
                    throw new Error.Type(this.TypeErrorText);
                }
            }

            return ExecuteRecursion(right, left);
        }

        #endregion

        #region Recursion

        private AType ExecuteRecursion(AType rightArgument, AType leftArgument)
        {
            if (leftArgument.IsArray && rightArgument.IsArray)
            {
                if (leftArgument.Length == rightArgument.Length)
                {
                    AType currentItem;
                    uint typeCounter = 0;

                    AType left = leftArgument;
                    AType right = rightArgument;
                    AType result = AArray.Create(ATypes.AArray);

                    for (int i = 0; i < left.Length; i++)
                    {
                        currentItem = ExecuteRecursion(right[i], left[i]);
                        typeCounter += (uint)((currentItem.Type == ATypes.AFloat) ? 1 : 0);

                        result.AddWithNoUpdate(currentItem);
                    }
                    result.UpdateInfo();

                    // If one element was not as the others then the typeCounter will be
                    // in an interval of:  0 < typeCounter < itemCount
                    if ((typeCounter != result.Length) && (typeCounter != 0))
                    {
                        result.ConvertToFloat();
                    }
                    return result;
                }
                else
                {
                    throw new Error.Length(this.LengthErrorText);
                }
            }
            else if (leftArgument.IsArray && rightArgument.IsPrimitive)
            {
                if (rightArgument.Type == ATypes.ANull)
                {
                    throw new Error.Length(this.LengthErrorText);
                }

                AType currentItem;
                uint typeCounter = 0;

                AType left = leftArgument;
                AType result = AArray.Create(ATypes.AArray);

                for (int i = 0; i < left.Length; i++)
                {
                    currentItem = ExecuteRecursion(rightArgument, left[i]);
                    typeCounter += (uint)((currentItem.Type == ATypes.AFloat) ? 1 : 0);

                    result.AddWithNoUpdate(currentItem);
                }
                result.UpdateInfo();

                if (leftArgument.Type == ATypes.ANull)
                {
                    result.Type = rightArgument.Type;
                }

                if ((typeCounter != result.Length) && (typeCounter != 0))
                {
                    result.ConvertToFloat();
                }

                return result;
            }
            else if (leftArgument.IsPrimitive && rightArgument.IsArray)
            {
                if (leftArgument.Type == ATypes.ANull)
                {
                    throw new Error.Length(this.LengthErrorText);
                }

                AType currentItem;
                uint typeCounter = 0;

                AType right = rightArgument;
                AType result = AArray.Create(ATypes.AArray);

                for (int i = 0; i < right.Length; i++)
                {
                    currentItem = ExecuteRecursion(right[i], leftArgument);
                    typeCounter += (uint)((currentItem.Type == ATypes.AFloat) ? 1 : 0);

                    result.AddWithNoUpdate(currentItem);
                }
                result.UpdateInfo();

                if (rightArgument.Type == ATypes.ANull)
                {
                    result.Type = leftArgument.Type;
                }

                if ((typeCounter != result.Length) && (typeCounter != 0))
                {
                    result.ConvertToFloat();
                }

                return result;
            }
            else
            {
                if (this.combination == CombinationKey(ATypes.ANull, ATypes.ANull))
                {
                    return AArray.ANull();
                }

                // Get the method for the current input type combination
                MethodInfo method = this.allowedMethods[this.combination];
                AType result = null;
                try
                {
                    // Invoke the method
                    result = (AType)method.Invoke(this, new object[] { rightArgument.Data, leftArgument.Data });
                }
                catch (TargetInvocationException ex)
                {
                    // Exception during method invoke

                    // Check if the inner exception was an A+ Error?    
                    if (ex.InnerException is Error)
                    {
                        // A+ Error during the method invoke
                        throw ex.InnerException; // we lose stack trace this way, but its okay :)
                    }
                    else
                    {
                        throw;
                    }
                }

                return result/* ?? new ANull()*/;
            }
        }

        #endregion
    }

}
