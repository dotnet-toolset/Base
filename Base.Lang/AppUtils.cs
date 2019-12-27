using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Base.Lang.Attributes;

namespace Base.Lang
{
    public static class AppUtils
    {
        private static string _subname;

        public static Assembly GetEntryAssembly()
        {
            var vCurrentDomain = AppDomain.CurrentDomain;
#if NETSTANDARD
            var vEntryAssembly = Assembly.GetEntryAssembly();
#else
            var vEntryAssembly = vCurrentDomain.DomainManager?.EntryAssembly;
            if (vEntryAssembly != null) return vEntryAssembly;
            vEntryAssembly = Assembly.GetEntryAssembly();
#endif
            if (vEntryAssembly != null) return vEntryAssembly;
            foreach (var vAssembly in vCurrentDomain.GetAssemblies())
                if (vAssembly.EntryPoint != null)
                {
                    vEntryAssembly = vAssembly;
                    break;
                }

            return vEntryAssembly;
        }

        public static T GetApplicationAttribute<T>() where T : Attribute
        {
            return GetEntryAssembly().GetAttribute<T>();
        }

        public static string GetApplicationCompany()
        {
            var vAttr = GetApplicationAttribute<AssemblyCompanyAttribute>();
            return vAttr?.Company;
        }

        public static string GetApplicationProduct()
        {
            var vProduct = GetApplicationAttribute<AssemblyProductAttribute>();
            return vProduct?.Product;
        }

        public static string GetApplicationName()
        {
            var sb = new StringBuilder(GetApplicationProduct());
            var name = GetApplicationAttribute<NameAttribute>()?.Name;
            if (!String.IsNullOrEmpty(name))
                sb.Append('-').Append(name);
            if (!String.IsNullOrEmpty(_subname))
                sb.Append('-').Append(_subname);
            if (sb.Length == 0)
            {
                var ea = GetEntryAssembly();
                if (ea != null) return Path.GetFileNameWithoutExtension(ea.Location);
                return null;
            }
            return sb.ToString();
        }

        public static void SetApplicationSubName(string subname)
        {
            if (_subname != null) throw new CodeBug("subname may be set only once");
            _subname = subname.CheckNotNull(nameof(subname));
        }

        public static string GetApplicationTitle()
        {
            var vTitle = GetApplicationAttribute<AssemblyTitleAttribute>();
            return vTitle?.Title;
        }

        public static Version GetApplicationVersion()
        {
            var vAssembly = GetEntryAssembly();
            return vAssembly?.GetName().Version;
        }

        public static Guid GetMvid()
        {
            var entryAssembly = AppUtils.GetEntryAssembly();
            if (entryAssembly == null) return Guid.Empty;
            foreach (var vModule in entryAssembly.GetModules())
                if (!vModule.IsResource())
                {
                    var vResult = vModule.ModuleVersionId;
                    if (vResult != Guid.Empty) return vResult;
                }

            return Guid.Empty;
        }

        public static string GetStartupPath()
        {
            return Path.GetDirectoryName(GetExecutablePath());
        }

        internal static string GetLocalPath(string fileName)
        {
            var uri = new Uri(fileName);
            return uri.LocalPath + uri.Fragment;
        }

        public static string GetExecutablePath()
        {
            var vAssembly = AppUtils.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var vResult = vAssembly.EscapedCodeBase;
            try
            {
                var vUri = new Uri(vResult);
                vResult = vUri.Scheme == "file" ? GetLocalPath(vResult) : vUri.ToString();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return vResult;
        }
    }
}