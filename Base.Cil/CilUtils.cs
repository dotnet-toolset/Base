using System.Reflection;
using System.Reflection.Emit;

namespace Base.Cil
{
    public static class CilUtils
    {
        private const string ModuleName = "dyncil";

        public static readonly ModuleBuilder ModuleBuilder;

        static CilUtils()
        {
#if NETSTANDARD
            var vAssembly =
                AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ModuleName), AssemblyBuilderAccess.Run);
            ModuleBuilder = vAssembly.DefineDynamicModule(ModuleName);
        #else
            var vAssembly =
                AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(ModuleName), AssemblyBuilderAccess.Run);
            ModuleBuilder = vAssembly.DefineDynamicModule(ModuleName, false);
#endif
        }

    }
}