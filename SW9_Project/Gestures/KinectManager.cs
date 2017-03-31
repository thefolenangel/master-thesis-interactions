using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Timers;
using System.Windows;

using DataSetGenerator;
using Point = System.Windows.Point;
using System.Windows.Media.Imaging;

namespace SW9_Project {
    class KinectManager {

        KinectSensor kinectSensor;
        IDrawingBoard board;
        Body[] bodies;
        KinectJointFilter filter;

        private Timer handChangeTimer;
        private double handChangeTime = 2; //seconds to wait for hand change

        public KinectManager(IDrawingBoard board) {
            
            this.board = board;
            filter = new KinectJointFilter(0.5f, 0.5f, 0.5f, 0.05f, 0.04f);
            //filter = new KinectJointFilter(0.7f, 0.3f, 1f, 1f, 1f);
            StartKinect();
            
        }

        private bool LeftHand = true;
        private HandState currentHandState = HandState.Open;
        private KinectGesture handGesture;
        
        MultiSourceFrameReader msfr;
        private bool StartKinect() {


            try {
                kinectSensor = KinectSensor.GetDefault();
                bodies = new Body[6];
                msfr = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body);
                msfr.MultiSourceFrameArrived += KinectManager_MultiSourceFrameArrived;
                kinectSensor.Open();
            }
            catch (Exception e) {
                Console.WriteLine("Could not start kinect! E: " + e.Message);
                return false;
            }

            //prepare the hand change timer
            handChangeTimer = new Timer();
            handChangeTimer.Interval = TimeSpan.FromSeconds(handChangeTime).TotalMilliseconds;
            handChangeTimer.Elapsed += HandChangeTimer_Elapsed;

            Timer initializeTime = new Timer();
            initializeTime.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            initializeTime.Elapsed += (sender, e) => {
                initialized = true;
                initializeTime.Dispose();
            };
            initializeTime.Start();

            return true;

        }

        public void Recalibrate() {
            initialized = false;
        }

        Queue<float> throwHandLocations = new Queue<float>();
        bool initialized = false;

        private void KinectManager_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e) {
            MultiSourceFrame msf = e.FrameReference.AcquireFrame();

            using (BodyFrame body = msf.BodyFrameReference.AcquireFrame()) {
                if (body != null) {

                    body.GetAndRefreshBodyData(bodies);

                    Body playerBody = (from b in bodies
                                       where b.IsTracked
                                       select b).FirstOrDefault();

                    if (playerBody != null) {
                        ParseBody(playerBody, (long)body.RelativeTime.TotalMilliseconds);
                    }
                }
            }

        }

        private bool HandStateChanged(HandState handstate) {
            if(handstate == HandState.Unknown) { return false; }
            if(handstate != currentHandState) {
                if(handstate == HandState.Open) {
                    currentHandState = handstate;
                    handGesture = new KinectGesture(GestureType.Pinch, GestureDirection.Push);
                    return true;
                } else if (handstate == HandState.Closed) {
                    currentHandState = handstate;
                    handGesture = new KinectGesture(GestureType.Pinch, GestureDirection.Pull);
                    return true;
                }
                return false;
            }
            return false;
        }

        private void HandChangeTimer_Elapsed(object sender, ElapsedEventArgs e) {
            LeftHand = !LeftHand;
        }

        float center = 0;
        Tuple<long, double> lastPoint = new Tuple<long, double>(0, 0);
        public void ParseBody(Body playerBody, long timeStamp) {

            filter.UpdateFilter(playerBody);
            var joints = filter.GetFilteredJoints();


            JointType pointerHand = LeftHand ? JointType.HandLeft : JointType.HandRight;
            JointType throwHand = LeftHand ? JointType.HandRight : JointType.HandLeft;

            var pointerLocation = joints[(int)pointerHand];
            var throwLocation = joints[(int)throwHand];

            var throwTrackingState = playerBody.Joints[throwHand].TrackingState;
            HandState handState = LeftHand ? playerBody.HandLeftState : playerBody.HandRightState;

            if (!initialized) {
                center = joints[(int)JointType.SpineShoulder].Y;
                LeftHand = joints[(int)JointType.HandLeft].Z < joints[(int)JointType.HandRight].Z;
                board.PointAt(pointerLocation.X, pointerLocation.Y - center);
                return;

            }

            bool t = joints[(int)JointType.HandLeft].Z < joints[(int)JointType.HandRight].Z;
            if (LeftHand != t) {
                if (!handChangeTimer.Enabled)
                {
                    handChangeTimer.Start();
                }
            }
            else {
                if (handChangeTimer.Enabled) {
                    handChangeTimer.Stop();
                }
            }

            KinectGesture throwGesture = IsThrowing(throwLocation.Z, timeStamp);

            if (throwGesture != null && 
                throwTrackingState == TrackingState.Tracked &&
                playerBody.Joints[pointerHand].Position.Y > playerBody.Joints[JointType.HipRight].Position.Y) {
                    GestureParser.AddKinectGesture(throwGesture);
            }
            

            if (HandStateChanged(handState)) {
                GestureParser.AddKinectGesture(handGesture);
            }
            board.PointAt(pointerLocation.X, pointerLocation.Y - center);
        }

        long lastThrowEvent = 0;

        private void CheckHandState(CameraSpacePoint[] joints) {

        }

        private KinectGesture IsThrowing(float currentLocation, long timestamp) {

            throwHandLocations.Enqueue(currentLocation);
            if (throwHandLocations.Count >= 15) {
                float initialPosition = throwHandLocations.Dequeue();

                if (timestamp - lastThrowEvent < 1000) { return null; }
                bool push = GestureParser.GetDirectionContext() == GestureDirection.Push ? true : false;
                if(initialPosition - currentLocation > 0.3 && push) {
                    lastThrowEvent = timestamp;
                    return new KinectGesture(GestureType.Throw, GestureDirection.Push);
                }
                else if(currentLocation - initialPosition > 0.3 && !push) {
                    lastThrowEvent = timestamp;
                    return new KinectGesture(GestureType.Throw, GestureDirection.Pull);
                }
            }

            return null;
        }
    }
}
