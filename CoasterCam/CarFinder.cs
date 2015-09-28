using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CoasterCam
{
    class CarFinder : ICamLocationFinder
    {
        private TrackedRide _attraction;

        private IEnumerator<Car> _seats;

        public CarFinder(TrackedRide attraction)
        {
            _attraction = attraction;

            _seats = ((IEnumerable<Car>)_attraction.GetComponentsInChildren<Car>()).GetEnumerator();
        }

        public GameObject GetNextLocation()
        {
            if (_seats.MoveNext())
            {
                return _seats.Current.gameObject;
            }

            return null;
        }
    }
}
