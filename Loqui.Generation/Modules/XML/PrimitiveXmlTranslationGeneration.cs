using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class PrimitiveXmlTranslationGeneration<T> : XmlTranslationGeneration
{
    private string _typeName;
    public virtual string TypeName(TypeGeneration typeGen) => _typeName;
    private bool? nullable;
    public bool Nullable => nullable ?? false || typeof(T).GetName().EndsWith("?");
    public bool CanBeNotNullable = true;

    public PrimitiveXmlTranslationGeneration(string typeName = null, bool? nullable = null)
    {
        this.nullable = nullable;
        _typeName = typeName ?? typeof(T).GetName().Replace("?", string.Empty);
    }

    protected virtual string ItemWriteAccess(TypeGeneration typeGen, Accessor itemAccessor)
    {
        return $"{itemAccessor.Access}{(typeGen.Nullable && typeGen.CanBeNullable(true) && CanBeNotNullable ? ".Value" : null)}";
    }

    public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
    {
        return $"{TypeName(typeGen)}XmlTranslation.Instance";
    }

    public override void GenerateWrite(
        FileGeneration fg,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor writerAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor nameAccessor,
        Accessor translationMaskAccessor)
    {
        using (var args = new ArgsWrapper(fg,
                   $"{TypeName(typeGen)}XmlTranslation.Instance.Write"))
        {
            args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
            args.Add($"name: {nameAccessor}");
            args.Add($"item: {ItemWriteAccess(typeGen, itemAccessor)}");
            if (typeGen.HasIndex)
            {
                args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
            }
            args.Add($"errorMask: {errorMaskAccessor}");
        }
    }

    public override void GenerateCopyIn(
        FileGeneration fg,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor nodeAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor translationMaskAccessor)
    {
        List<string> extraArgs = new List<string>();
        extraArgs.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {nodeAccessor}");

        WrapParseCall(
            new TranslationWrapParseArgs()
            {
                FG = fg,
                TypeGen = typeGen,
                TranslatorLine = $"{TypeName(typeGen)}XmlTranslation.Instance",
                MaskAccessor = errorMaskAccessor,
                ItemAccessor = itemAccessor,
                TranslationMaskAccessor = null,
                IndexAccessor = new Accessor(typeGen.IndexEnumInt),
                Do = typeGen.HasIndex,
                ExtraArgs = extraArgs.ToArray()
            });
    }

    public override XElement GenerateForXSD(
        ObjectGeneration obj,
        XElement rootElement,
        XElement choiceElement,
        TypeGeneration typeGen,
        string nameOverride = null)
    {
        var common = XmlMod.CommonXSDLocation(obj.ProtoGen);
        var relativePath = common.GetRelativePathTo(XmlMod.ObjectXSDLocation(obj));
        var includeElem = new XElement(
            XmlTranslationModule.XSDNamespace + "include",
            new XAttribute("schemaLocation", relativePath));
        if (!rootElement.Elements().Any((e) => e.ContentEqual(includeElem)))
        {
            rootElement.AddFirst(includeElem);
        }

        var elem = new XElement(XmlTranslationModule.XSDNamespace + "element");
        elem.Add(new XAttribute("name", nameOverride ?? typeGen.Name));
        elem.Add(new XAttribute("type", Nullable ? "NullableValueType" : "ValueType"));
        choiceElement.Add(elem);
        return elem;
    }

    public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
    {
        var nodeName = Nullable ? "NullableValueType" : "ValueType";
        if (rootElement.Elements().Any((e) => e.Attribute("name")?.Value.Equals(nodeName) ?? false)) return;

        rootElement.Add(
            new XElement(XmlTranslationModule.XSDNamespace + "complexType",
                new XAttribute("name", nodeName),
                new XElement(XmlTranslationModule.XSDNamespace + "attribute",
                    new XAttribute("name", "value"),
                    new XAttribute("use", Nullable ? "optional" : "required"))));
    }
}