using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace MovingSofaProblem
{
    public class Breadcrumb
    {
        public Breadcrumb(Vector3 position, Quaternion rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
    }

    public class PathHolder
    {
        List<Breadcrumb> path = new List<Breadcrumb>();
        
        public void Add(Vector3 position, Quaternion rotation)
        {
            var breadcrumb = new Breadcrumb(position, rotation);
            path.Add(breadcrumb);
        }
        
        public bool HasSegmentAt(int index)
        {
            return index < path.Count;
        }
        
        // TODO - Return a segment containing 2 breadcrumbs. Then segments go from 1 to the last index (count - 1)
        public Breadcrumb SegmentAt(int index)
        {
            return path.ElementAt<Breadcrumb>(index);
        }

        public int Count 
        {
            get { return path.Count; }
        }
    }
}
