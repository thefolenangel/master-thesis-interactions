package com.nui.android.activities;

import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.support.v4.content.ContextCompat;
import android.support.v4.view.GestureDetectorCompat;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.ScaleGestureDetector;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.ImageView;

import com.nui.android.AccelerometerMonitor;
import com.nui.android.Network;
import com.nui.android.PinchGestureListener;
import com.nui.android.R;
import com.nui.android.Shape;
import com.nui.android.SwipeGestureListener;
import com.nui.android.TouchGestureListener;

import java.lang.reflect.Array;
import java.net.DatagramPacket;
import java.util.ArrayDeque;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Random;

public class Bboard extends BaseActivity {
    //private Network network;
    public static Bboard instance = null;
    GestureDetectorCompat swipeDetector;
    ScaleGestureDetector pinchDetector;
    GestureDetectorCompat touchDetector;
    private AccelerometerMonitor acceloremeterSensor;

    private SwipeGestureListener swipeGestureListener;
    private PinchGestureListener pinchGestureListener;
    private TouchGestureListener touchGestureListener;

    public static String shape;
    public static String nextShape;
    public  String randomImage;
    public  String randomImageStroked;
    public ArrayDeque<String> randomDrawablePool;
    //TODO ImageView - exchange (data?) with Document and Image
    private ImageView mImageView;
    private ImageView DocumentView;

    private ImageView pullShape;
    private Button moveCursor;
    private boolean sendGyroData = true;

