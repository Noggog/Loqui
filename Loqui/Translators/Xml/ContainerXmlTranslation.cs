using System.Xml.Linq;
using Loqui.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Loqui.Xml;

public abstract class ContainerXmlTranslation<T> : IXmlTranslation<IEnumerable<T>>
{
    public abstract string ElementName { get; }

    public bool Parse(
        XElement node,
        [MaybeNullWhen(false)] out IEnumerable<T> enumer,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask)
    {
        var transl = XmlTranslator<T>.Translator;
        if (transl.Failed || transl.Value == null)
        {
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Reason}"));
            enumer = null;
            return false;
        }
        return Parse(
            node,
            out enumer,
            errorMask: errorMask,
            transl: transl.Value.Parse,
            translationMask: translationMask);
    }

    public bool Parse(
        XElement node,
        out IEnumerable<T> enumer,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask,
        XmlSubParseDelegate<T> transl)
    {
        var ret = new List<T>();
        int i = 0;
        var subCrystal = translationMask?.GetSubCrystal(0);
        foreach (var listElem in node.Elements())
        {
            using (errorMask?.PushIndex(i++))
            {
                try
                {
                    if (transl(listElem, out var subItem, errorMask, subCrystal))
                    {
                        ret.Add(subItem);
                    }
                }
                catch (Exception ex)
                    when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        enumer = ret;
        return true;
    }

    public void Write(
        XElement node,
        string? name,
        IEnumerable<T> item,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask)
    {
        var transl = XmlTranslator<T>.Translator;
        if (transl.Failed || transl.Value == null)
        {
            errorMask.ReportExceptionOrThrow(
                new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Reason}"));
            return;
        }
        Write(
            node: node,
            name: name,
            item: item,
            errorMask: errorMask,
            translationMask: translationMask,
            transl: (XElement n, T item1, ErrorMaskBuilder? errorMask2, TranslationCrystal? transCrystal2) => 
                transl.Value.Write(
                    node: n, 
                    name: "Item", 
                    item: item1,
                    errorMask: errorMask2,
                    translationMask: transCrystal2));
    }

    public void Write(
        XElement node,
        string? name,
        IEnumerable<T> item,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask,
        XmlSubWriteDelegate<T> transl)
    {
        var elem = new XElement(name!);
        node.Add(elem);
        int i = 0;
        var subCrystal = translationMask?.GetSubCrystal(0);
        foreach (var listObj in item)
        {
            using (errorMask?.PushIndex(i++))
            {
                try
                {
                    WriteSingleItem(elem, transl, listObj, errorMask, subCrystal);
                }
                catch (Exception ex)
                    when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }
    }

    public abstract void WriteSingleItem(
        XElement node,
        XmlSubWriteDelegate<T> transl,
        T item, 
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask);
        
    public void Write(
        XElement node,
        string name,
        IEnumerable<T> item,
        int fieldIndex,
        ErrorMaskBuilder? errorMask,
        TranslationCrystal? translationMask,
        XmlSubWriteDelegate<T> transl)
    {
        using (errorMask?.PushIndex(fieldIndex))
        {
            try
            {
                Write(
                    node: node,
                    name: name,
                    item: item,
                    errorMask: errorMask,
                    translationMask: translationMask,
                    transl: transl);
            }
            catch (Exception ex)
                when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
        }
    }
}