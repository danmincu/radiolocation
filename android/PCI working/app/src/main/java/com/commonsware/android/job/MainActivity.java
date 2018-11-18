/***
  Copyright (c) 2014-2016 CommonsWare, LLC
  Licensed under the Apache License, Version 2.0 (the "License"); you may not
  use this file except in compliance with the License. You may obtain a copy
  of the License at http://www.apache.org/licenses/LICENSE-2.0. Unless required
  by applicable law or agreed to in writing, software distributed under the
  License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS
  OF ANY KIND, either express or implied. See the License for the specific
  language governing permissions and limitations under the License.
  
  Covered in detail in the book _The Busy Coder's Guide to Android Development_
    https://commonsware.com/Android
 */

package com.commonsware.android.job;

import android.annotation.SuppressLint;
import android.annotation.TargetApi;
import android.app.AlarmManager;
import android.app.job.JobInfo;
import android.app.job.JobScheduler;
import android.content.ComponentName;
import android.content.Context;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.PersistableBundle;
import android.os.SystemClock;
import android.widget.ArrayAdapter;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.Switch;
import android.widget.Toast;

import com.commonsware.android.job.CellSites.CellInfo;
import com.commonsware.android.job.CellSites.PCellInfo;
import com.commonsware.android.job.CellSites.KScanner;
import com.commonsware.android.job.CellSites.Scanner;
import com.evernote.android.job.JobManager;
import com.evernote.android.job.JobRequest;
import com.evernote.android.job.util.support.PersistableBundleCompat;

import java.io.BufferedInputStream;
import java.io.DataOutputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;

import static android.Manifest.permission.ACCESS_COARSE_LOCATION;
import static android.Manifest.permission.ACCESS_FINE_LOCATION;
import static android.Manifest.permission.READ_PHONE_STATE;
import static android.Manifest.permission.RECEIVE_BOOT_COMPLETED;
import static android.Manifest.permission.WAKE_LOCK;
import static android.Manifest.permission.WRITE_EXTERNAL_STORAGE;

