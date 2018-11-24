using Loqui.Generation;
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
            if (!obj.HasLoquiBaseObject)
            {
                obj.NonLoquiBaseClass = "LoquiNotifyingObject";
            }
            obj.RequiredNamespaces.Add("ReactiveUI");
            await base.PostLoad(obj);
        }
    }
}
