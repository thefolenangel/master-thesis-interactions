package com.nui.android;

import android.util.Log;
import android.view.GestureDetector;
import android.view.MotionEvent;

import com.nui.android.activities.BaseActivity;
import com.nui.android.activities.Bboard;

/**
 * Created by bml on 14-11-2015.
 */
public class TouchGestureListener extends GestureDetector.SimpleOnGestureListener {
    IServer server;

    public TouchGestureListener(IServer server) {
        super();
        this.server = server;
    }

    @Override
    public boolean onSingleTapUp(MotionEvent event) {
        if (Bboard.instance != null) {
            Log.d("TOUCH", "onTouch() " + Bboard.GetSelectedShape());
            server.SendData(new MobileGesture(Bboard.GetSelectedShape(), "Pinch", "Pull", Bboard.instance.getRandomImageId()));
            return true;
        } else {
            Log.d("TOUCH", "onTouch() " + BaseActivity.GetSelectedShape());
            server.SendData(new MobileGesture(BaseActivity.GetSelectedShape(), "Pinch", "Pull"));
            return true;
        }
    }
}
