using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml;
using System.Xml.Linq;
using Noggog.Xml;
using Noggolloquy;
using Noggog.Notifying;

namespace Noggolloquy.Xml
{
    public class ListXmlTranslation<T> : ContainerXmlGeneration<T>
    {
        public readonly static ListXmlTranslation<T> Instance = new ListXmlTranslation<T>();

        public override string ElementName => "List";

        public void CopyIn(XElement root, INotifyingList<T> list, NotifyingFireParameters? cmds)
        {
            list.Clear();
            foreach (var listElem in root.Elements())
            {
                var parse = translator.Value.Parse(listElem);
                if (parse.Succeeded)
                {
                    list.Add(parse.Value);
                }
            }
        }

        public override TryGet<IEnumerable<T>> Parse(XElement root)
        {
            return TryGet<IEnumerable<T>>.Success(Parse_Internal(root));
        }

        private IEnumerable<T> Parse_Internal(XElement root)
        {
            foreach (var listElem in root.Elements())
            {
                var item = translator.Value.Parse(root);
                if (item.Failed) continue;
                yield return item.Value;
            }
        }

        public override void Write(XmlWriter writer, string name, IEnumerable<T> item)
        {
            using (new ElementWrapper(writer, ElementName))
            {
                writer.WriteAttributeString("name", name);
                foreach (var listObj in item)
                {
                    translator.Value.Write(writer, null, listObj);
                }
            }
        }
    }
}

namespace System
{
    public static class NoggListXmlTranlationExt
    {
        public static IEnumerable<M> CopyIn<T, M>(
            this ListXmlTranslation<T> transl,
            XElement root,
            INotifyingList<T> list,
            bool parseMask,
            Func<T> newFunc,
            NotifyingFireParameters? cmds)
            where T : IXmlTranslator<M>
        {
            List<M> masks = null;
            list.Clear(cmds);
            foreach (var listElem in root.Elements())
            {
                var parse = NoggXmlTranslation<T, M>.Instance.Parse(listElem, parseMask, newFunc);
                if (parse.Succeeded)
                {
                    list.Add(parse.Value.Nogg);
                }
                else
                {
                    if (masks == null)
                    {
                        masks = new List<M>();
                    }
                    masks.Add(parse.Value.Mask);
                }
            }
            return masks;
        }


        public static IEnumerable<M> Write<T, M>(
            this ListXmlTranslation<T> transl,
            XmlWriter writer,
            string name,
            IEnumerable<T> item,
            bool doMasks)
            where T : IXmlTranslator<M>
        {
            List<M> masks = null;
            using (new ElementWrapper(writer, transl.ElementName))
            {
                writer.WriteAttributeString("name", name);
                foreach (var listObj in item)
                {
                    if (doMasks)
                    {
                        NoggXmlTranslation<T, M>.Instance.Write(writer: writer, name: null, item: listObj, doMasks: doMasks, mask: out M mask);
                        if (mask != null)
                        {
                            if (masks == null)
                            {
                                masks = new List<M>();
                            }
                            masks.Add(mask);
                        }

                    }
                    else
                    {
                        NoggXmlTranslation<T, M>.Instance.Write(writer: writer, name: null, item: listObj);
                    }
                }
            }
            return masks;
        }
    }
}
