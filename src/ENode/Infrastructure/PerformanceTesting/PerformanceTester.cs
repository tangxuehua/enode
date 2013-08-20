using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ENode.Infrastructure.PerformanceTesting
{
    /// <summary>A utility class used to do performance test.
    /// </summary>
    public class PerformanceTester
    {
        private static readonly PerformanceTester _instance = new PerformanceTester();
        private readonly Dictionary<string, TimeRecorder> _timeRecorderDictionary = new Dictionary<string, TimeRecorder>();

        private PerformanceTester() { }

        /// <summary>
        /// 
        /// </summary>
        public static PerformanceTester Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeRecorderName"></param>
        /// <returns></returns>
        public TimeRecorder GetTimeRecorder(string timeRecorderName)
        {
            return GetTimeRecorder(timeRecorderName, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeRecorderName"></param>
        /// <param name="reset"></param>
        /// <returns></returns>
        public TimeRecorder GetTimeRecorder(string timeRecorderName, bool reset)
        {
            if (!_timeRecorderDictionary.ContainsKey(timeRecorderName))
            {
                _timeRecorderDictionary.Add(timeRecorderName, new TimeRecorder(timeRecorderName));
            }
            var recorder = _timeRecorderDictionary[timeRecorderName];

            if (reset)
            {
                recorder.Reset();
            }

            return recorder;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class TimeRecorder
    {
        #region Private Members

        private List<RecorderItem> _recorderItemList;
        private Stopwatch _stopWatch;

        #endregion

        #region Constructors

        public TimeRecorder(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
            _recorderItemList = new List<RecorderItem>();
            _stopWatch = new Stopwatch();
        }

        #endregion

        #region Public Properties

        public string Name { get; private set; }

        #endregion

        #region Public Methods

        public void Reset()
        {
            _stopWatch.Stop();
            _stopWatch.Reset();
            _recorderItemList.Clear();
        }
        public RecorderItem BeginRecorderItem(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            return new RecorderItem(this, description);
        }
        public string GenerateReport()
        {
            StringBuilder reportBuilder = new StringBuilder();

            reportBuilder.AppendLine(Environment.NewLine);
            reportBuilder.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");

            reportBuilder.AppendLine(string.Format("TimeRecorder Name:{0}  Total RecorderItem Times:{1}ms", Name, (GetTotalTicks() / 10000).ToString()));
            reportBuilder.AppendLine("RecorderItem Time Details:");
            reportBuilder.AppendLine(GenerateTreeReport());

            reportBuilder.AppendLine("------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine);

            return reportBuilder.ToString();
        }

        #endregion

        #region Internal Methods

        internal void AddCompletedRecorderItem(RecorderItem recorderItem)
        {
            if (recorderItem != null && recorderItem.IsCompleted)
            {
                _recorderItemList.Add(recorderItem);
            }
        }
        internal double GetCurrentTicks()
        {
            _stopWatch.Stop();
            double currentTicks = (double)_stopWatch.Elapsed.Ticks;
            _stopWatch.Start();
            return currentTicks;
        }

        #endregion

        #region Private Methods

        private string GenerateTreeReport()
        {
            string totalString = string.Empty;
            string leftSpace = "";
            string unitIndentString = "    ";
            List<string> recorderItemTimeStrings = new List<string>();
            List<RecorderItem> topLevelRecorderItems = null;

            topLevelRecorderItems = GetTopLevelRecorderItems();

            foreach (RecorderItem recorderItem in topLevelRecorderItems)
            {
                recorderItem.TreeNodeDeepLevel = 1;
            }

            foreach (RecorderItem recorderItem in topLevelRecorderItems)
            {
                BuildChildRecorderItemTree(recorderItem);
            }

            foreach (RecorderItem recorderItem in topLevelRecorderItems)
            {
                GenerateRecorderItemTimeStrings(recorderItem, leftSpace, unitIndentString, recorderItemTimeStrings);
                totalString += string.Join(Environment.NewLine, recorderItemTimeStrings.ToArray());
                if (topLevelRecorderItems.IndexOf(recorderItem) < topLevelRecorderItems.Count() - 1)
                {
                    totalString += Environment.NewLine;
                }
                recorderItemTimeStrings.Clear();
            }

            return totalString;
        }
        private void BuildChildRecorderItemTree(RecorderItem parentRecorderItem)
        {
            List<RecorderItem> childRecorderItems = GetChildRecorderItems(parentRecorderItem);
            foreach (RecorderItem childRecorderItem in childRecorderItems)
            {
                childRecorderItem.TreeNodeDeepLevel = parentRecorderItem.TreeNodeDeepLevel + 1;
                childRecorderItem.ParentRecorderItem = parentRecorderItem;
                parentRecorderItem.ChildRecorderItems.Add(childRecorderItem);
                BuildChildRecorderItemTree(childRecorderItem);
            }
        }
        private double GetTotalTicks()
        {
            if (_recorderItemList.Count == 0)
            {
                return 0D;
            }

            double total = 0;
            foreach (RecorderItem recorderItem in GetTopLevelRecorderItems())
            {
                total = total + recorderItem.TotalTicks;
            }
            return total;
        }
        private bool IsTopLevelRecorderItem(RecorderItem recorderItem)
        {
            if (recorderItem == null)
            {
                return false;
            }
            foreach (RecorderItem a in _recorderItemList)
            {
                if (a.Id == recorderItem.Id)
                {
                    continue;
                }
                if (a.StartTicks < recorderItem.StartTicks && a.EndTicks > recorderItem.EndTicks)
                {
                    return false;
                }
            }
            return true;
        }
        private List<RecorderItem> GetTopLevelRecorderItems()
        {
            List<RecorderItem> topLevelRecorderItems = new List<RecorderItem>();
            foreach (RecorderItem recorderItem in _recorderItemList)
            {
                if (IsTopLevelRecorderItem(recorderItem))
                {
                    topLevelRecorderItems.Add(recorderItem);
                }
            }
            return topLevelRecorderItems;
        }
        private RecorderItem GetDirectParent(RecorderItem recorderItem)
        {
            if (recorderItem == null)
            {
                return null;
            }
            foreach (RecorderItem a in _recorderItemList)
            {
                if (recorderItem.Id == a.Id)
                {
                    continue;
                }
                if (a.StartTicks < recorderItem.StartTicks && a.EndTicks > recorderItem.EndTicks)
                {
                    return a;
                }
            }
            return null;
        }
        private List<RecorderItem> GetChildRecorderItems(RecorderItem parentRecorderItem)
        {
            if (parentRecorderItem == null)
            {
                return new List<RecorderItem>();
            }
            List<RecorderItem> childRecorderItems = new List<RecorderItem>();
            foreach (RecorderItem recorderItem in _recorderItemList)
            {
                if (recorderItem.Id == parentRecorderItem.Id)
                {
                    continue;
                }
                if (recorderItem.StartTicks > parentRecorderItem.StartTicks && recorderItem.EndTicks < parentRecorderItem.EndTicks)
                {
                    RecorderItem directParent = GetDirectParent(recorderItem);
                    if (directParent != null && directParent.Id == parentRecorderItem.Id)
                    {
                        childRecorderItems.Add(recorderItem);
                    }
                }
            }
            return childRecorderItems;
        }
        private void GenerateRecorderItemTimeStrings(RecorderItem recorderItem, string leftSpace, string unitIndentString, List<string> recorderItemTimeStrings)
        {
            string recorderItemTimeStringFormat = "{0}{1}({2})  {3}  {4}  {5}";
            string recorderItemTimeLeftSpaceString = leftSpace;
            for (int i = 0; i <= recorderItem.TreeNodeDeepLevel - 1; i++)
            {
                recorderItemTimeLeftSpaceString += unitIndentString;
            }

            recorderItemTimeStrings.Add(string.Format(recorderItemTimeStringFormat, new object[] { recorderItemTimeLeftSpaceString, (recorderItem.TotalTicks / 10000).ToString() + "ms", GetTimePercent(recorderItem), recorderItem.Description, recorderItem.StartTime.ToString() + ":" + recorderItem.StartTime.Millisecond.ToString(), recorderItem.EndTime.ToString() + ":" + recorderItem.EndTime.Millisecond.ToString() }));

            foreach (RecorderItem childRecorderItem in recorderItem.ChildRecorderItems)
            {
                GenerateRecorderItemTimeStrings(childRecorderItem, leftSpace, unitIndentString, recorderItemTimeStrings);
            }
        }
        private string GetTimePercent(RecorderItem recorderItem)
        {
            if (recorderItem.TreeNodeDeepLevel == 1)
            {
                var totalTicks = GetTotalTicks();
                if (totalTicks == 0D)
                {
                    return "0.00%";
                }
                else
                {
                    return (recorderItem.TotalTicks / totalTicks).ToString("##.##%");
                }
            }
            else if (recorderItem.TreeNodeDeepLevel >= 2)
            {
                if (recorderItem.ParentRecorderItem.TotalTicks == 0)
                {
                    return "0.00%";
                }
                else
                {
                    return (recorderItem.TotalTicks / recorderItem.ParentRecorderItem.TotalTicks).ToString("##.##%");
                }
            }
            return "0.00%";
        }

        #endregion
    }
    /// <summary>
    /// 
    /// </summary>
    public class RecorderItem
    {
        #region Constructors

        public RecorderItem(TimeRecorder timeRecorder, string description)
        {
            if (timeRecorder == null)
            {
                throw new ArgumentNullException("timeRecorder");
            }

            Id = Guid.NewGuid().ToString();
            TimeRecorder = timeRecorder;
            StartTicks = TimeRecorder.GetCurrentTicks();
            StartTime = DateTime.Now;
            Description = description;
            IsCompleted = false;
            ChildRecorderItems = new List<RecorderItem>();
        }

        #endregion

        #region Public Properties

        public TimeRecorder TimeRecorder { get; private set; }
        public string Id { get; private set; }
        public RecorderItem ParentRecorderItem { get; set; }
        public List<RecorderItem> ChildRecorderItems { get; set; }
        public int TreeNodeDeepLevel { get; set; }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public string Description { get; private set; }
        public double StartTicks { get; private set; }
        public double EndTicks { get; private set; }
        public double TotalTicks
        {
            get { return EndTicks - StartTicks; }
        }
        public bool IsCompleted { get; private set; }

        #endregion

        #region Public Methods

        public void Complete()
        {
            EndTicks = TimeRecorder.GetCurrentTicks();
            EndTime = DateTime.Now;
            IsCompleted = true;
            TimeRecorder.AddCompletedRecorderItem(this);
        }

        #endregion
    }
}