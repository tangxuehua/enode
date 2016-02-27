using System;
using System.Collections.Generic;
using System.Linq;

namespace E12306.Domain
{
    /// <summary>车次，聚合根
    /// </summary>
    public class TrainShift
    {
        #region Private Variables

        private string _id;
        private string _name;
        private IList<Station>               _stations;        //车次经过的站点列表
        private IDictionary<int, Seat>       _soldSeats;       //已售出过票的座位列表
        private IDictionary<int, Seat>       _cleanSeats;      //未售出过票的座位列表
        private IDictionary<Segment, int>    _atomSegments;    //原子区间的可用座位数信息

        #endregion

        public string Id
        {
            get { return _id; }
        }
        public string Name
        {
            get { return _name; }
        }
        public IEnumerable<Station> Stations
        {
            get { return _stations; }
        }
        public IEnumerable<Seat> SoldSeats
        {
            get { return _soldSeats.Values; }
        }

        public TrainShift(string id, string name)
        {
            _id = id;
            _name = name;
            _stations = new List<Station>();
            _soldSeats = new Dictionary<int, Seat>();
            _cleanSeats = new Dictionary<int, Seat>();
            _atomSegments = new Dictionary<Segment, int>();
        }

        /// <summary>根据站点数和座位数初始化车次
        /// </summary>
        /// <param name="stationCount"></param>
        /// <param name="seatCount"></param>
        /// <returns></returns>
        public TrainShift Inatialize(int stationCount, int seatCount)
        {
            _stations.Clear();
            for (var i = 1; i <= stationCount; i++)
            {
                _stations.Add(new Station("station" + i, i, DateTime.Now));
                if (i >= 2)
                {
                    _atomSegments.Add(new Segment(i - 1, i), seatCount);
                }
            }

            _cleanSeats.Clear();
            for (var i = 1; i <= seatCount; i++)
            {
                var seat = new Seat(i);
                _cleanSeats.Add(seat.No, seat);
            }

            return this;
        }

        /// <summary>订票领域方法，实现订票的核心业务逻辑
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public Ticket BookTicket(Segment segment)
        {
            var atomSegments = GetAtomSegments(segment);
            if (!IsAllAtomSegmentHaveSeat(atomSegments))
            {
                return null;
            }

            var seat = SelectSeat(segment);
            if (seat != null)
            {
                SellSeat(seat, segment, atomSegments);
                return CreateTicket(seat, segment);
            }
            return null;
        }

