package com.nui.android;

import java.io.BufferedReader;
import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.net.Socket;
import java.io.PrintWriter;
import java.net.SocketOptions;
import java.security.spec.ECField;
import java.sql.Time;
import java.util.Timer;

import android.content.Context;
import android.support.v4.*;
import android.util.Log;
import android.os.Vibrator;


//JSON
import com.google.gson.Gson;
import com.nui.android.activities.BaseActivity;

/**
 * Created by Elias on 08-10-2015.
 */

public class Network implements IServer {

    String TAG = "Network";
    private static final String SERVER_IP = "192.168.1.2";

    Socket clientSocket;
    String host;
    int port;

    Gson gsonConverter;
    BaseActivity activity;

    PrintWriter out;

    private static Network instance;


    public static void initInstance(BaseActivity ba) {

        if (instance == null) {
            instance = new Network(SERVER_IP, 8000, ba);
        }
    }

    public static Network getInstance() {
        return instance;
    }

    public Network(String host, int port, BaseActivity activity){
        gsonConverter = new Gson();

        this.activity = activity;

        this.host = host;
        this.port = port;
        Thread connectionThread = new Thread(){
            public void run(){
                SetupConnection();
            }
        };
        connectionThread.start();
    }

    public void Reconnect() {
        Log.d("NETWORK", "Reconnecting...");
        Thread connectionThread = new Thread(){
            public void run(){
                SetupConnection();
            }
        };
        connectionThread.start();
    }

    private InetAddress HOST;
    private final int PORT = 49255;
    public DatagramSocket ds;

    private void SetupConnection() {
        Log.d("NETWORK", "Setting up connection...");
        /*try{
            if(activity.nt != null)
                synchronized(activity.nt) {
                    //activity.nt.wait();
                    HOST = InetAddress.getByName(host);
                    activity.dp.setAddress(HOST);
                    activity.dp.setPort(PORT);
                    //HOST = InetAddress.getByName("10.208.105.215");
                    ds = new DatagramSocket();
                    // InetAddress ia = InetAddress.getByName("192.168.1.255");
                    // ds.setBroadcast(true);
                    ds.connect(HOST, PORT);

                    Log.d("BaseActivity", "Socket is bound to " + String.valueOf(ds.getLocalPort()));
                    if(!activity.nt.isAlive()) {
                        activity.nt.start();
                    }
                }
        }catch(Exception e) {
            Log.d("Datagram connection", e.toString());
        }*/
        try {
            clientSocket = new Socket(host, port);
            out = new PrintWriter(clientSocket.getOutputStream(), true);
            StartListener((clientSocket.getInputStream()));
        } catch (Exception e) {
            if(e instanceof IOException || e instanceof EOFException){
                Log.d("NETWORK", "Could not set up connection");
                Reconnect();
            } else {
                Log.i(TAG, "Could not open a socket to " + host + ":" + port);
            }
        }
    }

    private void StartListener(InputStream inputStream){
        BufferedReader r = new BufferedReader(new InputStreamReader(inputStream));
        String line;
        try {
            while ((line = r.readLine()) != null) {
                if(line.equals("startpull")) {
                    activity.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    activity.StartPullTest();
                                }});
                }
                else if(line.equals("startpush")){
                    activity.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    activity.StartPushTest();
                                }});
                }
                else if(line.equals("nextshape:circle")){
                    activity.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    activity.SetShape("circle");
                                    //SendMessage("nextshape:" + activity.NextShape());
                                }
                            }
                    );
                }
                else if(line.equals("nextshape:square")){
                    activity.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    activity.SetShape("square");
                                    //SendMessage("nextshape:" + activity.NextShape());
                                }
                            }
                    );
                }
                else if(line.equals("ready")){
                    SendMessage("ready:"+activity.ReadyToStart());
                }
                else if(line.equals("switch")) {
                    // randomly switch the position of the figures
                    activity.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    activity.SwitchPosition();
                                }
                            }
                    );
                }
                else if(line.equals("pinch:pull")){
                    activity.AwaitingPullPinch(true);
                }
                else {
                    String[] lines = line.split(":");
                    if(lines.length > 1 && lines[0].equals("gesture")) {
                        activity.SetGesture(lines[1]);

                    }
                }
            }
        } catch(Exception e) {
            Log.e(TAG, e.getMessage());
        }
    }

    public void SendMessage(String message){
        try {
            out.println(message);
            out.flush();
        }catch (Exception e){
            Log.i(TAG, "Could not send message \"" + message + "\". Exception: " + e.getMessage());
            Reconnect();
        }
    }

    public void SendData(MobileGesture data){
        if(!activity.GetGesture().equalsIgnoreCase(data.Type))
            return;
        if (clientSocket == null)
            return;
        if(activity.PushOrPull() && data.Shape == null)
            return;
        if (!this.clientSocket.isConnected()) //should prevent some crashes if the network isn't connected.
            return;
        try {
            String t = gsonConverter.toJson(data);
            activity.ClearShapes();
            SendMessage(t);
        }
        catch (Exception e){
            Log.i("!", " " + e.getMessage()); // java.lang.NullPointerException: println needs a message
        }
    }


    public void Pause(){
        //TODO: IMPLEMENT
    }

    public void Resume(){
        //TODO: IMPLEMENT
    }

    public void FindServer(boolean unicast) {
        Thread UDPThread = new Thread(new UDPDiscover(activity,unicast));
        UDPThread.start();
    }

    public void SetHost(String host){
        this.host = host;
    }
    public String GetHost(){
        return host;
    }

}

