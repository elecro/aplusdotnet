using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

using DLR = System.Linq.Expressions;
using DYN = System.Dynamic;

using AplusCore.Compiler;
using AplusCore.Runtime;
using AplusCore.Types;

namespace AplusDllCompiler
{
    internal class AssemblyCreator
    {
        #region variables

        private string assemblyBaseName;
        private IDictionary<string, DLR.LambdaExpression> methods;

        #endregion

        #region properties

        public string AssemblyBaseName { get { return this.assemblyBaseName; } }
        public string AssemblyName { get { return this.assemblyBaseName + "Generated"; } }
        public string ModuleName { get { return this.AssemblyName; } }

        public ICollection<string> MethodNames { get { return this.methods.Keys; } }

        #endregion

        #region constructor

        public AssemblyCreator(string assemblyName)
        {
            this.methods = new Dictionary<string, DLR.LambdaExpression>();
            this.assemblyBaseName = assemblyName;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Add an A+ method to be built into a DLL
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        public void AddMethod(string name, DLR.LambdaExpression method)
        {
            this.methods[name] = method;
        }

        /// <summary>
        /// Builds a DLL based on the added methods and the assembly name specified
        /// </summary>
        /// <returns>Number of methods added to the generated DLL</returns>
        public List<string> Build()
        {
            List<string> generatedNames = new List<string>();

            AssemblyName asmName = new AssemblyName(this.AssemblyName);
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(this.ModuleName, this.ModuleName + ".dll", true);

            // Create a static class
            TypeBuilder staticTypeBuilder = moduleBuilder.DefineType(this.assemblyBaseName + "StaticClass", TypeAttributes.Public);

            // Add all A+ lambdas as a static method
            foreach (KeyValuePair<string, DLR.LambdaExpression> methodInfo in this.methods)
            {
                Type[] argumentTypes = methodInfo.Value.Parameters.Select(item => item.Type).ToArray();

                // Create a static method
                MethodBuilder staticMethodBuilder = staticTypeBuilder.DefineMethod(
                    methodInfo.Key,
                    MethodAttributes.Public | MethodAttributes.Static,
                    methodInfo.Value.ReturnType,
                    argumentTypes
                );

                methodInfo.Value.CompileToMethod(staticMethodBuilder);
                string argNames = string.Join(", ", argumentTypes.Select(item => item.Name));
                generatedNames.Add(string.Format("{0}.{1}({2})", staticTypeBuilder.Name, methodInfo.Key, argNames));
            }

            Type staticClass = staticTypeBuilder.CreateType();

            // Create a module initializer
            Type initializerClass = ConstructModuleInitializer(generatedNames, moduleBuilder, staticClass);

            // Create a non-static mapping.
            TypeBuilder instanceTypeBuilder = moduleBuilder.DefineType(this.assemblyBaseName + "Class", TypeAttributes.Public);

            // Create a private A+ runtime variable
            string runtimeName = "_aplusRuntime";
            FieldBuilder runtimeFieldBuilder = instanceTypeBuilder.DefineField(runtimeName, typeof(Aplus), FieldAttributes.Private);
            generatedNames.Add(string.Format("{0}.{1}", instanceTypeBuilder.Name, runtimeName));

            CreateConstructor(instanceTypeBuilder, runtimeFieldBuilder, initializerClass.GetMethod("Initalize"));
            generatedNames.Add(string.Format("{0}..ctor()", instanceTypeBuilder.Name));
            GenerateRuntimeGetter(instanceTypeBuilder, "Runtime", runtimeFieldBuilder);
            generatedNames.Add(string.Format("{0}.Runtime", instanceTypeBuilder.Name));

            foreach (KeyValuePair<string, DLR.LambdaExpression> methodInfo in this.methods)
            {
                GenerateInstanceCode(
                    instanceTypeBuilder,
                    methodInfo.Key,
                    methodInfo.Value,
                    runtimeFieldBuilder,
                    staticClass.GetMethod(methodInfo.Key, BindingFlags.Public | BindingFlags.Static)
                );
                string argNames = string.Join(", ", methodInfo.Value.Parameters
                    .Skip(1) // We skip the A+ runtime as that is now a private member and not an external argument
                    .Select(item => item.Type.Name));
                generatedNames.Add(string.Format("{0}.{1}({2})", instanceTypeBuilder.Name, methodInfo.Key, argNames));
            }

            instanceTypeBuilder.CreateType();
            asmBuilder.Save(asmName.Name + ".dll");

            return generatedNames;
        }

        #endregion

        #region type builder helpers


        /// <summary>
        /// Build a Module initializer class.
        /// </summary>
        /// <param name="generatedNames">List to which the generated names are added.</param>
        /// <param name="moduleBuilder">ModuleBuilder in which the Initializer should be created.</param>
        /// <param name="staticClass">The target class containing the target methods.</param>
        /// <returns>The generated Module Initializer class.</returns>
        private Type ConstructModuleInitializer(List<string> generatedNames, ModuleBuilder moduleBuilder, Type staticClass)
        {
            // Build up the function which assigns the method names in the A+ runtime to
            // thier respsective static method (which is from the 'staticClass')
            DLR.ParameterExpression runtimeParam = DLR.Expression.Parameter(typeof(Aplus), "__aplusRuntime__");
            DLR.ParameterExpression contextVar = DLR.Expression.Parameter(typeof(DYN.IDynamicMetaObjectProvider), "__module__");
            DLR.LambdaExpression initLambda = DLR.Expression.Lambda(
                DLR.Expression.Block(
                    new DLR.ParameterExpression[] { contextVar },
                    DLR.Expression.Assign(
                        contextVar,
                        DLR.Expression.PropertyOrField(runtimeParam, "Context")
                    ),
                    DLR.Expression.Block(
                        this.methods.Select(entry => BuildSetter(entry.Key, entry.Value, staticClass, contextVar))
                    ),
                    DLR.Expression.Empty()
                ),
                "Initalize",
                new DLR.ParameterExpression[] { runtimeParam }
            );

            // Build the Class & Method
            TypeBuilder typeBuilder = moduleBuilder.DefineType(this.assemblyBaseName + "InitializerClass", TypeAttributes.Public);
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                initLambda.Name,
                MethodAttributes.Public | MethodAttributes.Static,
                initLambda.ReturnType,
                initLambda.Parameters.Select(param => param.Type).ToArray()
            );

            string argNames = string.Join(", ", initLambda.Parameters.Select(param => param.Type.Name));
            generatedNames.Add(string.Format("{0}.{1}({2})", typeBuilder.Name, initLambda.Name, argNames));

            initLambda.CompileToMethod(methodBuilder);

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Helper method which creates a scope variable assignment for a method name and it's static class invoker.
        /// </summary>
        /// <remarks>
        /// An Expression tree for a scope variable assignment is created here. The value for the variable is
        /// an expression in which a new AFunction is created wrapping the target staic method in and reversing the order
        /// of the arguments.
        /// </remarks>
        /// <param name="methodName">Name of the target static method.</param>
        /// <param name="lambda">The LambdaExpression from which the target static method was built.</param>
        /// <param name="staticClass">The target static class.</param>
        /// <param name="contextVar">The A+ runtime context ParameterExpression.</param>
        /// <returns>An Expression Tree of a single scope variable assignment.</returns>
        private static DLR.Expression BuildSetter(string methodName, DLR.LambdaExpression lambda, Type staticClass, DLR.ParameterExpression contextVar)
        {
            MethodInfo targetMethod = staticClass.GetMethod(methodName);

            List<Type> callTypes = new List<Type>();
            callTypes.AddRange(targetMethod.GetParameters().Select(param => param.ParameterType));
            callTypes.Add(targetMethod.ReturnType);

            // Now then we'll do magic here:
            // As we reversed the order of the arguments on the generated static methods
            // calling those from the A+ lambda functions will have incorrect results.
            //
            // So we need a way to correctly pass the arguments (that is reverse again)
            //
            // What we'll do here is that we create a lambda function which reverses order of
            // the argument list for us. But! we keep in mind that the first argument must be the A+ runtime.

            // First we create the forwareder parameters
            List<DLR.ParameterExpression> parameters = targetMethod.GetParameters()
                .Select(param => DLR.Expression.Parameter(param.ParameterType, "forward_" + param.Name))
                .ToList();

            // Second we create the lambda method which has the reversed parameter order
            // just like a normal A+ method in the engine.
            DLR.Expression argumentForwarederLambda = DLR.Expression.Lambda(
                DLR.Expression.Call(
                    targetMethod,
                    parameters
                ),
                methodName + "_methodbinder",
                // This is the magic: reverse the call arguments order
                // example:
                //  (Aplus runtime, AType arg1, AType arg2) -> (Aplus runtime, AType arg2, AType arg1)
                parameters.Take(1).Concat(parameters.Skip(1).Reverse())
            );

            // Third we build the AFunc creator call.
            DLR.Expression aFunc = DLR.Expression.Call(
                typeof(AFunc).GetMethod("Create"),
                DLR.Expression.Constant(lambda.Name),
                argumentForwarederLambda,
                DLR.Expression.Constant(lambda.Parameters.Count),
                DLR.Expression.Constant(methodName)
            );

            string[] contextParts = Util.SplitUserName(lambda.Name);

            // And lastly we simply call the Engine's helper method to set the variable.
            return DLR.Expression.Call(
                Helpers.SetVariableMethod,
                contextVar,
                DLR.Expression.Constant(contextParts[0]),
                DLR.Expression.Constant(contextParts[1]),
                aFunc
            );
        }

        /// <summary>
        /// Creates the Constructor code for the class.
        /// </summary>
        /// <param name="instanceTypeBuilder">Type on which the Constructor should be built.</param>
        /// <param name="runtimeFieldBuilder">The FieldBuilder for the A+ runtime field.</param> 
        /// <param name="initializerClass">The initializer method which should be called with the A+ runtime.</param>
        private static void CreateConstructor(TypeBuilder instanceTypeBuilder, FieldBuilder runtimeFieldBuilder, MethodInfo initializerMethod)
        {
            // Create a constuctor to initialize the A+ runtime
            ConstructorBuilder constructorBuilder = instanceTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { });
            ILGenerator constructorIl = constructorBuilder.GetILGenerator();
            // Load 'this' & call the object's constructor
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Call, typeof(Object).GetConstructor(new Type[] { }));

