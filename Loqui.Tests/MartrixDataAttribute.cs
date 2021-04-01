using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Loqui.Tests
{
    public class MatrixTheoryData<T1, T2, T3, T4> : TheoryData<T1, T2, T3, T4>
    {
        public MatrixTheoryData(TheoryData<T1> data1, TheoryData<T2, T3, T4> data2)
        {
            Contract.Assert(data1 != null && data1.Any());
            Contract.Assert(data2 != null && data2.Any());

            foreach (var t1 in data1)
                foreach (var t2 in data2)
                    Add((T1)t1[0], (T2)t2[0], (T3)t2[1], (T4)t2[2]);
        }

        public MatrixTheoryData(TheoryData<T1, T2> data1, TheoryData<T3, T4> data2)
        {
            Contract.Assert(data1 != null && data1.Any());
            Contract.Assert(data2 != null && data2.Any());

            foreach (var t1 in data1)
                foreach (var t2 in data2)
                    Add((T1)t1[0], (T2)t1[1], (T3)t2[0], (T4)t2[1]);
        }

        public MatrixTheoryData(TheoryData<T1, T2, T3> data1, TheoryData<T4> data2)
        {
            Contract.Assert(data1 != null && data1.Any());
            Contract.Assert(data2 != null && data2.Any());

            foreach (var t1 in data1)
                foreach (var t2 in data2)
                    Add((T1)t1[0], (T2)t1[1], (T3)t1[2], (T4)t2[0]);
        }
    }

    public class MatrixTheoryData<T1, T2, T3> : TheoryData<T1, T2, T3>
    {
        public MatrixTheoryData(TheoryData<T1, T2> data1, TheoryData<T3> data2)
        {
            Contract.Assert(data1 != null && data1.Any());
            Contract.Assert(data2 != null && data2.Any());

            foreach (var t1 in data1)
                foreach (var t2 in data2)
                    Add((T1)t1[0], (T2)t1[1], (T3)t2[0]);
        }

        public MatrixTheoryData(TheoryData<T1> data1, TheoryData<T2, T3> data2)
        {
            Contract.Assert(data1 != null && data1.Any());
            Contract.Assert(data2 != null && data2.Any());

            foreach (var t1 in data1)
                foreach (var t2 in data2)
                    Add((T1)t1[0], (T2)t2[0], (T3)t2[1]);
        }
    }
}