using System.Collections.Generic;

using DLR = System.Linq.Expressions;

using AplusCore.Compiler.Grammar;
using AplusCore.Runtime.Function.Dyadic;
using AplusCore.Runtime.Function.Monadic;
using System;

namespace AplusCore.Compiler.AST
{
    class MethodChooser
    {
        #region Monadic Token Rewrite rules

        /// <summary>
        /// Monadic Token Type rewrite rules
        /// </summary>
        private static Dictionary<Tokens, Tokens> ReWriteRules = new Dictionary<Tokens, Tokens>()
        {
            { Tokens.ABSOLUTEVALUE, Tokens.RESIDUE },
            { Tokens.CEILING, Tokens.MAX },
            { Tokens.COUNT, Tokens.CHOOSE },
            { Tokens.DEFAULTFORMAT, Tokens.FORMAT },
            { Tokens.DEPTH, Tokens.MATCH },
            { Tokens.DISCLOSE, Tokens.GT },
            { Tokens.ENCLOSE, Tokens.LT },
            { Tokens.EXPONENTIAL, Tokens.POWER },
            { Tokens.EXECUTE, Tokens.EXECUTEINCONTEXT },
            { Tokens.FLOOR, Tokens.MIN },
            { Tokens.GRADEUP, Tokens.BINS },
            { Tokens.IDENTITY, Tokens.ADD },
            { Tokens.INTERVAL, Tokens.FIND },
            { Tokens.ITEMRAVEL, Tokens.RESTRUCTURE },
            { Tokens.MAPIN, Tokens.MAP },
            { Tokens.MATRIXINVERSE, Tokens.SOLVE },
            { Tokens.NATURALLOG, Tokens.LOG },
            { Tokens.NEGATE, Tokens.SUBTRACT },
            { Tokens.NOT, Tokens.LAMINATE },
            { Tokens.NULL, Tokens.LEFT },
            { Tokens.PACK, Tokens.DECODE },
            { Tokens.PARTITIONCOUNT, Tokens.PARTITION },
            { Tokens.PITIMES, Tokens.CIRCLE },
            { Tokens.PRINT, Tokens.DROP },
            { Tokens.RAKE, Tokens.MEMBER },
            { Tokens.RAVEL, Tokens.CATENATE },
            { Tokens.RAZE, Tokens.PICK },
            { Tokens.RECIPROCAL, Tokens.DIVIDE },
            { Tokens.REVERSE, Tokens.ROTATE },
            { Tokens.ROLL, Tokens.DEAL },
            { Tokens.SEPARATESYMBOLS, Tokens.COMBINESYMBOLS },
            { Tokens.SHAPE, Tokens.RESHAPE },
            { Tokens.SIGN, Tokens.MULTIPLY },
            { Tokens.SIGNAL, Tokens.TAKE },
            { Tokens.STOP, Tokens.AND },
            { Tokens.TRANSPOSE, Tokens.TRANSPOSEAXES },
            { Tokens.TYPE, Tokens.OR },
            { Tokens.UNPACK, Tokens.ENCODE },
            { Tokens.VALUE, Tokens.VALUEINCONTEXT }
        };

        #endregion


