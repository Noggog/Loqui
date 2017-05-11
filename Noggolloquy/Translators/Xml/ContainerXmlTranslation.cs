using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml;
using System.Xml.Linq;
using Noggog.Notifying;
using Noggog.Xml;

namespace Noggolloquy.Xml
{
    public delegate void XmlSubWriteDelegate<in T>(T item, out object maskObj);
    public abstract class ContainerXmlTranslation<T> : IXmlTranslation<IEnumerable<T>>
    {
        public abstract string ElementName { get; }

        public TryGet<IEnumerable<T>> Parse(XElement root, bool doMasks, out object maskObj)
        {
            var transl = XmlTranslator<T>.Translator;
            if (transl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}");
            }
            return Parse(transl.Item.Value, root, doMasks, out maskObj);
        }

        public TryGet<IEnumerable<T>> Parse(IXmlTranslation<T> transl, XElement root, bool doMasks, out object maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<IEnumerable<T>>.Failure;
                }
                else
                {
                    throw ex;
                }
            }
            return TryGet<IEnumerable<T>>.Succeed(Parse_Internal(transl, root, doMasks, out maskObj));
        }

        private IEnumerable<T> Parse_Internal(IXmlTranslation<T> transl, XElement root, bool doMasks, out object maskObj)
        {
            List<MaskItem<Exception, object>> maskList = null;
            var ret = new List<T>();
            foreach (var listElem in root.Elements())
            {
                try
                {
                    var get = ParseSingleItem(listElem, transl, doMasks, out object subMaskObj);
                    if (get.Succeeded)
                    {
                        ret.Add(get.Value);
                    }
                    else
                    {
                        if (!doMasks)
                        { // This shouldn't actually throw, as subparse is expected to throw if doMasks is off
                            throw new ArgumentException("Error parsing list.  Could not parse subitem.");
                        }
                        if (maskList == null)
                        {
                            maskList = new List<MaskItem<Exception, object>>();
                        }
                        maskList.Add(new MaskItem<Exception, object>(null, subMaskObj));
                    }
                }
                catch (Exception ex)
                {
                    if (!doMasks)
                    {
                        throw;
                    }
                    else
                    {
                        if (maskList == null)
                        {
                            maskList = new List<MaskItem<Exception, object>>();
                        }
                        maskList.Add(new MaskItem<Exception, object>(ex, null));
                    }
                }
            }
            maskObj = maskList;
            return ret;
        }

        public abstract TryGet<T> ParseSingleItem(XElement root, IXmlTranslation<T> transl, bool doMasks, out object maskObj);

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<T> item, 
            bool doMasks,
            out object maskObj)
        {
            var transl = XmlTranslator<T>.Translator;
            if (transl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}");
            }
            this.Write(
                writer: writer, 
                name: name, 
                item: item, 
                doMasks: doMasks, 
                maskObj: out maskObj, 
                transl: (T item1, out object obj) => transl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: doMasks, maskObj: out obj));
        }

        public void Write(
            XmlWriter writer,
            string name, 
            IEnumerable<T> item, 
            bool doMasks, 
            out object maskObj,
            XmlSubWriteDelegate<T> transl)
        {
            List<MaskItem<Exception, object>> maskList = null;
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString(XmlConstants.NAME_ATTRIBUTE, name);
                }
                foreach (var listObj in item)
                {
                    try
                    {
                        WriteSingleItem(writer, transl, listObj, doMasks, out object subMaskObj);
                        if (subMaskObj != null)
                        {
                            if (maskList == null)
                            {
                                maskList = new List<MaskItem<Exception, object>>();
                            }
                            maskList.Add(new MaskItem<Exception, object>(null, subMaskObj));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!doMasks)
                        {
                            throw;
                        }
                        if (maskList == null)
                        {
                            maskList = new List<MaskItem<Exception, object>>();
                        }
                        maskList.Add(new MaskItem<Exception, object>(ex, null));
                    }
                }
            }
            maskObj = maskList;
        }

        public abstract void WriteSingleItem(XmlWriter writer, XmlSubWriteDelegate<T> transl, T item, bool doMasks, out object maskObj);
    }
}
