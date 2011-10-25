using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace List
{
    class Environment
    {
        private readonly Environment mParent = null;
        private readonly Dictionary<string, Value> mDic = new Dictionary<string, Value>();

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
            if (mDic.ContainsKey(key))
                return mDic[key];
            if (mParent == null)
                throw new KeyNotFoundException("Could not find key '" + key + "'.");
            else
                return mParent.Get(key);
        }

        public void Add(string key, Value val)
        {
            mDic.Add(key, val);
        }

        public void Add(string[] keys, Value val)
        {
            foreach (var k in keys)
            {
                mDic.Add(k, val);
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
                mDic[key] = value;
            }
        }
    }
}
