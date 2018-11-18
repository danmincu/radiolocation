package com.commonsware.android.job.CellSites;

import android.os.Build.VERSION;
import android.os.Parcel;
import android.os.Parcelable;
import android.telephony.CellLocation;
import android.telephony.NeighboringCellInfo;
import android.telephony.cdma.CdmaCellLocation;
import android.telephony.gsm.GsmCellLocation;

import org.json.JSONException;
import org.json.JSONObject;

public class CellInfo implements Parcelable {
    public static final String CELL_RADIO_CDMA = "cdma";
    public static final String CELL_RADIO_GSM = "gsm";
    public static final String CELL_RADIO_LTE = "lte";
    public static final String CELL_RADIO_UNKNOWN = "";
    public static final String CELL_RADIO_WCDMA = "wcdma";
    public static final Creator<CellInfo> CellCreator = new CellCreator();
    public static final int UNKNOWN_ASU = -1;
    public static final int UNKNOWN_CID = -1;
    public static final int UNKNOWN_LAC = -1;
    public static final int UNKNOWN_PCI = -1;
    public static final int UNKNOWN_PSC = -1;
    public static final int UNKNOWN_SIGNAL_STRENGTH = -1000;
    private int mAsu;
    private String mCellRadio;
    private int mCid;
    private int mLac;
    private int mMcc;
    private int mMnc;
    private int mPscPci;
    private int mSignalStrength;
    private int mTa;
    private int mSimpleLevel;
    private byte isRegistered;

    /* renamed from: com.unwiredlabs.mobilesignaldebugger.Scanners.CellInfo$1 */
    static class CellCreator implements Creator<CellInfo> {

        public CellInfo createFromParcel(Parcel in) {
            return new CellInfo(in);
        }

        public CellInfo[] newArray(int size) {
            return new CellInfo[size];
        }
    }

    protected CellInfo(Parcel in) {
        this.mCellRadio = in.readString();
        this.mMcc = in.readInt();
        this.mMnc = in.readInt();
        this.mCid = in.readInt();
        this.mLac = in.readInt();
        this.mSignalStrength = in.readInt();
        this.mAsu = in.readInt();
        this.mTa = in.readInt();
        this.mPscPci = in.readInt();
        this.mSimpleLevel = in.readInt();
        this.isRegistered = in.readByte();
    }

    public CellInfo(PCellInfo pCellInfo) {
        this.mCellRadio = pCellInfo.cellRadio;
        this.mMcc = pCellInfo.mcc;
        this.mMnc = pCellInfo.mnc;
        this.mCid = pCellInfo.cid;
        this.mLac = pCellInfo.lac;
        this.mSignalStrength = pCellInfo.signalStrength;
        this.mAsu = pCellInfo.mAsu;
        this.mTa = pCellInfo.mTa;
        this.mPscPci = pCellInfo.pscPci;
        this.mSimpleLevel = pCellInfo.simpleLevel;
        this.isRegistered = pCellInfo.isRegistered;
    }

    public static final Creator<CellInfo> CREATOR = new Creator<CellInfo>() {
        @Override
        public CellInfo createFromParcel(Parcel in) {
            return new CellInfo(in);
        }

        @Override
        public CellInfo[] newArray(int size) {
            return new CellInfo[size];
        }
    };

    public void setSignalStrength(int signalStrength) {
        this.mSignalStrength = signalStrength;
    }

    public int getSignalStrength() {
        return this.mSignalStrength;
    }

    public CellInfo() {
        reset();
    }

    public CellInfo(NeighboringCellInfo nci, String networkOperator) {
        reset();
        this.mCellRadio = getCellRadioTypeName(nci.getNetworkType());
        setNetworkOperator(networkOperator);
        if (nci.getLac() >= 0) {
            this.mLac = nci.getLac();
        }
        if (nci.getCid() >= 0) {
            this.mCid = nci.getCid();
        }
        if (nci.getPsc() >= 0) {
            this.mPscPci = nci.getPsc();
        }
        if (nci.getRssi() != 99) {
            this.mSignalStrength = nci.getRssi();
        }
    }

    static String getCellRadioTypeName(int networkType) {
        switch (networkType) {
            case 1:
            case 2:
                return CELL_RADIO_GSM;
            case 3:
            case 8:
            case 9:
            case 10:
            case 15:
                return CELL_RADIO_WCDMA;
            case 5:
            case 6:
            case 7:
            case 11:
            case 12:
            case 14:
                return CELL_RADIO_CDMA;
            case 13:
                return CELL_RADIO_LTE;
            default:
                return "";
        }
    }

    private static String getRadioTypeName(int phoneType) {
        switch (phoneType) {
            case 1:
                return CELL_RADIO_GSM;
            case 2:
                return CELL_RADIO_CDMA;
            default:
                return "";
        }
    }

