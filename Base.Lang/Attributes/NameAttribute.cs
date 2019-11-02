using System;

namespace Base.Lang.Attributes
{
    public class NameAttribute : Attribute
    {
        public string Name { get; }
        public string Title { get; set; }

        public NameAttribute(string name)
        {
            Name = name;
        }

        public NameAttribute(string name, string title)
        {
            Name = name;
            Title = title;
        }
    }
}
