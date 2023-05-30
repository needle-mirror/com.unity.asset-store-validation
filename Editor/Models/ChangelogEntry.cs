using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEditor.PackageManager.AssetStoreValidation.Models
{
    class ChangelogEntry
    {
        readonly Match m_Entry;
        internal List<int> m_Headers = new List<int>();
        internal readonly int k_Index;
        internal string Version => m_Entry.Groups["version"].ToString();
        internal string Date => m_Entry.Groups["date"].ToString();

        internal ChangelogEntry(Match entry, int index)
        {
            m_Entry = entry;
            k_Index = index;
        }

        internal void AddHeader(int headerIndex) => m_Headers.Add(headerIndex);
        public override string ToString() => m_Entry.ToString();
    }
}