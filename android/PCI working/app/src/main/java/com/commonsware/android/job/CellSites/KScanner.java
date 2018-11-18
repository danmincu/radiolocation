package com.commonsware.android.job.CellSites;

import android.annotation.SuppressLint;
import android.annotation.TargetApi;
import android.content.Context;
import android.os.Build.VERSION;
import android.provider.Settings;
import android.telephony.CellIdentityCdma;
import android.telephony.CellIdentityGsm;
import android.telephony.CellIdentityLte;
import android.telephony.CellIdentityWcdma;
import android.telephony.CellInfoCdma;
import android.telephony.CellInfoGsm;
import android.telephony.CellInfoLte;
import android.telephony.CellInfoWcdma;
import android.telephony.CellLocation;
import android.telephony.CellSignalStrengthCdma;
import android.telephony.CellSignalStrengthGsm;
import android.telephony.CellSignalStrengthLte;
import android.telephony.CellSignalStrengthWcdma;
import android.telephony.NeighboringCellInfo;
import android.telephony.TelephonyManager;
import android.telephony.cdma.CdmaCellLocation;
import android.telephony.gsm.GsmCellLocation;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.List;


public class KScanner {
    protected static String name = Namer.getName(KScanner.class);
    protected final Context context;
    protected ILister iLister;
    protected TelephonyManager telephonyManager;
    protected boolean isInit;
    protected volatile int unknownSignalStrength = CellInfo.UNKNOWN_SIGNAL_STRENGTH;

    interface ILister {
        List<PCellInfo> getPCellInfoList(TelephonyManager telephonyManager);
    }

    static class EmptyLister implements ILister {
        private EmptyLister() {
        }

        public final List<PCellInfo> getPCellInfoList(TelephonyManager telephonyManager) {
            return Collections.emptyList();
        }
    }

    @TargetApi(18)
    class Lister implements ILister {
        final /* synthetic */ KScanner instance;

        private Lister(KScanner KScanner) {
            this.instance = KScanner;
        }

        @SuppressLint("MissingPermission")
        public final List<PCellInfo> getPCellInfoList(TelephonyManager telephonyManager) {
            List<android.telephony.CellInfo> allCellInfo = telephonyManager.getAllCellInfo();

            if (allCellInfo == null || allCellInfo.isEmpty()) {
                return Collections.emptyList();
            }

            List<PCellInfo> arrayList = new ArrayList(allCellInfo.size());

            for (android.telephony.CellInfo a : allCellInfo) {
                KScanner.populateInfo(arrayList, a, telephonyManager);
            }
            return arrayList;
        }
    }

    public KScanner(Context context) {
        this.context = context;
    }


    @SuppressLint("MissingPermission")
    public String getDeviceId() {
        String deviceUniqueIdentifier = null;
        TelephonyManager tm = (TelephonyManager) this.context.getSystemService(Context.TELEPHONY_SERVICE);
        if (null != tm) {
            deviceUniqueIdentifier = tm.getDeviceId();
        }
        if (null == deviceUniqueIdentifier || 0 == deviceUniqueIdentifier.length()) {
            deviceUniqueIdentifier = Settings.Secure.getString(this.context.getContentResolver(), Settings.Secure.ANDROID_ID);
        }
        return deviceUniqueIdentifier;
    }




