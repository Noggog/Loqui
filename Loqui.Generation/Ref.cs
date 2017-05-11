using System.Collections.Generic;

namespace Loqui.Generation
{
    public class Ref
    {
        public ObjectGeneration Obj;
        public virtual string Getter_InterfaceStr => Obj.Getter_InterfaceStr;
        public virtual string InterfaceStr => Obj.InterfaceStr;
        public virtual string ObjectName => Obj.ObjectName;
        public virtual string Name => Obj.Name;
        public virtual string ErrorMask => Obj.ErrorMask;
    }

    public class GenRef : Ref
    {
        public List<string> Generics = new List<string>();
        public override string Getter_InterfaceStr => $"{Obj.Getter_InterfaceStr_NoGenerics}<{string.Join(", ", Generics)}>";
        public override string ObjectName => $"{Obj.ObjectName}<{string.Join(", ", Generics)}> ";
        public override string Name => $"{Obj.Name}<{string.Join(", ", Generics)}>";
        public override string ErrorMask => $"base.ErrorMask{Obj.GenericTypes}";
    }
}
