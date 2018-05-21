using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Internal
{
    public class ErrorMaskBuilder : IDisposable
    {
        Stack<int> _depthStack;
        private int? _CurrentIndex;
        public int? CurrentIndex
        {
            get => _CurrentIndex ?? _depthStack.Peek();
            set => _CurrentIndex = value;
        }
        public List<(int[], Exception)> Exceptions;
        public List<(int[], string)> Warnings;

        internal IDisposable PushIndexInternal(int index)
        {
            if (!CurrentIndex.HasValue)
            {
                CurrentIndex = index;
                return this;
            }
            if (_depthStack == null)
            {
                _depthStack = new Stack<int>();
            }
            _depthStack.Push(CurrentIndex.Value);
            CurrentIndex = index;
            return this;
        }

        private int[] GetCurrentStack()
        {
            if (_CurrentIndex.HasValue)
            {
                return _depthStack.And(_CurrentIndex.Value).ToArray();
            }
            else
            {
                return _depthStack.ToArray();
            }
        }

        public void ReportException(Exception ex)
        {
            if (this.Exceptions == null)
            {
                this.Exceptions = new List<(int[], Exception)>();
            }
            this.Exceptions.Add((
                GetCurrentStack(),
                ex));
        }

        public void ReportWarning(string str)
        {
            if (this.Warnings == null)
            {
                this.Warnings = new List<(int[], string)>();
            }
            this.Warnings.Add((
                GetCurrentStack(),
                str));
        }

        public void Dispose()
        {
            if (_CurrentIndex.HasValue)
            {
                _CurrentIndex = null;
            }
            else
            {
                _depthStack?.Pop();
            }
        }
    }

    public static class ErrorMaskBuilderExt
    {
        public static IDisposable PushIndex(this ErrorMaskBuilder errorMask, int index)
        {
            if (errorMask == null) return Noggog.IDisposableExt.Nothing;
            errorMask.PushIndexInternal(index);
            return errorMask;
        }

        public static void ReportExceptionOrThrow(this ErrorMaskBuilder errorMask, Exception ex)
        {
            if (errorMask == null)
            {
                throw ex;
            }
            errorMask.ReportException(ex);
        }
    }
}
