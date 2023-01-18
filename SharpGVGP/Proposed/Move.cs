using System;
namespace SharpGVGP.Proposed
{
    /// <summary>
    /// Possible actions for the executer.
    /// 2048: First four
    /// Jelly Escape: First nine
    /// Pink Hour: All eighteen
    /// </summary>
    public enum Move
    {
        /// <summary>
        /// Press W
        /// </summary>
        UP,
        /// <summary>
        /// Press S
        /// </summary>
        DOWN,
        /// <summary>
        /// Press A
        /// </summary>
        LEFT,
        /// <summary>
        /// Press D
        /// </summary>
        RIGHT,
        /// <summary>
        /// Don't press anything
        /// </summary>
        NOTHING,
        /// <summary>
        /// Press W + A
        /// </summary>
        UPLEFT,
        /// <summary>
        /// Press W + D
        /// </summary>
        UPRIGHT,
        /// <summary>
        /// Press S + A
        /// </summary>
        DOWNLEFT,
        /// <summary>
        /// Press S + D
        /// </summary>
        DOWNRIGHT,
        /// <summary>
        /// Press Z
        /// </summary>
        JUMP,
        /// <summary>
        /// Press W + Z
        /// </summary>
        UPJUMP,
        /// <summary>
        /// Press S + Z
        /// </summary>
        DOWNJUMP,
        /// <summary>
        /// Press A + Z
        /// </summary>
        LEFTJUMP,
        /// <summary>
        /// Press D + Z
        /// </summary>
        RIGHTJUMP,
        /// <summary>
        /// Press W + A + Z
        /// </summary>
        UPLEFTJUMP,
        /// <summary>
        /// Press W + D + Z
        /// </summary>
        UPRIGHTJUMP,
        /// <summary>
        /// Press S + A + Z
        /// </summary>
        DOWNLEFTJUMP,
        /// <summary>
        /// Press S + D + Z
        /// </summary>
        DOWNRIGHTJUMP
    }
}
