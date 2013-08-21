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
        private static readonly Dictionary<string, TimeRecorder> TimeRecorderDictionary = new Dictionary<string, TimeRecorder>();

        /// <summary>Get a time recorder.
        /// </summary>
        /// <param name="timeRecorderName"></param>
        /// <returns></returns>
        public static TimeRecorder GetTimeRecorder(string timeRecorderName)
        {
            return GetTimeRecorder(timeRecorderName, false);
        }
        /// <summary>Get a time recorder.
        /// </summary>
        /// <param name="timeRecorderName"></param>
        /// <param name="reset"></param>
        /// <returns></returns>
        public static TimeRecorder GetTimeRecorder(string timeRecorderName, bool reset)
        {
            if (!TimeRecorderDictionary.ContainsKey(timeRecorderName))
            {
                TimeRecorderDictionary.Add(timeRecorderName, new TimeRecorder(timeRecorderName));
            }
            var recorder = TimeRecorderDictionary[timeRecorderName];

            if (reset)
            {
                recorder.Reset();
            }

            return recorder;
        }
    }
    /// <summary>A time recorder used to do performance test.
    /// </summary>
    public class TimeRecorder
    {
        #region Private Members

        private readonly List<RecorderItem> _recorderItemList;
        private readonly Stopwatch _stopWatch;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>The name of the time recorder.
        /// </summary>
        public string Name { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>Reset the time recorder, reset the time and clear all the recorder items.
        /// </summary>
        public void Reset()
        {
            _stopWatch.Stop();
            _stopWatch.Reset();
            _recorderItemList.Clear();
        }
        /// <summary>Begin a recorder item with some description.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RecorderItem BeginRecorderItem(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentNullException("description");
            }
            return new RecorderItem(this, description);
        }
        /// <summary>Generate a report for the current performance test.
        /// </summary>
        /// <returns></returns>
        public string GenerateReport()
        {
            var reportBuilder = new StringBuilder();

            reportBuilder.AppendLine(Environment.NewLine);
            reportBuilder.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");

            reportBuilder.AppendLine(string.Format("TimeRecorder Name:{0}  Total RecorderItem Times:{1}ms", Name, (GetTotalTicks() / 10000)));
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
            var currentTicks = (double)_stopWatch.Elapsed.Ticks;
            _stopWatch.Start();
            return currentTicks;
        }

        #endregion

        #region Private Methods

        private string GenerateTreeReport()
        {
            var totalString = string.Empty;
            const string leftSpace = "";
            const string unitIndentString = "    ";
            var recorderItemTimeStrings = new List<string>();
            List<RecorderItem> topLevelRecorderItems = null;

            topLevelRecorderItems = GetTopLevelRecorderItems();

            foreach (var recorderItem in topLevelRecorderItems)
            {
                recorderItem.TreeNodeDeepLevel = 1;
            }

            foreach (var recorderItem in topLevelRecorderItems)
            {
                BuildChildRecorderItemTree(recorderItem);
            }

            foreach (var recorderItem in topLevelRecorderItems)
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
            var childRecorderItems = GetChildRecorderItems(parentRecorderItem);
            foreach (var childRecorderItem in childRecorderItems)
            {
                childRecorderItem.TreeNodeDeepLevel = parentRecorderItem.TreeNodeDeepLevel + 1;
                childRecorderItem.ParentRecorderItem = parentRecorderItem;
                parentRecorderItem.ChildRecorderItems.Add(childRecorderItem);
                BuildChildRecorderItemTree(childRecorderItem);
            }
        }
        private double GetTotalTicks()
        {
            return _recorderItemList.Count == 0 ? 0D : GetTopLevelRecorderItems().Aggregate<RecorderItem, double>(0, (current, recorderItem) => current + recorderItem.TotalTicks);
        }
        private bool IsTopLevelRecorderItem(RecorderItem recorderItem)
        {
            return recorderItem != null && _recorderItemList.Where(a => a.Id != recorderItem.Id).All(a => !(a.StartTicks < recorderItem.StartTicks) || !(a.EndTicks > recorderItem.EndTicks));
        }
        private List<RecorderItem> GetTopLevelRecorderItems()
        {
            return _recorderItemList.Where(IsTopLevelRecorderItem).ToList();
        }
        private RecorderItem GetDirectParent(RecorderItem recorderItem)
        {
            return recorderItem == null ? null : _recorderItemList.Where(a => recorderItem.Id != a.Id).FirstOrDefault(a => a.StartTicks < recorderItem.StartTicks && a.EndTicks > recorderItem.EndTicks);
        }
        private IEnumerable<RecorderItem> GetChildRecorderItems(RecorderItem parentRecorderItem)
        {
            return parentRecorderItem == null ? new List<RecorderItem>() : (from recorderItem in _recorderItemList where recorderItem.Id != parentRecorderItem.Id where recorderItem.StartTicks > parentRecorderItem.StartTicks && recorderItem.EndTicks < parentRecorderItem.EndTicks let directParent = GetDirectParent(recorderItem) where directParent != null && directParent.Id == parentRecorderItem.Id select recorderItem).ToList();
        }
        private void GenerateRecorderItemTimeStrings(RecorderItem recorderItem, string leftSpace, string unitIndentString, List<string> recorderItemTimeStrings)
        {
            const string recorderItemTimeStringFormat = "{0}{1}({2})  {3}  {4}  {5}";
            var recorderItemTimeLeftSpaceString = leftSpace;
            for (var i = 0; i <= recorderItem.TreeNodeDeepLevel - 1; i++)
            {
                recorderItemTimeLeftSpaceString += unitIndentString;
            }

            recorderItemTimeStrings.Add(string.Format(recorderItemTimeStringFormat, new object[] { recorderItemTimeLeftSpaceString, (recorderItem.TotalTicks / 10000).ToString() + "ms", GetTimePercent(recorderItem), recorderItem.Description, recorderItem.StartTime.ToString() + ":" + recorderItem.StartTime.Millisecond.ToString(), recorderItem.EndTime.ToString() + ":" + recorderItem.EndTime.Millisecond.ToString() }));

            foreach (var childRecorderItem in recorderItem.ChildRecorderItems)
            {
                GenerateRecorderItemTimeStrings(childRecorderItem, leftSpace, unitIndentString, recorderItemTimeStrings);
            }
        }
        private string GetTimePercent(RecorderItem recorderItem)
        {
            if (recorderItem.TreeNodeDeepLevel == 1)
            {
                var totalTicks = GetTotalTicks();
                return (int)totalTicks == 0 ? "0.00%" : (recorderItem.TotalTicks / totalTicks).ToString("##.##%");
            }
            if (recorderItem.TreeNodeDeepLevel >= 2)
            {
                return (int)recorderItem.ParentRecorderItem.TotalTicks == 0 ? "0.00%" : (recorderItem.TotalTicks / recorderItem.ParentRecorderItem.TotalTicks).ToString("##.##%");
            }
            return "0.00%";
        }

        #endregion
    }
    /// <summary>Represents an item in the time recorder.
    /// </summary>
    public class RecorderItem
    {
        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="timeRecorder"></param>
        /// <param name="description"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>The owner timer recorder.
        /// </summary>
        public TimeRecorder TimeRecorder { get; private set; }
        /// <summary>The unique id of the recorder item.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>The parent recorder item.
        /// </summary>
        public RecorderItem ParentRecorderItem { get; set; }
        /// <summary>The child recorder items.
        /// </summary>
        public List<RecorderItem> ChildRecorderItems { get; set; }
        /// <summary>The tree level of the current recorder item.
        /// </summary>
        public int TreeNodeDeepLevel { get; set; }
        /// <summary>The start time of the recorder item.
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>The end time of the recorder item.
        /// </summary>
        public DateTime EndTime { get; private set; }
        /// <summary>The description of the recorder item.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>The start ticks of the recorder item.
        /// </summary>
        public double StartTicks { get; private set; }
        /// <summary>The end ticks of the recorder item.
        /// </summary>
        public double EndTicks { get; private set; }
        /// <summary>The total ticks of the recorder item.
        /// </summary>
        public double TotalTicks
        {
            get { return EndTicks - StartTicks; }
        }
        /// <summary>Represents whether the current recorder item is completed.
        /// </summary>
        public bool IsCompleted { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>Complete the current recorder item.
        /// </summary>
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