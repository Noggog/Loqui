using Loqui.Internal;
using Noggog;
using Noggog.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class LoquiXmlTranslation<T> : IXmlTranslation<T>
        where T : ILoquiObject
    {
        public static readonly LoquiXmlTranslation<T> Instance = new LoquiXmlTranslation<T>();
        private static readonly Lazy<string> _elementName = new Lazy<string>(() => LoquiRegistration.GetRegister(typeof(T))!.FullName);
        public string ElementName => _elementName.Value;
        private static readonly ILoquiRegistration? Registration = LoquiRegistration.GetRegister(typeof(T), returnNull: true);
        public delegate T CREATE_FUNC(XElement node, ErrorMaskBuilder? errorMaskBuilder, TranslationCrystal? translationMask);
        private static readonly Lazy<CREATE_FUNC> CREATE = new Lazy<CREATE_FUNC>(GetCreateFunc);

        private IEnumerable<KeyValuePair<ushort, object>> EnumerateObjects(
            ILoquiRegistration registration,
            XElement node,
            bool skipProtected,
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
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
                        if (!XmlTranslator.Instance.TryGetTranslator(type, out IXmlTranslation<object>? translator))
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
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
            where C : T, ILoquiObject
        {
            var fields = EnumerateObjects(
                item.Registration,
                node,
                skipProtected,
                errorMask: errorMask,
                translationMask: translationMask);
            var copyIn = LoquiRegistration.GetCopyInFunc<C>()!;
            copyIn(fields, item);
        }

        private static CREATE_FUNC GetCreateFunc()
        {
            return DelegateBuilder.BuildDelegate<CREATE_FUNC>(
                typeof(T).GetMethods()
                .Where((methodInfo) => methodInfo.Name.Equals("CreateFromXml"))
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
                .FirstOrDefault());
        }
        
        public void ParseInto(
            XElement node,
            int fieldIndex,
            IHasItem<T> item,
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
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
            [MaybeNullWhen(false)]out T item,
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
        {
            var typeStr = node.GetAttribute(XmlConstants.TYPE_ATTRIBUTE);
            string comparisonString = typeStr ?? node.Name.LocalName;
            if (Registration != null
                && (comparisonString.Equals(Registration.FullName)
                    || !LoquiRegistration.TryGetRegisterByFullName(comparisonString, out var regis)))
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
                if (XmlTranslator.Instance.GetTranslator(register.ClassType).Value.Parse(
                    node: node,
                    item: out var itemObj,
                    errorMask: errorMask,
                    translationMask: translationMask))
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

        public T Parse(
            XElement node,
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
        {
            if (Parse(node, out var item, errorMask, translationMask))
            {
                return item;
            }
            else
            {
                return default;
            }
        }

        public void Write(XElement node, string? name, T item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
        {
            throw new NotImplementedException();
        }
    }

    public class LoquiXmlTranslation
    {
        public static readonly LoquiXmlTranslation Instance = new LoquiXmlTranslation();

        public delegate T CREATE_FUNC<T>(
            XElement node,
            ErrorMaskBuilder? errorMaskBuilder,
            TranslationCrystal? translationMask);
        private static Dictionary<Type, object> createDict = new Dictionary<Type, object>();
        private static Dictionary<(Type Base, Type Actual), object> subCreateDict = new Dictionary<(Type Base, Type Actual), object>();

        private static CREATE_FUNC<T> GetCreateFunc<T>(Type t)
            where T : ILoquiObjectGetter
        {
            if (createDict.TryGetValue(t, out var createFunc))
            {
                return (CREATE_FUNC<T>)createFunc;
            }
            var method = t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where((methodInfo) => methodInfo.Name.Equals("CreateFromXml"))
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
                var f = DelegateBuilder.BuildDelegate<Func<XElement, ErrorMaskBuilder?, TranslationCrystal?, T>>(method);
                CREATE_FUNC<T> ret = (XElement node, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask) =>
                {
                    return f(node, errorMask, translationMask);
                };
                createDict[t] = ret;
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
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
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
                    CREATE_FUNC<T>? createFunc = createFuncGeneric as CREATE_FUNC<T>;
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
