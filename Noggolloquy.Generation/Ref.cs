using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class Ref
    {
        public ObjectGeneration Obj;
        public virtual string Getter_InterfaceStr { get { return Obj.Getter_InterfaceStr; } }
        public virtual string InterfaceStr { get { return Obj.InterfaceStr; } }
        public virtual string ObjectName { get { return Obj.ObjectName; } }
        public virtual string Name { get { return Obj.Name; } }
    }

    public class GenRef : Ref
    {
        public List<string> Generics = new List<string>();
        public override string Getter_InterfaceStr { get { return $"{Obj.Getter_InterfaceStr_NoGenerics}<{string.Join(", ", Generics)}>"; } }
        public override string ObjectName { get { return $"{Obj.ObjectName}<{string.Join(", ", Generics)}> "; } }
        public override string Name { get { return $"{Obj.Name}<{string.Join(", ", Generics)}>"; } }
    }
}
