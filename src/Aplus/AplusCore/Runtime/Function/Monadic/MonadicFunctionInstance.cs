using AplusCore.Runtime.Function.Monadic.NonScalar.Comprasion;
using AplusCore.Runtime.Function.Monadic.NonScalar.Computational;
using AplusCore.Runtime.Function.Monadic.NonScalar.Informational;
using AplusCore.Runtime.Function.Monadic.NonScalar.Other;
using AplusCore.Runtime.Function.Monadic.NonScalar.Selection;
using AplusCore.Runtime.Function.Monadic.NonScalar.Structural;
using AplusCore.Runtime.Function.Monadic.Operator.Reduction;
using AplusCore.Runtime.Function.Monadic.Operator.Scan;
using AplusCore.Runtime.Function.Monadic.Scalar.Arithmetic;
using AplusCore.Runtime.Function.Monadic.Scalar.Elementary;
using AplusCore.Runtime.Function.Monadic.Scalar.Logical;
using AplusCore.Runtime.Function.Monadic.Scalar.Miscellaneous;

namespace AplusCore.Runtime.Function.Monadic
{
    public class MonadicFunctionInstance
    {
        #region SCALAR

        #region Arithmetic

        public static readonly AbstractMonadicFunction Identity = new Identity();
        public static readonly AbstractMonadicFunction Negate = new Negate();

        #endregion

        #region Logical

        public static readonly AbstractMonadicFunction Not = new Not();

        #endregion

        #region Miscellaneous

        public static readonly AbstractMonadicFunction AbosulteValue = new AbsoluteValue();
        public static readonly AbstractMonadicFunction Ceiling = new Ceiling();
        public static readonly AbstractMonadicFunction Floor = new Floor();
        public static readonly AbstractMonadicFunction Reciprocal = new Reciprocal();
        public static readonly AbstractMonadicFunction Roll = new Roll();
        public static readonly AbstractMonadicFunction Sign = new Sign();

        #endregion

        #region Elementary

        public static readonly AbstractMonadicFunction Exponential = new Exponential();
        public static readonly AbstractMonadicFunction NaturalLog = new NaturalLog();
        public static readonly AbstractMonadicFunction PiTimes = new PiTimes();

        #endregion

        #endregion

        #region NONSCALAR

        #region Comprasion

        public static readonly AbstractMonadicFunction GradeDown = new GradeDown();
        public static readonly AbstractMonadicFunction GradeUp = new GradeUp();
        public static readonly AbstractMonadicFunction PartitionCount = new PartitionCount();

        #endregion

        #region Computational

        public static readonly AbstractMonadicFunction MatrixInverse = new MatrixInverse();
        public static readonly AbstractMonadicFunction Pack = new Pack();
        public static readonly AbstractMonadicFunction Unpack = new Unpack();

        #endregion

        #region Informational

        public static readonly AbstractMonadicFunction Count = new Count();
        public static readonly AbstractMonadicFunction Depth = new Depth();
        public static readonly AbstractMonadicFunction Shape = new Shape();
        public static readonly AbstractMonadicFunction Type = new TypeFunction();

        #endregion

        #region Selection

        public static readonly AbstractMonadicFunction NullFunction = new NullFunction();
        public static readonly AbstractMonadicFunction Right = new Right();
        public static readonly AbstractMonadicFunction SeparateSymbols = new SeparateSymbols();

        #endregion

        #region Structural

        public static readonly AbstractMonadicFunction Disclose = new Disclose();
        public static readonly AbstractMonadicFunction Enclose = new Enclose();
        public static readonly AbstractMonadicFunction Interval = new Interval();
        public static readonly AbstractMonadicFunction ItemRavel = new ItemRavel();
        public static readonly AbstractMonadicFunction Rake = new Rake();
        public static readonly AbstractMonadicFunction Ravel = new Ravel();
        public static readonly AbstractMonadicFunction Raze = new Raze();
        public static readonly AbstractMonadicFunction Reverse = new Reverse();
        public static readonly AbstractMonadicFunction Transpose = new Transpose();

        #endregion

        #region Other

        public static readonly AbstractMonadicFunction ExecuteFunction = new ExecuteFunction();
        public static readonly AbstractMonadicFunction DefaultFormat = new DefaultFormat();
        public static readonly AbstractMonadicFunction Print = new Print();
        public static readonly AbstractMonadicFunction Signal = new Signal();
        public static readonly AbstractMonadicFunction Stop = new Stop();
        public static readonly AbstractMonadicFunction MapIn = new MapIn();
        public static readonly AbstractMonadicFunction Value = new Value();

        #endregion

        #endregion

        #region OPERATOR

        #region Bitwise

        public static readonly AbstractMonadicFunction BitwiseNot = new BitwiseNot();

        #endregion

        #region Reduction

        public static readonly AbstractMonadicFunction ReduceAdd = new ReduceAdd();
        public static readonly AbstractMonadicFunction ReduceMultiply = new ReduceMultiply();
        public static readonly AbstractMonadicFunction ReduceOr = new ReduceOr();
        public static readonly AbstractMonadicFunction ReduceAnd = new ReduceAnd();
        public static readonly AbstractMonadicFunction ReduceMax = new ReduceMax();
        public static readonly AbstractMonadicFunction ReduceMin = new ReduceMin();

        #endregion

        #region Scan

        public static readonly AbstractMonadicFunction ScanAdd = new ScanAdd();
        public static readonly AbstractMonadicFunction ScanMultiply = new ScanMultiply();
        public static readonly AbstractMonadicFunction ScanMin = new ScanMin();
        public static readonly AbstractMonadicFunction ScanMax = new ScanMax();
        public static readonly AbstractMonadicFunction ScanAnd = new ScanAnd();
        public static readonly AbstractMonadicFunction ScanOr = new ScanOr();

        #endregion

        #endregion

    }
}
