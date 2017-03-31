package com.nui.android;

import android.util.Log;
import android.view.GestureDetector;
import android.view.MotionEvent;

import com.nui.android.activities.BaseActivity;
import com.nui.android.activities.Bboard;

import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by ericv on 11/2/2015.
 */
public class SwipeGestureListener extends GestureDetector.SimpleOnGestureListener {

    IServer server;
    boolean pinching = false;

    public SwipeGestureListener(IServer server){
        super();
        this.server = server;
    }

    boolean running = false;
    public void Stop(){
        running = false;
    }
    public void Start(){
        running = true;
    }

    public void Pinching(){
        if(!pinching) {
            pinching = true;
            Timer stopPinching = new Timer();
            TimerTask t = new TimerTask() {
                @Override
                public void run() {
                    pinching = false;
                }
            };
            stopPinching.schedule(t, 500);
        }
    }

    @Override
    public boolean onDown(MotionEvent event){
        return true;
    }

    @Override
    public boolean onFling(MotionEvent firstEvent, MotionEvent secondEvent, float vx, float vy){

        if(running) {
            if (!pinching) {
                float diff = firstEvent.getY() - secondEvent.getY();
                if (Bboard.instance != null){
                    if (diff >= 100f) {
                        server.SendData(new MobileGesture(Bboard.GetSelectedShape(), "Swipe", "Push", Bboard.instance.getRandomImageId()));
                        Log.d("SWIPE", "onFling() push " + Bboard.GetSelectedShape());
                    } else if (diff <= -100f) {
                        server.SendData(new MobileGesture(Bboard.GetSelectedShape(), "Swipe", "Pull", Bboard.instance.getRandomImageId()));
                        Log.d("SWIPE", "onFling() pull " + Bboard.GetSelectedShape());
                    }
                }
                    if (diff >= 100f) {
                    server.SendData(new MobileGesture(BaseActivity.GetSelectedShape(), "Swipe", "Push"));
                    Log.d("SWIPE", "onFling() push " + BaseActivity.GetSelectedShape());
                    } else if (diff <= -100f) {
                    server.SendData(new MobileGesture(BaseActivity.GetSelectedShape(), "Swipe", "Pull"));
                    Log.d("SWIPE", "onFling() pull " + BaseActivity.GetSelectedShape());
                    }
            }
        }
        return true;
    }

}
