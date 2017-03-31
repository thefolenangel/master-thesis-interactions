package com.nui.android;

import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by ericv on 10/14/2015.
 */
public abstract class SensorMonitor implements SensorEventListener {

    private static SensorManager manager;
    protected Sensor sensor;
    protected ArrayList<Sensor> sensors = new ArrayList<>();

    IServer server;

    protected long lastUpdate = 0;
    protected long lastUpdateThreshold = 100;

    public SensorMonitor(IServer server, Context context, int sensorType){
        this.server = server;
        manager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
        sensor = manager.getDefaultSensor(sensorType);
        manager.registerListener(this, sensor, SensorManager.SENSOR_DELAY_NORMAL);
    }

    public SensorMonitor(IServer server, Context context, int[] sensorsType){
        this.server = server;
        manager = (SensorManager) context.getSystemService(Context.SENSOR_SERVICE);
        for (int sensorType : sensorsType) {
            Sensor sensor = manager.getDefaultSensor(sensorType);
            sensors.add(sensor);
            manager.registerListener(this, sensor, SensorManager.SENSOR_DELAY_NORMAL);
        }
    }

    public void Pause(){
        manager.unregisterListener(this);
    }

    public void Resume(){
        if(sensor != null)
            manager.registerListener(this, sensor, SensorManager.SENSOR_DELAY_NORMAL);

        for(Sensor sensor : sensors) {
            manager.registerListener(this, sensor, SensorManager.SENSOR_DELAY_FASTEST);
        }
    }


}
