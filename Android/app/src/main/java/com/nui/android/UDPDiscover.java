package com.nui.android;

import android.content.Context;
import android.util.Log;
import android.widget.Toast;

import com.nui.android.activities.BaseActivity;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.net.SocketTimeoutException;
import java.util.Enumeration;

/**
 * Created by Elias on 21-11-2015.
 */
public class UDPDiscover implements Runnable {
    String TAG = "UDPDISCOVER";
    private String ipaddr;
    private int port;
    private boolean unicast;
    private BaseActivity pBase;

    //private Socket tSocket;
    public UDPDiscover(BaseActivity pBase, boolean unicast) {
        this.pBase = pBase;
        this.unicast = unicast;
    }

    public void run() {
        //Tell user that we are searching.
        pBase.runOnUiThread(
                new Runnable() {
                    @Override
                    public void run() {
                        Toast.makeText(pBase, "Searching...", Toast.LENGTH_SHORT).show();
                    }
                }
        );
        //This code has been altered to only run on the last 255 when unicasting.
        byte[] ipAddr;
        DatagramSocket c;
        boolean bfound = false;
        int bport = 49255; //this port is outside IANA registartion range, so probably won't annoy anyone.
        // Find the server using UDP broadcast
        try {
            //Open a random port to send the package

            c = new DatagramSocket(bport);
            c.setBroadcast(true);
            long startTime = System.nanoTime();
            byte[] sendData = "DISCOVER_IS903SEVER_REQUEST".getBytes();
            //byte[] sendData2 = "255Flood".getBytes();
            //Try the 255.255.255.255 first (seems to work)
            try {
                DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, InetAddress.getByName("255.255.255.255"), bport);
                c.send(sendPacket);
                System.out.println(getClass().getName() + ">>> Request packet sent to: 255.255.255.255 (DEFAULT)");
            } catch (Exception e) {
                Log.w(TAG, "255 fail: " + e.toString());
            }

            // Broadcast the message over all the network interfaces

            Enumeration interfaces = NetworkInterface.getNetworkInterfaces();
            while (interfaces.hasMoreElements()) {
                NetworkInterface networkInterface = (NetworkInterface) interfaces.nextElement();

                if (networkInterface.isLoopback() || !networkInterface.isUp()) {
                    continue; // Don't want to broadcast to the loopback interface
                }

                for (InterfaceAddress interfaceAddress : networkInterface.getInterfaceAddresses()) {
                    InetAddress broadcast = interfaceAddress.getBroadcast();

                    if (broadcast == null) {
                        continue;
                    }

                    ipAddr = broadcast.getAddress();

                    /*for(int i=1; i<255; i++) {
                        ipAddr[2] = (byte)i;
                        broadcast = InetAddress.getByAddress(ipAddr);*/

                    if(unicast){

                        for(int j=1; j<255; j++) {
                            ipAddr[3] = (byte) j;
                            broadcast = InetAddress.getByAddress(ipAddr);

                            // Send the broadcast package!
                            try {
                                DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, broadcast, bport);
                                c.send(sendPacket);
                            } catch (Exception e) {
                                Log.w(TAG, "Failed to send package to " + broadcast.toString());
                            }

                            System.out.println(getClass().getName() + ">>> Request packet sent to: " + broadcast.getHostAddress() + "; Interface: " + networkInterface.getDisplayName());
                        }
                    }
                    else{
                        // Send the broadcast package!
                        try {
                            DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, broadcast, bport);
                            c.send(sendPacket);
                        } catch (Exception e) {
                            Log.w(TAG, "Failed to send package to " + broadcast.toString());
                        }

                        System.out.println(getClass().getName() + ">>> Request packet sent to: " + broadcast.getHostAddress() + "; Interface: " + networkInterface.getDisplayName());

                    }
                    //}
                }
            }

            System.out.println(getClass().getName() + ">>> Done looping over all network interfaces. Now waiting for a reply!");
            Log.i(TAG, ">>> Done looping over all network interfaces. Now waiting for a reply!");
            long estimatedTime = System.nanoTime() - startTime;
            //System.out.println(getClass().getName() + ">>> Time elapsed: " + (estimatedTime/1000000000.0) + "seconds");
            Log.w(TAG,">>> Time elapsed: " + (estimatedTime/1000000000.0) + "seconds");
            //Wait for a response
            byte[] recvBuf = new byte[15000];
            c.setSoTimeout(5000);
            //DatagramPacket receivePacket = new DatagramPacket(recvBuf, recvBuf.length);

            boolean cLoop = true;
            long loopStart = System.nanoTime();
            while(cLoop) {
                DatagramPacket receivePacket = new DatagramPacket(recvBuf, recvBuf.length);

                try {
                    c.receive(receivePacket);
                }
                catch (SocketTimeoutException e) {
                    if ( ((System.nanoTime() - loopStart)/1000000000.0 ) > 5) {
                        cLoop = false;
                        System.out.println(getClass().getName() + ">>> SocketTimeOut");
                        Log.i(TAG, ">>> SocketTimeOut");
                        pBase.runOnUiThread(
                                new Runnable() {
                                    @Override
                                    public void run() {
                                        Toast.makeText(pBase, "Search timeout", Toast.LENGTH_SHORT).show();
                                    }
                                }
                        );
                        continue;
                    }
                    else{
                        continue;
                    }
                }
                //We have a response
                System.out.println(getClass().getName() + ">>> Broadcast response from server: " + receivePacket.getAddress().getHostAddress());
                Log.i(TAG, ">>> Broadcast response from server: " + receivePacket.getAddress().getHostAddress());
                //Check if the message is correct
                String message = new String(receivePacket.getData()).trim();
                Log.i(TAG," ->" + message);
                if (message.equals("DISCOVER_IS903SERVER_RESPONSE")) {
                    //DO SOMETHING WITH THE SERVER'S IP (for example, store it in your controller)
                    //Controller_Base.setServerIp(receivePacket.getAddress());
                    final String newIP = receivePacket.getAddress().getHostAddress();
                    pBase.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    Toast.makeText(pBase, "connecting: " + newIP, Toast.LENGTH_SHORT).show();
                                }
                            }
                    );
                    Network.getInstance().SetHost( newIP );
                    Network.getInstance().Reconnect();
                    //this.port = 8000; // can be implemented better.
                    cLoop = false;
                    bfound = true;
                }
                if ( ((System.nanoTime() - loopStart)/1000000000.0 ) > 5)
                {   cLoop = false;
                    //BaseActivity.PopText(pBase, "No server found.");
                    System.out.println(getClass().getName() + ">>> TimeOut");
                    Log.i(TAG, ">>> TimeOut");
                    pBase.runOnUiThread(
                            new Runnable() {
                                @Override
                                public void run() {
                                    Toast.makeText(pBase, "Search timeout", Toast.LENGTH_SHORT).show();
                                }
                            }
                    );
                }
            }

            //Close the port!
            c.close();
        } catch (Exception e) {
            Log.e(TAG, "UDP socket failed: " + e.toString());

        }
        Log.w(TAG, "Done broadcasting");
        if (bfound) {
            //BaseActivity.PopText(pBase,"Connecting...");
            //Network.getInstance().Reconnect();
            Log.w(TAG, "Network Setup complete.");
        }
    }
}
