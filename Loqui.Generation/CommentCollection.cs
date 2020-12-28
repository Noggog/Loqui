using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class CommentCollection
    {
        public readonly CommentWrapper Comments;
        public CommentWrapper SetterInterface;
        public CommentWrapper GetterInterface;

        public CommentCollection()
        {
            Comments = new CommentWrapper(null);
        }
    }
}