    private final Random random = new Random();
    private int count;
    private static int MAX_COUNT = 2;
    private boolean menuActive = true;
    public boolean calibrated = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        instance = this;
        setContentView(R.layout.activity_bboard);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);

        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_FULLSCREEN
                | View.SYSTEM_UI_FLAG_IMMERSIVE);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);


        initNetwork();
        swipeGestureListener = new SwipeGestureListener(Network.getInstance());
        pinchGestureListener = new PinchGestureListener(Network.getInstance());
        touchGestureListener =  new TouchGestureListener(Network.getInstance());
        acceloremeterSensor = new AccelerometerMonitor(Network.getInstance(), this);
        swipeDetector = new GestureDetectorCompat(this, swipeGestureListener);
        pinchDetector = new ScaleGestureDetector(this, pinchGestureListener);
        touchDetector = new GestureDetectorCompat(this, touchGestureListener);


        pullShape = (ImageView) findViewById(R.id.pull_shape);
        DocumentView = (ImageView) findViewById(R.id.document);
        mImageView = (ImageView) findViewById(R.id.image);
        pullShape.setVisibility(View.INVISIBLE);
        DocumentView.setVisibility(View.INVISIBLE);
        mImageView.setVisibility(View.INVISIBLE);
        count = 0;
        randomDrawablePool = new ArrayDeque<String>();
        RandomDrawableImage();

    }

    private boolean pushOrPull;

    private boolean gyroRunning = false;
    //TODO Overide and empty - making sure no gyro is used to avoid power drain.
    private void InitGyroScope(){

        sm = (SensorManager) getSystemService(SENSOR_SERVICE);
        // ODO provide support for gyroscope (rotation vector is flawed in early
        // versions of android)
        rv = sm.getDefaultSensor(Sensor.TYPE_GYROSCOPE);

        gyroRunning = true;

        // network thread
        nt = new Thread(new Runnable() {
            @Override
            public void run() {
                long lt = 0;

                while (true) {
                    if (end_nt) {
                        Log.d("BaseActivity", "Network thread ends.");
                        break;
                    }

                    if (sendGyroData && rv_sel.getLatestTimestamp() > lt) {
                        try {
                            Network.getInstance().ds.send(dp);
                            lt = rv_sel.getLatestTimestamp();
                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    }
                }
            }
        }, "UdpThread");

        nt.setPriority(Thread.MIN_PRIORITY);
        if(Network.getInstance().ds != null) {
            nt.start();
        }else {
            Network.getInstance().Reconnect();
        }
        // ODO rewrite the sensor acquisition with NDK
        rv_sel = new RotationVectorListener();

    }
    //TODO should just use SUPER
    public boolean PushOrPull(){
        //True = push, false = pull
        return pushOrPull;
    }
    //TODO should just use SUPER
    protected void initNetwork()
    {
        Network.initInstance(this);
    }
    //TODO not needed for fieldtest 1, return true?
//    public void StartPullTest(){
//        pushOrPull = false;
//        circleView.setVisibility(View.INVISIBLE);
//        squareView.setVisibility(View.INVISIBLE);
//        pullShape.setVisibility(View.VISIBLE);
//
//        pullShape.setOnTouchListener(new View.OnTouchListener() {
//            @Override
//            public boolean onTouch(View v, MotionEvent event) {
//
//                touchDetector.onTouchEvent(event);
//                swipeDetector.onTouchEvent(event);
//                pinchDetector.onTouchEvent(event);
//
//                return true;
//            }
//
//        });
//    }

    String gesture = "";
    public void SetGesture(String gesture){
        this.gesture = gesture;
        sendGyroData = false;
        pinchGestureListener.Stop();
        swipeGestureListener.Stop();
        switch (gesture){
            case "tilt": case "throw": acceloremeterSensor.SetTiltorThrow(gesture); break;
            case "swipe":{
                swipeGestureListener.Start();
                sendGyroData = true;
                break;
            }
            case "pinch": {
                pinchGestureListener.Start();
                break;
            }
            default: break;
        }
    }

    public String GetGesture(){
        return gesture;
    }


    public void StartPushTest(){

        pushOrPull = true;
        ClearShapes();
        pullShape.setVisibility(View.INVISIBLE);
        DocumentView.setVisibility(View.VISIBLE);
        mImageView.setVisibility(View.VISIBLE);


        DocumentView.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                shape = Shape.Document;
                int imageResource = getResources().getIdentifier(randomImage, null, getPackageName());

                touchDetector.onTouchEvent(event);
                swipeDetector.onTouchEvent(event);
                pinchDetector.onTouchEvent(event);

                if(event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE) {
                    DocumentView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.document_stroke));
                    mImageView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), imageResource));
                }

                return true;
            }

        });

        mImageView.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                shape = Shape.Image;
                int imageResourceStroke = getResources().getIdentifier(randomImageStroked, null, getPackageName());

                touchDetector.onTouchEvent(event);
                swipeDetector.onTouchEvent(event);
                pinchDetector.onTouchEvent(event);


                if(event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE) {
                    mImageView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), imageResourceStroke));
                    DocumentView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.document));
                }

                return true;
            }

        });
    }

    boolean pullPinchWaiting = false;
    public void AwaitingPullPinch(boolean waiting){
        pullPinchWaiting = waiting;
    }

    public void ClearShapes(){
        shape = null;
        int imageResource = getResources().getIdentifier(randomImage, null, getPackageName());
        DocumentView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.document));
        mImageView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), imageResource));
    }
    //TODO overide, need to use different shape views.
    // can perhaps use super if image data can be overrided by the bboard xml file.
    public void SwitchPosition() {
        RandomDrawableImage();
        ClearShapes();
        if(count > MAX_COUNT || random.nextBoolean()) {
            count = 0;
            int TopShapeTop = DocumentView.getTop();
            int TopShapeBottom = DocumentView.getBottom();
            int BottomShapeTop = mImageView.getTop();
            int BottomShapeBottom = mImageView.getBottom();

            DocumentView.setTop(BottomShapeTop);
            DocumentView.setBottom(BottomShapeBottom);
            mImageView.setTop(TopShapeTop);
            mImageView.setBottom(TopShapeBottom);
        } else {
            count++;
        }
    }

    public void SetShape(String shape) {
        ClearShapes();

        if(shape.equals("circle")) {
            pullShape.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.circle));
        }
        else {
            pullShape.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.square));
        }
    }

    public boolean ReadyToStart(){
        return true;
    }

    public static String GetSelectedShape(){
        return shape;
    }

    public void CloseApp(){
        this.finish();
        Intent intent = new Intent(Intent.ACTION_MAIN);
        intent.addCategory(Intent.CATEGORY_HOME);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        startActivity(intent);
    }

    // fixed by bjarke
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {

        menu.add(Menu.NONE, R.id.network_discovery, Menu.NONE, R.string.network_discovery);
        menu.add(Menu.NONE, R.id.reconnect_action, Menu.NONE, R.string.reconnect_action);
        menu.add(Menu.NONE, R.id.resetGyro, Menu.NONE, R.string.resetGyro);
        menu.add(Menu.NONE, R.id.close_app_action, Menu.NONE, R.string.close_app_action);

        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.network_discovery:
                Network.getInstance().FindServer(true);
                return true;
            case R.id.reconnect_action:
                Network.getInstance().Reconnect();
                return true;
            case R.id.resetGyro:
                calibrated = !calibrated;
                rv_sel.countX = 0;
                rv_sel.countZ = 0;
                Network.getInstance().SendMessage("resetgyro");
                return true;
            case R.id.close_app_action:
                //android.os.Process.killProcess(android.os.Process.myPid());
                CloseApp();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    @Override
    protected void onPause(){
        super.onPause();
        acceloremeterSensor.Pause();
        if(gyroRunning){
            sm.unregisterListener(rv_sel);
        }
        Network.getInstance().Pause();
    }

    @Override
    protected void onResume(){
        super.onResume();
        Network.getInstance().Resume();
        acceloremeterSensor.Resume();
        if(gyroRunning) {
            sm.registerListener(rv_sel, rv, SensorManager.SENSOR_DELAY_GAME);
        }
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (nt.isAlive()) {
            end_nt = true;
        }
    }

    @Override
    protected void onStop() {
        super.onStop();
    }

    private SensorManager sm;
    private Sensor rv;
    private RotationVectorListener rv_sel;
    private byte[] msg = new byte[100];
    public DatagramPacket dp = new DatagramPacket(msg, msg.length);
    public Thread nt;
    private boolean end_nt;

    class RotationVectorListener implements SensorEventListener {
        private long time = 0;
        private float calibrateZ = 0;
        private float calibrateX = 0;
        private float calibrateY = 0;

        private float virtualX = 0;
        private float virtualY = 0;
        private float virtualZ = 0;
        //		private String info_text;
        public float countX = 0;
        public float countZ = 0;

        public long getLatestTimestamp() {
            return time;
        }

        @Override
        public void onSensorChanged(SensorEvent event) {
            if(time == 0)
                time = event.timestamp;

            float x = event.values[0];
            float y = event.values[1];
            float z = event.values[2];

            countX += x;
            countZ += z;

            if(!calibrated){
                calibrateZ = z;
                calibrateX = x;
                calibrateY = y;
                calibrated = true;
            }

            virtualX = x-calibrateX;
            virtualY = y-calibrateY;
            virtualZ = z-calibrateZ;

            //Log.d("Gyro: ", "X: " + x + " Y: " + y + " Z: " + z);
            byte[] buf = ("gyrodata:time:"+ event.timestamp +":x:"+countX+":y:"+y+":z:"+countZ).getBytes();
            dp.setData(buf);
            time = event.timestamp;
        }

        @Override
        public void onAccuracyChanged(Sensor sensor, int accuracy) {
            // TODO Auto-generated method stub

        }
    }

    @Override
    public void onBackPressed() {
        if(menuActive)
            super.onBackPressed();
    }
    @Override
    public boolean onPrepareOptionsMenu (Menu menu) {
        return menuActive;
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        int action = event.getAction();
        int keyCode = event.getKeyCode();
        switch (keyCode) {
            case KeyEvent.KEYCODE_VOLUME_UP:
                if (action == KeyEvent.ACTION_DOWN) {
                    menuActive = !menuActive;
                }
                return true;
            default:
                return super.dispatchKeyEvent(event);
        }
    }

    public void RandomDrawableImage(){
        if (randomDrawablePool.isEmpty()){
            PopulateRandomDrawable();
            RandomDrawableImage();
        }else{
            randomImage = randomDrawablePool.removeFirst();
            randomImageStroked = randomImage+"_stroke";
        }
    }

    public int getRandomImageId (){
        if (shape == null ||  shape.equals(Shape.Document)) {
            return 0;
        }
        return getResources().getIdentifier(randomImage, null, getPackageName());
    }

    public void  PopulateRandomDrawable(){
        List<String> random = new ArrayList<>();
        Collections.addAll(random,"drawable/cat","drawable/flower","drawable/sky","drawable/temple" ,"drawable/tiger", "drawable/hearth", "drawable/mery","drawable/weight","drawable/church","drawable/batman");
        Collections.shuffle(random);
        for (String s : random) {
            randomDrawablePool.add(s);
        }
    }

}
