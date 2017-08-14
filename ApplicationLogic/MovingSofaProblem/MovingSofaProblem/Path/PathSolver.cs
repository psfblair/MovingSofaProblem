using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.MovingSofa.Scripts.MovingSofaProblem.Path
{
    // We want a function that takes a path and returns a different path.
    // The measure has to pass through each plane at each breadcrumb
    // A plane at a breadcrumb is defined as the plane where the segment is normal to the
    // end breadcrumb of the segment. But it should also be only the portion of the plane
    // that is actually exposed, and not include or extend behind walls.

    // For each segment, we travel along that segment until we bump something
    // If we bump something, we back up 10cm 
    // We try various things - roll, pitch, yaw - move forward until we hit the normal plane
        // Can make "forward" the original forward to the breadcrumb we were originally targeting
        // esp. if we roll
        // If we pitch or yaw, we want to go forward in the direction of the pitch or yaw
        // (is that always the case?)
    // If we hit it without bumping, we replace the breadcrumb we targeted with the one where
    // we hit the normal plane.
    // Need some heuristics - e.g., if I have a bed going through a door I need to roll 90 degrees
    // We can roll 180 degrees in either direction
    // We can pitch or yaw +/- 90 degrees
    // Somehow we need to retain what we've already tried so we don't repeat it.
    // Do we always want to travel narrow-face forwards?

    class PathSolver
    {
    }

    /* 
     * How do we move the measure? Do we need something in Update and a position interpolator?
     * 1. Attach trigger collider to object.
     * 2. Move measure along segment until trigger. Stop when triggered.
     * 3. Calculate 10 cm back along segment. Put measure there. (What if we're < 10cm from beginning of segment?)
     * 4. 
     */
}
