using AplusCore.Runtime.Function.Dyadic.NonScalar.Comparison;
using AplusCore.Runtime.Function.Dyadic.NonScalar.Computational;
using AplusCore.Runtime.Function.Dyadic.NonScalar.Other;
using AplusCore.Runtime.Function.Dyadic.NonScalar.Selection;
using AplusCore.Runtime.Function.Dyadic.NonScalar.Structural;
using AplusCore.Runtime.Function.Dyadic.Product;
using AplusCore.Runtime.Function.Dyadic.Scalar.Arithmetic;
using AplusCore.Runtime.Function.Dyadic.Scalar.Bitwise;
using AplusCore.Runtime.Function.Dyadic.Scalar.Elementary;
using AplusCore.Runtime.Function.Dyadic.Scalar.Logical;
using AplusCore.Runtime.Function.Dyadic.Scalar.Miscellaneous;
using AplusCore.Runtime.Function.Dyadic.Scalar.Relational;

namespace AplusCore.Runtime.Function.Dyadic
{
    public static class DyadicFunctionInstance
    {
        #region SCALAR

        #region Arithmetic

        public static readonly AbstractDyadicFunction Add = new Add();
        public static readonly AbstractDyadicFunction Divide = new Divide();
        public static readonly AbstractDyadicFunction Multiply = new Multiply();
        public static readonly AbstractDyadicFunction Subtract = new Subtract();
        
        #endregion

        #region Logical

        public static readonly AbstractDyadicFunction And = new And();
        public static readonly AbstractDyadicFunction Or = new Or();

        #endregion

        #region Miscellaneous

        public static readonly AbstractDyadicFunction CombineSymbols = new CombineSymbols();
        public static readonly AbstractDyadicFunction Max = new Max();
        public static readonly AbstractDyadicFunction Min = new Min();
        public static readonly AbstractDyadicFunction Residue = new Residue();
        
        #endregion

        #region Relational

        public static readonly AbstractDyadicFunction EqualTo = new EqualTo();
        public static readonly AbstractDyadicFunction GreaterThan = new GreaterThan();
        public static readonly AbstractDyadicFunction GreaterThanOrEqualTo = new GreaterThanOrEqualTo();
        public static readonly AbstractDyadicFunction LessThan = new LessThan();
        public static readonly AbstractDyadicFunction LessThanOrEqualTo = new LessThanOrEqualTo();
        public static readonly AbstractDyadicFunction NotEqualTo = new NotEqualTo();

        #endregion

        #region Elementary

        public static readonly AbstractDyadicFunction Circle = new Circle();
        public static readonly AbstractDyadicFunction Log = new Log();
        public static readonly AbstractDyadicFunction Power = new Power();

        #endregion

        #endregion

        #region NONSCALAR

        #region Comparison

        public static readonly AbstractDyadicFunction Bins = new Bins();
        public static readonly AbstractDyadicFunction Find = new Find();
        public static readonly AbstractDyadicFunction Match = new Match();
        public static readonly AbstractDyadicFunction Member = new Member();

        #endregion

        #region Computational

        public static readonly AbstractDyadicFunction Deal = new Deal();
        public static readonly AbstractDyadicFunction Decode = new Decode();
        public static readonly AbstractDyadicFunction Encode = new Encode();
        public static readonly AbstractDyadicFunction Solve = new Solve();

        #endregion

        #region Selection

        public static readonly Choose Choose = new Choose();
        public static readonly AbstractDyadicFunction Left = new Left();
        public static readonly Pick Pick = new Pick();

        #endregion

        #region Structural

        public static readonly AbstractDyadicFunction Catenate = new Catenate();
        public static readonly AbstractDyadicFunction Drop = new Drop();
        public static readonly AbstractDyadicFunction Expand = new Expand();
        public static readonly AbstractDyadicFunction Laminate = new Laminate();
        public static readonly AbstractDyadicFunction Partition = new Partition();
        public static readonly AbstractDyadicFunction Replicate = new Replicate();
        public static readonly AbstractDyadicFunction Reshape = new Reshape();
        public static readonly AbstractDyadicFunction Restructure = new Restructure();
        public static readonly AbstractDyadicFunction Rotate = new Rotate();
        public static readonly AbstractDyadicFunction Take = new Take();
        public static readonly AbstractDyadicFunction TransposeAxis = new TransposeAxes();

        #endregion

        #region Other

        public static readonly AbstractDyadicFunction Cast = new Cast();
        public static readonly AbstractDyadicFunction ExecuteInContext = new ExecuteInContext();
        public static readonly AbstractDyadicFunction Format = new Format();
        public static readonly AbstractDyadicFunction Map = new Map();
        public static readonly AbstractDyadicFunction ValueInContext = new ValueInContext();

        #endregion

        #endregion

        #region OPERATOR

        #region Inner Product

        public static readonly AbstractDyadicFunction IPAddMultiply = new IPAddMultiply();
        public static readonly AbstractDyadicFunction IPMaxAdd = new IPMaxAdd();
        public static readonly AbstractDyadicFunction IPMinAdd = new IPMinAdd();

        #endregion

        #region Outer Product

        public static readonly AbstractDyadicFunction OPAdd = new OPAdd();
        public static readonly AbstractDyadicFunction OPDivide = new OPDivide();
        public static readonly AbstractDyadicFunction OPEqual = new OPEqual();
        public static readonly AbstractDyadicFunction OPGreater = new OPGreater();
        public static readonly AbstractDyadicFunction OPGreaterEqual = new OPGreaterEqual();
        public static readonly AbstractDyadicFunction OPLess = new OPLess();
        public static readonly AbstractDyadicFunction OPLessEqual = new OPLessEqual();
        public static readonly AbstractDyadicFunction OPMax = new OPMax();
        public static readonly AbstractDyadicFunction OPMin = new OPMin();
        public static readonly AbstractDyadicFunction OPMultiply = new OPMultiply();
        public static readonly AbstractDyadicFunction OPNotEqual = new OPNotEqual();
        public static readonly AbstractDyadicFunction OPPower = new OPPower();
        public static readonly AbstractDyadicFunction OPResidue = new OPResidue();
        public static readonly AbstractDyadicFunction OPSubtract = new OPSubtract();

        #endregion

        #region Bitwise

        public static readonly AbstractDyadicFunction BitwiseAnd = new BitwiseAnd();
        public static readonly AbstractDyadicFunction BitwiseOr = new BitwiseOr();
        public static readonly AbstractDyadicFunction BitwiseLess = new BitwiseLess();
        public static readonly AbstractDyadicFunction BitwiseLessEqual = new BitwiseLessEqual();
        public static readonly AbstractDyadicFunction BitwiseEqual = new BitwiseEqual();
        public static readonly AbstractDyadicFunction BitwiseGreaterEqual = new BitwiseGreaterEqual();
        public static readonly AbstractDyadicFunction BitwiseGreater = new BitwiseGreater();
        public static readonly AbstractDyadicFunction BitwiseNotEqual = new BitwiseNotEqual();
        public static readonly AbstractDyadicFunction BitwiseCast = new BitwiseCast();

        #endregion

        #endregion
    }
}