    @TargetApi(18)
    protected static boolean populateInfo(List<PCellInfo> list, android.telephony.CellInfo cellInfo, TelephonyManager telephonyManager) {
        if (telephonyManager.getPhoneType() == 0) {
            return false;
        }
        PCellInfo PCellInfo;
        int mcc;
        int mnc;
        int lac;
        int cid;
        int asuLevel;
        int pci;
        int psc;
        int timingAdvance;
        int simpleLevel;
        int signalStrength;
        boolean z;
        boolean isReg = cellInfo.isRegistered();
        if (cellInfo instanceof CellInfoGsm) {
            CellIdentityGsm cellIdentity = ((CellInfoGsm) cellInfo).getCellIdentity();
            if (!(cellIdentity.getMcc() == Integer.MAX_VALUE || cellIdentity.getMnc() == Integer.MAX_VALUE)) {
                CellSignalStrengthGsm cellSignalStrength = ((CellInfoGsm) cellInfo).getCellSignalStrength();
                PCellInfo = new PCellInfo();
                mcc = cellIdentity.getMcc();
                mnc = cellIdentity.getMnc();
                lac = cellIdentity.getLac();
                cid = cellIdentity.getCid();
                asuLevel = cellSignalStrength.getAsuLevel();
                simpleLevel = cellSignalStrength.getLevel();
                signalStrength = cellSignalStrength.getDbm();
                PCellInfo.cellRadio = "gsm";
                if (mcc == Integer.MAX_VALUE) {
                    mcc = -1;
                }
                PCellInfo.mcc = mcc;
                PCellInfo.mnc = mnc != Integer.MAX_VALUE ? mnc : -1;
                PCellInfo.lac = lac != Integer.MAX_VALUE ? lac : -1;
                PCellInfo.cid = cid != Integer.MAX_VALUE ? cid : -1;
                PCellInfo.mAsu = asuLevel;
                PCellInfo.simpleLevel = simpleLevel;
                PCellInfo.signalStrength = signalStrength;
                if (isReg) PCellInfo.isRegistered = 1;
                list.add(PCellInfo);
                z = true;
            }
            z = false;
        } else if (cellInfo instanceof CellInfoCdma) {
            PCellInfo PCellInfo2 = new PCellInfo();
            CellIdentityCdma cellIdentity2 = ((CellInfoCdma) cellInfo).getCellIdentity();
            CellSignalStrengthCdma cellSignalStrength2 = ((CellInfoCdma) cellInfo).getCellSignalStrength();
            lac = cellIdentity2.getBasestationId();
            mnc = cellIdentity2.getNetworkId();
            mcc = cellIdentity2.getSystemId();

            int latitude = cellIdentity2.getLatitude();
            int longitude = cellIdentity2.getLongitude();

            asuLevel = cellSignalStrength2.getDbm();
            simpleLevel = cellSignalStrength2.getLevel();
            signalStrength = cellSignalStrength2.getDbm();
            PCellInfo2.cellRadio = "cdma";
            if (mcc == Integer.MAX_VALUE) {
                mcc = -1;
            }
            PCellInfo2.mnc = mcc;
            PCellInfo2.lac = mnc != Integer.MAX_VALUE ? mnc : -1;
            PCellInfo2.cid = lac != Integer.MAX_VALUE ? lac : -1;
            PCellInfo2.signalStrength = asuLevel;
            PCellInfo2.simpleLevel = simpleLevel;
            PCellInfo2.signalStrength = signalStrength;
            if (isReg) PCellInfo2.isRegistered = 1;
            list.add(PCellInfo2);
            z = true;
        } else {
            if (cellInfo instanceof CellInfoLte) {
                CellIdentityLte cellIdentity3 = ((CellInfoLte) cellInfo).getCellIdentity();
                if (!(cellIdentity3.getMnc() == Integer.MAX_VALUE || cellIdentity3.getMcc() == Integer.MAX_VALUE) || (cellIdentity3.getPci() >= 0 && cellIdentity3.getPci() < 504)) {
                    PCellInfo = new PCellInfo();
                    CellSignalStrengthLte cellSignalStrength3 = ((CellInfoLte) cellInfo).getCellSignalStrength();
                    mcc = cellIdentity3.getMcc();
                    mnc = cellIdentity3.getMnc();
                    cid = cellIdentity3.getCi();
                    pci = cellIdentity3.getPci();
                    lac = cellIdentity3.getTac();
                    int asuLevel2 = cellSignalStrength3.getAsuLevel();
                    timingAdvance = cellSignalStrength3.getTimingAdvance();
                    simpleLevel = cellSignalStrength3.getLevel();
                    signalStrength = cellSignalStrength3.getDbm();
                    PCellInfo.cellRadio = "lte";
                    if (mcc == Integer.MAX_VALUE) {
                        mcc = -1;
                    }
                    PCellInfo.mcc = mcc;
                    PCellInfo.mnc = mnc != Integer.MAX_VALUE ? mnc : -1;
                    PCellInfo.lac = lac != Integer.MAX_VALUE ? lac : -1;
                    PCellInfo.cid = cid != Integer.MAX_VALUE ? cid : -1;
                    PCellInfo.pscPci = pci != Integer.MAX_VALUE ? pci : -1;
                    PCellInfo.mAsu = asuLevel2;
                    PCellInfo.mTa = timingAdvance;
                    PCellInfo.simpleLevel = simpleLevel;
                    PCellInfo.signalStrength = signalStrength;
                    if (isReg) PCellInfo.isRegistered = 1;
                    list.add(PCellInfo);
                    z = true;
                }
            }
            z = false;
        }
        if (z || VERSION.SDK_INT < 18) {
            return z;
        }
        if (VERSION.SDK_INT >= 18 && (cellInfo instanceof CellInfoWcdma)) {
            CellIdentityWcdma cellIdentity4 = ((CellInfoWcdma) cellInfo).getCellIdentity();
            if (!(cellIdentity4.getMnc() == Integer.MAX_VALUE || cellIdentity4.getMcc() == Integer.MAX_VALUE)) {
                PCellInfo = new PCellInfo();
                CellSignalStrengthWcdma cellSignalStrength4 = ((CellInfoWcdma) cellInfo).getCellSignalStrength();
                mcc = cellIdentity4.getMcc();
                mnc = cellIdentity4.getMnc();
                lac = cellIdentity4.getLac();
                cid = cellIdentity4.getCid();
                psc = cellIdentity4.getPsc();
                simpleLevel =cellSignalStrength4.getLevel();
                signalStrength = cellSignalStrength4.getDbm();
                timingAdvance = cellSignalStrength4.getAsuLevel();
                PCellInfo.cellRadio = "wcdma";
                if (mcc == Integer.MAX_VALUE) {
                    mcc = -1;
                }
                PCellInfo.mcc = mcc;
                PCellInfo.mnc = mnc != Integer.MAX_VALUE ? mnc : -1;
                PCellInfo.lac = lac != Integer.MAX_VALUE ? lac : -1;
                PCellInfo.cid = cid != Integer.MAX_VALUE ? cid : -1;
                PCellInfo.pscPci = psc != Integer.MAX_VALUE ? psc : -1;
                PCellInfo.mAsu = timingAdvance;
                PCellInfo.simpleLevel = simpleLevel;
                PCellInfo.signalStrength = signalStrength;
                if (isReg) PCellInfo.isRegistered = 1;
                list.add(PCellInfo);
                return true;
            }
        }
        return false;
    }