        /// <summary>
        /// Return the A+ native monadic function for the specified monadic token
        /// </summary>
        /// <returns><b>null</b> if no monadic function found</returns>
        internal static DLR.MemberExpression GetMonadicMethod(Token token)
        {
            Type baseT = typeof(MonadicFunctionInstance);
            switch (token.Type)
            {
                #region Monadic Scalar

                case Tokens.ABSOLUTEVALUE:
                    return baseT.Field("AbosulteValue");
                case Tokens.CEILING:
                    return baseT.Field("Ceiling");
                case Tokens.EXPONENTIAL:
                    return baseT.Field("Exponential");
                case Tokens.FLOOR:
                    return baseT.Field("Floor");
                case Tokens.IDENTITY:
                    return baseT.Field("Identity");
                case Tokens.NATURALLOG:
                    return baseT.Field("NaturalLog");
                case Tokens.NEGATE:
                    return baseT.Field("Negate");
                case Tokens.NOT:
                    return baseT.Field("Not");
                case Tokens.PITIMES:
                    return baseT.Field("PiTimes");
                case Tokens.RECIPROCAL:
                    return baseT.Field("Reciprocal");
                case Tokens.ROLL:
                    return baseT.Field("Roll");
                case Tokens.SIGN:
                    return baseT.Field("Sign");

                #endregion

                #region Monadic Non Scalar

                case Tokens.COUNT:
                    return baseT.Field("Count");
                case Tokens.DEPTH:
                    return baseT.Field("Depth");
                case Tokens.DISCLOSE:
                    return baseT.Field("Disclose");
                case Tokens.ENCLOSE:
                    return baseT.Field("Enclose");
                case Tokens.EXECUTE:
                    return baseT.Field("ExecuteFunction");
                case Tokens.DEFAULTFORMAT:
                    return baseT.Field("DefaultFormat");
                case Tokens.GRADEDOWN:
                    return baseT.Field("GradeDown");
                case Tokens.GRADEUP:
                    return baseT.Field("GradeUp");
                case Tokens.INTERVAL:
                    return baseT.Field("Interval");
                case Tokens.ITEMRAVEL:
                    return baseT.Field("ItemRavel");
                case Tokens.MAPIN:
                    return baseT.Field("MapIn");
                case Tokens.MATRIXINVERSE:
                    return baseT.Field("MatrixInverse");
                case Tokens.NULL:
                    return baseT.Field("NullFunction");
                case Tokens.PACK:
                    return baseT.Field("Pack");
                case Tokens.PARTITIONCOUNT:
                    return baseT.Field("PartitionCount");
                case Tokens.PRINT:
                    return baseT.Field("Print");
                case Tokens.RAKE:
                    return baseT.Field("Rake");
                case Tokens.RAVEL:
                    return baseT.Field("Ravel");
                case Tokens.RAZE:
                    return baseT.Field("Raze");
                case Tokens.REVERSE:
                    return baseT.Field("Reverse");
                case Tokens.RIGHT:
                    return baseT.Field("Right");
                case Tokens.SEPARATESYMBOLS:
                    return baseT.Field("SeparateSymbols");
                case Tokens.SHAPE:
                    return baseT.Field("Shape");
                case Tokens.SIGNAL:
                    return baseT.Field("Signal");
                case Tokens.STOP:
                    return baseT.Field("Stop");
                case Tokens.TRANSPOSE:
                    return baseT.Field("Transpose");
                case Tokens.TYPE:
                    return baseT.Field("Type");
                case Tokens.UNPACK:
                    return baseT.Field("Unpack");
                case Tokens.VALUE:
                    return baseT.Field("Value");

                #endregion

                #region Monadic Operator

                case Tokens.RADD:
                    return baseT.Field("ReduceAdd");
                case Tokens.RMULTIPLY:
                    return baseT.Field("ReduceMultiply");
                case Tokens.ROR:
                    return baseT.Field("ReduceOr");
                case Tokens.RAND:
                    return baseT.Field("ReduceAnd");
                case Tokens.RMAX:
                    return baseT.Field("ReduceMax");
                case Tokens.RMIN:
                    return baseT.Field("ReduceMin");
                case Tokens.SADD:
                    return baseT.Field("ScanAdd");
                case Tokens.SMULTIPLY:
                    return baseT.Field("ScanMultiply");
                case Tokens.SMIN:
                    return baseT.Field("ScanMin");
                case Tokens.SMAX:
                    return baseT.Field("ScanMax");
                case Tokens.SAND:
                    return baseT.Field("ScanAnd");
                case Tokens.SOR:
                    return baseT.Field("ScanOr");

                #endregion

                #region Bitwise Operator

                case Tokens.BWNOT:
                    return baseT.Field("BitwiseNot");

                #endregion

                default:
                    return null;
            }
        }

