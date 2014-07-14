using System;

namespace UniquenessConstraintSample
{
    /// <summary>对版块索引信息的抽象接口，定义操作版块索引信息的接口
    /// </summary>
    public interface ISectionIndexStore
    {
        SectionIndex FindBySectionId(string sectionId);
        SectionIndex FindBySectionName(string sectionName);
        void Add(SectionIndex index);
        void Update(string indexId, string sectionName);
    }
    /// <summary>版块索引信息
    /// </summary>
    public class SectionIndex
    {
        public string IndexId { get; private set; }
        public string SectionId { get; private set; }
        public string SectionName { get; private set; }

        public SectionIndex(string indexId, string sectionId, string sectionName)
        {
            IndexId = indexId;
            SectionId = sectionId;
            SectionName = sectionName;
        }
    }
}