            // Load 'this'
            constructorIl.Emit(OpCodes.Ldarg_0);
            // Create a new Aplus object
            constructorIl.Emit(OpCodes.Newobj, typeof(Aplus).GetConstructor(new Type[] { }));
            // Store the new object into the A+ runtime variable
            constructorIl.Emit(OpCodes.Stfld, runtimeFieldBuilder);

            // Load 'this'
            constructorIl.Emit(OpCodes.Ldarg_0);
            // Load the A+ runtime variable
            constructorIl.Emit(OpCodes.Ldfld, runtimeFieldBuilder);
            // Call the static 'Initalizer' method
            constructorIl.Emit(OpCodes.Call, initializerMethod);

            constructorIl.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Generate a method to allow acces to the internal A+ runtime
        /// </summary>
        /// <param name="instanceTypeBuilder">The Type on which the method should be created</param>
        /// <param name="name">Name of the method to generate</param>
        /// <param name="runtimeFieldBuilder">Reference to the A+ runtime field</param>
        private static void GenerateRuntimeGetter(TypeBuilder instanceTypeBuilder, string name, FieldBuilder runtimeFieldBuilder)
        {
            MethodBuilder getRuntimeBuilder = instanceTypeBuilder.DefineMethod("get_" + name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName);
            getRuntimeBuilder.SetReturnType(typeof(Aplus));
            ILGenerator il = getRuntimeBuilder.GetILGenerator();
            // Load 'this'
            il.Emit(OpCodes.Ldarg_0);
            // Load the value from the A+ runtime field
            il.Emit(OpCodes.Ldfld, runtimeFieldBuilder);
            // Return the value
            il.Emit(OpCodes.Ret);

            PropertyBuilder propBuilder = instanceTypeBuilder.DefineProperty(name, PropertyAttributes.None, typeof(Aplus), new Type[] { });
            propBuilder.SetGetMethod(getRuntimeBuilder);
        }