        /// <summary>为给定的订票区间选择一个可用的座位，优先回收已出售的座位
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private Seat SelectSeat(Segment segment)
        {
            //优先回收已出售的座位
            foreach (var seat in _soldSeats.Values)
            {
                if (seat.CanSellToSegment(segment))
                {
                    return seat;
                }
            }
            //如果已出售的座位不符合当前区间的出售条件，则尝试从座位池取出第一个座位进行出售
            return _cleanSeats.Values.FirstOrDefault();
        }
        /// <summary>将给定座位出售给给定的订票区间
        /// </summary>
        /// <param name="seat"></param>
        /// <param name="segment"></param>
        /// <param name="atomSegments"></param>
        private void SellSeat(Seat seat, Segment segment, IEnumerable<Segment> atomSegments)
        { 
            foreach (var atomSegment in atomSegments)
            {
                _atomSegments[atomSegment] = _atomSegments[atomSegment] - 1;
            }

            seat.AddSoldSegment(segment);
            if (!_soldSeats.ContainsKey(seat.No))
            {
                _soldSeats.Add(seat.No, seat);
                _cleanSeats.Remove(seat.No);
            }
        }
        /// <summary>创建一张票
        /// </summary>
        /// <param name="seat"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        private Ticket CreateTicket(Seat seat, Segment segment)
        {
            var startStation = _stations.Single(x => x.No == segment.StartNo);
            var endStation = _stations.Single(x => x.No == segment.EndNo);
            return new Ticket(seat.No, startStation, endStation);
        }
        /// <summary>判断给定的原子区间的可用票数是否都大于0
        /// </summary>
        /// <param name="atomSegments"></param>
        /// <returns></returns>
        private bool IsAllAtomSegmentHaveSeat(IEnumerable<Segment> atomSegments)
        {
            foreach (var atomSegment in atomSegments)
            {
                if (_atomSegments[atomSegment] <= 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>获取给定区间的原子区间列表
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        private IEnumerable<Segment> GetAtomSegments(Segment segment)
        {
            return _atomSegments.Where(x => x.Key.StartNo >= segment.StartNo && x.Key.EndNo <= segment.EndNo).Select(x => x.Key).ToList();
        }
    }

    /// <summary>车票，聚合根
    /// </summary>
    public class Ticket
    {
        public string Id { get; private set; }
        public int SeatNo { get; private set; }
        public Station Start { get; private set; }
        public Station End { get; private set; }

        public Ticket(int seatNo, Station start, Station end)
        {
            Id = Guid.NewGuid().ToString();
            SeatNo = seatNo;
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return string.Format("SeatNo: {0}, Segment [{1}, {2}]", SeatNo, Start.No, End.No);
        }
    }
    /// <summary>座位，实体
    /// </summary>
    public class Seat
    {
        private IList<Segment> _soldSegments = new List<Segment>();

        /// <summary>座位号，车次内唯一
        /// </summary>
        public int No { get; private set; }

        /// <summary>返回所有已出售的区间
        /// </summary>
        public IEnumerable<Segment> SoldSegments
        {
            get { return _soldSegments; }
        }

        public Seat(int no)
        {
            No = no;
        }

        /// <summary>判断当前座位是否可以出售给给定的区间
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool CanSellToSegment(Segment segment)
        {
            //如果当前座位的已出售区间中有任何一个区间和当前区间重叠，则认为当前座位不允许出售给当前区间
            foreach (var soldSegment in _soldSegments)
            {
                if (IsSegmentIntersect(soldSegment, segment))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>添加一个已出售的区间
        /// </summary>
        /// <param name="segment"></param>
        public void AddSoldSegment(Segment segment)
        {
            _soldSegments.Add(segment);
        }

        /// <summary>判断两个区间是否重叠
        /// </summary>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <returns></returns>
        private bool IsSegmentIntersect(Segment segment1, Segment segment2)
        {
            return (segment1.StartNo > segment2.StartNo && segment1.StartNo < segment2.EndNo)
                || (segment1.EndNo > segment2.StartNo && segment1.EndNo < segment2.EndNo)
                || (segment1.StartNo <= segment2.StartNo && segment1.EndNo >= segment2.EndNo)
                || (segment1.StartNo >= segment2.StartNo && segment1.EndNo <= segment2.EndNo);
        }
    }
    /// <summary>订票时传的区间信息，值对象
    /// </summary>
    public class Segment
    {
        public int StartNo { get; private set; }
        public int EndNo { get; private set; }

        public Segment(int startNo, int endNo)
        {
            if (startNo >= endNo)
            {
                throw new ArgumentException("Start stationNo must small than end stationNo.");
            }
            StartNo = startNo;
            EndNo = endNo;
        }

        #region Value Object Stuff

        public static bool operator ==(Segment left, Segment right)
        {
            return IsEqual(left, right);
        }
        public static bool operator !=(Segment left, Segment right)
        {
            return !IsEqual(left, right);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Segment;
            return this.StartNo == other.StartNo && this.EndNo == other.EndNo;
        }
        public override int GetHashCode()
        {
            return StartNo.GetHashCode() ^ EndNo.GetHashCode();
        }

        private static bool IsEqual(Segment left, Segment right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        #endregion
    }
    /// <summary>车次内定义的站点信息，实体
    /// </summary>
    public class Station
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public int No { get; private set; }
        public DateTime StartTime { get; private set; }

        public Station(string name, int no, DateTime startTime)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            No = no;
            StartTime = startTime;
        }
    }
}