    private String getNetworkOperator() {
        String networkOperator = this.telephonyManager.getNetworkOperator();
        return (networkOperator == null || networkOperator.length() <= 3) ? this.telephonyManager.getSimOperator() : networkOperator;
    }

    private PCellInfo getCellInfoFromCellLocation() {
        @SuppressLint("MissingPermission") CellLocation cellLocation = this.telephonyManager.getCellLocation();
        if (cellLocation == null) {
            return null;
        }
        try {
            PCellInfo PCellInfo = new PCellInfo();
            int i = this.unknownSignalStrength;
            int networkType = this.telephonyManager.getNetworkType();
            String c = getNetworkOperator();
            Integer valueOf = i == CellInfo.UNKNOWN_SIGNAL_STRENGTH ? null : Integer.valueOf(i);
            if (cellLocation instanceof GsmCellLocation) {
                GsmCellLocation gsmCellLocation = (GsmCellLocation) cellLocation;
                PCellInfo.defaultPCellInfo();
                PCellInfo.cellRadio = PCellInfo.determineNetworkType(networkType);
                PCellInfo.validateAndExtractMccMnc(c);
                networkType = gsmCellLocation.getLac();
                int cid = gsmCellLocation.getCid();
                if (networkType >= 0) {
                    PCellInfo.lac = networkType;
                }
                if (cid >= 0) {
                    PCellInfo.cid = cid;
                }
                if (VERSION.SDK_INT >= 9) {
                    i = gsmCellLocation.getPsc();
                    if (i >= 0) {
                        PCellInfo.pscPci = i;
                    }
                }
                if (valueOf != null) {
                    PCellInfo.signalStrength = valueOf.intValue();
                }
            } else if (cellLocation instanceof CdmaCellLocation) {
                CdmaCellLocation cdmaCellLocation = (CdmaCellLocation) cellLocation;
                PCellInfo.defaultPCellInfo();
                PCellInfo.cellRadio = PCellInfo.determineNetworkType(networkType);
                PCellInfo.validateAndExtractMccMnc(c);
                PCellInfo.mnc = cdmaCellLocation.getSystemId();
                PCellInfo.lac = cdmaCellLocation.getNetworkId();
                PCellInfo.cid = cdmaCellLocation.getBaseStationId();
                if (valueOf != null) {
                    PCellInfo.signalStrength = valueOf.intValue();
                }
            } else {
                throw new IllegalArgumentException("Unexpected CellLocation type: " + cellLocation.getClass().getName());
            }
            return PCellInfo;
        } catch (IllegalArgumentException e) {
            new StringBuilder("Skip invalid or incomplete CellLocation: ").append(cellLocation);
            return null;
        }
    }