    public boolean isCellRadioValid() {
        return (this.mCellRadio == null || this.mCellRadio.length() <= 0 || this.mCellRadio.equals("0")) ? false : true;
    }

    public String getCellRadio() {
        return this.mCellRadio;
    }

    public int getMcc() {
        return this.mMcc;
    }

    public int getMnc() {
        return this.mMnc;
    }

    public int getCid() {
        return this.mCid;
    }

    public int getLac() {
        return this.mLac;
    }

    public int getPsc() {
        return this.mPscPci;
    }

    public int getAsu() {
        return this.mAsu;
    }

    public int getSimpleLevel() { return  this.mSimpleLevel; }

    public byte getIsRegistered() { return this.isRegistered; }

    public void setIsRegistered(byte value) { this.isRegistered = value; }

    public JSONObject toJSONObject() {
        JSONObject obj = new JSONObject();
        try {
            obj.put("radio", getCellRadio());
            obj.put("cid", this.mCid);
            obj.put("lac", this.mLac);
            obj.put("mcc", this.mMcc);
            obj.put("mnc", this.mMnc);
            obj.put("simpleLevel", this.mSimpleLevel);
            obj.put("isRegistered", this.isRegistered);
            if (this.mSignalStrength != UNKNOWN_SIGNAL_STRENGTH) {
                obj.put("signal", this.mSignalStrength);
            }
            if (this.mTa != -1) {
                obj.put("timingAdvance", this.mTa);
            }
            if (this.mPscPci != -1) {
                obj.put("psc", this.mPscPci);
            }
            if (this.mAsu != -1) {
                obj.put("asu", this.mAsu);
            }
            return obj;
        } catch (JSONException jsonE) {
            throw new IllegalStateException(jsonE);
        }
    }

    public String getCellIdentity() {
        return getCellRadio() + " " + getMcc() + " " + getMnc() + " " + getLac() + " " + getCid() + " " + getPsc();
    }

    public int describeContents() {
        return 0;
    }

    public void writeToParcel(Parcel dest, int flags) {
        dest.writeString(this.mCellRadio);
        dest.writeInt(this.mMcc);
        dest.writeInt(this.mMnc);
        dest.writeInt(this.mCid);
        dest.writeInt(this.mLac);
        dest.writeInt(this.mSignalStrength);
        dest.writeInt(this.mAsu);
        dest.writeInt(this.mTa);
        dest.writeInt(this.mPscPci);
        dest.writeInt(this.mSimpleLevel);
        dest.writeByte(this.isRegistered);
    }

    void reset() {
        this.mCellRadio = CELL_RADIO_GSM;
        this.mMcc = -1;
        this.mMnc = -1;
        this.mLac = -1;
        this.mCid = -1;
        this.mSignalStrength = UNKNOWN_SIGNAL_STRENGTH;
        this.mAsu = -1;
        this.mTa = -1;
        this.mPscPci = -1;
        this.mSimpleLevel = -1;
        this.isRegistered = 0;
    }

    void setCellLocation(CellLocation cl, int networkType, String networkOperator, Integer signalStrength) {
        if (cl instanceof GsmCellLocation) {
            GsmCellLocation gcl = (GsmCellLocation) cl;
            reset();
            this.mCellRadio = getCellRadioTypeName(networkType);
            setNetworkOperator(networkOperator);
            int lac = gcl.getLac();
            int cid = gcl.getCid();
            if (lac >= 0) {
                this.mLac = lac;
            }
            if (cid >= 0) {
                this.mCid = cid;
            }
            if (VERSION.SDK_INT >= 9) {
                int psc = gcl.getPsc();
                if (psc >= 0) {
                    this.mPscPci = psc;
                }
            }
            if (signalStrength != null) {
                this.mSignalStrength = signalStrength.intValue();
            }
        } else if (cl instanceof CdmaCellLocation) {
            CdmaCellLocation cdl = (CdmaCellLocation) cl;
            reset();
            this.mCellRadio = getCellRadioTypeName(networkType);
            setNetworkOperator(networkOperator);
            this.mMnc = cdl.getSystemId();
            this.mLac = cdl.getNetworkId();
            this.mCid = cdl.getBaseStationId();
            if (signalStrength != null) {
                this.mSignalStrength = signalStrength.intValue();
            }
        } else {
            throw new IllegalArgumentException("Unexpected CellLocation type: " + cl.getClass().getName());
        }
    }

    public void setGsmCellInfo(int mcc, int mnc, int lac, int cid, int asu, int mSignalStrength, int simpleLevel) {
        this.mCellRadio = CELL_RADIO_GSM;
        if (mcc == Integer.MAX_VALUE) {
            mcc = -1;
        }
        this.mMcc = mcc;
        if (mnc == Integer.MAX_VALUE) {
            mnc = -1;
        }
        this.mMnc = mnc;
        if (lac == Integer.MAX_VALUE) {
            lac = -1;
        }
        this.mLac = lac;
        if (cid == Integer.MAX_VALUE) {
            cid = -1;
        }
        this.mCid = cid;
        this.mAsu = asu;
        this.mSignalStrength = mSignalStrength;
    }

