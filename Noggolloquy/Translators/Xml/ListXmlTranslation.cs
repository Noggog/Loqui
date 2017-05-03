﻿using Noggolloquy.Xml;
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
    public class ListXmlTranslation<T> : ContainerXmlTranslation<T>
    {
        public readonly static ListXmlTranslation<T> Instance = new ListXmlTranslation<T>();

        public override string ElementName => "List";

        public override bool WriteSingleItem(XmlWriter writer, T item, bool doMasks, out object maskObj)
        {
            if (translator.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(T)}. {translator.Item.Reason}");
            }
            return translator.Item.Value.Write(writer, null, item, doMasks, out maskObj);
        }

        public override TryGet<T> ParseSingleItem(XElement root, bool doMasks, out object maskObj)
        {
            if (translator.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(T)}. {translator.Item.Reason}");
            }
            return translator.Item.Value.Parse(root, doMasks, out maskObj);
        }
    }
}
