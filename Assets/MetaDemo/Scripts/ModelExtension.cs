namespace Agora.Rtm
{
    public static class MyExtensions
    {
        public static string ToMyString(this StateItem item)
        {
            return $"{item.key}:{item.value}";
        }

        public static string ToMyString(this StateItem[] items)
        {
            string str = "[";
            for (int i = 0; i < items.Length; i++)
            {
                str += "{" + items[i].ToMyString() + "}";
                if (i != items.Length - 1)
                {
                    str += ",";
                }
            }
            return str + "]";
        }
        public static string ToMyString(this SnapshotInfo info)
        {
            string str = @"{""userStateList"":";
            str += info.userStateList.ToMyString();
            return str + "}";
        }

        public static string ToMyString(this UserState state)
        {
            string str = @"{""userId"":""" + state.userId + @""",""states"":";
            str += state.states.ToMyString() + "}";
            return str;
        }

        public static string ToMyString(this UserState[] states)
        {
            string str = "[";
            for (int i = 0; i < states.Length; i++)
            {
                str += states[i].ToMyString();
                if (i != states.Length - 1)
                {
                    str += ",";
                }
            }

            return str + "]";
        }
    }
}

