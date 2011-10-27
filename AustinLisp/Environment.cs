using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AustinLisp
{
    class Environment
    {
        private readonly Environment mParent = null;
        private List mValues = List.Nil;

        public Environment()
            : this(null)
        {
            mParent = null;
        }
        public Environment(Environment parent)
        {
            this.mParent = parent;
        }

        public Value Get(string key)
        {
            List l = this.mValues;
            while (l != List.Nil)
            {
                List kvp = l.Val as List;
                string k = (kvp.Val as Word).Val;
                if (k == key)
                    return kvp.Next.Val;
                l = l.Next;
            }
            if (mParent == null)
                throw new KeyNotFoundException("Could not find key '" + key + "'.");
            else
                return mParent.Get(key);
        }

        public void Add(string key, Value val)
        {
            mValues = new List(new List(new Word(key), new List(val, List.Nil)), mValues);
        }

        public void Add(string[] keys, Value val)
        {
            foreach (var k in keys)
            {
                Add(k, val);
            }
        }

        public Value this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Add(key, value);
            }
        }

        public List AsList()
        {
            List parentList = List.Nil;
            if (mParent != null)
                parentList = mParent.AsList();
            return new List(mValues, parentList);
        }
    }
}
