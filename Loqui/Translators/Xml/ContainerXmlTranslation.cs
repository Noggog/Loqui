using Loqui.Xml;
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

namespace Loqui.Xml
{
    public delegate TryGet<T> XmlSubParseDelegate<T, ErrMask>(XElement root, out ErrMask maskObj);
    public delegate void XmlSubWriteDelegate<in T, ErrMask>(T item, out ErrMask maskObj);
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
            return Parse(
                root, 
                doMasks,
                out maskObj,
                transl: (XElement r, out object obj) => transl.Item.Value.Parse(root: r, doMasks: doMasks, maskObj: out obj));
        }
        
        public TryGet<IEnumerable<T>> Parse<ErrMask>(
            XElement root,
            bool doMasks,
            out object maskObj,
            XmlSubParseDelegate<T, ErrMask> transl)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (!doMasks) throw ex;
                maskObj = ex;
                return TryGet<IEnumerable<T>>.Failure;
            }
            List<MaskItem<Exception, ErrMask>> maskList = null;
            var ret = new List<T>();
            foreach (var listElem in root.Elements())
            {
                try
                {
                    var get = transl(listElem, out var subMaskObj);
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
                            maskList = new List<MaskItem<Exception, ErrMask>>();
                        }
                        maskList.Add(new MaskItem<Exception, ErrMask>(null, subMaskObj));
                    }
                }
                catch (Exception ex)
                {
                    if (!doMasks) throw;
                    if (maskList == null)
                    {
                        maskList = new List<MaskItem<Exception, ErrMask>>();
                    }
                    maskList.Add(new MaskItem<Exception, ErrMask>(ex, default(ErrMask)));
                }
            }
            maskObj = maskList;
            return TryGet<IEnumerable<T>>.Succeed(ret);
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
            this.Write<object>(
                writer: writer, 
                name: name, 
                item: item, 
                doMasks: doMasks, 
                maskObj: out maskObj, 
                transl: (T item1, out object obj) => transl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: doMasks, maskObj: out obj));
        }

        public void Write<ErrMask>(
            XmlWriter writer,
            string name, 
            IEnumerable<T> item, 
            bool doMasks, 
            out object maskObj,
            XmlSubWriteDelegate<T, ErrMask> transl)
        {
            List<MaskItem<Exception, ErrMask>> maskList = null;
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
                        WriteSingleItem(writer, transl, listObj, doMasks, out ErrMask subMaskObj);
                        if (subMaskObj != null)
                        {
                            if (maskList == null)
                            {
                                maskList = new List<MaskItem<Exception, ErrMask>>();
                            }
                            maskList.Add(new MaskItem<Exception, ErrMask>(null, subMaskObj));
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
                            maskList = new List<MaskItem<Exception, ErrMask>>();
                        }
                        maskList.Add(new MaskItem<Exception, ErrMask>(ex, default(ErrMask)));
                    }
                }
            }
            maskObj = maskList;
        }

        public abstract void WriteSingleItem<ErrMask>(XmlWriter writer, XmlSubWriteDelegate<T, ErrMask> transl, T item, bool doMasks, out ErrMask maskObj);
    }
}