    @SuppressLint("MissingPermission")
    private List<PCellInfo> getCellInfoFromNeighboringCellInfo() {

        Collection<NeighboringCellInfo> neighboringCellInfo = this.telephonyManager.getNeighboringCellInfo();

        if (neighboringCellInfo == null || neighboringCellInfo.isEmpty()) {
            return Collections.emptyList();
        }

        String c = getNetworkOperator();
        List<PCellInfo> arrayList = new ArrayList(neighboringCellInfo.size());
        for (NeighboringCellInfo neighboringCellInfo2 : neighboringCellInfo) {
            try {
                PCellInfo PCellInfo = new PCellInfo(neighboringCellInfo2, c);
                Object obj = (PCellInfo.cellRadio == null || PCellInfo.cellRadio.length() <= 0 || PCellInfo.cellRadio.equals("0")) ? null : 1;
                if (obj != null) {
                    arrayList.add(PCellInfo);
                }
            } catch (IllegalArgumentException e) {
                new StringBuilder("Skip invalid or incomplete NeighboringCellInfo: ").append(neighboringCellInfo2);
            }
        }
        return arrayList;
    }

    public final synchronized void initialize() {
        if (!this.isInit) {
            TelephonyManager telephonyManager = this.telephonyManager;
            if (telephonyManager == null) {
                telephonyManager = (TelephonyManager) this.context.getSystemService(Context.TELEPHONY_SERVICE);
            }
            Object obj = (telephonyManager == null || !(telephonyManager.getPhoneType() == 2 || telephonyManager.getPhoneType() == 1)) ? null : 1;
            if (obj != null) {
                this.isInit = true;
                if (this.telephonyManager == null) {
                    if (VERSION.SDK_INT >= 18) {
                        this.iLister = new Lister(this);
                    } else {
                        this.iLister = new EmptyLister();
                    }
                    this.telephonyManager = (TelephonyManager) this.context.getSystemService(Context.TELEPHONY_SERVICE);
                    if (this.telephonyManager != null) {
                        switch (this.telephonyManager.getPhoneType()) {
                            case 0:
                            case 3:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }

    public final synchronized List<PCellInfo> ScanAndGetInfo() {
        List<PCellInfo> arrayList = new ArrayList<PCellInfo>();
        Collection a = this.iLister.getPCellInfoList(this.telephonyManager);
        if (a.isEmpty()) {
            PCellInfo d = getCellInfoFromCellLocation();
            if (d != null) {
                arrayList.add(d);
                arrayList.addAll(getCellInfoFromNeighboringCellInfo());
            }
        } else {
            arrayList.addAll(a);
            arrayList.addAll(getCellInfoFromNeighboringCellInfo());
        }
        return arrayList;
    }
}
