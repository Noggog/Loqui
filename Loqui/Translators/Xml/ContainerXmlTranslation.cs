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
using Loqui.Internal;

namespace Loqui.Xml
{
    public abstract class ContainerXmlTranslation<T, M> : IXmlTranslation<IEnumerable<T>, MaskItem<Exception, IEnumerable<M>>>
    {
        public abstract string ElementName { get; }

        public bool Parse(XElement root, out IEnumerable<T> enumer, ErrorMaskBuilder errorMask)
        {
            var transl = XmlTranslator<T, M>.Translator;
            if (transl.Item.Failed)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}"));
                enumer = null;
                return false;
            }
            return Parse(
                root,
                out enumer,
                errorMask: errorMask,
                transl: transl.Item.Value.Parse);
        }

        public bool Parse(
            XElement root,
            out IEnumerable<T> enumer,
            ErrorMaskBuilder errorMask,
            XmlSubParseDelegate<T> transl)
        {
            try
            {
                var ret = new List<T>();
                int i = 0;
                foreach (var listElem in root.Elements())
                {
                    using (errorMask.PushIndex(i++))
                    {
                        if (transl(listElem, out var subItem, errorMask))
                        {
                            ret.Add(subItem);
                        }
                    }
                }
                enumer = ret;
                return true;
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
                enumer = null;
                return false;
            }
        }

        public TryGet<IEnumerable<T>> Parse(
            XElement root,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            XmlSubParseDelegate<T> transl)
        {
            using (errorMask.PushIndex(fieldIndex))
            {
                var ret = this.Parse(
                    root,
                    out var subItem,
                    errorMask);
                return TryGet<IEnumerable<T>>.Create(
                    ret,
                    subItem);
            }
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<T> item,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<M>> maskObj)
        {
            try
            {
                var transl = XmlTranslator<T, M>.Translator;
                if (transl.Item.Failed)
                {
                    throw new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}");
                }
                this.Write(
                    node: node,
                    name: name,
                    item: item,
                    doMasks: doMasks,
                    maskObj: out maskObj,
                    transl: (XElement n, T item1, bool internalDoMasks, out M obj) => transl.Item.Value.Write(node: n, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj));
            }
            catch (Exception ex)
            when (doMasks)
            {
                maskObj = new MaskItem<Exception, IEnumerable<M>>(ex, null);
            }
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<T> item,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<M>> maskObj,
            XmlSubWriteDelegate<T, M> transl)
        {
            try
            {
                List<M> maskList = null;
                var elem = new XElement(name);
                node.Add(elem);
                foreach (var listObj in item)
                {
                    WriteSingleItem(elem, transl, listObj, doMasks, out M subMaskObj);
                    if (subMaskObj != null)
                    {
                        if (maskList == null)
                        {
                            maskList = new List<M>();
                        }
                        maskList.Add(subMaskObj);
                    }
                }
                if (maskList != null)
                {
                    maskObj = new MaskItem<Exception, IEnumerable<M>>(null, maskList);
                }
                else
                {
                    maskObj = null;
                }
            }
            catch (Exception ex)
            when (doMasks)
            {
                maskObj = new MaskItem<Exception, IEnumerable<M>>(ex, null);
            }
        }

        public abstract void WriteSingleItem<ErrMask>(XElement node, XmlSubWriteDelegate<T, ErrMask> transl, T item, bool doMasks, out ErrMask maskObj);

        public void Write<Mask>(
            XElement node,
            string name,
            IEnumerable<T> item,
            int fieldIndex,
            Func<Mask> errorMask,
            XmlSubWriteDelegate<T, M> transl)
            where Mask : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                item: item,
                doMasks: errorMask != null,
                maskObj: out var subMask,
                transl: transl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<Mask>(
            XElement node,
            string name,
            IHasItem<IEnumerable<T>> item,
            int fieldIndex,
            Func<Mask> errorMask,
            XmlSubWriteDelegate<T, M> transl)
            where Mask : IErrorMask
        {
            this.Write(
                node,
                name,
                item.Item,
                errorMask != null,
                out var subMask,
                transl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }
    }
}
