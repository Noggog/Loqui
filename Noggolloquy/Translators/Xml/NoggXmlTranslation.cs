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

namespace Noggolloquy.Xml
{
    public delegate void NoggXmlCopyInFunction(XElement root, object item, NotifyingFireParameters? cmds, bool doMasks, out object mask);
    public delegate void NoggXmlWriteFunction(XmlWriter writer, string name, object item, bool doMasks, out object mask);

    public class NoggXmlTranslation<T, M> : IXmlTranslation<T>
        where T : INoggolloquyObjectGetter
        where M : IErrorMask, new()
    {
        public readonly static NoggXmlTranslation<T, M> Instance = new NoggXmlTranslation<T, M>();
        private static readonly string _elementName = NoggolloquyRegistration.GetRegister(typeof(T)).FullName;
        public string ElementName => _elementName;

        private IEnumerable<KeyValuePair<ushort, object>> EnumerateObjects(
            INoggolloquyRegistration registration,
            XElement root,
            bool skipReadonly,
            bool doMasks,
            Func<IErrorMask> mask)
        {
            List<KeyValuePair<ushort, object>> ret = new List<KeyValuePair<ushort, object>>();
            try
            {
                foreach (var elem in root.Elements())
                {
                    if (!elem.TryGetAttribute("name", out XAttribute name))
                    {
                        if (doMasks)
                        {
                            mask().Warnings.Add("Skipping field that did not have name");
                        }
                        continue;
                    }

                    var i = registration.GetNameIndex(name.Value);
                    if (!i.HasValue)
                    {
                        if (doMasks)
                        {
                            mask().Warnings.Add("Skipping field that did not exist anymore with name: " + name);
                        }
                        continue;
                    }

                    var readOnly = registration.IsReadOnly(i.Value);
                    if (readOnly && skipReadonly) continue;

                    try
                    {
                        var type = registration.GetNthType(i.Value);
                        if (!XmlTranslator.TryGetTranslator(type, out IXmlTranslation<object> translator))
                        {
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
            bool skipReadonly,
            bool doMasks,
            out M mask,
            NotifyingFireParameters? cmds)
            where C : T, INoggolloquyObject
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
                skipReadonly,
                doMasks,
                maskGet);
            var copyIn = NoggolloquyRegistration.GetCopyInFunc<C>();
            copyIn(fields, item);
            mask = maskObj;
        }

        public TryGet<T> Parse(XElement root, bool doMasks, out object mask)
        {
            var regis = NoggolloquyRegistration.GetRegister(typeof(T));
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
                regis,
                root,
                skipReadonly: false,
                doMasks: doMasks,
                mask: maskGet);
            var create = NoggolloquyRegistration.GetCreateFunc<T>();
            var ret = create(fields);
            mask = maskObj;
            return TryGet<T>.Succeed(ret);
        }

        public bool Write(XmlWriter writer, string name, T item, bool doMasks, out M mask)
        {
            using (new ElementWrapper(writer, item.Registration.Name))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    writer.WriteAttributeString("name", name);
                }
                mask = default(M);

                try
                {
                    for (ushort i = 0; i < item.Registration.FieldCount; i++)
                    {
                        try
                        {
                            if (!item.GetNthObjectHasBeenSet(i)) continue;

                            var type = item.Registration.GetNthType(i);
                            object subMaskObj;
                            if (!XmlTranslator.TryGetTranslator(type, out IXmlTranslation<object> translator))
                            {
                                throw new ArgumentException($"No XML Translator found for {type}");
                            }
                            translator.Write(writer, item.Registration.GetNthName(i), item.GetNthObject(i), doMasks, out subMaskObj);

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
            return mask == null;
        }
        
        public bool Write(XmlWriter writer, string name, T item, bool doMasks, out object maskObj)
        {
            if (this.Write(writer, name, item, doMasks, out M mask))
            {
                maskObj = mask;
                return true;
            }
            maskObj = null;
            return false;
        }
    }
}
