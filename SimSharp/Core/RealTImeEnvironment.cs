using System;

namespace SimSharp.Core
{
    public class RealTimeEnvironment : Environment
    {
        DateTime _envStart;
        DateTime _realStart;
        double _factor;
        bool _strict;

        public RealTimeEnvironment(DateTime initialTime, double factor = 1d, bool strict = true) : base(initialTime)
        {
            _factor = factor;
            _strict = strict;
            _envStart = initialTime;
            _realStart = DateTime.Now;
        }

        public void Sync()
        {
            _realStart = DateTime.Now;
        }

        public override void Step()
        {
            var now = DateTime.Now;
            var eventTime = Peek();
            var realTime = _realStart +  TimeSpan.FromSeconds (((eventTime - _envStart).Seconds * _factor));

            if ( _strict && (now - realTime).TotalSeconds > _factor )
            {
                throw new StopSimulationException($"Simulation too slow for real time ({now - realTime})");
            }

            while(true)
            {
                var delta = realTime - DateTime.Now;

                if ( delta <= TimeSpan.Zero )
                    break;

                System.Threading.Thread.Sleep(delta);
            }

            base.Step();
        }

    }
}
