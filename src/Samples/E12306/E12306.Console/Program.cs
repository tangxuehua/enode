using E12306.Domain;

namespace E12306.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var stationCount = 4;
            var seatCount = 3;
            var trainShift = new TrainShift("0001", "Z19").Inatialize(stationCount, seatCount);

            var ticket = trainShift.BookTicket(new Segment(1, 2));
            PrintTicket(ticket);

            ticket = trainShift.BookTicket(new Segment(2, 3));
            PrintTicket(ticket);

            ticket = trainShift.BookTicket(new Segment(3, 4));
            PrintTicket(ticket);

            ticket = trainShift.BookTicket(new Segment(1, 4));
            PrintTicket(ticket);

            ticket = trainShift.BookTicket(new Segment(1, 4));
            PrintTicket(ticket);

            ticket = trainShift.BookTicket(new Segment(1, 4));
            PrintTicket(ticket);

            System.Console.ReadLine();
        }

        static void PrintTicket(Ticket ticket)
        {
            if (ticket == null)
            {
                System.Console.WriteLine("No available ticket.");
            }
            else
            {
                System.Console.WriteLine(ticket);
            }
        }
    }
}
