using Loqui.Generation;
using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{ 
    public class ReactiveModule : GenerationModule
    {
        public override async Task PostLoad(ObjectGeneration obj)
        {
            if (!obj.IterateFields().Any(f => f.NotifyingType == NotifyingType.ReactiveUI)) return;
            var opt = obj.Node.GetAttribute(Constants.RX_BASE_OPTION, obj.RxBaseOptionDefault);
            if (!obj.HasLoquiBaseObject)
            {
                obj.NonLoquiBaseClass = opt.ToStringFast_Enum_Only();
                switch (opt)
                {
                    case RxBaseOption.LoquiNotifyingObject:
                        break;
                    case RxBaseOption.ViewModel:
                        obj.RequiredNamespaces.Add("Noggog.WPF");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            obj.RequiredNamespaces.Add("ReactiveUI");
            await base.PostLoad(obj);
        }
    }
}
