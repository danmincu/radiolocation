package com.commonsware.android.job.CellSites;

import android.annotation.SuppressLint;
import android.annotation.TargetApi;
import android.content.Context;
import android.os.Build.VERSION;
import android.telephony.CellIdentityCdma;
import android.telephony.CellIdentityGsm;
import android.telephony.CellIdentityLte;
import android.telephony.CellIdentityWcdma;
import android.telephony.CellInfo;
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
import android.util.Log;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

public class Scanner {
    public static final String TAG = "Scanner : ";
    private static TelephonyManager telephonyManager;
    private Context context;

    public Scanner(Context context) {
        this.context = context;
        if (telephonyManager == null) {
            telephonyManager = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);
        }
    }

    private String getNetworkOperator() {
        String networkOperator = telephonyManager.getNetworkOperator();
        if (networkOperator == null || networkOperator.length() <= 3) {
            return telephonyManager.getSimOperator();
        }
        return networkOperator;
    }

    @SuppressLint("MissingPermission")
    private ArrayList<com.commonsware.android.job.CellSites.CellInfo> getNeighboringCells() {
        ArrayList<com.commonsware.android.job.CellSites.CellInfo> records = new ArrayList();
        com.commonsware.android.job.CellSites.CellInfo serving = getCurrentCellInfo();
        if (serving != null) {
            records.add(serving);
        }
        Collection<NeighboringCellInfo> cells = telephonyManager.getNeighboringCellInfo();
        if (!(cells == null || cells.isEmpty())) {
            String networkOperator = getNetworkOperator();
            for (NeighboringCellInfo nci : cells) {
                try {
                    com.commonsware.android.job.CellSites.CellInfo record = new com.commonsware.android.job.CellSites.CellInfo(nci, networkOperator);
                    if (record.isCellRadioValid()) {
                        records.add(record);
                    }
                } catch (IllegalArgumentException iae) {
                    Log.e(TAG, "Skip invalid or incomplete NeighboringCellInfo: " + nci, iae);
                }
            }
        }
        return records;
    }

    @SuppressLint("MissingPermission")
    protected com.commonsware.android.job.CellSites.CellInfo getCurrentCellInfo() {
        CellLocation currentCell = telephonyManager.getCellLocation();
        if (currentCell == null) {
            return null;
        }
        try {
            Integer num;
            com.commonsware.android.job.CellSites.CellInfo info = new com.commonsware.android.job.CellSites.CellInfo();
            int signalStrength = info.getSignalStrength();
            int networkType = telephonyManager.getNetworkType();
            String networkOperator = getNetworkOperator();
            if (signalStrength == com.commonsware.android.job.CellSites.CellInfo.UNKNOWN_SIGNAL_STRENGTH) {
                num = null;
            } else {
                num = Integer.valueOf(signalStrength);
            }
            info.setCellLocation(currentCell, networkType, networkOperator, num);
            return info;
        } catch (IllegalArgumentException iae) {
            Log.e(TAG, "Skip invalid or incomplete CellLocation: " + currentCell, iae);
            return null;
        }
    }

    /* JADX WARNING: inconsistent code. */
    /* Code decompiled incorrectly, please refer to instructions dump. */
    @SuppressLint("MissingPermission")
    public ArrayList<com.commonsware.android.job.CellSites.CellInfo> getAllCellInfo() {

        ArrayList<com.commonsware.android.job.CellSites.CellInfo> result = new ArrayList<>();

        List<CellInfo> cells = null;

        if (VERSION.SDK_INT >= 18) {
            cells = this.telephonyManager.getAllCellInfo();
        }
        else
        {
            return this.getNeighboringCells();
        }

        if (cells != null)
        {
            for (int i =0; i < cells.size(); i++)
            {
                this.addCellToList(result, cells.get(i),telephonyManager);
            }
        }

        return result;


        /*
        r7 = this;
        r3 = 0;
        r4 = telephonyManager;
        if (r4 != 0) goto L_0x0016;
    L_0x0005:
        r4 = r7.context;
        r5 = "phone";
        r4 = r4.getSystemService(r5);
        r4 = (android.telephony.TelephonyManager) r4;
        telephonyManager = r4;
        r4 = telephonyManager;
        if (r4 != 0) goto L_0x0016;
    L_0x0015:
        return r3;
    L_0x0016:
        r1 = 0;
        r4 = android.os.Build.VERSION.SDK_INT;	 Catch:{ SecurityException -> 0x0038 }
        r5 = 17;
        if (r4 < r5) goto L_0x0033;
    L_0x001d:
        r4 = telephonyManager;	 Catch:{ SecurityException -> 0x0038 }
        r1 = r4.getAllCellInfo();	 Catch:{ SecurityException -> 0x0038 }
        if (r1 == 0) goto L_0x002b;
    L_0x0025:
        r4 = r1.isEmpty();
        if (r4 == 0) goto L_0x0043;
    L_0x002b:
        r4 = "Scanner : ";
        r5 = "No cell towers";
        android.util.Log.d(r4, r5);
        goto L_0x0015;
    L_0x0033:
        r3 = r7.getNeighboringCells();	 Catch:{ SecurityException -> 0x0038 }
        goto L_0x0015;
    L_0x0038:
        r0 = move-exception;
        r4 = "Scanner : ";
        r5 = r0.toString();
        android.util.Log.e(r4, r5);
        goto L_0x0015;
    L_0x0043:
        r4 = "Scanner : ";
        r5 = new java.lang.StringBuilder;
        r5.<init>();
        r6 = "observed size : ";
        r5 = r5.append(r6);
        r6 = r1.size();
        r5 = r5.append(r6);
        r5 = r5.toString();
        android.util.Log.d(r4, r5);
        r3 = new java.util.ArrayList;
        r4 = r1.size();
        r3.<init>(r4);
        r4 = r1.iterator();
    L_0x006c:
        r5 = r4.hasNext();
        if (r5 == 0) goto L_0x007e;
    L_0x0072:
        r2 = r4.next();
        r2 = (android.telephony.CellInfo) r2;
        r5 = telephonyManager;
        r7.addCellToList(r3, r2, r5);
        goto L_0x006c;
    L_0x007e:
        r4 = "Scanner : ";
        r5 = new java.lang.StringBuilder;
        r5.<init>();
        r6 = "returned size : ";
        r5 = r5.append(r6);
        r6 = r3.size();
        r5 = r5.append(r6);
        r6 = " ";
        r5 = r5.append(r6);
        r6 = r3.toString();
        r5 = r5.append(r6);
        r5 = r5.toString();
        android.util.Log.d(r4, r5);
        goto L_0x0015;
        */
        //throw new UnsupportedOperationException("Method not decompiled: com.unwiredlabs.mobilesignaldebugger.Scanners.Scanner.getAllCellInfo():java.util.ArrayList<com.unwiredlabs.mobilesignaldebugger.Scanners.CellInfo>");
    }

    @SuppressLint("MissingPermission")
    @TargetApi(17)
    protected boolean addCellToList(ArrayList<com.commonsware.android.job.CellSites.CellInfo> cells, CellInfo observedCell, TelephonyManager tm) {
        if (tm.getPhoneType() == 0) {
            return false;
        }
        boolean added = false;
        com.commonsware.android.job.CellSites.CellInfo cell;
        if (observedCell instanceof CellInfoGsm) {
            CellIdentityGsm ident = ((CellInfoGsm) observedCell).getCellIdentity();
            if (!((ident.getMcc() == Integer.MAX_VALUE || ident.getMnc() == Integer.MAX_VALUE) && ident.getLac() == Integer.MAX_VALUE)) {
                CellSignalStrengthGsm strength = ((CellInfoGsm) observedCell).getCellSignalStrength();
                cell = new com.commonsware.android.job.CellSites.CellInfo();
                if (observedCell.isRegistered()) cell.setIsRegistered((byte)1);
                cell.setGsmCellInfo(ident.getMcc(), ident.getMnc(), ident.getLac(), ident.getCid(), strength.getAsuLevel(), strength.getDbm(), strength.getLevel());
                cells.add(cell);
                added = true;
            }
        } else if (observedCell instanceof CellInfoCdma) {
            cell = new com.commonsware.android.job.CellSites.CellInfo();
            CellIdentityCdma ident2 = ((CellInfoCdma) observedCell).getCellIdentity();
            //todo here ((CellInfoCdma) observedCell).getCellSignalStrength().getLevel()

            int latitude = ident2.getLatitude();
            int longitude = ident2.getLongitude();
            CellSignalStrengthCdma strengthCdma = ((CellInfoCdma) observedCell).getCellSignalStrength();

            cell.setCdmaCellInfo(ident2.getBasestationId(), ident2.getNetworkId(), ident2.getSystemId(), strengthCdma.getDbm(),strengthCdma.getLevel());
            if (observedCell.isRegistered()) cell.setIsRegistered((byte)1);
            cells.add(cell);
            added = true;
        } else if (observedCell instanceof CellInfoLte) {
            CellIdentityLte ident3 = ((CellInfoLte) observedCell).getCellIdentity();
            if (!(ident3.getMnc() == Integer.MAX_VALUE || ident3.getMcc() == Integer.MAX_VALUE) || (ident3.getPci() >= 0 && ident3.getPci() < 504)) {
                cell = new com.commonsware.android.job.CellSites.CellInfo();
                CellSignalStrengthLte strength2 = ((CellInfoLte) observedCell).getCellSignalStrength();
                cell.setLteCellInfo(ident3.getMcc(),
                        ident3.getMnc(),
                        ident3.getCi(),
                        ident3.getPci(),
                        ident3.getTac(),
                        strength2.getAsuLevel(),
                        strength2.getDbm(),
                        strength2.getTimingAdvance(),
                        strength2.getLevel());
                if (observedCell.isRegistered()) cell.setIsRegistered((byte)1);
                cells.add(cell);
                added = true;
            }
        }
        if (added || VERSION.SDK_INT < 18) {
            return added;
        }
        return addWCDMACellToList(cells, observedCell, tm);
    }

    @TargetApi(17)
    protected boolean addWCDMACellToList(List<com.commonsware.android.job.CellSites.CellInfo> cells, CellInfo observedCell, TelephonyManager tm) {
        if (VERSION.SDK_INT < 18 || !(observedCell instanceof CellInfoWcdma)) {
            return false;
        }
        CellIdentityWcdma ident = ((CellInfoWcdma) observedCell).getCellIdentity();
        if ((ident.getMnc() == Integer.MAX_VALUE || ident.getMcc() == Integer.MAX_VALUE) && (ident.getPsc() < 0 || ident.getPsc() >= 512)) {
            return false;
        }
        com.commonsware.android.job.CellSites.CellInfo cell = new com.commonsware.android.job.CellSites.CellInfo();
        CellSignalStrengthWcdma strength = ((CellInfoWcdma) observedCell).getCellSignalStrength();
        cell.setWcdmaCellInfo(ident.getMcc(), ident.getMnc(), ident.getLac(), ident.getCid(), ident.getPsc(), strength.getAsuLevel(), strength.getDbm());
        if (observedCell.isRegistered()) cell.setIsRegistered((byte)1);
        cells.add(cell);
        return true;
    }
}
