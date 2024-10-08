﻿
using StereoKit;
using System;
using System.Timers;

namespace Scritps.Services
{
    public class HandTracking
    {
        //TODO: make custom types or structs for the values (degree/radian)
        double fixedDeltaTime = 0.02;
        System.Timers.Timer handTrackingTimer = new System.Timers.Timer(0020); // hard coded fixed amount

        Hand rightHand = Input.Hand(Handed.Right);
        Hand leftHand = Input.Hand(Handed.Left);

        Vec3 headForward;
        Vec3 headPosition;

        Vec3 rightHandPosition;
        Vec3 leftHandPosition;

        Vec3 rightHandRelativePosition;
        Vec3 leftHandRelativePosition;

        Vec3 dirtyRightHandPosition = new Vec3();
        Vec3 dirtyLeftHandPosition = new Vec3();

        Vec3 dirtyRightHandRelativePosition = new Vec3();
        Vec3 dirtyLeftHandRelativePosition = new Vec3();

        double radianRightHandVerticalAngle;
        double radianLeftHandVerticalAngle;

        double radianRightHandHorizontalAngle;
        double radianLeftHandHorizontalAngle;

        double degreeRightHandVerticalAngle;
        double degreeLeftHandVerticalAngle;

        double degreeRightHandHorizontalAngle;
        double degreeLeftHandHorizontalAngle;

        Vec2 rightHandAnglesDegree = new Vec2();
        Vec2 leftHandAnglesDegree = new Vec2();

        Vec2 rightHandAnglesRadian = new Vec2();
        Vec2 leftHandAnglesRadian = new Vec2();

        Vec3 rightHandVelocity = new Vec3();
        Vec3 leftHandVelocity = new Vec3();

        Vec3 rightHandRelativeVelocity = new Vec3();
        Vec3 leftHandRelativeVelocity = new Vec3();

        public Action RightFastHand;
        public Action LeftHandDownFast;

        private float inputBuffer;

        public HandTracking()
        {
            Init();
        }

        private void Init()
        {
            handTrackingTimer.Elapsed += UpdateHandTracking;
            handTrackingTimer.Elapsed += UpdateHandTrackingChecks;
            handTrackingTimer.Enabled = true;
            handTrackingTimer.Start();
        }

        private void UpdateHandTracking(object sender, ElapsedEventArgs e)
        {
            GetHeadPositions();
            CalculateHandsPosition();
            CalculateRadianAngles();
            CalculateDegreeAngles();
        }

        #region Info
        private void GetHeadPositions()
        {
            headForward = Input.Head.Forward;
            headPosition = Input.Head.position;
        }

        private void CalculateHandsPosition()
        {
            Hand rightHand = Input.Hand(Handed.Right);
            Hand leftHand = Input.Hand(Handed.Left);

            rightHandPosition = rightHand.palm.position;
            leftHandPosition = leftHand.palm.position;

            rightHandRelativePosition = rightHandPosition - headPosition;
            leftHandRelativePosition = leftHandPosition - headPosition;

            rightHandVelocity = CalculateVelocity(dirtyRightHandPosition, rightHandPosition);
            leftHandVelocity = CalculateVelocity(dirtyLeftHandPosition, leftHandPosition);
            rightHandRelativeVelocity = CalculateVelocity(dirtyRightHandRelativePosition, rightHandRelativePosition);
            leftHandRelativeVelocity = CalculateVelocity(dirtyLeftHandRelativePosition, leftHandRelativePosition);

            dirtyRightHandPosition = rightHandPosition;
            dirtyLeftHandPosition = leftHandPosition;
            dirtyRightHandRelativePosition = rightHandRelativePosition;
            dirtyLeftHandRelativePosition = leftHandRelativePosition;
        }

        private Vec3 CalculateVelocity(Vec3 pointA, Vec3 pointB)
        {
            Vec3 velocity = new Vec3();

            if (Vec3.Distance(pointA, pointB) == 0)
            {
                return new Vec3();
            }

            //TODO: refacto custom types Vector3 double

            velocity = (pointB - pointA) * (float)(1 / fixedDeltaTime) * 100;

            return velocity;
        }