        /// <summary>
        /// Generate the 'forwarder' call to the static class's method
        /// </summary>
        /// <param name="typeBuiler"></param>
        /// <param name="name"></param>
        /// <param name="dlrMethod"></param>
        /// <param name="runtimeFieldBuilder"></param>
        /// <param name="targetMethod"></param>
        private static void GenerateInstanceCode(TypeBuilder typeBuiler, string name, DLR.LambdaExpression dlrMethod, FieldBuilder runtimeFieldBuilder, MethodInfo targetMethod)
        {
            Type[] argumentTypes = dlrMethod.Parameters
                    .Skip(1) // We'll skip the A+ runtime Argument now
                    .Select(item => item.Type)
                    .ToArray();

            // && Create an instance method
            MethodBuilder methodBuilder = typeBuiler.DefineMethod(
                name,
                MethodAttributes.Public,
                dlrMethod.ReturnType,
                argumentTypes
            );

            // Add names for paramters
            // We start from 1 as the first is the A+ runtime
            // and for the DefineParameter the 0 is the return paramter
            for (int i = 1; i < dlrMethod.Parameters.Count; i++)
            {
                DLR.ParameterExpression parameter = dlrMethod.Parameters[i];
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameter.Name);
            }

            // Build up static method call
            ILGenerator il = methodBuilder.GetILGenerator();

            // Load the static method
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, runtimeFieldBuilder);

            // Load the arguments for the static method
            for (int arg = 0; arg < targetMethod.GetParameters().Length - 1; arg++)
            {
                il.Emit(OpCodes.Ldarg_S, arg + 1);
            }

            // Call & return
            il.Emit(OpCodes.Call, targetMethod);
            il.Emit(OpCodes.Ret);
        }

        #endregion
    }
}
