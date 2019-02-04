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
        public delegate T CREATE_FUNC(XElement node, ErrorMaskBuilder errorMaskBuilder, TranslationCrystal translationMask);
        private static readonly Lazy<CREATE_FUNC> CREATE = new Lazy<CREATE_FUNC>(GetCreateFunc);
        public delegate void WRITE_FUNC(XElement node, T item, string name, ErrorMaskBuilder errorMask, TranslationCrystal translationMask);
        private static readonly Lazy<WRITE_FUNC> WRITE = new Lazy<WRITE_FUNC>(GetWriteFunc);

        private IEnumerable<KeyValuePair<ushort, object>> EnumerateObjects(
            ILoquiRegistration registration,
            XElement node,
            bool skipProtected,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var ret = new List<KeyValuePair<ushort, object>>();
            foreach (var elem in node.Elements())
            {
                var i = registration.GetNameIndex(elem.Name.LocalName);
                if (!i.HasValue)
                {
                    errorMask?.ReportWarning($"Skipping field that did not exist anymore with name: {elem.Name.LocalName}");
                    continue;
                }

                if (registration.IsProtected(i.Value) && skipProtected) continue;
                if (!translationMask?.GetShouldTranslate(i.Value) ?? false) continue;

                using (errorMask?.PushIndex(i.Value))
                {
                    try
                    {
                        var type = registration.GetNthType(i.Value);
                        if (!XmlTranslator.Instance.TryGetTranslator(type, out IXmlTranslation<object> translator))
                        {
                            XmlTranslator.Instance.TryGetTranslator(type, out translator);
                            throw new ArgumentException($"No Xml Translator found for {type}");
                        }
                        if (translator.Parse(
                            node: elem,
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
                }
            }
            return ret;
        }

        public void CopyIn<C>(
            XElement node,
            C item,
            bool skipProtected,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            NotifyingFireParameters cmds)
            where C : T, ILoquiObject
        {
            var fields = EnumerateObjects(
                item.Registration,
                node,
                skipProtected,
                errorMask: errorMask,
                translationMask: translationMask);
            var copyIn = LoquiRegistration.GetCopyInFunc<C>();
            copyIn(fields, item);
        }

        private static CREATE_FUNC GetCreateFunc()
        {
            return DelegateBuilder.BuildDelegate<CREATE_FUNC>(
                typeof(T).GetMethods()
                .Where((methodInfo) => methodInfo.Name.Equals("Create_Xml"))
                .Where((methodInfo) => methodInfo.IsStatic
                    && methodInfo.IsPublic)
                .Where((methodInfo) => methodInfo.ReturnType.Equals(typeof(T)))
                .Where(methodInfo =>
                {
                    var param = methodInfo.GetParameters();
                    if (param.Length != 3) return false;
                    if (!param[0].ParameterType.Equals(typeof(XElement))) return false;
                    if (!param[1].ParameterType.Equals(typeof(ErrorMaskBuilder))) return false;
                    if (!param[2].ParameterType.Equals(typeof(TranslationCrystal))) return false;
                    return true;
                })
                .First());
        }

        private static WRITE_FUNC GetWriteFunc()
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
            XElement node,
            int fieldIndex,
            IHasItem<T> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    if (Parse(
                        node,
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
            }
        }

        public bool Parse(
            XElement node,
            out T item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var typeStr = node.GetAttribute(XmlConstants.TYPE_ATTRIBUTE);
            if (typeStr == null
                || typeStr.Equals(Registration.FullName))
            {
                item = CREATE.Value(
                    node: node,
                    errorMaskBuilder: errorMask,
                    translationMask: translationMask);
                return true;
            }
            else
            {
                var register = LoquiRegistration.GetRegisterByFullName(typeStr ?? node.Name.LocalName);
                if (register == null)
                {
                    var ex = new ArgumentException($"Unknown Loqui type: {node.Name.LocalName}");
                    if (errorMask == null) throw ex;
                    errorMask.ReportException(ex);
                    item = default(T);
                    return false;
                }
                var ret = XmlTranslator.Instance.GetTranslator(register.ClassType).Item.Value.Parse(
                    node: node,
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
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
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

    public class LoquiXmlTranslation
    {
        public static readonly LoquiXmlTranslation Instance = new LoquiXmlTranslation();

        public delegate void WRITE_FUNC<T>(
            XElement node,
            T item,
            string name,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask);
        private static Dictionary<Type, object> writeDict = new Dictionary<Type, object>();

        public delegate T CREATE_FUNC<T>(
            XElement node,
            ErrorMaskBuilder errorMaskBuilder,
            TranslationCrystal translationMask);
        private static Dictionary<Type, object> createDict = new Dictionary<Type, object>();
        private static Dictionary<(Type Base, Type Actual), object> subCreateDict = new Dictionary<(Type Base, Type Actual), object>();

        public static WRITE_FUNC<T> GetWriteFunc<T>(Type t)
            where T : ILoquiObjectGetter
        {
            if (writeDict.TryGetValue(t, out var writeFunc))
            {
                return (WRITE_FUNC<T>)writeFunc;
            }
            var method = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
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
                WRITE_FUNC<T> ret = (XElement node, T item, string name, ErrorMaskBuilder errorMask, TranslationCrystal translationMask) =>
                {
                    if (item == null)
                    {
                        throw new NullReferenceException("Cannot write Xml for a null item.");
                    }
                    f(item, node, errorMask, translationMask, name);
                };
                writeDict[t] = ret;
                return ret;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Write<T>(
            XElement node,
            string name,
            T item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
            where T : ILoquiObjectGetter
        {
            GetWriteFunc<T>(item.GetType())(
                node,
                item,
                name,
                errorMask,
                translationMask);
        }

        public void Write<T>(
            XElement node,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
            where T : ILoquiObjectGetter
        {
            this.Write(
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask,
                translationMask: translationMask);
        }

        public void Write<T>(
            XElement node,
            string name,
            T item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
            where T : ILoquiObjectGetter
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
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
            }
        }

        public void Write<T>(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
            where T : ILoquiObjectGetter
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

        private static CREATE_FUNC<T> GetCreateFunc<T>(Type t)
            where T : ILoquiObjectGetter
        {
            if (createDict.TryGetValue(t, out var createFunc))
            {
                return (CREATE_FUNC<T>)createFunc;
            }
            var method = t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where((methodInfo) => methodInfo.Name.Equals("Create_Xml"))
                .Where((methodInfo) => methodInfo.ReturnType.Equals(t))
                .Where(methodInfo =>
                {
                    var param = methodInfo.GetParameters();
                    if (param.Length != 3) return false;
                    if (!param[0].ParameterType.Equals(typeof(XElement))) return false;
                    if (!param[1].ParameterType.Equals(typeof(ErrorMaskBuilder))) return false;
                    if (!param[2].ParameterType.Equals(typeof(TranslationCrystal))) return false;
                    return true;
                })
                .First();
            if (!method.IsGenericMethod)
            {
                var f = DelegateBuilder.BuildDelegate<Func<XElement, ErrorMaskBuilder, TranslationCrystal, T>>(method);
                CREATE_FUNC<T> ret = (XElement node, ErrorMaskBuilder errorMask, TranslationCrystal translationMask) =>
                {
                    return f(node, errorMask, translationMask);
                };
                writeDict[t] = ret;
                return ret;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool TryCreate<T>(
            XElement node,
            out T item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
            where T : ILoquiObjectGetter
        {
            try
            {
                if (!LoquiRegistration.TryGetRegisterByFullName(node.Name.LocalName, out var registration))
                {
                    errorMask.ReportWarning($"Unknown {typeof(T).Name} subclass: {node.Name.LocalName}");
                    item = default;
                    return false;
                }
                if (subCreateDict.TryGetValue((typeof(T), registration.ClassType), out var createFuncGeneric))
                {
                    CREATE_FUNC<T> createFunc = createFuncGeneric as CREATE_FUNC<T>;
                    if (createFunc == null)
                    {
                        item = default;
                        return false;
                    }

                    item = createFunc(node, errorMask, translationMask);
                    return true;
                }
                else
                {
                    var createFunc = GetCreateFunc<T>(registration.ClassType);
                    item = createFunc(node, errorMask, translationMask);
                    subCreateDict[(typeof(T), registration.ClassType)] = createFunc;
                    return true;
                }
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
                item = default;
                return false;
            }
        }
    }
}
