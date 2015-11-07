using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

using DLR = System.Linq.Expressions;

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

            AssemblyName asmName = new AssemblyName(this.assemblyBaseName + "Generated");
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Save);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(this.assemblyBaseName + "Module", this.assemblyBaseName + "Module.dll", true);
            TypeBuilder staticTypeBuilder = moduleBuilder.DefineType(this.assemblyBaseName + "StaticClass", TypeAttributes.Public);

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

            // Create a non-static mapping.
            Type staticClass = staticTypeBuilder.CreateType();

            TypeBuilder instanceTypeBuilder = moduleBuilder.DefineType(this.assemblyBaseName + "Class", TypeAttributes.Public);
            // Create a private A+ runtime variable
            string runtimeName = "_aplusRuntime";
            FieldBuilder runtimeFieldBuilder = instanceTypeBuilder.DefineField(runtimeName, typeof(Aplus), FieldAttributes.Private);
            generatedNames.Add(string.Format("{0}.{1}", instanceTypeBuilder.Name, runtimeName));

            CreateConstructor(instanceTypeBuilder, runtimeFieldBuilder);
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
                string argNames = string.Join(", ", methodInfo.Value.Parameters.Select(item => item.Type.Name));
                generatedNames.Add(string.Format("{0}.{1}({2})", instanceTypeBuilder.Name, methodInfo.Key, argNames));
            }

            instanceTypeBuilder.CreateType();
            asmBuilder.Save(asmName.Name + ".dll");

            return generatedNames;
        }

        #endregion

        #region type builder helpers

        /// <summary>
        /// Creates the Constructor code for the class.
        /// </summary>
        /// <param name="instanceTypeBuilder">Type on which the Constructor should be built.</param>
        /// <param name="runtimeFieldBuilder">The FieldBuilder for the A+ runtime field.</param> 
        private static void CreateConstructor(TypeBuilder instanceTypeBuilder, FieldBuilder runtimeFieldBuilder)
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