        /// <summary>
        /// Return the A+ native dyadic function for the specified dyadic token.
        /// </summary>
        /// <returns><b>null</b> if no dyadic function found</returns>
        internal static DLR.MemberExpression GetDyadicMethod(Token token)
        {
            Type baseT = typeof(DyadicFunctionInstance);
            switch (token.Type)
            {
                #region Dyadic Scalar

                case Tokens.ADD:
                    return baseT.Field("Add");
                case Tokens.AND:
                    return baseT.Field("And");
                case Tokens.CIRCLE:
                    return baseT.Field("Circle");
                case Tokens.COMBINESYMBOLS:
                    return baseT.Field("CombineSymbols");
                case Tokens.DIVIDE:
                    return baseT.Field("Divide");
                case Tokens.EQUAL:
                    return baseT.Field("EqualTo");
                case Tokens.GT:
                    return baseT.Field("GreaterThan");
                case Tokens.GTE:
                    return baseT.Field("GreaterThanOrEqualTo");
                case Tokens.LOG:
                    return baseT.Field("Log");
                case Tokens.LT:
                    return baseT.Field("LessThan");
                case Tokens.LTE:
                    return baseT.Field("LessThanOrEqualTo");
                case Tokens.MAX:
                    return baseT.Field("Max");
                case Tokens.MIN:
                    return baseT.Field("Min");
                case Tokens.MULTIPLY:
                    return baseT.Field("Multiply");
                case Tokens.NOTEQUAL:
                    return baseT.Field("NotEqualTo");
                /*case Tokens.OR:
                    //Cast nonscalar primitive functon has not implementeted yet!
                    return DyadicFunctionInstance.Or;
                    return DyadicFunctionInstance.Cast;*/
                case Tokens.POWER:
                    return baseT.Field("Power");
                case Tokens.RESIDUE:
                    return baseT.Field("Residue");
                case Tokens.SUBTRACT:
                    return baseT.Field("Subtract");

                #endregion

                #region Dyadic Non Scalar

                case Tokens.BINS:
                    return baseT.Field("Bins");
                case Tokens.CATENATE:
                    return baseT.Field("Catenate");
                case Tokens.CHOOSE:
                    return baseT.Field("Choose");
                case Tokens.DEAL:
                    return baseT.Field("Deal");
                case Tokens.DECODE:
                    return baseT.Field("Decode");
                case Tokens.DROP:
                    return baseT.Field("Drop");
                case Tokens.ENCODE:
                    return baseT.Field("Encode");
                case Tokens.EXECUTEINCONTEXT:
                    return baseT.Field("ExecuteInContext");
                case Tokens.EXPAND:
                    return baseT.Field("Expand");
                case Tokens.FORMAT:
                    return baseT.Field("Format");
                case Tokens.FIND:
                    return baseT.Field("Find");
                case Tokens.LAMINATE:
                    return baseT.Field("Laminate");
                case Tokens.LEFT:
                    return baseT.Field("Left");
                case Tokens.MAP:
                    return baseT.Field("Map");
                case Tokens.MATCH:
                    return baseT.Field("Match");
                case Tokens.MEMBER:
                    return baseT.Field("Member");
                case Tokens.PARTITION:
                    return baseT.Field("Partition");
                case Tokens.PICK:
                    return baseT.Field("Pick");
                case Tokens.REPLICATE:
                    return baseT.Field("Replicate");
                case Tokens.RESHAPE:
                    return baseT.Field("Reshape");
                case Tokens.RESTRUCTURE:
                    return baseT.Field("Restructure");
                case Tokens.ROTATE:
                    return baseT.Field("Rotate");
                case Tokens.SOLVE:
                    return baseT.Field("Solve");
                case Tokens.TAKE:
                    return baseT.Field("Take");
                case Tokens.TRANSPOSEAXES:
                    return baseT.Field("TransposeAxis");
                case Tokens.VALUEINCONTEXT:
                    return baseT.Field("ValueInContext");

                #endregion

                #region Inner Products

                case Tokens.IPADDMULTIPLY:
                    return baseT.Field("IPAddMultiply");
                case Tokens.IPMAXADD:
                    return baseT.Field("IPMaxAdd");
                case Tokens.IPMINADD:
                    return baseT.Field("IPMinAdd");

                #endregion

                #region Outer Products

                case Tokens.OPADD:
                    return baseT.Field("OPAdd");
                case Tokens.OPDIVIDE:
                    return baseT.Field("OPDivide");
                case Tokens.OPEQUAL:
                    return baseT.Field("OPEqual");
                case Tokens.OPGT:
                    return baseT.Field("OPGreater");
                case Tokens.OPGTE:
                    return baseT.Field("OPGreaterEqual");
                case Tokens.OPLT:
                    return baseT.Field("OPLess");
                case Tokens.OPLTE:
                    return baseT.Field("OPLessEqual");
                case Tokens.OPMAX:
                    return baseT.Field("OPMax");
                case Tokens.OPMIN:
                    return baseT.Field("OPMin");
                case Tokens.OPMULTIPLY:
                    return baseT.Field("OPMultiply");
                case Tokens.OPNOTEQUAL:
                    return baseT.Field("OPNotEqual");
                case Tokens.OPPOWER:
                    return baseT.Field("OPPower");
                case Tokens.OPRESIDUE:
                    return baseT.Field("OPResidue");
                case Tokens.OPSUBSTRACT:
                    return baseT.Field("OPSubtract");

                #endregion

                #region Bitwise Operators

                case Tokens.BWAND:
                    return baseT.Field("BitwiseAnd");
                case Tokens.BWOR:
                    return baseT.Field("BitwiseOr");
                case Tokens.BWLESS:
                    return baseT.Field("BitwiseLess");
                case Tokens.BWLESSEQUAL:
                    return baseT.Field("BitwiseLessEqual");
                case Tokens.BWEQUAL:
                    return baseT.Field("BitwiseEqual");
                case Tokens.BWGREATEREQUAL:
                    return baseT.Field("BitwiseGreaterEqual");
                case Tokens.BWGREATER:
                    return baseT.Field("BitwiseGreater");
                case Tokens.BWNOTEQUAL:
                    return baseT.Field("BitwiseNotEqual");

                #endregion

                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts a Monadic token to a Dyadic Token. If no conversion is available then the Token is not modified
        /// </summary>
        /// <param name="token"></param>
        /// <returns>True if there was a conversion otherwise false</returns>
        internal static bool ConvertToDyadicToken(Token token)
        {
            if (ReWriteRules.ContainsKey(token.Type))
            {
                token.Type = ReWriteRules[token.Type];
                return true;
            }

            return false;
        }
    }
}
