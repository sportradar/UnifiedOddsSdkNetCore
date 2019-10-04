/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public static class StaticRandom
    {
        private static int _seed;

        private static readonly ThreadLocal<Random> ThreadLocal = new ThreadLocal<Random>
            (() => new Random(Interlocked.Increment(ref _seed)));

        static StaticRandom()
        {
            _seed = Environment.TickCount;
        }

        public static Random Instance => ThreadLocal.Value;

        public static string S(int limit = 0)
        {
            return limit > 1 ? Instance.Next(1, limit).ToString() : Instance.Next().ToString();
        }

        public static int I(int limit = 0)
        {
            return limit > 1 ? Instance.Next(1, limit) : Instance.Next();
        }

        public static URN Urn(string type = "", int limit = 0)
        {
            if (string.IsNullOrEmpty(type))
            {
                type = "match";
            }
            var id = limit > 1 ?  Instance.Next(1, limit) : Instance.Next();

            return new URN("sr", type, id);
        }

        public static URN Urn(int id, string type = "")
        {
            if (string.IsNullOrEmpty(type))
            {
                type = "match";
            }

            return new URN("sr", type, id);
        }

        public static string S1000 => S(1000);

        public static string S10000 => S(10000);

        public static string S10000P => S(900000000) + 100000;

        public static int I1000 => I(1000);

        public static string S100 => S(100);

        public static int I100 => I(100);

        public static URN U1000 => Urn(string.Empty, 1000);

        public static bool B => I(100) > 49;
    }
}
