package com.commonsware.android.job.CellSites;

import android.os.Parcel;
import android.os.Parcelable;
import android.telephony.NeighboringCellInfo;

public class PCellInfo implements Parcelable {
    public static final Creator<PCellInfo> CREATOR = new PCellInfoCreator();
    private static final String name = Namer.getName(PCellInfo.class);
    public String cellRadio;
    public int mcc;
    public int mnc;
    public int cid;
    public int lac;
    int signalStrength;
    public int mAsu;
    int mTa;
    public int pscPci;
    public int simpleLevel;
    public byte isRegistered;

    static class PCellInfoCreator implements Creator<PCellInfo> {
        PCellInfoCreator() {
        }

        public final /* synthetic */ PCellInfo createFromParcel(Parcel parcel) {
            return new PCellInfo(parcel);
        }

        public final /* bridge */ /* synthetic */ PCellInfo[] newArray(int i) {
            return new PCellInfo[i];
        }
    }

    public PCellInfo() {
        defaultPCellInfo();
    }

    protected PCellInfo(Parcel parcel) {
        this.cellRadio = parcel.readString();
        this.mcc = parcel.readInt();
        this.mnc = parcel.readInt();
        this.cid = parcel.readInt();
        this.lac = parcel.readInt();
        this.signalStrength = parcel.readInt();
        this.mAsu = parcel.readInt();
        this.mTa = parcel.readInt();
        this.pscPci = parcel.readInt();
        this.simpleLevel = parcel.readInt();
        this.isRegistered = parcel.readByte();
    }

    public PCellInfo(NeighboringCellInfo neighboringCellInfo, String str) {
        defaultPCellInfo();
        this.cellRadio = PCellInfo.determineNetworkType(neighboringCellInfo.getNetworkType());
        validateAndExtractMccMnc(str);
        if (neighboringCellInfo.getLac() >= 0) {
            this.lac = neighboringCellInfo.getLac();
        }
        if (neighboringCellInfo.getCid() >= 0) {
            this.cid = neighboringCellInfo.getCid();
        }
        if (neighboringCellInfo.getPsc() >= 0) {
            this.pscPci = neighboringCellInfo.getPsc();
        }
        if (neighboringCellInfo.getRssi() != 99) {
            this.signalStrength = neighboringCellInfo.getRssi();
        }
    }

    static String determineNetworkType(int i) {
        switch (i) {
            case 1:
            case 2:
                return "gsm";
            case 3:
            case 8:
            case 9:
            case 10:
            case 15:
                return "wcdma";
            case 5:
            case 6:
            case 7:
            case 11:
            case 12:
            case 14:
                return "cdma";
            case 13:
                return "lte";
            default:
                IllegalArgumentException illegalArgumentException = new IllegalArgumentException("Unexpected network type: " + i);
                return "";
        }
    }

    final void defaultPCellInfo() {
        this.cellRadio = "gsm";
        this.mcc = -1;
        this.mnc = -1;
        this.lac = -1;
        this.cid = -1;
        this.signalStrength = CellInfo.UNKNOWN_SIGNAL_STRENGTH;
        this.mAsu = -1;
        this.mTa = -1;
        this.pscPci = -1;
        this.simpleLevel = -1;
        this.isRegistered = 0;
    }

    final void validateAndExtractMccMnc(String str) {
        if (str == null || str.length() < 5 || str.length() > 8) {
            throw new IllegalArgumentException("Bad mccMnc: " + str);
        }
        this.mcc = Integer.parseInt(str.substring(0, 3));
        this.mnc = Integer.parseInt(str.substring(3));
    }

    public int describeContents() {
        return 0;
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof PCellInfo)) {
            return false;
        }
        PCellInfo PCellInfo = (PCellInfo) o;
        return this.cellRadio.equals(PCellInfo.cellRadio) &&
               this.mcc == PCellInfo.mcc &&
               this.mnc == PCellInfo.mnc &&
               this.cid == PCellInfo.cid &&
               this.lac == PCellInfo.lac &&
               this.signalStrength == PCellInfo.signalStrength &&
               this.mAsu == PCellInfo.mAsu &&
               this.mTa == PCellInfo.mTa &&
               this.pscPci == PCellInfo.pscPci &&
               this.simpleLevel == PCellInfo.simpleLevel &&
               this.isRegistered == PCellInfo.isRegistered;

    }

    public int hashCode() {
        return ((((((((((((((((this.cellRadio.hashCode() + 527) * 31) + this.mcc) * 31) + this.mnc) * 31) + this.cid) * 31) + this.lac) * 31) + this.signalStrength) * 31) + this.mAsu) * 31) + this.mTa) * 31) + this.pscPci;
    }

    public static String header() {
        return "Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg";

    }

    public String toString() {
        String mta = (this.mTa == Integer.MAX_VALUE) ? "max" : ((Integer)this.mTa).toString();

        return this.cellRadio + ','
                + this.mcc + ","
                + this.mnc + ","
                + this.cid + ","
                + this.lac + ","
                + this.signalStrength + ","
                + this.simpleLevel + ","
                + this.mAsu + "," +
                mta + ","
                + this.pscPci + ","
                + this.isRegistered;
    }

    public void writeToParcel(Parcel dest, int i) {
        dest.writeString(this.cellRadio);
        dest.writeInt(this.mcc);
        dest.writeInt(this.mnc);
        dest.writeInt(this.cid);
        dest.writeInt(this.lac);
        dest.writeInt(this.signalStrength);
        dest.writeInt(this.mAsu);
        dest.writeInt(this.mTa);
        dest.writeInt(this.pscPci);
        dest.writeInt(this.simpleLevel);
        dest.writeByte(this.isRegistered);
    }
}