    public void setWcdmaCellInfo(int mcc, int mnc, int lac, int cid, int psc, int asu, int signalStrength) {
        this.mCellRadio = CELL_RADIO_WCDMA;
        if (mcc == Integer.MAX_VALUE) {
            mcc = -1;
        }
        this.mMcc = mcc;
        if (mnc == Integer.MAX_VALUE) {
            mnc = -1;
        }
        this.mMnc = mnc;
        if (lac == Integer.MAX_VALUE) {
            lac = -1;
        }
        this.mLac = lac;
        if (cid == Integer.MAX_VALUE) {
            cid = -1;
        }
        this.mCid = cid;
        if (psc == Integer.MAX_VALUE) {
            psc = -1;
        }
        this.mPscPci = psc;
        this.mAsu = asu;
        this.mSignalStrength = signalStrength;
    }

    public void setLteCellInfo(int mcc, int mnc, int ci, int psc, int lac, int asu, int signalStrength, int ta, int simpleLevel) {
        this.mCellRadio = CELL_RADIO_LTE;
        if (mcc == Integer.MAX_VALUE) {
            mcc = -1;
        }
        this.mMcc = mcc;
        if (mnc == Integer.MAX_VALUE) {
            mnc = -1;
        }
        this.mMnc = mnc;
        if (lac == Integer.MAX_VALUE) {
            lac = -1;
        }
        this.mLac = lac;
        if (ci == Integer.MAX_VALUE) {
            ci = -1;
        }
        this.mCid = ci;
        if (psc == Integer.MAX_VALUE) {
            psc = -1;
        }
        this.mPscPci = psc;
        this.mAsu = asu;
        this.mSignalStrength = signalStrength;
        this.mTa = ta;
        this.mSimpleLevel = simpleLevel;
    }

    void setCdmaCellInfo(int baseStationId, int networkId, int systemId, int dbm, int simpleLevel) {
        this.mCellRadio = CELL_RADIO_CDMA;
        if (systemId == Integer.MAX_VALUE) {
            systemId = -1;
        }
        this.mMnc = systemId;
        if (networkId == Integer.MAX_VALUE) {
            networkId = -1;
        }
        this.mLac = networkId;
        if (baseStationId == Integer.MAX_VALUE) {
            baseStationId = -1;
        }
        this.mCid = baseStationId;
        this.mSignalStrength = dbm;
        this.mSimpleLevel = simpleLevel;
    }

    void setNetworkOperator(String mccMnc) {
        if (mccMnc == null || mccMnc.length() < 5 || mccMnc.length() > 8) {
            throw new IllegalArgumentException("Bad mccMnc: " + mccMnc);
        }
        this.mMcc = Integer.parseInt(mccMnc.substring(0, 3));
        this.mMnc = Integer.parseInt(mccMnc.substring(3));
    }

    public boolean equals(Object o) {
        if (o == this) {
            return true;
        }
        if (!(o instanceof CellInfo)) {
            return false;
        }
        CellInfo ci = (CellInfo) o;
        if (this.mCellRadio.equals(ci.mCellRadio) &&
                this.mMcc == ci.mMcc &&
                this.mMnc == ci.mMnc &&
                this.mCid == ci.mCid &&
                this.mLac == ci.mLac &&
                this.mSignalStrength == ci.mSignalStrength &&
                this.mAsu == ci.mAsu &&
                this.mTa == ci.mTa &&
                this.mPscPci == ci.mPscPci &&
                this.isRegistered == ci.isRegistered) {
            return true;
        }
        return false;
    }

    public int hashCode() {
        return ((((((((((((((((this.mCellRadio.hashCode() + 527) * 31) + this.mMcc) * 31) + this.mMnc) * 31) + this.mCid) * 31) + this.mLac) * 31) + this.mSignalStrength) * 31) + this.mAsu) * 31) + this.mTa) * 31) + this.mPscPci;
    }

    public static String header()
    {
        return "Radio,Mcc,Mnc,Cid,Lac,SignalS,Level,mAsu,mTa,PscPci,isReg";
    }

    public String toString() {
        String mta = (this.mTa == Integer.MAX_VALUE) ? "max" : ((Integer)this.mTa).toString();
        return this.mCellRadio + "," +
                this.mMcc + "," +
                this.mMnc + "," +
                this.mCid + "," +
                this.mLac + "," +
                this.mSignalStrength + "," +
                this.mSimpleLevel + "," +
                this.mAsu + "," +
                mta + "," +
                this.mPscPci + "," +
                this.isRegistered;
    }

    public String getTowerId() {
        return "" + getCid() + ":" + getLac() + ":" + getPsc();
    }
}
