using System;
using System.Linq;

namespace SimSharp.Core
{
    public class InteractiveConsoleEnvironment : Environment
    {
        private object locker = new object();
        private int _stepCountDown = 0;
        private DateTime _stopDate = DateTime.MaxValue;
        private bool _stepForwardNSteps = false;
        private bool _stepToDate = false;
        private bool _runToEnd = false;

        public InteractiveConsoleEnvironment(DateTime initialDateTime, int randomSeed, TimeSpan? defaultStep = null)
            : base(initialDateTime, randomSeed, defaultStep)
        {
            displayOptions();
        }

        public InteractiveConsoleEnvironment(DateTime initialDateTime, TimeSpan? defaultStep = null)
            : base(initialDateTime, defaultStep)
        {
            displayOptions();
        }

        public InteractiveConsoleEnvironment(int randomSeed, TimeSpan? defaultStep = null)
            : base(randomSeed, defaultStep)
        {
            displayOptions();
        }

        public InteractiveConsoleEnvironment(TimeSpan? defaultStep)
            : base(defaultStep)
        {
            displayOptions();
        }

        public InteractiveConsoleEnvironment() : base()
        {
            displayOptions();
        }


        public override void Step()
        {
            Event evt;
            lock ( locker )
            {
                if ( Queue.Count == 0 )
                {
                    var next = ScheduleQ.Dequeue();
                    Now = next.Priority;
                    evt = next.Event;
                }
                else
                {
                    evt = Queue.Dequeue();
                }
            }

            evt.Process();

            Console.WriteLine(string.Format($"{Now}: {evt}"));

            bool pauseForInput = shouldPauseForInput();

            if ( pauseForInput )
            {
                var key = Console.ReadKey();
                processInput(key);
            }
        }

        private bool shouldPauseForInput()
        {
            bool pauseForInput = false;

            if ( _runToEnd )
            {
            }
            else if ( _stepToDate )
            {
                var p = Peek();

                if ( Now >= _stopDate || (Now < _stopDate && p >= _stopDate) )
                {
                    _stepToDate = false;
                    _stopDate = DateTime.MaxValue;
                    pauseForInput = true;
                }
            }
            else if ( _stepForwardNSteps )
            {
                _stepCountDown--;
                if ( _stepCountDown == 0 )
                {
                    _stepForwardNSteps = false;
                    pauseForInput = true;
                }
            }
            else
            {
                pauseForInput = true;
            }

            return pauseForInput;
        }

        private void processInput(ConsoleKeyInfo input)
        {
            Console.WriteLine();

            if ( input.Key == ConsoleKey.H )
            {
                displayOptions();
                var newInput = Console.ReadKey();
                processInput(newInput);
            }

            if ( input.Key == ConsoleKey.Spacebar || input.Key == ConsoleKey.Enter )
            {
                return;
            }

            if ( input.Key == ConsoleKey.Q )
            {
                processQuery();
                return;
            }

            if ( input.Key == ConsoleKey.N )
            {
                bool success = false;

                do
                {
                    Console.WriteLine("Pause after how many events?");
                    string eventsString = Console.ReadLine();
                    int eventsToSkip;
                    success = int.TryParse(eventsString, out eventsToSkip) && eventsToSkip > 0;
                    if ( success )
                    {
                        _stepCountDown = eventsToSkip;
                    }

                } while ( !success );
                _stepForwardNSteps = true;
                return;
            }

            if ( input.Key == ConsoleKey.D )
            {
                DateTime stopDate;
                bool success = false;

                do
                {
                    Console.WriteLine("Pause before what date? (dd/mm/yyyy mm:ss)");
                    var newInput = Console.ReadLine();

                    success = DateTime.TryParse(newInput, out stopDate) && stopDate > Now;

                } while ( !success );

                _stopDate = stopDate;
                _stepToDate = true;
                return;
            }

            if ( input.Key == ConsoleKey.X )
            {
                _runToEnd = true;
                return;
            }
        }

        private void displayOptions()
        {
            Console.WriteLine("H\t\t\tShow Help");
            Console.WriteLine("Spacebar or Enter\tNext Event");
            Console.WriteLine("n\t\t\tSkip N Events");
            Console.WriteLine("d\t\t\tRun to Date");
            Console.WriteLine("x\t\t\tRun to end");
            Console.WriteLine("q\t\t\tQuery simulation");
        }

        private void processQuery(bool showOptions = true)
        {
            if (showOptions)
            {
                Console.WriteLine("n\t\t\tNumber of events scheduled");
                Console.WriteLine("x\t\t\tStop querying");
            }

            var newInput = Console.ReadKey();
            Console.WriteLine();

            bool moreQueries = true;

            if (newInput.Key == ConsoleKey.N)
            {
                Console.WriteLine($"{Queue.Count} events are scheduled");
                Console.WriteLine($"{ScheduleQ.Count} nodes in queue");
            }
            else if (newInput.Key == ConsoleKey.X)
            {
                moreQueries = false;
            }

            if( moreQueries )
                processQuery(false);
        }
    }
}
