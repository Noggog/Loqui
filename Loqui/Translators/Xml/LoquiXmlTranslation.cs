using Loqui.Internal;
using Noggog;
using Noggog.Notifying;
using Noggog.Utility;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class LoquiXmlTranslation<T, M> : IXmlTranslation<T, M>
        where T : ILoquiObject
        where M : class, IErrorMask, new()
    {
        public static readonly LoquiXmlTranslation<T, M> Instance = new LoquiXmlTranslation<T, M>();
        private static readonly string _elementName = LoquiRegistration.GetRegister(typeof(T)).FullName;
        public string ElementName => _elementName;
        private static readonly ILoquiRegistration Registration = LoquiRegistration.GetRegister(typeof(T));
        public delegate TryGet<T> CREATE_FUNC(XElement root, ErrorMaskBuilder errorMaskBuilder);
        private static readonly Lazy<CREATE_FUNC> CREATE = new Lazy<CREATE_FUNC>(GetCreateFunc);
        public delegate void WRITE_FUNC(XElement node, T item, string name, bool doMasks, out M errorMask);
        private static readonly Lazy<WRITE_FUNC> WRITE = new Lazy<WRITE_FUNC>(GetWriteFunc);

        private IEnumerable<KeyValuePair<ushort, object>> EnumerateObjects(
            ILoquiRegistration registration,
            XElement root,
            bool skipProtected,
            ErrorMaskBuilder errorMask)
        {
            var ret = new List<KeyValuePair<ushort, object>>();
            try
            {
                foreach (var elem in root.Elements())
                {
                    var i = registration.GetNameIndex(elem.Name.LocalName);
                    if (!i.HasValue)
                    {
                        errorMask?.ReportWarning($"Skipping field that did not exist anymore with name: {elem.Name.LocalName}");
                        continue;
                    }

                    if (registration.IsProtected(i.Value) && skipProtected) continue;

                    using (errorMask.PushIndex(i.Value))
                    {
                        try
                        {
                            var type = registration.GetNthType(i.Value);
                            if (!XmlTranslator.Instance.TryGetTranslator(type, out IXmlTranslation<object, object> translator))
                            {
                                XmlTranslator.Instance.TryGetTranslator(type, out translator);
                                throw new ArgumentException($"No XML Translator found for {type}");
                            }
                            if (translator.Parse(elem, out var obj, errorMask))
                            {
                                ret.Add(new KeyValuePair<ushort, object>(i.Value, obj));
                            }
                        }
                        catch (Exception ex)
                        when (errorMask != null)
                        {
                            errorMask.ReportException(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            return ret;
        }

        public void CopyIn<C>(
            XElement root,
            C item,
            bool skipProtected,
            ErrorMaskBuilder errorMask,
            NotifyingFireParameters cmds)
            where C : T, ILoquiObject
        {
            var fields = EnumerateObjects(
                item.Registration,
                root,
                skipProtected,
                errorMask);
            var copyIn = LoquiRegistration.GetCopyInFunc<C>();
            copyIn(fields, item);
        }

        public static CREATE_FUNC GetCreateFunc()
        {
            return DelegateBuilder.BuildDelegate<CREATE_FUNC>(
                typeof(T).GetMethods()
                .Where((methodInfo) => methodInfo.Name.Equals("Create_XML"))
                .Where((methodInfo) => methodInfo.IsStatic
                    && methodInfo.IsPublic)
                .Where((methodInfo) => methodInfo.ReturnType.Equals(typeof(TryGet<T>)))
                .First());
        }

        public static WRITE_FUNC GetWriteFunc()
        {
            var method = typeof(T).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where((methodInfo) => methodInfo.Name.Equals("Write_XML_Internal"))
                .First();
            if (!method.IsGenericMethod)
            {
                var f = DelegateBuilder.BuildDelegate<Func<T, XElement, bool, string, object>>(method);
                return (XElement node, T item, string name, bool doMasks, out M errorMask) =>
                {
                    if (item == null)
                    {
                        throw new NullReferenceException("Cannot write XML for a null item.");
                    }
                    errorMask = (M)f(item, node, doMasks, name);
                };
            }
            else
            {
                var f = DelegateBuilder.BuildGenericDelegate<Func<T, XElement, bool, string, object>>(typeof(T), new Type[] { typeof(M).GenericTypeArguments[0] }, method);
                return (XElement node, T item, string name, bool doMasks, out M errorMask) =>
                {
                    if (item == null)
                    {
                        throw new NullReferenceException("Cannot write XML for a null item.");
                    }
                    errorMask = (M)f(item, node, doMasks, name);
                };
            }
        }

        public bool Parse(XElement root, out T item, ErrorMaskBuilder errorMask)
        {
            try
            {
                var typeStr = root.GetAttribute(XmlConstants.TYPE_ATTRIBUTE);
                if (typeStr != null
                    && typeStr.Equals(Registration.FullName))
                {
                    var ret = CREATE.Value(
                        root: root,
                        errorMaskBuilder: errorMask);
                    item = ret.Value;
                    return ret.Succeeded;
                }
                else
                {
                    var register = LoquiRegistration.GetRegisterByFullName(typeStr ?? root.Name.LocalName);
                    if (register == null)
                    {
                        var ex = new ArgumentException($"Unknown Loqui type: {root.Name.LocalName}");
                        if (errorMask == null) throw ex;
                        errorMask.ReportException(ex);
                        item = default(T);
                        return false;
                    }
                    var ret = XmlTranslator.Instance.GetTranslator(register.ClassType).Item.Value.Parse(
                        root: root,
                        item: out var itemObj,
                        errMask: errorMask);
                    if (ret)
                    {
                        item = (T)itemObj;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            item = default(T);
            return false;
        }

        public void Write(XElement node, string name, T item, bool doMasks, out M errorMask)
        {
            WRITE.Value(node, item, name, doMasks, out errorMask);
        }

        public void Write(XElement node, string name, T item, bool doMasks, out MaskItem<Exception, M> errorMask)
        {
            try
            {
                WRITE.Value(node, item, name, doMasks, out var subMask);
                errorMask = subMask == null ? null : new MaskItem<Exception, M>(null, subMask);
            }
            catch (Exception ex)
            when (doMasks)
            {
                errorMask = new MaskItem<Exception, M>(ex, default(M));
            }
        }

        public void Write<Mask>(
            XElement node,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }

        public void Write<Mask>(
            XElement node,
            string name,
            T item,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                item: item,
                doMasks: errorMask != null,
                errorMask: out M subMask);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<Mask>(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }
    }
}
