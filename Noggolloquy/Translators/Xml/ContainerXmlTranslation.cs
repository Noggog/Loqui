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
    public abstract class ContainerXmlTranslation<T> : IXmlTranslation<IEnumerable<T>>
    {
        protected static INotifyingItemGetter<IXmlTranslation<T>> translator;

        static ContainerXmlTranslation()
        {
            translator = XmlTranslator<T>.Translator;
        }
        
        public abstract string ElementName { get; }
        
        public TryGet<IEnumerable<T>> Parse(XElement root, bool doMasks, out object maskObj)
        {
            return TryGet<IEnumerable<T>>.Success(Parse_Internal(root, doMasks, out maskObj));
        }

        private IEnumerable<T> Parse_Internal(XElement root, bool doMasks, out object maskObj)
        {
            List<MaskItem<Exception, object>> maskList = null;
            List<T> ret = new List<T>();
            foreach (var listElem in root.Elements())
            {
                try
                {
                    var get = ParseSingleItem(listElem, doMasks, out object subMaskObj);
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
                        maskList.Add(new MaskItem<Exception, object>(ex, null));
                    }
                }
            }
            maskObj = maskList;
            return ret;
        }

        protected abstract TryGet<T> ParseSingleItem(XElement root, bool doMasks, out object maskObj);
        
        public bool Write(XmlWriter writer, string name, IEnumerable<T> item, bool doMasks, out object maskObj)
        {
            List<MaskItem<Exception, object>> maskList = null;
            using (new ElementWrapper(writer, ElementName))
            {
                writer.WriteAttributeString("name", name);
                foreach (var listObj in item)
                {
                    try
                    {
                        if (!WriteSingleItem(writer, listObj, doMasks, out object subMaskObj))
                        {
                            if (!doMasks)
                            { // This shouldn't actually throw, as subparse is expected to throw if doMasks is off
                                throw new ArgumentException($"Error writing list.  Could not write subitem: {listObj}");
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
                            maskList.Add(new MaskItem<Exception, object>(ex, null));
                        }
                    }
                }
            }
            maskObj = maskList;
            return maskObj != null;
        }

        public abstract bool WriteSingleItem(XmlWriter writer, T item, bool doMasks, out object maskObj);
    }
}
