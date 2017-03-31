package com.nui.android;

/**
 * Created by ericv on 10/13/2015.
 */
public class MobileGesture {

    public MobileGesture(String shape, String type, String direction){
        Type = type;
        Shape = shape;
        Direction = direction;
    }

    public MobileGesture(String shape, String type, String direction, Integer imgid){
        Type = type;
        Shape = shape;
        Direction = direction;
        ImgID = imgid;
    }
    public MobileGesture(String shape, String type,  Integer imgid){
        this(shape, type, "", imgid);
    }

    public MobileGesture(String shape, String type){
        this(shape, type, "", 0);
    }

    public String Type;
    public String Shape;
    public String Direction;
    public Integer ImgID;

}
