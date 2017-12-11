using Noggog;
using Noggog.Notifying;
using Noggog.Utility;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public delegate T CREATE_FUNC(XElement root, bool doMasks, out M errorMask);
        private static readonly Lazy<CREATE_FUNC> CREATE = new Lazy<CREATE_FUNC>(GetCreateFunc);
        public delegate void WRITE_FUNC(XmlWriter writer, T item, string name, bool doMasks, out M errorMask);
        private static readonly Lazy<WRITE_FUNC> WRITE = new Lazy<WRITE_FUNC>(GetWriteFunc);

        private IEnumerable<KeyValuePair<ushort, object>> EnumerateObjects(
            ILoquiRegistration registration,
            XElement root,
            bool skipProtected,
            bool doMasks,
            Func<IErrorMask> mask)
        {
            var ret = new List<KeyValuePair<ushort, object>>();
            try
            {
                foreach (var elem in root.Elements())
                {
                    var i = registration.GetNameIndex(elem.Name.LocalName);
                    if (!i.HasValue)
                    {
                        if (doMasks)
                        {
                            mask().Warnings.Add($"Skipping field that did not exist anymore with name: {elem.Name.LocalName}");
                        }
                        continue;
                    }

                    if (registration.IsProtected(i.Value) && skipProtected) continue;

                    try
                    {
                        var type = registration.GetNthType(i.Value);
                        if (!XmlTranslator.Instance.TryGetTranslator(type, out IXmlTranslation<object, object> translator))
                        {
                            XmlTranslator.Instance.TryGetTranslator(type, out translator);
                            throw new ArgumentException($"No XML Translator found for {type}");
                        }
                        var objGet = translator.Parse(elem, doMasks, out var subMaskObj);
                        if (doMasks && subMaskObj != null)
                        {
                            mask().SetNthMask(i.Value, subMaskObj);
                        }
                        if (objGet.Succeeded)
                        {
                            ret.Add(new KeyValuePair<ushort, object>(i.Value, objGet.Value));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (doMasks)
                        {
                            mask().SetNthException(i.Value, ex);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (doMasks)
                {
                    mask().Overall = ex;
                }
                else
                {
                    throw;
                }
            }
            return ret;
        }

        public void CopyIn<C>(
            XElement root,
            C item,
            bool skipProtected,
            bool doMasks,
            out M mask,
            NotifyingFireParameters? cmds)
            where C : T, ILoquiObject
        {
            var maskObj = default(M);
            Func<IErrorMask> maskGet;
            if (doMasks)
            {
                maskGet = () =>
                {
                    if (maskObj == null)
                    {
                        maskObj = new M();
                    }
                    return maskObj;
                };
            }
            else
            {
                maskGet = null;
            }
            var fields = EnumerateObjects(
                item.Registration,
                root,
                skipProtected,
                doMasks,
                maskGet);
            var copyIn = LoquiRegistration.GetCopyInFunc<C>();
            copyIn(fields, item);
            mask = maskObj;
        }

        public static CREATE_FUNC GetCreateFunc()
        {
            var f = DelegateBuilder.BuildDelegate<Func<XElement, bool, (T item, M mask)>>(
                typeof(T).GetMethods()
                .Where((methodInfo) => methodInfo.Name.Equals("Create_XML"))
                .Where((methodInfo) => methodInfo.IsStatic
                    && methodInfo.IsPublic)
                .Where((methodInfo) => methodInfo.ReturnType.Equals(typeof(ValueTuple<T, M>)))
                .First());
            return (XElement root, bool doMasks, out M errorMask) =>
            {
                var ret = f(root, doMasks);
                errorMask = ret.mask;
                return ret.item;
            };
        }

        public static WRITE_FUNC GetWriteFunc()
        {
            var f = DelegateBuilder.BuildDelegate<Func<XmlWriter, T, string, bool, M>>(
                typeof(T).GetMethods()
                .Where((methodInfo) => methodInfo.Name.Equals("Write_XML"))
                .Where((methodInfo) => methodInfo.IsStatic
                    && methodInfo.IsPublic)
                .Where((methodInfo) => methodInfo.ReturnType.Equals(typeof(M)))
                .First());
            return (XmlWriter writer, T item, string name, bool doMasks, out M errorMask) =>
            {
                errorMask = f(writer, item, name, doMasks);
            };
        }

        public TryGet<T> Parse(XElement root, bool doMasks, out MaskItem<Exception, M> errorMask)
        {
            try
            {
                var typeStr = root.GetAttribute(XmlConstants.TYPE_ATTRIBUTE);
                if (typeStr != null
                    && typeStr.Equals(Registration.FullName))
                {
                    var ret = TryGet<T>.Succeed(CREATE.Value(
                        root: root,
                        doMasks: doMasks,
                        errorMask: out var subMask));
                    errorMask = subMask == null ? null : new MaskItem<Exception, M>(null, subMask);
                    return ret;
                }
                else
                {
                    var register = LoquiRegistration.GetRegisterByFullName(typeStr ?? root.Name.LocalName);
                    if (register == null)
                    {
                        var ex = new ArgumentException($"Unknown Loqui type: {root.Name.LocalName}");
                        if (!doMasks) throw ex;
                        errorMask = new MaskItem<Exception, M>(
                            ex,
                            default(M));
                        return TryGet<T>.Failure;
                    }
                    var tryGet = XmlTranslator.Instance.GetTranslator(register.ClassType).Item.Value.Parse(
                        root: root,
                        doMasks: doMasks,
                        maskObj: out var subErrorMaskObj).Bubble((o) => (T)o);
                    errorMask = subErrorMaskObj == null ? null : new MaskItem<Exception, M>(null, (M)subErrorMaskObj);
                    return tryGet;
                }
            }
            catch (Exception ex)
            when (doMasks)
            {
                errorMask = new MaskItem<Exception, M>(ex, default(M));
                return TryGet<T>.Failure;
            }
        }

        public TryGet<T> Parse(XElement root, bool doMasks, out M errorMask)
        {
            var ret = Parse(root, doMasks, out MaskItem<Exception, M> subMask);
            if (subMask?.Overall != null)
            {
                throw subMask.Overall;
            }
            errorMask = subMask?.Specific;
            return ret;
        }

        public void Write(XmlWriter writer, string name, T item, bool doMasks, out M errorMask)
        {
            WRITE.Value(writer, item, name, doMasks, out errorMask);
        }

        public void Write(XmlWriter writer, string name, T item, bool doMasks, out MaskItem<Exception, M> errorMask)
        {
            try
            {
                WRITE.Value(writer, item, name, doMasks, out var subMask);
                errorMask = subMask == null ? null : new MaskItem<Exception, M>(null, subMask);
            }
            catch (Exception ex)
            when (doMasks)
            {
                errorMask = new MaskItem<Exception, M>(ex, default(M));
            }
        }

        public void Write<Mask>(
            XmlWriter writer,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            this.Write(
                writer,
                name,
                item.Item,
                errorMask != null,
                out M subMask);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<Mask>(
            XmlWriter writer,
            string name,
            T item,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            this.Write(
                writer,
                name,
                item,
                errorMask != null,
                out M subMask);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<Mask>(
            XmlWriter writer,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                writer: writer,
                name: name,
                item: item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }
    }
}
