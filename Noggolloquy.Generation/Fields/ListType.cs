using System;

namespace Noggolloquy.Generation
{
    public class ListType : ContainerType
    {
        public override string TypeName
        {
            get
            {
                return "INotifyingList<" + this.ItemTypeName + ">";
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine("private readonly INotifyingList<" + ItemTypeName + "> _" + this.Name + " = new NotifyingList<" + ItemTypeName + ">();");
            fg.AppendLine($"public INotifyingList{(this.Protected ? "Getter" : string.Empty)}<{ItemTypeName}> {this.Name} {{ get {{ return _{this.Name}; }} }}");
            GenerateInterfaceMembers(fg, "_" + this.Name);
        }

        public void GenerateInterfaceMembers(FileGeneration fg, string member)
        {
            using (new RegionWrapper(fg, "Interface Members"))
            {
                // Get nth
                fg.AppendLine("public " + ItemTypeName + " GetNth" + this.Name + "(int index)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("return " + member + "[index];");
                }
                if (this.isLevSingle)
                {
                    fg.AppendLine($"{GetterTypeName} {this.ObjectGen.Getter_InterfaceStr}.GetNth{this.Name}(int index)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("return " + member + "[index];");
                    }
                }

                fg.AppendLine("INotifyingList" + (this.Protected ? "Getter" : string.Empty) + "<" + this.ItemTypeName + "> " + this.ObjectGen.InterfaceStr + "." + this.Name + " { get { return " + member + "; } }");
                fg.AppendLine("INotifyingListGetter<" + this.ItemTypeName + "> " + this.ObjectGen.Getter_InterfaceStr + "." + this.Name + " { get { return " + member + "; } }");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (this.isLevSingle)
            {
                fg.AppendLine("new " + ItemTypeName + " GetNth" + this.Name + "(int index);");
            }
            fg.AppendLine("new INotifyingList" + (this.Protected ? "Getter" : string.Empty) + "<" + ItemTypeName + "> " + this.Name + " { get; }");
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine(GetterTypeName + " GetNth" + this.Name + "(int index);");
            fg.AppendLine("INotifyingListGetter<" + ItemTypeName + "> " + this.Name + " { get; }");
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine("return " + identifier + "." + this.Name + ";");
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdAccessor)
        {
            fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
            using (new BraceWrapper(fg))
            {
                if (defaultFallbackAccessor == null || !this.isLevSingle)
                {
                    GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdAccessor);
                }
                else
                {
                    fg.AppendLine("int i = 0;");
                    fg.AppendLine("List<" + this.ItemTypeName + "> defList = " + defaultFallbackAccessor + "?." + this.Name + ".ToList();");
                    fg.AppendLine(accessorPrefix + "." + this.Name + ".SetTo(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine(rhsAccessorPrefix + "." + this.Name + ".Select((s) =>");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine("return s.Copy(defList?[i++]);");
                        }
                    }
                    fg.AppendLine("), " + cmdAccessor + ");");
                }
            }
            fg.AppendLine("else");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("if (" + defaultFallbackAccessor + " == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine(accessorPrefix + "." + this.Name + ".Unset(" + cmdAccessor + ".ToUnsetParams());");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    GenerateCopy(fg, accessorPrefix, defaultFallbackAccessor, cmdAccessor);
                }
            }
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdAccessor)
        {
            GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdAccessor);
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdAccessor)
        {
            fg.AppendLine(accessorPrefix + "." + this.Name + ".SetTo(" + rhsAccessorPrefix + "." + this.Name + (this.isLevSingle ? ".Select((s) => s.Copy())" : string.Empty) + ", " + cmdAccessor + ");");
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine(accessorPrefix + "." + this.Name + ".Unset(" + cmdAccessor + ".ToUnsetParams());");
        }
    }
}
