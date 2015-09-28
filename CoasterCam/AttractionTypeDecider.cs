using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CoasterCam
{
    class AttractionTypeDecider
    {
        private GameObject _go;

        public enum AttractionType
        {
            NonTracked,
            Tracked
        }

        public AttractionTypeDecider(GameObject go)
        {
            _go = go;
        }

        public AttractionType? GetType()
        {
            TrackedRide tr = FindTrackedRide();

            if (tr == null)
            {
                Attraction attr = FindAttraction();

                if (attr == null)
                {
                    return null;
                }

                return AttractionType.NonTracked;
            }

            return AttractionType.Tracked;
        }

        private TrackedRide FindTrackedRide()
        {
            TrackedRide trackedRide = _go.GetComponentInParent<TrackedRide>();

            if (trackedRide != null)
            {
                return trackedRide;
            }

            return null;
        }

        private Attraction FindAttraction()
        {
            Attraction attraction = _go.GetComponentInParent<Attraction>();

            if (attraction != null)
            {
                return attraction;
            }

            return null;
        }
    }
}
