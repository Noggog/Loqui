using Noggog;
using Noggog.Notifying;
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
        where T : ILoquiObjectGetter
        where M : IErrorMask, new()
    {
        public static readonly LoquiXmlTranslation<T, M> Instance = new LoquiXmlTranslation<T, M>();
        private static readonly string _elementName = LoquiRegistration.GetRegister(typeof(T)).FullName;
        public string ElementName => _elementName;

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
                        if (!XmlTranslator.TryGetTranslator(type, out IXmlTranslation<object, object> translator))
                        {
                            XmlTranslator.TryGetTranslator(type, out translator);
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

        public TryGet<T> Parse(XElement root, bool doMasks, out M mask)
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
            try
            {
                var regis = LoquiRegistration.GetRegister(typeof(T));
                var fields = EnumerateObjects(
                    regis,
                    root,
                    skipProtected: false,
                    doMasks: doMasks,
                    mask: maskGet);
                var create = LoquiRegistration.GetCreateFunc<T>();
                var ret = create(fields);
                mask = maskObj;
                return TryGet<T>.Succeed(ret);
            }
            catch (Exception ex)
            {
                if (!doMasks) throw;
                maskGet().Overall = ex;
                mask = maskObj;
                return TryGet<T>.Failure;
            }
        }

        public void Write(XmlWriter writer, string name, T item, bool doMasks, out M mask)
        {
            using (new ElementWrapper(writer, name))
            {
                mask = default(M);

                try
                {
                    writer.WriteAttributeString(XmlConstants.TYPE_ATTRIBUTE, ElementName);
                    for (ushort i = 0; i < item.Registration.FieldCount; i++)
                    {
                        try
                        {
                            if (!item.GetNthObjectHasBeenSet(i)) continue;

                            var type = item.Registration.GetNthType(i);
                            if (!XmlTranslator.TryGetTranslator(type, out IXmlTranslation<object, object> translator))
                            {
                                throw new ArgumentException($"No XML Translator found for {type}");
                            }
                            translator.Write(writer, item.Registration.GetNthName(i), item.GetNthObject(i), doMasks, out var subMaskObj);

                            if (subMaskObj != null)
                            {
                                if (mask == null)
                                {
                                    mask = new M();
                                }
                                mask.SetNthMask(i, subMaskObj);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (doMasks)
                            {
                                if (mask == null)
                                {
                                    mask = new M();
                                }
                                mask.SetNthException(i, ex);
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
                        if (mask == null)
                        {
                            mask = new M();
                        }
                        mask.Overall = ex;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