public class MainActivity extends AbstractPermissionActivity
    implements CompoundButton.OnCheckedChangeListener, LocationListener {
  private static final long[] PERIODS={
      60000,
      AlarmManager.INTERVAL_FIFTEEN_MINUTES,
      AlarmManager.INTERVAL_HALF_HOUR,
      AlarmManager.INTERVAL_HOUR
  };
  private static final int JOB_ID=1337;
  static final String KEY_DOWNLOAD="isDownload";
  private Spinner type=null;
  private Spinner period=null;
  private EditText editText=null;
  private Switch download=null;
  private AlarmManager alarms=null;
  private int unifiedJobId=-1;
  private UIUpdater mUIUpdater=null;
  private LocationManager lmgr=null;

  @Override
  protected void onCreate(Bundle savedInstanceState) {
       super.onCreate(savedInstanceState);
      lmgr=(LocationManager)this.getSystemService(Context.LOCATION_SERVICE);
  }

    @SuppressLint("MissingPermission")
    @Override
    protected void onStart() {
        super.onStart();
//        lmgr.requestLocationUpdates(LocationManager.GPS_PROVIDER,10000,50, this);

    }

    @Override
    @SuppressWarnings({"MissingPermission"})
    public void onStop() {
        //lmgr.removeUpdates(this);

        super.onStop();
    }


    @Override
    public void onLocationChanged(Location location) {
        double roundedLat = (double) Math.round(location.getLatitude() * 10000d) / 10000d;
        double roundedLon = (double) Math.round(location.getLongitude() * 10000d) / 10000d;
    }

    @Override
    public void onProviderEnabled(String provider) {

    }

    @Override
    public void onProviderDisabled(String provider) {

    }

    @Override
    public void onStatusChanged(String provider, int status, Bundle extras) {

    }

    protected String[] getDesiredPermissions() {
    return(new String[]{WRITE_EXTERNAL_STORAGE, READ_PHONE_STATE, ACCESS_COARSE_LOCATION, ACCESS_FINE_LOCATION, RECEIVE_BOOT_COMPLETED, WAKE_LOCK});
  }

  @Override
  protected void onPermissionDenied() {
    Toast
      .makeText(this, R.string.msg_sorry, Toast.LENGTH_LONG)
      .show();
    finish();
  }

  @SuppressWarnings("ResourceType")
  @Override
  public void onReady(Bundle savedInstanceState) {
    setContentView(R.layout.main);
    type=(Spinner)findViewById(R.id.type);
    editText = (EditText)findViewById(R.id.editText);


    ArrayAdapter<String> types=
        new ArrayAdapter<String>(this,
            android.R.layout.simple_spinner_item,
            getResources().getStringArray(R.array.types));

    types.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
    type.setAdapter(types);

    period=(Spinner)findViewById(R.id.period);

    ArrayAdapter<String> periods=
        new ArrayAdapter<String>(this,
            android.R.layout.simple_spinner_item,
            getResources().getStringArray(R.array.periods));

    periods.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
    period.setAdapter(periods);

    download=(Switch)findViewById(R.id.download);

    ((Switch)findViewById(R.id.scheduled))
      .setOnCheckedChangeListener(this);

    alarms=(AlarmManager)getSystemService(ALARM_SERVICE);


    mUIUpdater = new UIUpdater(new Runnable() {
      @Override
      public void run() {
        Invoke();
      }
    });
  }

  @Override
  @SuppressWarnings({"MissingPermission"})
  public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
    toggleWidgets(!isChecked);

    switch(type.getSelectedItemPosition()) {
        case 0:
          if (isChecked) {
              mUIUpdater.startUpdates();
          }
          else
            mUIUpdater.stopUpdates();
            //Invoke();
            break;
      case 1:
        manageExact(isChecked);
        break;

      case 2:
        manageInexact(isChecked);
        break;

      case 3:
        manageUnified(isChecked);
        break;

      case 4:
        manageJobScheduler(isChecked);
        break;
    }
  }

    @TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR1)
    private long age_ms_api_17(Location last) {
        return (SystemClock.elapsedRealtimeNanos() - last
                .getElapsedRealtimeNanos()) / (1000000 * 1000);
    }


    @SuppressLint("MissingPermission")
  private void Invoke() {

    Location location =  lmgr.getLastKnownLocation(LocationManager.GPS_PROVIDER);


    KScanner scan = new KScanner(this);
    scan.initialize();
    //todo - determine the main cell (not the neighbours)
    List<PCellInfo> cellSites = scan.ScanAndGetInfo();
    String deviceId = scan.getDeviceId();
    editText.setText("");
    StringBuilder sb = new StringBuilder();

    Scanner scanner = new Scanner(this);
    ArrayList<CellInfo> cells = scanner.getAllCellInfo();
    //merge
    for (PCellInfo pcell: cellSites) {
      CellInfo c = new CellInfo(pcell);
        if (!cells.contains(c))
          cells.add(c);
    }

    if (cells != null && cells.size() > 0)
    {
      sb.append("#deviceId,deviceTime\n");
      sb.append(deviceId +',' + Calendar.getInstance().getTime().getTime() + '\n');
      sb.append("#latitude,longitude,age,accuracy,speed,bearing\n");
      sb.append(String.format("%.6f,%.6f,%s,%s,%s,%s", location.getLatitude(), location.getLongitude(),age_ms_api_17(location), location.hasAccuracy() ? location.getAccuracy() : "?",
              location.hasSpeed() ? location.getSpeed() : "?", location.hasBearing() ? location.getBearing() : "?" )+'\n');
      sb.append("#" + CellInfo.header()+'\n');
      for (int i =0; i < cells.size(); i++)
      {
        sb.append(cells.get(i).toString() + '\n');
      }
    }

    editText.setText(sb.toString());

    final String parameter = sb.toString();
    Thread thread = new Thread(new Runnable() {
      String p = "{ 'location':'" + parameter + "'}";
      @Override
      public void run() {
        try  {
          URL url = new URL("http://radiolocation.ownme.ca:5013/collector/radioLocation");
          HttpURLConnection conn = (HttpURLConnection) url.openConnection();

          conn.setRequestMethod("POST");

          byte[] postData       = p.getBytes( StandardCharsets.UTF_8 );
          int    postDataLength = postData.length;

          conn.setDoOutput( true );
          conn.setInstanceFollowRedirects( false );
          conn.setRequestMethod( "POST" );
          conn.setRequestProperty( "Content-Type", "application/json");
          conn.setRequestProperty( "charset", "utf-8");
          conn.setRequestProperty( "Content-Length", Integer.toString( postDataLength ));
          conn.setUseCaches( false );
          try( DataOutputStream wr = new DataOutputStream( conn.getOutputStream())) {
            wr.write( postData );
          }

          // read the response
          System.out.println("Response Code: " + conn.getResponseCode());
          InputStream in = new BufferedInputStream(conn.getInputStream());
          String response = org.apache.commons.io.IOUtils.toString(in, "UTF-8");
          System.out.println(response);
        } catch (Exception e) {
          e.printStackTrace();
        }
      }
    });
    thread.start();
  }

  public class UIUpdater {
    // Create a Handler that uses the Main Looper to run in
    private Handler mHandler = new Handler(Looper.getMainLooper());

    private Runnable mStatusChecker;
    private int UPDATE_INTERVAL = 10000;

    /**
     * Creates an UIUpdater object, that can be used to
     * perform UIUpdates on a specified time interval.
     *
     * @param uiUpdater A runnable containing the update routine.
     */
    public UIUpdater(final Runnable uiUpdater) {
      mStatusChecker = new Runnable() {
        @Override
        public void run() {
          // Run the passed runnable
          uiUpdater.run();
          // Re-run it after the update interval
          mHandler.postDelayed(this, UPDATE_INTERVAL);
        }
      };
    }

    /**
     * The same as the default constructor, but specifying the
     * intended update interval.
     *
     * @param uiUpdater A runnable containing the update routine.
     * @param interval  The interval over which the routine
     *                  should run (milliseconds).
     */
    public UIUpdater(Runnable uiUpdater, int interval){
      this(uiUpdater);
      UPDATE_INTERVAL = interval;
    }

    /**
     * Starts the periodical update routine (mStatusChecker
     * adds the callback to the handler).
     */
    public synchronized void startUpdates(){
      mStatusChecker.run();
    }

    /**
     * Stops the periodical update routine from running,
     * by removing the callback.
     */
    public synchronized void stopUpdates(){
      mHandler.removeCallbacks(mStatusChecker);
    }

    void startRepeatingTask() {
      mStatusChecker.run();
    }

    void stopRepeatingTask() {
      mHandler.removeCallbacks(mStatusChecker);
    }
  }


  private void toggleWidgets(boolean enable) {
    type.setEnabled(enable);
    period.setEnabled(enable);
    download.setEnabled(enable);
  }

  private void manageExact(boolean start) {
    if (start) {
      long period=getPeriod();

      PollReceiver.scheduleExactAlarm(this, alarms, period,
          download.isChecked());
    }
    else {
      PollReceiver.cancelAlarm(this, alarms);
    }
  }

  private void manageInexact(boolean start) {
    if (start) {
      long period=getPeriod();

      PollReceiver.scheduleInexactAlarm(this, alarms, period,
          download.isChecked());
    }
    else {
      PollReceiver.cancelAlarm(this, alarms);
    }
  }

  private void manageUnified(boolean start) {
    if (start) {
      final JobRequest.Builder b=
        new JobRequest.Builder(DemoUnifiedJob.JOB_TAG);
      PersistableBundleCompat extras=new PersistableBundleCompat();

      if (download.isChecked()) {
        extras.putBoolean(KEY_DOWNLOAD, true);
        b
          .setExtras(extras)
          .setRequiredNetworkType(JobRequest.NetworkType.CONNECTED);
      }
      else {
        b.setRequiredNetworkType(JobRequest.NetworkType.ANY);
      }

      b
        .setPeriodic(getPeriod())
        .setRequiresCharging(false)
        .setRequiresDeviceIdle(false);

      unifiedJobId=b.build().schedule();
    }
    else {
      JobManager.instance().cancel(unifiedJobId);
    }
  }

  @TargetApi(Build.VERSION_CODES.LOLLIPOP_MR1)
  private void manageJobScheduler(boolean start) {
    JobScheduler jobs=
      (JobScheduler)getSystemService(JOB_SCHEDULER_SERVICE);

    if (start) {
      JobInfo.Builder b=new JobInfo.Builder(JOB_ID,
          new ComponentName(this, DemoJobService.class));
      PersistableBundle pb=new PersistableBundle();

      if (download.isChecked()) {
        pb.putBoolean(KEY_DOWNLOAD, true);
        b.setExtras(pb).setRequiredNetworkType(JobInfo.NETWORK_TYPE_ANY);
      } else {
        b.setRequiredNetworkType(JobInfo.NETWORK_TYPE_NONE);
      }

      b.setPeriodic(getPeriod()).setPersisted(false)
          .setRequiresCharging(false).setRequiresDeviceIdle(true);

      jobs.schedule(b.build());
    }
    else {
      jobs.cancel(JOB_ID);
    }
  }

  private long getPeriod() {
    return(PERIODS[period.getSelectedItemPosition()]);
  }
}