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
    public class LoquiXmlTranslation<T> : IXmlTranslation<T>
        where T : ILoquiObject
    {
        public static readonly LoquiXmlTranslation<T> Instance = new LoquiXmlTranslation<T>();
        private static readonly string _elementName = LoquiRegistration.GetRegister(typeof(T)).FullName;
        public string ElementName => _elementName;
        private static readonly ILoquiRegistration Registration = LoquiRegistration.GetRegister(typeof(T));
        public delegate T CREATE_FUNC(XElement root, ErrorMaskBuilder errorMaskBuilder, TranslationCrystal translationMask);
        private static readonly Lazy<CREATE_FUNC> CREATE = new Lazy<CREATE_FUNC>(GetCreateFunc);
        public delegate void WRITE_FUNC(XElement node, T item, string name, ErrorMaskBuilder errorMask, TranslationCrystal translationMask);
        private static readonly Lazy<WRITE_FUNC> WRITE = new Lazy<WRITE_FUNC>(GetWriteFunc);

        private IEnumerable<KeyValuePair<ushort, object>> EnumerateObjects(
            ILoquiRegistration registration,
            XElement root,
            bool skipProtected,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var ret = new List<KeyValuePair<ushort, object>>();
            foreach (var elem in root.Elements())
            {
                var i = registration.GetNameIndex(elem.Name.LocalName);
                if (!i.HasValue)
                {
                    errorMask?.ReportWarning($"Skipping field that did not exist anymore with name: {elem.Name.LocalName}");
                    continue;
                }

                if (registration.IsProtected(i.Value) && skipProtected) continue;
                if (!translationMask?.GetShouldTranslate(i.Value) ?? false) continue;

                try
                {
                    errorMask?.PushIndex(i.Value);
                    var type = registration.GetNthType(i.Value);
                    if (!XmlTranslator.Instance.TryGetTranslator(type, out IXmlTranslation<object> translator))
                    {
                        XmlTranslator.Instance.TryGetTranslator(type, out translator);
                        throw new ArgumentException($"No Xml Translator found for {type}");
                    }
                    if (translator.Parse(
                        root: elem, 
                        item: out var obj,
                        errorMask: errorMask,
                        translationMask: translationMask?.GetSubCrystal(i.Value)))
                    {
                        ret.Add(new KeyValuePair<ushort, object>(i.Value, obj));
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
                finally
                {
                    errorMask?.PopIndex();
                }
            }
            return ret;
        }

        public void CopyIn<C>(
            XElement root,
            C item,
            bool skipProtected,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            NotifyingFireParameters cmds)
            where C : T, ILoquiObject
        {
            var fields = EnumerateObjects(
                item.Registration,
                root,
                skipProtected,
                errorMask: errorMask,
                translationMask: translationMask);
            var copyIn = LoquiRegistration.GetCopyInFunc<C>();
            copyIn(fields, item);
        }

        public static CREATE_FUNC GetCreateFunc()
        {
            return DelegateBuilder.BuildDelegate<CREATE_FUNC>(
                typeof(T).GetMethods()
                .Where((methodInfo) => methodInfo.Name.Equals("Create_Xml"))
                .Where((methodInfo) => methodInfo.IsStatic
                    && methodInfo.IsPublic)
                .Where((methodInfo) => methodInfo.ReturnType.Equals(typeof(T)))
                .Where((methodInfo) => methodInfo.GetParameters().Count() == 3)
                .First());
        }

        public static WRITE_FUNC GetWriteFunc()
        {
            var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where((methodInfo) => methodInfo.Name.Equals("Write_Xml"))
                .Where(methodInfo =>
                {
                    var param = methodInfo.GetParameters();
                    if (param.Length != 4) return false;
                    if (!param[0].ParameterType.Equals(typeof(XElement))) return false;
                    if (!param[1].ParameterType.Equals(typeof(ErrorMaskBuilder))) return false;
                    if (!param[2].ParameterType.Equals(typeof(TranslationCrystal))) return false;
                    if (!param[3].ParameterType.Equals(typeof(string))) return false;
                    return true;
                })
                .First();
            if (!method.IsGenericMethod)
            {
                var f = DelegateBuilder.BuildDelegate<Action<T, XElement, ErrorMaskBuilder, TranslationCrystal, string>>(method);
                return (XElement node, T item, string name, ErrorMaskBuilder errorMask, TranslationCrystal translationMask) =>
                {
                    if (item == null)
                    {
                        throw new NullReferenceException("Cannot write Xml for a null item.");
                    }
                    f(item, node, errorMask, translationMask, name);
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ParseInto(
            XElement root, 
            int fieldIndex, 
            IHasItem<T> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(
                    root,
                    out var i,
                    errorMask: errorMask,
                    translationMask: translationMask))
                {
                    item.Item = i;
                }
                else
                {
                    item.Unset();
                }
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
        }

        public bool Parse(
            XElement root, 
            out T item, 
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var typeStr = root.GetAttribute(XmlConstants.TYPE_ATTRIBUTE);
            if (typeStr == null
                || typeStr.Equals(Registration.FullName))
            {
                item = CREATE.Value(
                    root: root,
                    errorMaskBuilder: errorMask,
                    translationMask: translationMask);
                return true;
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
                    errorMask: errorMask,
                    translationMask: translationMask);
                if (ret)
                {
                    item = (T)itemObj;
                    return true;
                }
                else
                {
                    item = default(T);
                    return false;
                }
            }
        }

        public void Write(
            XElement node, 
            string name,
            T item,
            ErrorMaskBuilder errorMask, 
            TranslationCrystal translationMask)
        {
            WRITE.Value(
                node, 
                item, 
                name, 
                errorMask,
                translationMask);
        }

        public void Write(
            XElement node,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            errorMask?.PushIndex(fieldIndex);
            this.Write(
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask,
                translationMask: translationMask);
        }

        public void Write(
            XElement node,
            string name,
            T item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node: node,
                    name: name,
                    item: item,
                    errorMask: errorMask,
                    translationMask: translationMask);
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
        }

        public void Write(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask,
                translationMask: translationMask);
        }
    }
}