        private void CalculateRadianAngles()
        {
            radianRightHandVerticalAngle = Math.Atan2(rightHandRelativePosition.z, rightHandRelativePosition.y) - Math.Atan2(headForward.z, headForward.y);
            radianLeftHandVerticalAngle = Math.Atan2(leftHandRelativePosition.z, leftHandRelativePosition.y) - Math.Atan2(headForward.z, headForward.y);

            radianRightHandHorizontalAngle = Math.Atan2(rightHandRelativePosition.z, rightHandRelativePosition.x) - Math.Atan2(headForward.z, headForward.x);
            radianLeftHandHorizontalAngle = Math.Atan2(leftHandRelativePosition.z, leftHandRelativePosition.x) - Math.Atan2(headForward.z, headForward.x);

            rightHandAnglesRadian.x = (float)radianRightHandHorizontalAngle;
            rightHandAnglesRadian.y = (float)radianRightHandVerticalAngle;
            leftHandAnglesRadian.x = (float)radianLeftHandHorizontalAngle;
            leftHandAnglesRadian.y = (float)radianLeftHandVerticalAngle;
        }

        private void CalculateDegreeAngles()
        {
            degreeRightHandVerticalAngle = 180 / Math.PI * radianRightHandVerticalAngle;
            degreeLeftHandVerticalAngle = 180 / Math.PI * radianLeftHandVerticalAngle;

            degreeRightHandHorizontalAngle = 180 / Math.PI * radianRightHandHorizontalAngle;
            degreeLeftHandHorizontalAngle = 180 / Math.PI * radianLeftHandHorizontalAngle;

            rightHandAnglesDegree.x = (float)degreeRightHandHorizontalAngle;
            rightHandAnglesDegree.y = (float)degreeRightHandVerticalAngle;
            leftHandAnglesDegree.x = (float)degreeLeftHandHorizontalAngle;
            leftHandAnglesDegree.y = (float)degreeLeftHandVerticalAngle;
        }
        #endregion

        private void UpdateHandTrackingChecks(object sender, ElapsedEventArgs e)
        {
            if (Vec3.Distance(leftHand.palm.position, rightHand.palm.position) <= 0.07f)
            {
                if (leftHandVelocity.x >= 100 && rightHandVelocity.x <= 100)
                {
                    if (leftHand.IsPinched && rightHand.IsPinched)
                    {
                        SkyLogEvents.CancelOpenLog.Invoke();
                    }
                }
            }

            // player commands
            if (leftHand.IsGripped) {
                if (rightHandVelocity.y <= -100) {
                    SkyLogEvents.Pause.Invoke();
                    Console.WriteLine("pause");
                }

                if (rightHandVelocity.y >= 100) {
                    SkyLogEvents.Play.Invoke();
                    Console.WriteLine("play");
                }

                if (rightHandVelocity.x >= 100) {
                    SkyLogEvents.RightFrame.Invoke();
                    Console.WriteLine("RightFrame");
                }

                if (rightHandVelocity.x <= -100) {
                    SkyLogEvents.LeftFrame.Invoke();
                    Console.WriteLine("LeftFrame");
                }
            }

            if (Vec3.Dot(rightHand.palm.Forward.Normalized, leftHand.palm.Forward.Normalized) <= -0.60) {
                if (rightHandVelocity.y >= 120) {
                    SkyLogEvents.BackToLive.Invoke();
                }
            }

            if (Vec3.Dot(leftHand.palm.Forward, headForward) <= -0.60)
            {
                if (rightHandVelocity.x >= 120)
                {
                    if (rightHand.IsPinched && (Vec3.Dot(rightHand.palm.Forward.Normalized, headForward.Normalized) >= 0.9)) {
                        SkyLogEvents.NewLog.Invoke();
                    }
                }

                if (rightHandVelocity.x <= -120)
                {
                    if (rightHand.IsPinched && (Vec3.Dot(rightHand.palm.Forward.Normalized, headForward.Normalized) >= 0.9)) {
                        SkyLogEvents.CloseLog.Invoke();
                    }
                }

                if (rightHandVelocity.y >= 120)
                {
                    if (rightHand.IsPinched && (Vec3.Dot(rightHand.palm.Forward.Normalized, headForward.Normalized) >= 0.9)) {
                        SkyLogEvents.CancelLog.Invoke();
                    }
                }
            }

            if (leftHandVelocity.x >= 100 && rightHandVelocity.x <= 100) {
                if (leftHand.IsPinched && rightHand.IsPinched) {
                    SkyLogEvents.ClearMarkers.Invoke();
                }
            }

            if (leftHand.IsGripped) {
                SkyLogEvents.CopyPlayerTimeCode.Invoke();
            }

            if (leftHandVelocity.x >= 120 && leftHandAnglesDegree.x <= -30)
            {
                SkyLogEvents.Tab.Invoke();
            }

            if (rightHandVelocity.y <= -140 && rightHandAnglesDegree.y <= 15)
            {
                //RightFastHand.Invoke();
            }
        }
    }
}
