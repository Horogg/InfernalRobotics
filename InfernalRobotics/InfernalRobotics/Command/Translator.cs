﻿using InfernalRobotics.Control;

namespace InfernalRobotics.Command
{
    /*
     * <summary>
     * This class acts as a translator for UI - it implement basic commands for servos
     * and translates them to internal routines for servo controller.
     *
     * Thus later we can add more sophisticated controller commands, or change their behavior
     * per part basis without breaking the way UI works.
     *
     * More basic commands may be added too. Future API calls will most likely be hooked to this class.
     *
     */

    public class Translator
    {
        public void Init(bool axisInversion, bool motionLock, IServo servo)
        {
            IsAxisInverted = axisInversion;
            IsMotionLock = motionLock;
            this.servo = servo;
        }

        private IServo servo;

        // conversion data
        public bool IsAxisInverted { get; set; }
        public bool IsMotionLock { get; set; }
        public float GetSpeedUnit()
        {
            // the speed from part.cfg is used as the default unit of speed
            return servo.RawServo.rotateJoint ? servo.RawServo.keyRotateSpeed : servo.RawServo.keyTranslateSpeed;
        }

        // external interface
        /// <summary>
        /// Move the servo to the specified pos and speed.
        /// </summary>
        /// <param name="pos">Position in external coordinates</param>
        /// <param name="speed">Speed as multiplier</param>
        public void Move(float pos, float speed)
        {
            if (!servo.RawServo.Interpolator.Active)
                servo.RawServo.ConfigureInterpolator();

            if (!IsMotionLock)
                servo.RawServo.Interpolator.SetCommand(ToInternalPos(pos), speed * GetSpeedUnit());
            else
                servo.RawServo.Interpolator.SetCommand(0, 0);
        }

        public void MoveIncremental(float posDelta, float speed)
        {
            if (!servo.RawServo.Interpolator.Active)
                servo.RawServo.ConfigureInterpolator();

            float axisCorrection = IsAxisInverted ? -1 : 1;
            servo.RawServo.Interpolator.SetIncrementalCommand(posDelta*axisCorrection, speed * GetSpeedUnit());
        }

        public void Stop()
        {
            Move(0, 0);
        }

        public bool IsMoving()
        {
            return servo.RawServo.Interpolator.Active && (servo.RawServo.Interpolator.CmdVelocity != 0f);
        }

        public float ToInternalPos(float externalPos)
        {
            if (IsAxisInverted)
                return servo.RawServo.MinPosition + servo.RawServo.MaxPosition - externalPos;
            return externalPos;
        }

        public float ToExternalPos(float internalPos)
        {
            if (IsAxisInverted)
                return servo.RawServo.MinPosition + servo.RawServo.MaxPosition - internalPos;
            return internalPos;
        }
    }
}