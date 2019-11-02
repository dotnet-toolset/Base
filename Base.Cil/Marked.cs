using System;
using System.Linq;
using System.Reflection;
using Base.Cil.Attributes;

namespace Base.Cil
{
    public static class Marked
    {
        public static MemberInfo GetMember(Type type, int id) =>
            type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                            BindingFlags.Static)
                .FirstOrDefault(m => CustomAttributeExtensions.GetCustomAttribute<MarkAttribute>((MemberInfo) m)?.Id == id);

        public static T GetMember<T>(Type type, int id)
            where T : MemberInfo =>
            type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                            BindingFlags.Static).OfType<T>()
                .FirstOrDefault(m => m.GetCustomAttribute<MarkAttribute>()?.Id == id);

        public static TM GetMember<TT, TM>(int id)
            where TM : MemberInfo =>
            typeof(TT).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                  BindingFlags.Static).OfType<TM>().FirstOrDefault(m =>
                m.GetCustomAttribute<MarkAttribute>()?.Id == id);
    }
}